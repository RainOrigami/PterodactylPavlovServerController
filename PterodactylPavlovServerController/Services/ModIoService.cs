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
        }
        catch (Exception)
        {
            this.mapDetailCache = new Dictionary<long, MapWorkshopModel>();
        }
    }

    public MapWorkshopModel GetMapDetail(long mapId)
    {
        lock (this.mapDetailCache)
        {
            if (!this.mapDetailCache.ContainsKey(mapId))
            {
                this.mapDetailCache.Add(mapId, this.loadMapDetail(mapId));
                File.WriteAllText(this.configuration["mapscache"]!, JsonConvert.SerializeObject(this.mapDetailCache));
            }

            return this.mapDetailCache[mapId];
        }
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
            throw new ModIoException();
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
