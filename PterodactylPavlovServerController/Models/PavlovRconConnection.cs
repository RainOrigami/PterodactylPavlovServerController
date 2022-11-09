using PterodactylPavlovServerController.Contexts;
using PterodactylPavlovServerController.Services;
using PterodactylPavlovServerDomain.Models;

// TODO: not really a model, move somewhere else
namespace PterodactylPavlovServerController.Models;

public class PavlovRconConnection : IDisposable
{
    public delegate void PlayerDetailUpdate(string serverId, PlayerDetailModel playerDetail);

    public delegate void ServerError(string serverId, string error);

    public delegate void ServerUpdated(string serverId);

    private readonly IConfiguration configuration;
    private readonly PavlovRconService pavlovRconService;

    private readonly CancellationTokenSource updateCancellationTokenSource = new();
    private Dictionary<ulong, PlayerDetailModel>? playerDetails;
    private Dictionary<ulong, PlayerListPlayerModel>? playerListPlayers;

    public PavlovRconConnection(PterodactylServerModel pterodactylServer, PavlovRconService pavlovRconService, IConfiguration configuration)
    {
        this.PterodactylServer = pterodactylServer;
        this.pavlovRconService = pavlovRconService;
        this.configuration = configuration;
    }

    public string ServerId => this.PterodactylServer.ServerId;
    public bool? Online { get; set; }

    public PterodactylServerModel PterodactylServer { get; }
    public ServerInfoModel? ServerInfo { get; private set; }
    public IReadOnlyDictionary<ulong, PlayerListPlayerModel>? PlayerListPlayers => this.playerListPlayers;
    public IReadOnlyDictionary<ulong, PlayerDetailModel>? PlayerDetails => this.playerDetails;

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
                this.updateServerInfo();
                this.updatePlayerList();
                this.updatePlayerDetails();
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

    private void updateServerInfo()
    {
        try
        {
            this.ServerInfo = this.pavlovRconService.GetServerInfo(this.ServerId);
        }
        catch (Exception ex)
        {
            this.Online = false;
            this.OnServerOnlineStateChanged?.Invoke(this.ServerId);
            this.OnServerErrorRaised?.Invoke(this.ServerId, ex.Message);

            return;
        }

        if (!this.Online.HasValue || !this.Online.Value)
        {
            this.Online = true;
            this.OnServerOnlineStateChanged?.Invoke(this.ServerId);
        }

        this.OnServerInfoUpdated?.Invoke(this.ServerId);
    }

    public event ServerUpdated? OnPlayerListUpdated;

    private void updatePlayerList()
    {
        if (!this.Online.HasValue || !this.Online.Value)
        {
            return;
        }

        try
        {
            PlayerListPlayerModel[] playerListPlayerModels = this.pavlovRconService.GetActivePlayers(this.ServerId);
            this.playerListPlayers = playerListPlayerModels.ToDictionary(k => ulong.Parse(k.UniqueId), v => v);
        }
        catch (Exception ex)
        {
            this.OnServerErrorRaised?.Invoke(this.ServerId, ex.Message);
            return;
        }

        this.OnPlayerListUpdated?.Invoke(this.ServerId);

        using (PavlovServerContext context = new(this.configuration))
        {
            foreach (PlayerListPlayerModel player in this.playerListPlayers.Values)
            {
                PlayerListPlayerModel? dbPlayer = context.Players.FirstOrDefault(p => p.UniqueId == player.UniqueId && p.ServerId == this.ServerId);
                if (dbPlayer == null)
                {
                    player.ServerId = this.ServerId;
                    context.Players.Add(player);
                }
                else if (dbPlayer.Username != player.Username)
                {
                    dbPlayer.Username = player.Username;
                }
            }

            context.SaveChanges();
        }
    }

    public event PlayerDetailUpdate? OnPlayerDetailUpdated;

    private void updatePlayerDetails()
    {
        if (!this.Online.HasValue || !this.Online.Value || this.PlayerListPlayers == null)
        {
            return;
        }

        List<PlayerDetailModel> newPlayerDetails = new();
        foreach (ulong playerId in this.PlayerListPlayers.Keys)
        {
            try
            {
                PlayerDetailModel? playerDetail = this.pavlovRconService.GetActivePlayerDetail(this.ServerId, playerId.ToString());

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

        this.playerDetails = newPlayerDetails.ToDictionary(k => ulong.Parse(k.UniqueId), v => v);
    }
}
