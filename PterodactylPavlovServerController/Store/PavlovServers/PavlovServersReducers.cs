using Fluxor;

namespace PterodactylPavlovServerController.Store.PavlovServers
{
    public class PavlovServersReducers
    {
        //[ReducerMethod]
        //public static PavlovServersState OnServerSet(PavlovServersState pavlovServersState, PavlovServersAddAction pavlovServersAddAction)
        //{
        //    Dictionary<string, ServerInfoModel> servers = (Dictionary<string, ServerInfoModel>)pavlovServersState.Servers;
        //    if (servers.ContainsKey(pavlovServersAddAction.ServerInfoModel.ServerId))
        //    {
        //        servers.Remove(pavlovServersAddAction.ServerInfoModel.ServerId);
        //    }
        //    servers.Add(pavlovServersAddAction.ServerInfoModel.ServerId, pavlovServersAddAction.ServerInfoModel);

        //    return pavlovServersState with
        //    {
        //        Servers = servers
        //    };
        //}

        [ReducerMethod]
        public static PavlovServersState OnServerSetNameFromGameIni(PavlovServersState pavlovServersState, PavlovServerAddNameFromGameIniAction pavlovServerAddNameFromGameIniAction)
        {
            Dictionary<string, string> serverNames = (Dictionary<string, string>)pavlovServersState.ServerNamesFromGameIni;

            if (serverNames.ContainsKey(pavlovServerAddNameFromGameIniAction.ServerId))
            {
                serverNames.Remove(pavlovServerAddNameFromGameIniAction.ServerId);
            }

            serverNames.Add(pavlovServerAddNameFromGameIniAction.ServerId, pavlovServerAddNameFromGameIniAction.ServerName);

            return pavlovServersState with
            {
                ServerNamesFromGameIni = serverNames
            };
        }

        //[ReducerMethod]
        //public static PavlovServersState OnServerSetOnlineState(PavlovServersState pavlovServersState, PavlovServersSetOnlineStateAction pavlovServersSetOnlineStateAction)
        //{
        //    Dictionary<string, bool> serverStates = (Dictionary<string, bool>)pavlovServersState.Online;
        //    if (serverStates.ContainsKey(pavlovServersSetOnlineStateAction.ServerId))
        //    {
        //        serverStates.Remove(pavlovServersSetOnlineStateAction.ServerId);
        //    }
        //    serverStates.Add(pavlovServersSetOnlineStateAction.ServerId, pavlovServersSetOnlineStateAction.Online);

        //    return pavlovServersState with
        //    {
        //        Online = serverStates
        //    };
        //}
    }
}
