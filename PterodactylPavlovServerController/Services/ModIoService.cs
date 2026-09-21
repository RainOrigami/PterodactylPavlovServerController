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
    private readonly Dictionary<long, DateTime> unavailableSince = new();

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

            MapWorkshopModel mapDetail = this.loadMapDetail(mapId);

            lock (this.mapDetailCache)
            {
                this.mapDetailCache[mapId] = mapDetail;

                if (mapDetail.Unavailable)
                {
                    this.unavailableSince[mapId] = DateTime.Now;
                    return mapDetail;
                }

                this.unavailableSince.Remove(mapId);
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
                if (this.unavailableSince.TryGetValue(mapId, out DateTime failedAt) && failedAt > DateTime.Now.Subtract(unavailableRetryAfter))
                {
                    mapDetail = cachedMapDetail;
                    return true;
                }
            }
        }

        mapDetail = null;
        return false;
    }

    private MapWorkshopModel loadMapDetail(long mapId)
    {
        while (this.lastFetch > DateTime.Now.AddMilliseconds(-1000))
        {
            Thread.Sleep(1000);
        }

        string? rawMapData = null;

        this.lastFetch = DateTime.Now;
        using (TextReader reader = new StreamReader(new HttpClient().Send(new HttpRequestMessage(HttpMethod.Get, $"https://api.mod.io/v1/games/3959/mods/{mapId}?api_key={this.configuration["modio_apikey"]}")).Content.ReadAsStream()))
        {
            rawMapData = reader.ReadToEnd();
        }

        JObject? mapDetails = JsonConvert.DeserializeObject<JObject>(rawMapData)!;

        string? name = mapDetails?["name"]?.ToString();
        string? imageUrl = mapDetails?["logo"]?["thumb_320x180"]?.ToString();
        string? nameId = mapDetails?["name_id"]?.ToString();
        string? profileUrl = mapDetails?["profile_url"]?.ToString();

        if (rawMapData == null || mapDetails == null || name == null || imageUrl == null || profileUrl == null || nameId == null)
        {
            Console.Error.WriteLine("Something is fucked:");
            Console.WriteLine(rawMapData ?? "NO RAW MAP DATA");
            Console.WriteLine(mapDetails?.ToString() ?? "NO MAP DETAILS");
            Console.WriteLine(name ?? "NO NAME");
            Console.WriteLine(nameId ?? "NO NAME ID");
            Console.WriteLine(imageUrl ?? "NO IMAGE URL");
            Console.WriteLine(profileUrl ?? "NO PROFILE URL");
            return new MapWorkshopModel()
            {
                Id = mapId,
                Name = mapId.ToString(),
                Unavailable = true,
            };
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
