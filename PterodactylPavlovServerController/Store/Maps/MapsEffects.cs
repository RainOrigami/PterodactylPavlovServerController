using Fluxor;
using PterodactylPavlovServerController.Services;
using PterodactylPavlovServerDomain.Models;

namespace PterodactylPavlovServerController.Store.Maps
{
    public class MapsEffects
    {
        private readonly SteamWorkshopService steamWorkshopService;
        private readonly PavlovServerService pavlovServerService;
        private readonly IState<MapsState> mapsState;

        public MapsEffects(SteamWorkshopService steamWorkshopService, PavlovServerService pavlovServerService, IState<MapsState> mapsState)
        {
            this.steamWorkshopService = steamWorkshopService;
            this.pavlovServerService = pavlovServerService;
            this.mapsState = mapsState;
        }

        [EffectMethod]
        public async Task LoadMap(MapsLoadAction mapsLoadAction, IDispatcher dispatcher)
        {
            if (mapsState.Value.MapDetails.Any(m => m.Id == mapsLoadAction.MapId))
            {
                return;
            }

            dispatcher.Dispatch(new MapsAddAction(steamWorkshopService.GetMapDetail(mapsLoadAction.MapId)));

            await Task.CompletedTask;
        }

        [EffectMethod]
        public async Task LoadServerMaps(MapsLoadServerAction mapsLoadServerAction, IDispatcher dispatcher)
        {
            MapRowModel[] mapRows = pavlovServerService.GetCurrentMapRotation(mapsLoadServerAction.ServerId);
            dispatcher.Dispatch(new MapsAddServerAction(mapsLoadServerAction.ServerId, mapRows));

            await Task.CompletedTask;
        }
    }
}
