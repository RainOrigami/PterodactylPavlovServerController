using Fluxor;
using PterodactylPavlovServerDomain.Models;

namespace PterodactylPavlovServerController.Store.Maps
{
    [FeatureState]
    public record MapsState
    {
        public IReadOnlyDictionary<long, MapDetailModel> MapDetails { get; init; } = new Dictionary<long, MapDetailModel>();
        public IReadOnlyDictionary<string, MapRowModel[]> ServerMaps { get; init; } = new Dictionary<string, MapRowModel[]>();
    }
}
