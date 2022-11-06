using PterodactylPavlovServerDomain.Exceptions;
using PterodactylPavlovServerDomain.Models;
using System.Text.RegularExpressions;

namespace PterodactylPavlovServerController.Services
{
    public class PavlovServerService
    {
        private readonly PterodactylService pterodactylService;
        private readonly IConfiguration configuration;

        public PavlovServerService(PterodactylService pterodactylService, IConfiguration configuration)
        {
            this.pterodactylService = pterodactylService;
            this.configuration = configuration;
        }

        public string GetServerName(string serverId)
        {
            return pterodactylService.ReadFile(serverId, configuration["pavlov_gameinipath"]).Split('\n').FirstOrDefault(l => l.StartsWith("ServerName="))?.Replace("ServerName=", "") ?? "Unnamed server";
        }

        public MapRowModel[] UpdateMaps(string serverId, MapRowModel[] mapRows)
        {
            if (mapRows.Where(r => !r.IsValid).Any())
            {
                throw new InvalidMapsException(mapRows.Where(r => !r.IsValid).ToArray());
            }

            string mapRotationList = String.Join('\n', mapRows.Select(m => m.MapRotationString));

            List<string> gameIniContent = pterodactylService.ReadFile(serverId, configuration["pavlov_gameinipath"]).Split('\n').Select(l => l.Trim('\r')).Where(l => !l.StartsWith("MapRotation=")).ToList();
            gameIniContent.AddRange(mapRows.Select(r => r.MapRotationString));
            pterodactylService.WriteFile(serverId, configuration["pavlov_gameinipath"], String.Join('\n', gameIniContent));

            return mapRows;
        }

        private static readonly Regex mapRotationLineRegex = new Regex(@"^MapRotation=\(MapId=""UGC(?<id>\d+)"", GameMode=""(?<gamemode>[^""]+)""\)$");

        public MapRowModel[] GetCurrentMapRotation(string serverId)
        {
            return pterodactylService.ReadFile(serverId, configuration["pavlov_gameinipath"]).Split('\n').Select(l => l.Trim('\r')).Where(l => l.StartsWith("MapRotation=")).Select(l => mapRotationLineRegex.Match(l)).Where(m => m.Success).Select(m => new MapRowModel() { GameMode = m.Groups["gamemode"].Value, URL = $"https://steamcommunity.com/sharedfiles/filedetails/?id={m.Groups["id"].Value}", MapName = String.Empty, PageTitle = String.Empty }).ToArray();
        }
    }
}
