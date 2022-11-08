namespace PterodactylPavlovServerController.Store.PavlovServers
{
    //public class PavlovServersLoadAction
    //{
    //    public PavlovServersLoadAction(string serverId)
    //    {
    //        this.ServerId = serverId;
    //    }

    //    public string ServerId { get; }
    //}

    //public class PavlovServersAddAction
    //{
    //    public PavlovServersAddAction(ServerInfoModel serverInfoModel)
    //    {
    //        this.ServerInfoModel = serverInfoModel;
    //    }

    //    public ServerInfoModel ServerInfoModel { get; }
    //}

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

    //public class PavlovServersSetOnlineStateAction
    //{
    //    public PavlovServersSetOnlineStateAction(string serverId, bool online)
    //    {
    //        this.ServerId = serverId;
    //        this.Online = online;
    //    }

    //    public string ServerId { get; }
    //    public bool Online { get; }
    //}

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
