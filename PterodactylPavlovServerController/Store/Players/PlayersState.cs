using Fluxor;
using Steam.Models.SteamCommunity;

namespace PterodactylPavlovServerController.Store.Players
{
    [FeatureState]
    public record PlayersState
    {
        //public IReadOnlyDictionary<string, IReadOnlyDictionary<ulong, PlayerListPlayerModel>> PlayerList { get; init; } = new Dictionary<string, IReadOnlyDictionary<ulong, PlayerListPlayerModel>>();
        //public IReadOnlyDictionary<string, IReadOnlyDictionary<ulong, PlayerDetailModel>> PlayerDetailList { get; init; } = new Dictionary<string, IReadOnlyDictionary<ulong, PlayerDetailModel>>();
        public IReadOnlyDictionary<ulong, PlayerSummaryModel> PlayerSummaries { get; init; } = new Dictionary<ulong, PlayerSummaryModel>();
        public IReadOnlyDictionary<ulong, IReadOnlyCollection<PlayerBansModel>> PlayerBans { get; init; } = new Dictionary<ulong, IReadOnlyCollection<PlayerBansModel>>();
    }
}
