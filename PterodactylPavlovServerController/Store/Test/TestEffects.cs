using Fluxor;
using PterodactylPavlovServerController.Services;

namespace PterodactylPavlovServerController.Store.Test
{
    public class TestEffects
    {
        private readonly PterodactylService pterodactylService;

        public TestEffects(PterodactylService pterodactylService)
        {
            this.pterodactylService = pterodactylService;
        }

        [EffectMethod]
        public async Task LoadTests(TestLoadAction testLoadAction, IDispatcher dispatcher)
        {
            await Task.Delay(1000);

            dispatcher.Dispatch(new TestSetValueAction(pterodactylService.GetServers()));
        }
    }
}
