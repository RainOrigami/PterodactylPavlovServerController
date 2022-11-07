using Fluxor;
using PterodactylPavlovServerDomain.Models;

namespace PterodactylPavlovServerController.Store.PavlovServers
{
    [FeatureState]
    public record PavlovServersState
    {
        public ServerInfoModel[] Servers { get; init; } = Array.Empty<ServerInfoModel>();
        public IReadOnlyDictionary<string, string> ServerNamesFromGameIni { get; init; } = new Dictionary<string, string>();
    }
}
