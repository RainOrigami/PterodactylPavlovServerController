using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Castle.Core.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PterodactylPavlovServerController.Exceptions;
using PterodactylPavlovServerDomain.Exceptions;
using PterodactylPavlovServerDomain.Models;
using System.Net;
using System.Security.Permissions;

namespace PterodactylPavlovServerController.Services;

public class ModIoService : IMapSourceService
{
    private readonly IConfiguration configuration;
    private readonly Dictionary<long, MapWorkshopModel> mapDetailCache;
    private DateTime lastFetch = DateTime.MinValue;

    public ModIoService(IConfiguration configuration)
    {
        this.configuration = configuration;

        try
        {
            this.mapDetailCache = JsonConvert.DeserializeObject<Dictionary<long, MapWorkshopModel>>(File.ReadAllText(configuration["mapscache"]!)) ?? new Dictionary<long, MapWorkshopModel>();

            // Entries written before Unavailable existed: a failed lookup left the
            // id as the name and no image. Mark them so they get retried.
            foreach (KeyValuePair<long, MapWorkshopModel> cacheEntry in this.mapDetailCache)
            {
                if (!cacheEntry.Value.HasImage && cacheEntry.Value.Name == cacheEntry.Key.ToString())
                {
                    cacheEntry.Value.Unavailable = true;
                }
            }
        }
        catch (Exception)
        {
            this.mapDetailCache = new Dictionary<long, MapWorkshopModel>();
        }
    }

    private static readonly TimeSpan unavailableRetryAfter = TimeSpan.FromHours(1);
    private static readonly TimeSpan removedRetryAfter = TimeSpan.FromDays(1);
    private readonly Dictionary<long, DateTime> retryUnavailableAt = new();

    // One gate per map id, so two callers asking for the same uncached map wait
    // for a single fetch instead of fetching it twice.
    private readonly Dictionary<long, object> mapLoadGates = new();

    public MapWorkshopModel GetMapDetail(long mapId)
    {
        if (tryGetCached(mapId, out MapWorkshopModel? cached))
        {
            return cached!;
        }

        // The fetch itself must happen outside the cache lock: loadMapDetail
        // sleeps up to a second to honour mod.io's rate limit and then blocks on
        // HTTP. Holding the cache lock across that stalled every other map
        // lookup in the process behind it.
        object gate;
        lock (this.mapLoadGates)
        {
            if (!this.mapLoadGates.TryGetValue(mapId, out object? existingGate))
            {
                existingGate = new object();
                this.mapLoadGates[mapId] = existingGate;
            }

            gate = existingGate;
        }

        lock (gate)
        {
            // Another caller may have filled it while we waited for the gate.
            if (tryGetCached(mapId, out MapWorkshopModel? filled))
            {
                return filled!;
            }

            MapWorkshopModel mapDetail = this.loadMapDetail(mapId, out bool removed);

            lock (this.mapDetailCache)
            {
                this.mapDetailCache[mapId] = mapDetail;

                if (mapDetail.Unavailable)
                {
                    this.retryUnavailableAt[mapId] = DateTime.Now.Add(removed ? ModIoService.removedRetryAfter : ModIoService.unavailableRetryAfter);
                    return mapDetail;
                }

                this.retryUnavailableAt.Remove(mapId);
                File.WriteAllText(this.configuration["mapscache"]!, JsonConvert.SerializeObject(this.mapDetailCache.Where(m => !m.Value.Unavailable).ToDictionary(m => m.Key, m => m.Value)));
            }

            return mapDetail;
        }
    }

    private bool tryGetCached(long mapId, out MapWorkshopModel? mapDetail)
    {
        lock (this.mapDetailCache)
        {
            if (this.mapDetailCache.TryGetValue(mapId, out MapWorkshopModel? cachedMapDetail))
            {
                if (!cachedMapDetail.Unavailable)
                {
                    mapDetail = cachedMapDetail;
                    return true;
                }

                // A failed lookup is only remembered in memory and retried after
                // a while: a map can come back, and a mod.io outage must not
                // poison the on-disk cache permanently.
                if (this.retryUnavailableAt.TryGetValue(mapId, out DateTime retryAt) && retryAt > DateTime.Now)
                {
                    mapDetail = cachedMapDetail;
                    return true;
                }
            }
        }

        mapDetail = null;
        return false;
    }

    /// <summary>
    ///     Stand-in for a map mod.io cannot tell us about. It goes by the same
    ///     UGC id the server logs use, so it still lines up with its stats.
    /// </summary>
    private static MapWorkshopModel unavailableMap(long mapId)
    {
        return new MapWorkshopModel
        {
            Id = mapId,
            Name = $"UGC{mapId}",
            Unavailable = true,
        };
    }

    private MapWorkshopModel loadMapDetail(long mapId, out bool removed)
    {
        while (this.lastFetch > DateTime.Now.AddMilliseconds(-1000))
        {
            Thread.Sleep(1000);
        }

        string rawMapData;
        removed = false;

        this.lastFetch = DateTime.Now;
        HttpResponseMessage response = new HttpClient().Send(new HttpRequestMessage(HttpMethod.Get, $"https://api.mod.io/v1/games/3959/mods/{mapId}?api_key={this.configuration["modio_apikey"]}"));
        using (TextReader reader = new StreamReader(response.Content.ReadAsStream()))
        {
            rawMapData = reader.ReadToEnd();
        }

        JObject? mapDetails = null;
        try
        {
            mapDetails = JsonConvert.DeserializeObject<JObject>(rawMapData);
        }
        catch (JsonException)
        {
            // Not JSON at all (an error page from a proxy, most likely): handled
            // below like any other unusable answer.
        }

        // A map deleted or hidden on mod.io answers "could not be found"
        // (error_ref 15024, sent as a 403). That is a definite answer rather than
        // an outage: the map keeps its stats under its UGC id, and is asked about
        // again far less often.
        if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Gone || mapDetails?["error"]?["error_ref"]?.ToString() == "15024")
        {
            Console.WriteLine($"Map {mapId} is no longer on mod.io, showing it as UGC{mapId}");
            removed = true;
            return ModIoService.unavailableMap(mapId);
        }

        string? name = mapDetails?["name"]?.ToString();
        string? imageUrl = mapDetails?["logo"]?["thumb_320x180"]?.ToString();
        string? nameId = mapDetails?["name_id"]?.ToString();
        string? profileUrl = mapDetails?["profile_url"]?.ToString();

        if (mapDetails == null || name == null || imageUrl == null || profileUrl == null || nameId == null)
        {
            string answer = rawMapData.Length > 300 ? rawMapData[..300] + "..." : rawMapData;
            Console.Error.WriteLine($"Could not resolve map {mapId} from mod.io (HTTP {(int)response.StatusCode}): {answer.ReplaceLineEndings(" ")}");
            return ModIoService.unavailableMap(mapId);
        }

        return new MapWorkshopModel
        {
            Id = mapId,
            URL = profileUrl,
            Name = name,
            NameId = nameId,
            ImageURL = imageUrl,
        };
    }
}
