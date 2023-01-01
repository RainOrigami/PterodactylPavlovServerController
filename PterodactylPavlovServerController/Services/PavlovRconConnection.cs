using Microsoft.EntityFrameworkCore;
using PavlovVR_Rcon.Models.Pavlov;
using PterodactylPavlovServerController.Contexts;
using PterodactylPavlovServerDomain.Models;

namespace PterodactylPavlovServerController.Services;

public class PavlovRconConnection : IDisposable
{
    public delegate void PlayerDetailUpdate(string serverId, PlayerDetail playerDetail);

    public delegate void ServerError(string serverId, string error);

    public delegate void ServerUpdated(string serverId);

    private readonly IConfiguration configuration;
    private readonly PavlovRconService pavlovRconService;
    private readonly SteamService steamService;
    private PavlovServerContext? context = null;
    private readonly CancellationTokenSource updateCancellationTokenSource = new();
    private Dictionary<ulong, PlayerDetail>? playerDetails;
    private Dictionary<ulong, Player>? playerListPlayers;
    private Dictionary<ulong, DateTime> playerConnectionTime = new();

    private ulong[]? banList;
    public string ApiKey { get; }

    public PavlovRconConnection(string apiKey, PterodactylServerModel pterodactylServer, PavlovRconService pavlovRconService, SteamService steamService, IConfiguration configuration)
    {
        PterodactylServer = pterodactylServer;
        this.pavlovRconService = pavlovRconService;
        this.steamService = steamService;
        this.configuration = configuration;
        ApiKey = apiKey;
    }

    public string ServerId => PterodactylServer.ServerId;

    public bool? Online { get; private set; }

    public PterodactylServerModel PterodactylServer { get; }
    public ServerInfo? ServerInfo { get; private set; }
    public IReadOnlyDictionary<ulong, Player>? PlayerListPlayers => playerListPlayers;
    public IReadOnlyDictionary<ulong, PlayerDetail>? PlayerDetails => playerDetails;
    public ulong[]? BanList => banList;

    public void Dispose()
    {
        updateCancellationTokenSource.Cancel();
    }

    public void Run()
    {
        Task.Run(run);
    }

    private async Task run()
    {
        while (!updateCancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                if (context != null)
                {
                    await context.DisposeAsync();
                }

                context = new(this.configuration);
                await updateServerInfo();
                await updatePlayerList();
                await updatePlayerDetails();
                await updatePlayerSummaries();
                await updatePlayerBans();
                await unbanPlayers();

                await context.SaveChangesAsync();
                await context.DisposeAsync();
                context = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            await Task.Delay(1000);
        }
    }

    public event ServerUpdated? OnServerInfoUpdated;
    public event ServerUpdated? OnServerOnlineStateChanged;
    public event ServerError? OnServerErrorRaised;

    private int failCount;
    private const int maxFailCount = 3;

    private async Task updateServerInfo()
    {
        try
        {
            ServerInfo = await pavlovRconService.GetServerInfo(ApiKey, ServerId);
            banList = await pavlovRconService.Banlist(ApiKey, ServerId);
        }
        catch (Exception ex)
        {
            if (failCount++ >= maxFailCount)
            {

                Online = false;
                OnServerOnlineStateChanged?.Invoke(ServerId);
                OnServerErrorRaised?.Invoke(ServerId, $"Error during server info update: {ex.Message}");
            }

            return;
        }

        if (!Online.HasValue || !Online.Value)
        {
            Online = true;
            OnServerOnlineStateChanged?.Invoke(ServerId);
        }

        failCount = 0;

        OnServerInfoUpdated?.Invoke(ServerId);
    }

    public event ServerUpdated? OnPlayerListUpdated;

    private async Task updatePlayerList()
    {
        if (!Online.HasValue || !Online.Value || failCount > 0)
        {
            return;
        }

        try
        {
            Player[] playerListPlayerModels = await pavlovRconService.GetActivePlayers(ApiKey, ServerId);
            playerListPlayers = playerListPlayerModels.Where(p => p.UniqueId.HasValue).ToDictionary(k => k.UniqueId!.Value, v => v);
        }
        catch (Exception ex)
        {
            OnServerErrorRaised?.Invoke(ServerId, $"Error during player list update: {ex.Message}");
            return;
        }

        OnPlayerListUpdated?.Invoke(ServerId);

        foreach (Player player in playerListPlayers.Values)
        {
            await persistPlayer(player);
        }

        await measurePlayerOnlineTimes(playerListPlayers.Values.ToList());
    }

    public event PlayerDetailUpdate? OnPlayerDetailUpdated;

    private async Task updatePlayerDetails()
    {
        if (!Online.HasValue || !Online.Value || PlayerListPlayers == null || failCount > 0)
        {
            return;
        }

        List<PlayerDetail> newPlayerDetails = new();
        foreach (ulong playerId in PlayerListPlayers.Keys)
        {
            try
            {
                PlayerDetail playerDetail = await pavlovRconService.GetActivePlayerDetail(ApiKey, ServerId, playerId);

                if (playerDetail == null)
                {
                    throw new Exception("Failed to fetch player details");
                }

                newPlayerDetails.Add(playerDetail);
                OnPlayerDetailUpdated?.Invoke(ServerId, playerDetail);
            }
            catch (Exception ex)
            {
                OnServerErrorRaised?.Invoke(ServerId, $"Error during player detail update: {ex.Message}");
            }
        }

        playerDetails = newPlayerDetails.ToDictionary(k => k.UniqueId, v => v);

        await measurePlayerIncome(newPlayerDetails);
    }

    public async Task updatePlayerSummaries()
    {
        if (!Online.HasValue || !Online.Value || PlayerListPlayers == null || failCount > 0)
        {
            return;
        }

        foreach (ulong playerId in PlayerListPlayers.Keys)
        {
            try
            {
                await steamService.GetPlayerSummary(playerId);
            }
            catch { }
        }
    }

    public async Task updatePlayerBans()
    {
        if (!Online.HasValue || !Online.Value || PlayerListPlayers == null || failCount > 0)
        {
            return;
        }

        foreach (ulong playerId in PlayerListPlayers.Keys)
        {
            try
            {
                await steamService.GetBans(playerId);
            }
            catch { }
        }
    }

    private async Task persistPlayer(Player player)
    {
        PersistentPavlovPlayerModel? dbPlayer = await context.Players.SingleOrDefaultAsync(p => p.UniqueId == player.UniqueId && p.ServerId == ServerId);
        if (dbPlayer == null)
        {
            context.Players.Add(new PersistentPavlovPlayerModel()
            {
                ServerId = ServerId,
                UniqueId = player.UniqueId!.Value,
                Username = player.Username,
                LastSeen = DateTime.Now.ToUniversalTime()
            });
        }
        else
        {
            if (dbPlayer.Username != player.Username)
            {
                dbPlayer.Username = player.Username;
            }

            dbPlayer.LastSeen = DateTime.Now.ToUniversalTime();
        }
    }

    private async Task measurePlayerOnlineTimes(List<Player> players)
    {
        foreach (ulong playerId in playerConnectionTime.Keys)
        {
            if (players.All(p => p.UniqueId != playerId))
            {
                playerConnectionTime.Remove(playerId);
            }
        }

        foreach (ulong playerId in players.Select(p => p.UniqueId!.Value))
        {
            if (!playerConnectionTime.TryGetValue(playerId, out DateTime lastMeasured))
            {
                playerConnectionTime.Add(playerId, DateTime.Now);
                lastMeasured = DateTime.Now;
            }

            PersistentPavlovPlayerModel? dbPlayer = await context.Players.SingleOrDefaultAsync(p => p.UniqueId == playerId && p.ServerId == ServerId);
            if (dbPlayer == null)
            {
                continue;
            }

            dbPlayer.TotalTime += DateTime.Now - lastMeasured;
            playerConnectionTime[playerId] = DateTime.Now;
        }
    }

    private Dictionary<ulong, int> lastPlayerMoney = new();

    private async Task measurePlayerIncome(List<PlayerDetail> players)
    {
        foreach (ulong playerId in lastPlayerMoney.Keys)
        {
            if (players.All(p => p.UniqueId != playerId))
            {
                lastPlayerMoney.Remove(playerId);
            }
        }

        foreach (PlayerDetail player in players)
        {
            if (!lastPlayerMoney.TryGetValue(player.UniqueId, out int lastMoney))
            {
                lastPlayerMoney.Add(player.UniqueId, player.Cash);
                lastMoney = player.Cash;
            }

            PersistentPavlovPlayerModel? dbPlayer = await context.Players.SingleOrDefaultAsync(p => p.UniqueId == player.UniqueId && p.ServerId == ServerId);
            if (dbPlayer == null)
            {
                continue;
            }

            if (player.Cash != 900 && lastMoney != 900)
            {
                if (ServerInfo?.MapLabel == "UGC2835308991")
                {
                    dbPlayer.EFPCash = player.Cash;
                }
                dbPlayer.TotalMoneyEarned += (uint)Math.Max(player.Cash - lastMoney, 0);
            }

            lastPlayerMoney[player.UniqueId] = player.Cash;
        }

    }

    private async Task unbanPlayers()
    {
        foreach (PersistentPavlovPlayerModel bannedPlayer in context.Players.Where(p => p.ServerId == ServerId && p.UnbanAt != null && p.UnbanAt < DateTime.Now))
        {
            try
            {
                if (await pavlovRconService.UnbanPlayer(ApiKey, ServerId, bannedPlayer.UniqueId))
                {
                    bannedPlayer.UnbanAt = null;
                }
            }
            catch { }
        }
    }
}
