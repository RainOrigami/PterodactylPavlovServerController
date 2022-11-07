using Fluxor;
using PterodactylPavlovServerController.Services;

namespace PterodactylPavlovServerController.Store.Servers
{
    public class PterodactylServersEffects
    {
        private readonly PterodactylService pterodactylService;

        public PterodactylServersEffects(PterodactylService pterodactylService)
        {
            this.pterodactylService = pterodactylService;
        }

        [EffectMethod(typeof(PterodactylServersLoadAction))]
        public async Task LoadServers(IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new PterodactylServersSetAction(pterodactylService.GetServers()));

            await Task.CompletedTask;
        }
    }
}
