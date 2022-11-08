using PterodactylPavlovServerController.Contexts;
using PterodactylPavlovServerController.Services;
using PterodactylPavlovServerDomain.Models;

// TODO: not really a model, move somewhere else
namespace PterodactylPavlovServerController.Models
{
    public class PavlovRconConnection : IDisposable
    {
        private readonly PavlovRconService pavlovRconService;
        private readonly IConfiguration configuration;

        public string ServerId => PterodactylServer.ServerId;
        public bool? Online { get; set; } = null;

        public PterodactylServerModel PterodactylServer { get; }
        public ServerInfoModel? ServerInfo { get; private set; }
        public IReadOnlyDictionary<ulong, PlayerListPlayerModel>? PlayerListPlayers => playerListPlayers;
        private Dictionary<ulong, PlayerListPlayerModel>? playerListPlayers = null;
        public IReadOnlyDictionary<ulong, PlayerDetailModel>? PlayerDetails => playerDetails;
        private Dictionary<ulong, PlayerDetailModel>? playerDetails = null;

        private readonly CancellationTokenSource updateCancellationTokenSource = new();

        public PavlovRconConnection(PterodactylServerModel pterodactylServer, PavlovRconService pavlovRconService, IConfiguration configuration)
        {
            this.PterodactylServer = pterodactylServer;
            this.pavlovRconService = pavlovRconService;
            this.configuration = configuration;
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
                    updateServerInfo();
                    updatePlayerList();
                    updatePlayerDetails();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                await Task.Delay(1000);
            }
        }

        public delegate void ServerUpdated(string serverId);
        public event ServerUpdated? OnServerInfoUpdated;
        public event ServerUpdated? OnServerOnlineStateChanged;

        public delegate void ServerError(string serverId, string error);
        public event ServerError? OnServerErrorRaised;

        private void updateServerInfo()
        {
            try
            {
                ServerInfo = pavlovRconService.GetServerInfo(ServerId);
            }
            catch (Exception ex)
            {
                Online = false;
                OnServerOnlineStateChanged?.Invoke(ServerId);
                OnServerErrorRaised?.Invoke(ServerId, ex.Message);
                //dispatcher.Dispatch(new PavlovServersSetOnlineStateAction(ServerId, false));
                //dispatcher.Dispatch(new PavlovServersSetErrorAction(ServerId, ex.Message));

                return;
            }

            if (!Online.HasValue || !Online.Value)
            {
                Online = true;
                OnServerOnlineStateChanged?.Invoke(ServerId);
                //dispatcher.Dispatch(new PavlovServersSetOnlineStateAction(ServerId, true));
            }

            OnServerInfoUpdated?.Invoke(ServerId);
            //dispatcher.Dispatch(new PavlovServersAddAction(ServerInfo));
        }

        public event ServerUpdated? OnPlayerListUpdated;

        private void updatePlayerList()
        {
            if (!Online.HasValue || !Online.Value)
            {
                return;
            }

            try
            {
                PlayerListPlayerModel[] playerListPlayerModels = pavlovRconService.GetActivePlayers(ServerId);
                playerListPlayers = playerListPlayerModels.ToDictionary(k => ulong.Parse(k.UniqueId), v => v);

                //dispatcher.Dispatch(new PlayersAddListAction(ServerId, PlayerListPlayers));
            }
            catch (Exception ex)
            {
                OnServerErrorRaised?.Invoke(ServerId, ex.Message);
                //dispatcher.Dispatch(new PavlovServersSetErrorAction(ServerId, ex.Message));
                return;
            }

            OnPlayerListUpdated?.Invoke(ServerId);

            using (PavlovServerContext context = new(configuration))
            {
                foreach (PlayerListPlayerModel player in playerListPlayers.Values)
                {
                    PlayerListPlayerModel? dbPlayer = context.Players.FirstOrDefault(p => p.UniqueId == player.UniqueId && p.ServerId == ServerId);
                    if (dbPlayer == null)
                    {
                        player.ServerId = ServerId;
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

        public delegate void PlayerDetailUpdate(string serverId, PlayerDetailModel playerDetail);
        public event PlayerDetailUpdate? OnPlayerDetailUpdated;

        private void updatePlayerDetails()
        {
            if (!Online.HasValue || !Online.Value || PlayerListPlayers == null)
            {
                return;
            }

            List<PlayerDetailModel> newPlayerDetails = new();
            foreach (ulong playerId in PlayerListPlayers.Keys)
            {
                try
                {
                    PlayerDetailModel? playerDetail = pavlovRconService.GetActivePlayerDetail(ServerId, playerId.ToString());

                    if (playerDetail == null)
                    {
                        throw new Exception("Failed to fetch player details");
                    }

                    newPlayerDetails.Add(playerDetail);
                    OnPlayerDetailUpdated?.Invoke(ServerId, playerDetail);
                    //dispatcher.Dispatch(new PlayersAddDetailAction(ServerId, playerDetail));
                }
                catch (Exception ex)
                {
                    OnServerErrorRaised?.Invoke(ServerId, ex.Message);
                    //dispatcher.Dispatch(new PavlovServersSetErrorAction(ServerId, ex.Message));
                }
            }

            playerDetails = newPlayerDetails.ToDictionary(k => ulong.Parse(k.UniqueId), v => v);
        }

        public void Dispose()
        {
            updateCancellationTokenSource.Cancel();
        }
    }
}
