using Steam.Models.SteamCommunity;

namespace PterodactylPavlovServerController.Store.Players
{
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
