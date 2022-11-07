using PterodactylPavlovServerDomain.Models;

namespace PterodactylPavlovServerController.Store.PavlovServers
{
    public class PavlovServersLoadAction
    {
        public PavlovServersLoadAction(string serverId)
        {
            this.ServerId = serverId;
        }

        public string ServerId { get; }
    }

    public class PavlovServersAddAction
    {
        public PavlovServersAddAction(ServerInfoModel serverInfoModel)
        {
            this.ServerInfoModel = serverInfoModel;
        }

        public ServerInfoModel ServerInfoModel { get; }
    }

    public class PavlovServerNameFromGameIniAction
    {
        public PavlovServerNameFromGameIniAction(string serverId, string serverName)
        {
            this.ServerId = serverId;
            this.ServerName = serverName;
        }

        public string ServerId { get; }
        public string ServerName { get; }
    }
}
