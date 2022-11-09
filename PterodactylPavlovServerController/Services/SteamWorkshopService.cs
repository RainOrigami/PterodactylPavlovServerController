using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Newtonsoft.Json;
using PterodactylPavlovServerController.Exceptions;
using PterodactylPavlovServerDomain.Models;

namespace PterodactylPavlovServerController.Services;

public class SteamWorkshopService
{
    private readonly IConfiguration configuration;
    private readonly Dictionary<long, MapWorkshopModel> mapDetailCache;
    private DateTime lastFetch = DateTime.MinValue;

    public SteamWorkshopService(IConfiguration configuration)
    {
        this.configuration = configuration;

        try
        {
            this.mapDetailCache = JsonConvert.DeserializeObject<Dictionary<long, MapWorkshopModel>>(File.ReadAllText(configuration["workshop_mapscache"]!)) ?? new Dictionary<long, MapWorkshopModel>();
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
                File.WriteAllText(this.configuration["workshop_mapscache"]!, JsonConvert.SerializeObject(this.mapDetailCache));
            }

            return this.mapDetailCache[mapId];
        }
    }

    private MapWorkshopModel loadMapDetail(long mapId)
    {
        while (this.lastFetch > DateTime.Now.AddSeconds(-1))
        {
            Thread.Sleep(1000);
        }

        string mapUrl = $"https://steamcommunity.com/sharedfiles/filedetails/?id={mapId}";

        HtmlParser mapPageParser = new();
        IHtmlDocument mapPageDocument = mapPageParser.ParseDocument(new HttpClient().GetStreamAsync(mapUrl).GetAwaiter().GetResult());

        string? mapImageUrl = mapPageDocument.QuerySelector("img#previewImageMain")?.GetAttribute("src");
        if (mapImageUrl is null)
        {
            mapImageUrl = mapPageDocument.QuerySelector("img#previewImage")?.GetAttribute("src");
        }

        if (mapImageUrl is not null)
        {
            mapImageUrl = mapImageUrl.Substring(0, mapImageUrl.IndexOf('?'));
        }

        this.lastFetch = DateTime.Now;

        return new MapWorkshopModel
        {
            Id = mapId,
            URL = mapUrl,
            Name = mapPageDocument.QuerySelector("div.workshopItemTitle")?.TextContent ?? throw new SteamWorkshopException(),
            ImageURL = mapImageUrl,
        };
    }
}
