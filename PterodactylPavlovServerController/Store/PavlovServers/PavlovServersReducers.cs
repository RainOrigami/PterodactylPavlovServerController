using Fluxor;

namespace PterodactylPavlovServerController.Store.PavlovServers
{
    public class PavlovServersReducers
    {
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
    }
}
