using Microsoft.EntityFrameworkCore;
using PavlovVR_Rcon.Models.Pavlov;
using PterodactylPavlovServerController.Contexts;
using PterodactylPavlovServerController.Services;
using PterodactylPavlovServerDomain.Models;

// TODO: not really a model, move somewhere else
namespace PterodactylPavlovServerController.Models;

public class PavlovRconConnection : IDisposable
{
    public delegate void PlayerDetailUpdate(string serverId, PlayerDetail playerDetail);

    public delegate void ServerError(string serverId, string error);

    public delegate void ServerUpdated(string serverId);

    private readonly IConfiguration configuration;
    private readonly PavlovRconService pavlovRconService;
    private readonly SteamService steamService;
    private readonly PavlovServerContext context;
    private readonly CancellationTokenSource updateCancellationTokenSource = new();
    private Dictionary<ulong, PlayerDetail>? playerDetails;
    private Dictionary<ulong, Player>? playerListPlayers;
    private Dictionary<ulong, DateTime> playerConnectionTime = new();

    private ulong[]? banList;
    public string ApiKey { get; }

    public PavlovRconConnection(string apiKey, PterodactylServerModel pterodactylServer, PavlovRconService pavlovRconService, SteamService steamService, IConfiguration configuration)
    {
        this.PterodactylServer = pterodactylServer;
        this.pavlovRconService = pavlovRconService;
        this.steamService = steamService;
        this.configuration = configuration;
        this.ApiKey = apiKey;
        this.context = new(this.configuration);
    }

    public string ServerId => this.PterodactylServer.ServerId;

    public bool? Online { get; private set; }

    public PterodactylServerModel PterodactylServer { get; }
    public ServerInfo? ServerInfo { get; private set; }
    public IReadOnlyDictionary<ulong, Player>? PlayerListPlayers => this.playerListPlayers;
    public IReadOnlyDictionary<ulong, PlayerDetail>? PlayerDetails => this.playerDetails;
    public ulong[]? BanList => this.banList;

    public void Dispose()
    {
        this.updateCancellationTokenSource.Cancel();
    }

    public void Run()
    {
        Task.Run(this.run);
    }

    private async Task run()
    {
        while (!this.updateCancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                await this.updateServerInfo();
                await this.updatePlayerList();
                await this.updatePlayerDetails();
                await this.updatePlayerSummaries();
                await this.updatePlayerBans();
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
            this.ServerInfo = await this.pavlovRconService.GetServerInfo(this.ApiKey, this.ServerId);
            this.banList = await this.pavlovRconService.Banlist(this.ApiKey, this.ServerId);
        }
        catch (Exception ex)
        {
            if (this.failCount++ >= maxFailCount)
            {

                this.Online = false;
                this.OnServerOnlineStateChanged?.Invoke(this.ServerId);
                this.OnServerErrorRaised?.Invoke(this.ServerId, $"Error during server info update: {ex.Message}");
            }

            return;
        }

        if (!this.Online.HasValue || !this.Online.Value)
        {
            this.Online = true;
            this.OnServerOnlineStateChanged?.Invoke(this.ServerId);
        }

        this.failCount = 0;

        this.OnServerInfoUpdated?.Invoke(this.ServerId);
    }

    public event ServerUpdated? OnPlayerListUpdated;

    private async Task updatePlayerList()
    {
        if (!this.Online.HasValue || !this.Online.Value || failCount > 0)
        {
            return;
        }

        try
        {
            Player[] playerListPlayerModels = await this.pavlovRconService.GetActivePlayers(this.ApiKey, this.ServerId);
            this.playerListPlayers = playerListPlayerModels.Where(p => p.UniqueId.HasValue).ToDictionary(k => k.UniqueId!.Value, v => v);
        }
        catch (Exception ex)
        {
            this.OnServerErrorRaised?.Invoke(this.ServerId, $"Error during player list update: {ex.Message}");
            return;
        }

        this.OnPlayerListUpdated?.Invoke(this.ServerId);

        foreach (Player player in this.playerListPlayers.Values)
        {
            await this.persistPlayer(player);
        }

        await this.measurePlayerOnlineTimes(this.playerListPlayers.Values.ToList());

        await context.SaveChangesAsync();
    }

    public event PlayerDetailUpdate? OnPlayerDetailUpdated;

    private async Task updatePlayerDetails()
    {
        if (!this.Online.HasValue || !this.Online.Value || this.PlayerListPlayers == null || failCount > 0)
        {
            return;
        }

        List<PlayerDetail> newPlayerDetails = new();
        foreach (ulong playerId in this.PlayerListPlayers.Keys)
        {
            try
            {
                PlayerDetail playerDetail = await this.pavlovRconService.GetActivePlayerDetail(this.ApiKey, this.ServerId, playerId);

                if (playerDetail == null)
                {
                    throw new Exception("Failed to fetch player details");
                }

                newPlayerDetails.Add(playerDetail);
                this.OnPlayerDetailUpdated?.Invoke(this.ServerId, playerDetail);
            }
            catch (Exception ex)
            {
                this.OnServerErrorRaised?.Invoke(this.ServerId, $"Error during player detail update: {ex.Message}");
            }
        }

        this.playerDetails = newPlayerDetails.ToDictionary(k => k.UniqueId, v => v);
    }

    public async Task updatePlayerSummaries()
    {
        if (!this.Online.HasValue || !this.Online.Value || this.PlayerListPlayers == null || failCount > 0)
        {
            return;
        }

        foreach (ulong playerId in this.PlayerListPlayers.Keys)
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
        if (!this.Online.HasValue || !this.Online.Value || this.PlayerListPlayers == null || failCount > 0)
        {
            return;
        }

        foreach (ulong playerId in this.PlayerListPlayers.Keys)
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
        PersistentPavlovPlayer? dbPlayer = await context.Players.SingleOrDefaultAsync(p => p.UniqueId == player.UniqueId && p.ServerId == this.ServerId);
        if (dbPlayer == null)
        {
            context.Players.Add(new PersistentPavlovPlayer()
            {
                ServerId = this.ServerId,
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

            PersistentPavlovPlayer? dbPlayer = await context.Players.SingleOrDefaultAsync(p => p.UniqueId == playerId && p.ServerId == this.ServerId);
            if (dbPlayer == null)
            {
                continue;
            }

            dbPlayer.TotalTime += (DateTime.Now - lastMeasured);
            playerConnectionTime[playerId] = DateTime.Now;
        }
    }
}
