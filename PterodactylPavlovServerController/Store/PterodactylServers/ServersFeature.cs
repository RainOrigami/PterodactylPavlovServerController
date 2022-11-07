using Fluxor;
using PterodactylPavlovServerDomain.Models;

namespace PterodactylPavlovServerController.Store.Servers
{
    public class ServersFeature : Feature<ServersState>
    {
        public override string GetName() => "Servers";
        protected override ServersState GetInitialState() => new ServersState(Array.Empty<PterodactylServerModel>(), Array.Empty<ServerInfoModel>(), false);
    }
}
