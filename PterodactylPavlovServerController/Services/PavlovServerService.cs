using PterodactylPavlovServerDomain.Exceptions;
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

    public string GetServerName(string serverId)
    {
        return this.pterodactylService.ReadFile(serverId, this.configuration["pavlov_gameinipath"]!).Split('\n').FirstOrDefault(l => l.StartsWith("ServerName="))?.Replace("ServerName=", "") ?? "Unnamed server";
    }

    public GoogleSheetsMapRowModel[] UpdateMaps(string serverId, GoogleSheetsMapRowModel[] mapRows)
    {
        if (mapRows.Any(r => !r.IsValid))
        {
            throw new InvalidMapsException(mapRows.Where(r => !r.IsValid).ToArray());
        }

        List<string> gameIniContent = this.pterodactylService.ReadFile(serverId, this.configuration["pavlov_gameinipath"]!).Split('\n').Select(l => l.Trim('\r')).Where(l => !l.StartsWith("MapRotation=")).ToList();
        gameIniContent.AddRange(mapRows.Select(r => r.MapRotationString));
        this.pterodactylService.WriteFile(serverId, this.configuration["pavlov_gameinipath"]!, string.Join('\n', gameIniContent));

        return mapRows;
    }

    public ServerMapModel[] GetCurrentMapRotation(string serverId)
    {
        return this.pterodactylService.ReadFile(serverId, this.configuration["pavlov_gameinipath"]!).Split('\n').Select(l => l.Trim('\r')).Where(l => l.StartsWith("MapRotation=")).Select(l => PavlovServerService.mapRotationLineRegex.Match(l)).Where(m => m.Success).Select(m => new ServerMapModel
        {
            MapLabel = m.Groups["id"].Value,
            GameMode = m.Groups["gamemode"].Value,
        }).ToArray();
    }
}
