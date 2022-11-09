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

        [EffectMethod]
        public async Task PlayersLoadSummary(PlayersLoadSummaryAction playersLoadSummaryAction, IDispatcher dispatcher)
        {
            try
            {
                dispatcher.Dispatch(new PlayersAddSummaryAction(steamService.GetPlayerSummary(playersLoadSummaryAction.PlayerId)));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            await Task.CompletedTask;
        }

        [EffectMethod]
        public async Task PlayersLoadBans(PlayersLoadBansAction playersLoadBansAction, IDispatcher dispatcher)
        {
            try
            {
                dispatcher.Dispatch(new PlayersAddBansAction(playersLoadBansAction.PlayerId, steamService.GetBans(playersLoadBansAction.PlayerId)));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            await Task.CompletedTask;
        }
    }
}
