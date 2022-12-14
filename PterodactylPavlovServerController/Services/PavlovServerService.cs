using PterodactylPavlovServerDomain.Models;
using System.Text.RegularExpressions;

namespace PterodactylPavlovServerController.Services;

public class PavlovServerService
{
    private static readonly Regex mapRotationLineRegex = new(@"^MapRotation=\(MapId=""(?<id>[^""]+)"", GameMode=""(?<gamemode>[^""]+)""\)$");
    private readonly IConfiguration configuration;
    private readonly PterodactylService pterodactylService;

    public PavlovServerService(PterodactylService pterodactylService, IConfiguration configuration)
    {
        this.pterodactylService = pterodactylService;
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
        return (await this.pterodactylService.ReadFile(apiKey, serverId, this.configuration["pavlov_gameinipath"]!)).Split('\n').Select(l => l.Trim('\r')).Where(l => l.StartsWith("MapRotation=")).Select(l => PavlovServerService.mapRotationLineRegex.Match(l)).Where(m => m.Success).Select(m => new ServerMapModel
        {
            MapLabel = m.Groups["id"].Value,
            GameMode = m.Groups["gamemode"].Value,
        }).ToArray();
    }
}
