using PterodactylPavlovServerDomain.Models;
using System.Text.RegularExpressions;

namespace PterodactylPavlovServerController.Services;

public class PavlovServerService
{
    private static readonly Regex mapRotationLineRegex = new(@"^MapRotation=\(MapId=""(?<id>[^""]+)"", ?GameMode=""(?<gamemode>[^""]+)""\)$");
    private readonly IConfiguration configuration;
    private readonly PterodactylService pterodactylService;
    private readonly IMapSourceService mapSourceService;

    public PavlovServerService(PterodactylService pterodactylService, IMapSourceService mapSourceService, IConfiguration configuration)
    {
        this.pterodactylService = pterodactylService;
        this.mapSourceService = mapSourceService;
        this.configuration = configuration;
    }

    public async Task<string> GetServerName(string apiKey, string serverId)
    {
        return (await this.pterodactylService.ReadFile(apiKey, serverId, this.configuration["pavlov_gameinipath"]!)).Split('\n').FirstOrDefault(l => l.StartsWith("ServerName="))?.Replace("ServerName=", "") ?? "Unnamed server";
    }

    public async Task ApplyMapList(string apiKey, string serverId, ServerMapModel[] maps)
    {
        List<string> gameIniContent = (await this.pterodactylService.ReadFile(apiKey, serverId, this.configuration["pavlov_gameinipath"]!)).Split('\n').Select(l => l.Trim('\r')).Where(l => !l.StartsWith("MapRotation=")).ToList();
        gameIniContent.AddRange(maps.Select(r => $"MapRotation=(MapId=\"{r.MapLabel}\", GameMode=\"{r.GameMode}\")"));
        this.pterodactylService.WriteFile(apiKey, serverId, this.configuration["pavlov_gameinipath"]!, string.Join('\n', gameIniContent));
    }

    public async Task<ServerMapModel[]> GetCurrentMapRotation(string apiKey, string serverId)
    {
        return (await this.pterodactylService.ReadFile(apiKey, serverId, this.configuration["pavlov_gameinipath"]!)).Split('\n').Select(l => l.Trim('\r')).Where(l => l.StartsWith("MapRotation=")).Select(l => PavlovServerService.mapRotationLineRegex.Match(l)).Where(m => m.Success).Select(m => {

            string mapName = m.Groups["id"].Value;
            string nameId = m.Groups["id"].Value;
            if (m.Groups["id"].Value.StartsWith("UGC") && long.TryParse(m.Groups["id"].Value[3..], out long mapId))
            {
                MapWorkshopModel mapModel = this.mapSourceService.GetMapDetail(mapId);
                mapName = mapModel.Name;
                nameId = mapModel.NameId;
            }

            return new ServerMapModel
            {
                MapLabel = m.Groups["id"].Value,
                GameMode = m.Groups["gamemode"].Value,
                NameId = nameId,
            };
        }).ToArray();
    }
}
