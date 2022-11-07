using Fluxor;
using PterodactylPavlovServerDomain.Models;

namespace PterodactylPavlovServerController.Store.PavlovServers
{
    public class PavlovServersReducers
    {
        [ReducerMethod]
        public static PavlovServersState OnServerSet(PavlovServersState pavlovServersState, PavlovServersAddAction pavlovServersAddAction)
        {
            List<ServerInfoModel> servers = pavlovServersState.Servers.Where(s => s.ServerId != pavlovServersAddAction.ServerInfoModel.ServerId).ToList();
            servers.Add(pavlovServersAddAction.ServerInfoModel);

            return pavlovServersState with
            {
                Servers = servers.ToArray()
            };
        }

        [ReducerMethod]
        public static PavlovServersState OnServerSetNameFromGameIni(PavlovServersState pavlovServersState, PavlovServerNameFromGameIniAction pavlovServerNameFromGameIniAction)
        {
            Dictionary<string, string> serverNames = (Dictionary<string, string>)pavlovServersState.ServerNamesFromGameIni;

            if (serverNames.ContainsKey(pavlovServerNameFromGameIniAction.ServerId))
            {
                serverNames.Remove(pavlovServerNameFromGameIniAction.ServerId);
            }

            serverNames.Add(pavlovServerNameFromGameIniAction.ServerId, pavlovServerNameFromGameIniAction.ServerName);

            return pavlovServersState with
            {
                ServerNamesFromGameIni = serverNames
            };
        }
    }
}
