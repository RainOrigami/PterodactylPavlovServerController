using System.Text.RegularExpressions;
using PterodactylPavlovServerDomain.Exceptions;
using PterodactylPavlovServerDomain.Models;

namespace PterodactylPavlovServerController.Services;

public class PavlovServerService
{
    private static readonly Regex mapRotationLineRegex = new(@"^MapRotation=\(MapId=""UGC(?<id>\d+)"", GameMode=""(?<gamemode>[^""]+)""\)$");
    private readonly IConfiguration configuration;
    private readonly PterodactylService pterodactylService;

    public PavlovServerService(PterodactylService pterodactylService, IConfiguration configuration)
    {
        this.pterodactylService = pterodactylService;
        this.configuration = configuration;
    }

    public string GetServerName(string serverId)
    {
        return this.pterodactylService.ReadFile(serverId, this.configuration["pavlov_gameinipath"]).Split('\n').FirstOrDefault(l => l.StartsWith("ServerName="))?.Replace("ServerName=", "") ?? "Unnamed server";
    }

    public MapRowModel[] UpdateMaps(string serverId, MapRowModel[] mapRows)
    {
        if (mapRows.Where(r => !r.IsValid).Any())
        {
            throw new InvalidMapsException(mapRows.Where(r => !r.IsValid).ToArray());
        }

        string mapRotationList = string.Join('\n', mapRows.Select(m => m.MapRotationString));

        List<string> gameIniContent = this.pterodactylService.ReadFile(serverId, this.configuration["pavlov_gameinipath"]).Split('\n').Select(l => l.Trim('\r')).Where(l => !l.StartsWith("MapRotation=")).ToList();
        gameIniContent.AddRange(mapRows.Select(r => r.MapRotationString));
        this.pterodactylService.WriteFile(serverId, this.configuration["pavlov_gameinipath"], string.Join('\n', gameIniContent));

        return mapRows;
    }

    public MapRowModel[] GetCurrentMapRotation(string serverId)
    {
        return this.pterodactylService.ReadFile(serverId, this.configuration["pavlov_gameinipath"]).Split('\n').Select(l => l.Trim('\r')).Where(l => l.StartsWith("MapRotation=")).Select(l => PavlovServerService.mapRotationLineRegex.Match(l)).Where(m => m.Success).Select(m => new MapRowModel
        {
            GameMode = m.Groups["gamemode"].Value,
            URL = $"https://steamcommunity.com/sharedfiles/filedetails/?id={m.Groups["id"].Value}",
            MapName = string.Empty,
            PageTitle = string.Empty,
        }).ToArray();
    }
}
