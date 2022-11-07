using Fluxor;

namespace PterodactylPavlovServerController.Store.Servers
{
    public class PterodactylServersReducers
    {
        [ReducerMethod]
        public static PterodactylServersState OnServersSet(PterodactylServersState pterodactylServersState, PterodactylServersSetAction pterodactylServersSetAction)
        {
            return new PterodactylServersState()
            {
                Servers = pterodactylServersSetAction.Servers
            };
        }
    }
}
