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

        //[EffectMethod]
        //public async Task LoadServer(PavlovServersLoadAction pavlovServersLoadAction, IDispatcher dispatcher)
        //{
        //    if (!pavlovServersState.Value.ServerNamesFromGameIni.ContainsKey(pavlovServersLoadAction.ServerId))
        //    {
        //        dispatcher.Dispatch(new PavlovServerLoadNameFromGameIniAction(pavlovServersLoadAction.ServerId));
        //    }

        //    try
        //    {
        //        PavlovRconConnection connection = pavlovRconConnectionService.GetServer(pavlovServersLoadAction.ServerId);
        //        ServerInfoModel? serverInfo = connection.ServerInfo;
        //        if (serverInfo != null)
        //        {
        //            dispatcher.Dispatch(new PavlovServersAddAction(serverInfo));
        //        }

        //        dispatcher.Dispatch(new PavlovServersSetOnlineStateAction(pavlovServersLoadAction.ServerId, connection.Online));
        //    }
        //    catch (Exception ex)
        //    {
        //        dispatcher.Dispatch(new PavlovServersSetOnlineStateAction(pavlovServersLoadAction.ServerId, false));
        //        dispatcher.Dispatch(new PavlovServersSetErrorAction(pavlovServersLoadAction.ServerId, ex.Message));
        //    }

        //    await Task.CompletedTask;
        //}

        [EffectMethod]
        public async Task LoadServerNameFromGameIni(PavlovServerLoadNameFromGameIniAction pavlovServerLoadNameFromGameIniAction, IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new PavlovServerAddNameFromGameIniAction(pavlovServerLoadNameFromGameIniAction.ServerId, pavlovServerService.GetServerName(pavlovServerLoadNameFromGameIniAction.ServerId)));

            await Task.CompletedTask;
        }
    }
}
