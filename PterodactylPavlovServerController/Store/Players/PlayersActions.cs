using Steam.Models.SteamCommunity;

namespace PterodactylPavlovServerController.Store.Players
{
    //public class PlayersLoadListAction
    //{
    //    public PlayersLoadListAction(string serverId)
    //    {
    //        this.ServerId = serverId;
    //    }

    //    public string ServerId { get; }
    //}

    //public class PlayersAddListAction
    //{
    //    public PlayersAddListAction(string serverId, IReadOnlyDictionary<ulong, PlayerListPlayerModel> playerList)
    //    {
    //        this.ServerId = serverId;
    //        this.PlayerList = playerList;
    //    }

    //    public string ServerId { get; }
    //    public IReadOnlyDictionary<ulong, PlayerListPlayerModel> PlayerList { get; }
    //}

    //public class PlayersLoadDetailAction
    //{
    //    public PlayersLoadDetailAction(string serverId, ulong playerId)
    //    {
    //        this.ServerId = serverId;
    //        this.PlayerId = playerId;
    //    }

    //    public string ServerId { get; }
    //    public ulong PlayerId { get; }
    //}

    //public class PlayersAddDetailAction
    //{
    //    public PlayersAddDetailAction(string serverId, PlayerDetailModel playerDetailModel)
    //    {
    //        this.ServerId = serverId;
    //        this.PlayerDetailModel = playerDetailModel;
    //    }

    //    public string ServerId { get; }
    //    public PlayerDetailModel PlayerDetailModel { get; }
    //}

    public class PlayersLoadSummaryAction
    {
        public PlayersLoadSummaryAction(ulong playerId)
        {
            this.PlayerId = playerId;
        }

        public ulong PlayerId { get; }
    }

    public class PlayersAddSummaryAction
    {
        public PlayersAddSummaryAction(PlayerSummaryModel playerSummary)
        {
            this.PlayerSummary = playerSummary;
        }

        public PlayerSummaryModel PlayerSummary { get; }
    }

    public class PlayersLoadBansAction
    {
        public PlayersLoadBansAction(ulong playerId)
        {
            this.PlayerId = playerId;
        }

        public ulong PlayerId { get; }
    }

    public class PlayersAddBansAction
    {
        public PlayersAddBansAction(ulong playerId, IReadOnlyCollection<PlayerBansModel> playerBans)
        {
            this.PlayerId = playerId;
            this.PlayerBans = playerBans;
        }

        public ulong PlayerId { get; }
        public IReadOnlyCollection<PlayerBansModel> PlayerBans { get; }
    }
}
