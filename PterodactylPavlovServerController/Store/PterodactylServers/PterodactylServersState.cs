using Fluxor;
using PterodactylPavlovServerDomain.Models;

namespace PterodactylPavlovServerController.Store.Servers
{
    [FeatureState]
    public record PterodactylServersState
    {
        public PterodactylServerModel[]? Servers { get; init; }
    }
}
