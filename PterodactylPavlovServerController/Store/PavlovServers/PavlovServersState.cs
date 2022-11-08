using Fluxor;

namespace PterodactylPavlovServerController.Store.PavlovServers
{
    [FeatureState]
    public record PavlovServersState
    {
        public IReadOnlyDictionary<string, string> ServerNamesFromGameIni { get; init; } = new Dictionary<string, string>();
    }
}
