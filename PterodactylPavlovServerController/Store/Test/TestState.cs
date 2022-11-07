using Fluxor;
using PterodactylPavlovServerDomain.Models;

namespace PterodactylPavlovServerController.Store.Test
{
    [FeatureState]
    public record TestState
    {
        public PterodactylServerModel[]? Value { get; init; }
    }
}
