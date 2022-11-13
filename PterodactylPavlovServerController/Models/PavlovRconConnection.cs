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

    private readonly CancellationTokenSource updateCancellationTokenSource = new();
    private Dictionary<ulong, PlayerDetail>? playerDetails;
    private Dictionary<ulong, Player>? playerListPlayers;
    private ulong[]? banList;
    public string ApiKey { get; }

    public PavlovRconConnection(string apiKey, PterodactylServerModel pterodactylServer, PavlovRconService pavlovRconService, IConfiguration configuration)
    {
        this.PterodactylServer = pterodactylServer;
        this.pavlovRconService = pavlovRconService;
        this.configuration = configuration;
        this.ApiKey = apiKey;
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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
                this.OnServerErrorRaised?.Invoke(this.ServerId, ex.Message);
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
            this.playerListPlayers = playerListPlayerModels.ToDictionary(k => k.UniqueId, v => v);
        }
        catch (Exception ex)
        {
            this.OnServerErrorRaised?.Invoke(this.ServerId, ex.Message);
            return;
        }

        this.OnPlayerListUpdated?.Invoke(this.ServerId);

        await using PavlovServerContext context = new(this.configuration);

        foreach (Player player in this.playerListPlayers.Values)
        {
            PersistentPavlovPlayer? dbPlayer = context.Players.FirstOrDefault(p => p.UniqueId == player.UniqueId && p.ServerId == this.ServerId);
            if (dbPlayer == null)
            {
                context.Players.Add(new PersistentPavlovPlayer()
                {
                    ServerId = this.ServerId,
                    UniqueId = player.UniqueId,
                    Username = player.Username
                });
            }
            else if (dbPlayer.Username != player.Username)
            {
                dbPlayer.Username = player.Username;
            }
        }

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
                this.OnServerErrorRaised?.Invoke(this.ServerId, ex.Message);
            }
        }

        this.playerDetails = newPlayerDetails.ToDictionary(k => k.UniqueId, v => v);
    }
}
