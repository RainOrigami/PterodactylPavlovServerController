using Fluxor;
using Steam.Models.SteamCommunity;

namespace PterodactylPavlovServerController.Store.Players
{
    [FeatureState]
    public record PlayersState
    {
        public IReadOnlyDictionary<ulong, PlayerSummaryModel> PlayerSummaries { get; init; } = new Dictionary<ulong, PlayerSummaryModel>();
        public IReadOnlyDictionary<ulong, IReadOnlyCollection<PlayerBansModel>> PlayerBans { get; init; } = new Dictionary<ulong, IReadOnlyCollection<PlayerBansModel>>();
    }
}
