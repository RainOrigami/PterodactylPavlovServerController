namespace PterodactylPavlovServerController.Store.Servers
{
    //public class PterodactylServersEffects
    //{
    //    private readonly PavlovRconConnectionService pavlovRconConnectionService;

    //    public PterodactylServersEffects(PavlovRconConnectionService pavlovRconConnectionService)
    //    {
    //        this.pavlovRconConnectionService = pavlovRconConnectionService;
    //    }

    //    [EffectMethod(typeof(PterodactylServersLoadAction))]
    //    public async Task LoadServers(IDispatcher dispatcher)
    //    {
    //        try
    //        {
    //            dispatcher.Dispatch(new PterodactylServersSetAction(pavlovRconConnectionService.GetAllConnections().Select(c => c.PterodactylServer).ToArray()));
    //        }
    //        catch (Exception e)
    //        {
    //            Console.WriteLine(e.Message);
    //        }
    //        await Task.CompletedTask;
    //    }
    //}
}
