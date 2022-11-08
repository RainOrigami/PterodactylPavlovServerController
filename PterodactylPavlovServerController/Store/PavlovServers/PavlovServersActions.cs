namespace PterodactylPavlovServerController.Store.PavlovServers
{
    public class PavlovServerLoadNameFromGameIniAction
    {
        public PavlovServerLoadNameFromGameIniAction(string serverId)
        {
            this.ServerId = serverId;
        }

        public string ServerId { get; }
    }

    public class PavlovServerAddNameFromGameIniAction
    {
        public PavlovServerAddNameFromGameIniAction(string serverId, string serverName)
        {
            this.ServerId = serverId;
            this.ServerName = serverName;
        }

        public string ServerId { get; }
        public string ServerName { get; }
    }

    public class PavlovServersSetErrorAction
    {
        public PavlovServersSetErrorAction(string serverId, string? error)
        {
            this.ServerId = serverId;
            this.Error = error;
        }

        public string ServerId { get; }
        public string? Error { get; }
    }
}
