using Fluxor;

namespace PterodactylPavlovServerController.Store.PavlovServers
{
    [FeatureState]
    public record PavlovServersState
    {
        //public IReadOnlyDictionary<string, ServerInfoModel> Servers { get; init; } = new Dictionary<string, ServerInfoModel>();
        public IReadOnlyDictionary<string, string> ServerNamesFromGameIni { get; init; } = new Dictionary<string, string>();
        //public IReadOnlyDictionary<string, bool> Online { get; init; } = new Dictionary<string, bool>();
    }
}
