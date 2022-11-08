using Fluxor;
using PterodactylPavlovServerController.Store.PavlovServers;
using PterodactylPavlovServerController.Store.Players;
using PterodactylPavlovServerDomain.Models;

namespace PterodactylPavlovServerController.Services
{
    public class RconToFluxorForwarderService : IDisposable
    {
        private readonly PavlovRconConnectionService pavlovRconConnectionService;
        private readonly IDispatcher dispatcher;

        public RconToFluxorForwarderService(PavlovRconConnectionService pavlovRconConnectionService, IDispatcher dispatcher)
        {
            this.pavlovRconConnectionService = pavlovRconConnectionService;
            this.dispatcher = dispatcher;

            //this.pavlovRconConnectionService.OnServersUpdated += PavlovRconConnectionServiceOnOnServersUpdated;
            //PavlovRconConnectionServiceOnOnServersUpdated(pavlovRconConnectionService.GetAllConnections().Select(c => c.PterodactylServer).ToArray());
        }

        private void PavlovRconConnectionServiceOnOnServersUpdated(PterodactylServerModel[] servers)
        {
            //dispatcher.Dispatch(new PterodactylServersSetAction(servers));

            //foreach (PterodactylServerModel server in servers)
            //{
            //    PavlovRconConnection connection = pavlovRconConnectionService.GetServer(server.ServerId);
            //    connection.OnServerErrorRaised -= ConnectionOnOnServerErrorRaised;
            //    connection.OnServerOnlineStateChanged -= ConnectionOnOnServerOnlineStateChanged;
            //    connection.OnPlayerDetailUpdated -= ConnectionOnOnPlayerDetailUpdated;
            //    connection.OnPlayerListUpdated -= ConnectionOnOnPlayerListUpdated;
            //    connection.OnServerInfoUpdated -= ConnectionOnOnServerInfoUpdated;

            //    connection.OnServerErrorRaised += ConnectionOnOnServerErrorRaised;
            //    connection.OnServerOnlineStateChanged += ConnectionOnOnServerOnlineStateChanged;
            //    connection.OnPlayerDetailUpdated += ConnectionOnOnPlayerDetailUpdated;
            //    connection.OnPlayerListUpdated += ConnectionOnOnPlayerListUpdated;
            //    connection.OnServerInfoUpdated += ConnectionOnOnServerInfoUpdated;
            //}
        }

        private void ConnectionOnOnServerInfoUpdated(string serverId)
        {
            //ServerInfoModel? serverInfo = pavlovRconConnectionService.GetServer(serverId).ServerInfo;
            //if (serverInfo == null)
            //{
            //    return;
            //}

            //dispatcher.Dispatch(new PavlovServersSetOnlineStateAction(serverId, true));
            //dispatcher.Dispatch(new PavlovServersAddAction(serverInfo));
        }

        private void ConnectionOnOnPlayerListUpdated(string serverId)
        {
            var list = pavlovRconConnectionService.GetServer(serverId).PlayerListPlayers;
            dispatcher.Dispatch(new PlayersAddListAction(serverId, list));
        }

        private void ConnectionOnOnPlayerDetailUpdated(string serverId, PlayerDetailModel playerDetail)
        {
            dispatcher.Dispatch(new PlayersAddDetailAction(serverId, playerDetail));
        }

        private void ConnectionOnOnServerOnlineStateChanged(string serverId)
        {
            //bool online = pavlovRconConnectionService.GetServer(serverId).Online;
            //dispatcher.Dispatch(new PavlovServersSetOnlineStateAction(serverId, online));
        }

        private void ConnectionOnOnServerErrorRaised(string serverId, string error)
        {
            dispatcher.Dispatch(new PavlovServersSetErrorAction(serverId, error));
        }

        public void Dispose()
        {
            //foreach (PterodactylServerModel server in pavlovRconConnectionService.GetAllConnections().Select(c => c.PterodactylServer))
            //{
            //    PavlovRconConnection connection = pavlovRconConnectionService.GetServer(server.ServerId);
            //    connection.OnServerErrorRaised -= ConnectionOnOnServerErrorRaised;
            //    connection.OnServerOnlineStateChanged -= ConnectionOnOnServerOnlineStateChanged;
            //    connection.OnPlayerDetailUpdated -= ConnectionOnOnPlayerDetailUpdated;
            //    connection.OnPlayerListUpdated -= ConnectionOnOnPlayerListUpdated;
            //    connection.OnServerInfoUpdated -= ConnectionOnOnServerInfoUpdated;
            //}
        }
    }
}
