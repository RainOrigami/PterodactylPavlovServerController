using Microsoft.JSInterop;
using Newtonsoft.Json.Linq;
using PavlovVR_Rcon.Models.Pavlov;
using PterodactylPavlovServerController.Models;
using PterodactylPavlovServerDomain.Models;
using Steam.Models.SteamCommunity;

namespace PterodactylPavlovServerController.Services;

/// <summary>
///     Development aid: lets a fake player card be spawned from the browser
///     console so card formatting can be checked with no player on the server.
///     State is static and in-memory only - nothing is written anywhere.
/// </summary>
public static class DemoPlayerService
{
    private static readonly List<DemoPlayerModel> demoPlayers = new();

    public static event Action? OnChanged;

    public static IReadOnlyList<DemoPlayerModel> Players
    {
        get
        {
            lock (demoPlayers)
            {
                return demoPlayers.ToArray();
            }
        }
    }

    [JSInvokable]
    public static string SpawnDemoPlayer(string? optionsJson)
    {
        JObject options = string.IsNullOrWhiteSpace(optionsJson)
            ? new JObject()
            : JObject.Parse(optionsJson);

        string? name = options["name"]?.ToString();
        int ping = options["ping"]?.Value<int>() ?? 42;
        int score = options["score"]?.Value<int>() ?? 1337;
        int cash = options["cash"]?.Value<int>() ?? 4200;
        int kills = options["kills"]?.Value<int>() ?? 21;
        int deaths = options["deaths"]?.Value<int>() ?? 7;
        int assists = options["assists"]?.Value<int>() ?? 3;
        int teamId = options["team"]?.Value<int>() ?? 0;
        int vacBans = options["vacBans"]?.Value<int>() ?? 0;
        int gameBans = options["gameBans"]?.Value<int>() ?? 0;
        bool banned = options["banned"]?.Value<bool>() ?? false;
        bool online = options["online"]?.Value<bool>() ?? true;
        string country = options["country"]?.ToString() ?? "DE";

        // Fake ids sit in a range Steam does not hand out, so a demo card can
        // never collide with, or be mistaken for, a real player.
        ulong uniqueId;
        lock (demoPlayers)
        {
            uniqueId = 1_000_000_000_000_000UL + (ulong)demoPlayers.Count;
        }

        string playerName = name ?? $"Demo Player {uniqueId % 1000}";

        DemoPlayerModel demoPlayer = new()
        {
            UniqueId = uniqueId,
            Banned = banned,
            PlayerListPlayer = online ? new Player { UniqueId = uniqueId, Username = playerName } : null,
            PlayerDetail = online
                ? new PlayerDetail
                {
                    UniqueId = uniqueId,
                    PlayerName = playerName,
                    KDA = $"{kills}/{deaths}/{assists}",
                    Cash = cash,
                    Dead = options["dead"]?.Value<bool>() ?? false,
                    TeamId = teamId,
                    Score = score,
                    Ping = ping,
                }
                : null,
            Summary = new PlayerSummaryModel
            {
                SteamId = uniqueId,
                Nickname = playerName,
                ProfileUrl = $"https://steamcommunity.com/profiles/{uniqueId}",
                AvatarFullUrl = options["avatar"]?.ToString() ?? "https://pavlov.bloodisgood.net/gunimages/unknown.png",
                CountryCode = country,
            },
            Bans = vacBans > 0 || gameBans > 0
                ? new[]
                {
                    new PlayerBansModel
                    {
                        SteamId = uniqueId.ToString(),
                        VACBanned = vacBans > 0,
                        NumberOfVACBans = (uint)vacBans,
                        NumberOfGameBans = (uint)gameBans,
                        DaysSinceLastBan = (uint)(options["daysSinceLastBan"]?.Value<int>() ?? 30),
                    },
                }
                : Array.Empty<PlayerBansModel>(),
            DbPlayer = new PersistentPavlovPlayerModel
            {
                UniqueId = uniqueId,
                ServerId = options["serverId"]?.ToString() ?? string.Empty,
                Username = playerName,
                LastSeen = DateTime.Now.AddHours(-3),
                TotalTime = TimeSpan.FromHours(options["totalHours"]?.Value<int>() ?? 12),
                BanReason = banned ? options["banReason"]?.ToString() ?? "Demo ban reason" : null,
            },
        };

        lock (demoPlayers)
        {
            demoPlayers.Add(demoPlayer);
        }

        OnChanged?.Invoke();

        return $"Spawned demo player {playerName} ({uniqueId})";
    }

    [JSInvokable]
    public static string ClearDemoPlayers()
    {
        int count;
        lock (demoPlayers)
        {
            count = demoPlayers.Count;
            demoPlayers.Clear();
        }

        OnChanged?.Invoke();

        return $"Removed {count} demo player(s)";
    }
}
