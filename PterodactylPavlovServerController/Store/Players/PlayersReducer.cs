using Fluxor;
using Steam.Models.SteamCommunity;

namespace PterodactylPavlovServerController.Store.Players
{
    public class PlayersReducer
    {
        [ReducerMethod]
        public static PlayersState OnPlayersSummaryAdd(PlayersState playersState, PlayersAddSummaryAction playersAddSummaryAction)
        {
            Dictionary<ulong, PlayerSummaryModel> playerSummaries = (Dictionary<ulong, PlayerSummaryModel>)playersState.PlayerSummaries;
            if (playerSummaries.ContainsKey(playersAddSummaryAction.PlayerSummary.SteamId))
            {
                playerSummaries.Remove(playersAddSummaryAction.PlayerSummary.SteamId);
            }
            playerSummaries.Add(playersAddSummaryAction.PlayerSummary.SteamId, playersAddSummaryAction.PlayerSummary);

            return playersState with
            {
                PlayerSummaries = playerSummaries
            };
        }

        [ReducerMethod]
        public static PlayersState OnPlayersBansAdd(PlayersState playersState, PlayersAddBansAction playersAddBansAction)
        {
            Dictionary<ulong, IReadOnlyCollection<PlayerBansModel>> playerBans = (Dictionary<ulong, IReadOnlyCollection<PlayerBansModel>>)playersState.PlayerBans;
            if (playerBans.ContainsKey(playersAddBansAction.PlayerId))
            {
                playerBans.Remove(playersAddBansAction.PlayerId);
            }
            playerBans.Add(playersAddBansAction.PlayerId, playersAddBansAction.PlayerBans);

            return playersState with
            {
                PlayerBans = playerBans
            };
        }
    }
}
