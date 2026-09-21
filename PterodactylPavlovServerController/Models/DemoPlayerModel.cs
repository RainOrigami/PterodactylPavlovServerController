using PavlovVR_Rcon.Models.Pavlov;
using PterodactylPavlovServerDomain.Models;
using Steam.Models.SteamCommunity;

namespace PterodactylPavlovServerController.Models;

/// <summary>
///     A fabricated player used to inspect PlayerCard formatting without needing
///     a real player on the server. Spawned from the browser console, never
///     persisted, and only ever held in memory for the current process.
/// </summary>
public class DemoPlayerModel
{
    public ulong UniqueId { get; init; }
    public Player? PlayerListPlayer { get; init; }
    public PlayerDetail? PlayerDetail { get; init; }
    public PlayerSummaryModel? Summary { get; init; }
    public IReadOnlyCollection<PlayerBansModel>? Bans { get; init; }
    public PersistentPavlovPlayerModel? DbPlayer { get; init; }
    public bool Banned { get; init; }
}
