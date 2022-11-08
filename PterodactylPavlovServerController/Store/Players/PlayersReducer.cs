using Fluxor;
using Steam.Models.SteamCommunity;

namespace PterodactylPavlovServerController.Store.Players
{
    public class PlayersReducer
    {
        //[ReducerMethod]
        //public static PlayersState OnPlayersListAdd(PlayersState playersState, PlayersAddListAction playersAddListAction)
        //{
        //    Dictionary<string, IReadOnlyDictionary<ulong, PlayerListPlayerModel>> playerList = (Dictionary<string, IReadOnlyDictionary<ulong, PlayerListPlayerModel>>)playersState.PlayerList;
        //    if (playerList.ContainsKey(playersAddListAction.ServerId))
        //    {
        //        playerList.Remove(playersAddListAction.ServerId);
        //    }
        //    playerList.Add(playersAddListAction.ServerId, playersAddListAction.PlayerList);

        //    return playersState with
        //    {
        //        PlayerList = playerList
        //    };
        //}

        //[ReducerMethod]
        //public static PlayersState OnPlayersDetailAdd(PlayersState playersState, PlayersAddDetailAction playersAddDetailAction)
        //{
        //    ulong playerId = ulong.Parse(playersAddDetailAction.PlayerDetailModel.UniqueId);

        //    Dictionary<string, IReadOnlyDictionary<ulong, PlayerDetailModel>> playerDetailList = (Dictionary<string, IReadOnlyDictionary<ulong, PlayerDetailModel>>)playersState.PlayerDetailList;
        //    if (!playerDetailList.ContainsKey(playersAddDetailAction.ServerId))
        //    {
        //        playerDetailList.Add(playersAddDetailAction.ServerId, new Dictionary<ulong, PlayerDetailModel>());
        //    }

        //    Dictionary<ulong, PlayerDetailModel> playerDetails = (Dictionary<ulong, PlayerDetailModel>)playerDetailList[playersAddDetailAction.ServerId];
        //    if (playerDetails.ContainsKey(playerId))
        //    {
        //        playerDetails.Remove(playerId);
        //    }
        //    playerDetails.Add(playerId, playersAddDetailAction.PlayerDetailModel);

        //    return playersState with
        //    {
        //        PlayerDetailList = playerDetailList
        //    };
        //}

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
