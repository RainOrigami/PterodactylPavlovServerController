using Fluxor;
using PterodactylPavlovServerController.Services;

namespace PterodactylPavlovServerController.Store.Players
{
    public class PlayersEffects
    {
        private readonly SteamService steamService;

        public PlayersEffects(SteamService steamService)
        {
            this.steamService = steamService;
        }

        //[EffectMethod]
        //public async Task PlayersLoadList(PlayersLoadListAction playersLoadListAction, IDispatcher dispatcher)
        //{
        //    try
        //    {
        //        PavlovRconConnection connection = pavlovRconConnectionService.GetServer(playersLoadListAction.ServerId);
        //        dispatcher.Dispatch(new PlayersAddListAction(playersLoadListAction.ServerId, connection.PlayerListPlayers));
        //    }
        //    catch (Exception ex)
        //    {
        //        dispatcher.Dispatch(new PavlovServersSetErrorAction(playersLoadListAction.ServerId, ex.Message));
        //    }

        //    await Task.CompletedTask;
        //}

        //[EffectMethod]
        //public async Task PlayersLoadDetail(PlayersLoadDetailAction playersLoadDetailAction, IDispatcher dispatcher)
        //{
        //    try
        //    {
        //        PavlovRconConnection connection = pavlovRconConnectionService.GetServer(playersLoadDetailAction.ServerId);
        //        if (!connection.PlayerDetails.TryGetValue(playersLoadDetailAction.PlayerId, out PlayerDetailModel? playerDetailModel))
        //        {
        //            return;
        //        }

        //        dispatcher.Dispatch(new PlayersAddDetailAction(playersLoadDetailAction.ServerId, playerDetailModel));
        //    }
        //    catch (Exception ex)
        //    {
        //        dispatcher.Dispatch(new PavlovServersSetErrorAction(playersLoadDetailAction.ServerId, ex.Message));
        //    }

        //    await Task.CompletedTask;
        //}

        [EffectMethod]
        public async Task PlayersLoadSummary(PlayersLoadSummaryAction playersLoadSummaryAction, IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new PlayersAddSummaryAction(steamService.GetPlayerSummary(playersLoadSummaryAction.PlayerId)));

            await Task.CompletedTask;
        }

        [EffectMethod]
        public async Task PlayersLoadBans(PlayersLoadBansAction playersLoadBansAction, IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new PlayersAddBansAction(playersLoadBansAction.PlayerId, steamService.GetBans(playersLoadBansAction.PlayerId)));

            await Task.CompletedTask;
        }
    }
}
