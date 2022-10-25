using PterodactylPavlovServerController.Exceptions;
using PterodactylPavlovServerController.Models;

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

            string mapRotationList = String.Join(Environment.NewLine, mapRows.Select(m => m.MapRotationString));

            List<string> gameIniContent = pterodactylService.ReadFile(serverId, configuration["pavlov_gameinipath"]).Split(Environment.NewLine).Where(l => !l.StartsWith("MapRotation=")).ToList();
            gameIniContent.AddRange(mapRows.Select(r => r.MapRotationString));
            pterodactylService.WriteFile(serverId, configuration["pavlov_gameinipath"], String.Join(Environment.NewLine, gameIniContent));

            return mapRows;
        }
    }
}
