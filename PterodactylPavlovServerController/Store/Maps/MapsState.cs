using Fluxor;
using PterodactylPavlovServerDomain.Models;

namespace PterodactylPavlovServerController.Store.Maps
{
    [FeatureState]
    public record MapsState
    {
        public MapDetailModel[] MapDetails { get; init; } = Array.Empty<MapDetailModel>();
        public IReadOnlyDictionary<string, MapRowModel[]> ServerMaps { get; init; } = new Dictionary<string, MapRowModel[]>();
    }
}
