using Fluxor;
using PterodactylPavlovServerController.Services;

namespace PterodactylPavlovServerController.Store.PavlovServers
{
    public class PavlovServersEffects
    {
        private readonly PavlovRconService pavlovRconService;
        private readonly PavlovServerService pavlovServerService;

        public PavlovServersEffects(PavlovRconService pavlovRconService, PavlovServerService pavlovServerService)
        {
            this.pavlovRconService = pavlovRconService;
            this.pavlovServerService = pavlovServerService;
        }

        [EffectMethod]
        public async Task LoadServer(PavlovServersLoadAction pavlovServersLoadAction, IDispatcher dispatcher)
        {
            try
            {
                dispatcher.Dispatch(new PavlovServersAddAction(pavlovRconService.GetServerInfo(pavlovServersLoadAction.ServerId)));
            }
            catch (Exception) { }

            dispatcher.Dispatch(new PavlovServerNameFromGameIniAction(pavlovServersLoadAction.ServerId, pavlovServerService.GetServerName(pavlovServersLoadAction.ServerId)));

            await Task.CompletedTask;
        }
    }
}
