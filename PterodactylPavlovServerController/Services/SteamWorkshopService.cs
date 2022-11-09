using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Newtonsoft.Json;
using PterodactylPavlovServerController.Exceptions;
using PterodactylPavlovServerDomain.Models;

namespace PterodactylPavlovServerController.Services
{
    public class SteamWorkshopService
    {
        private readonly IConfiguration configuration;
        private Dictionary<long, MapDetailModel> mapDetailCache;
        private DateTime lastFetch = DateTime.MinValue;

        public SteamWorkshopService(IConfiguration configuration)
        {
            this.configuration = configuration;

            try
            {
                mapDetailCache = JsonConvert.DeserializeObject<Dictionary<long, MapDetailModel>>(File.ReadAllText(configuration["workshop_mapscache"])) ?? new Dictionary<long, MapDetailModel>();
            }
            catch (Exception)
            {
                mapDetailCache = new Dictionary<long, MapDetailModel>();
            }
        }

        public MapDetailModel GetMapDetail(long mapId)
        {
            lock (mapDetailCache)
            {
                if (!mapDetailCache.ContainsKey(mapId))
                {
                    mapDetailCache.Add(mapId, loadMapDetail(mapId));
                    File.WriteAllText(configuration["workshop_mapscache"], JsonConvert.SerializeObject(mapDetailCache));
                }

                return mapDetailCache[mapId];
            }
        }

        private MapDetailModel loadMapDetail(long mapId)
        {
            while (lastFetch > DateTime.Now.AddSeconds(-1))
            {
                Thread.Sleep(1000);
            }

            string mapUrl = $"https://steamcommunity.com/sharedfiles/filedetails/?id={mapId}";

            HtmlParser mapPageParser = new HtmlParser();
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

            lastFetch = DateTime.Now;

            return new MapDetailModel()
            {
                Id = mapId,
                URL = mapUrl,
                Name = mapPageDocument.QuerySelector("div.workshopItemTitle")?.TextContent ?? throw new SteamWorkshopException(),
                ImageURL = mapImageUrl
            };
        }
    }
}
