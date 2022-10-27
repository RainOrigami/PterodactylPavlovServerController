using PterodactylPavlovServerController.Exceptions;
using PterodactylPavlovServerController.Models;
using System.Text.RegularExpressions;

namespace PterodactylPavlovServerController.Services
{
    public class ServerControlService
    {
        private readonly IConfiguration configuration;
        private readonly PterodactylService pterodactylService;

        public ServerControlService(IConfiguration configuration, PterodactylService pterodactylService)
        {
            this.configuration = configuration;
            this.pterodactylService = pterodactylService;
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

        //MapRotation=(MapId="UGC2362993920", GameMode="SND")
        private static readonly Regex mapRotationLineRegex = new Regex(@"^MapRotation=\(MapId=""UGC(?<id>\d+)"", GameMode=""(?<gamemode>[^""]+)""\)$");

        public MapRowModel[] GetCurrentMapRotation(string serverId)
        {
            return pterodactylService.ReadFile(serverId, configuration["pavlov_gameinipath"]).Split('\n').Select(l => l.Trim('\r')).Where(l => l.StartsWith("MapRotation=")).Select(l => mapRotationLineRegex.Match(l)).Where(m => m.Success).Select(m => new MapRowModel() { GameMode = m.Groups["gamemode"].Value, URL = $"https://steamcommunity.com/sharedfiles/filedetails/?id={m.Groups["id"].Value}", MapName = String.Empty, PageTitle = String.Empty }).ToArray();
        }
    }
}
