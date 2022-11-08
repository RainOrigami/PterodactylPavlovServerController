using Fluxor;
using PterodactylPavlovServerController.Services;

namespace PterodactylPavlovServerController.Store.PavlovServers
{
    public class PavlovServersEffects
    {
        private readonly PavlovServerService pavlovServerService;
        private readonly PavlovRconConnectionService pavlovRconConnectionService;
        private readonly IState<PavlovServersState> pavlovServersState;

        public PavlovServersEffects(PavlovServerService pavlovServerService, PavlovRconConnectionService pavlovRconConnectionService, IState<PavlovServersState> pavlovServersState)
        {
            this.pavlovServerService = pavlovServerService;
            this.pavlovRconConnectionService = pavlovRconConnectionService;
            this.pavlovServersState = pavlovServersState;
        }

        [EffectMethod]
        public async Task LoadServerNameFromGameIni(PavlovServerLoadNameFromGameIniAction pavlovServerLoadNameFromGameIniAction, IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new PavlovServerAddNameFromGameIniAction(pavlovServerLoadNameFromGameIniAction.ServerId, pavlovServerService.GetServerName(pavlovServerLoadNameFromGameIniAction.ServerId)));

            await Task.CompletedTask;
        }
    }
}
