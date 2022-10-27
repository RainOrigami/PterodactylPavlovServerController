using Newtonsoft.Json;
using PavlovVR_Rcon;
using PterodactylPavlovServerController.Exceptions;
using PterodactylPavlovServerController.Models;
using System.Text.Json;

namespace PterodactylPavlovServerController.Services
{
    public class PavlovRconService
    {
        private readonly PterodactylService pterodactylService;
        private static Dictionary<string, PavlovRcon> pavlovRconConnections = new Dictionary<string, PavlovRcon>();

        public PavlovRconService(PterodactylService pterodactylService)
        {
            this.pterodactylService = pterodactylService;
        }

        public PlayerListPlayerModel[] GetActivePlayers(string serverId)
        {
            PavlovRcon pavlovRcon = openConnection(serverId);
            string playerList = pavlovRcon.SendCommand(RconCommandType.RefreshList);
            return JsonConvert.DeserializeObject<PlayerListPlayerModel[]>(JsonDocument.Parse(playerList).RootElement.GetProperty("PlayerList").ToString()) ?? throw new RconException();
        }

        public PlayerDetailModel? GetActivePlayerDetail(string serverId, string uniqueId)
        {
            PavlovRcon pavlovRcon = openConnection(serverId);
            JsonElement result = JsonDocument.Parse(pavlovRcon.SendCommand(RconCommandType.InspectPlayer, uniqueId)).RootElement;
            if (!result.GetProperty("Successful").GetBoolean())
            {
                return null;
            }

            return JsonConvert.DeserializeObject<PlayerDetailModel>(result.GetProperty("PlayerInfo").ToString()) ?? null;
        }

        public ServerInfoModel GetServerInfo(string serverId)
        {
            PavlovRcon pavlovRcon = openConnection(serverId);
            string response = pavlovRcon.SendCommand(RconCommandType.ServerInfo);
            ServerInfoModel serverInfoModel = JsonConvert.DeserializeObject<ServerInfoModel>(JsonDocument.Parse(response).RootElement.GetProperty("ServerInfo").ToString() ?? throw new RconException()) ?? throw new RconException();
            serverInfoModel.ServerId = serverId;
            return serverInfoModel;
        }

        public void SwitchMap(string serverId, long mapId, string gameMode)
        {
            if (!Enum.TryParse(gameMode, out PavlovGameMode pavlovGameMode))
            {
                throw new ArgumentException("Invalid game mode specified", nameof(gameMode));
            }

            PavlovRcon pavlovRcon = openConnection(serverId);
            pavlovRcon.SendCommand(RconCommandType.SwitchMap, $"UGC{mapId}", pavlovGameMode.ToString());
        }

        public void RotateMap(string serverId)
        {
            PavlovRcon pavlovRcon = openConnection(serverId);
            pavlovRcon.SendCommand(RconCommandType.RotateMap);
        }

        public void KickPlayer(string serverId, string uniqueId)
        {
            PavlovRcon pavlovRcon = openConnection(serverId);
            pavlovRcon.SendCommand(RconCommandType.Kick, uniqueId);
        }

        public void BanPlayer(string serverId, string uniqueId)
        {
            PavlovRcon pavlovRcon = openConnection(serverId);
            JsonElement result = JsonDocument.Parse(pavlovRcon.SendCommand(RconCommandType.Ban, uniqueId)).RootElement;
            if (!result.GetProperty("Successful").GetBoolean())
            {
                throw new RconException();
            }
        }

        public void UnbanPlayer(string serverId, string uniqueId)
        {
            PavlovRcon pavlovRcon = openConnection(serverId);
            JsonElement result = JsonDocument.Parse(pavlovRcon.SendCommand(RconCommandType.Unban, uniqueId)).RootElement;
            if (!result.GetProperty("Successful").GetBoolean())
            {
                throw new RconException();
            }
        }

        public string[] Banlist(string serverId)
        {
            PavlovRcon pavlovRcon = openConnection(serverId);
            return JsonDocument.Parse(pavlovRcon.SendCommand(RconCommandType.Banlist)).RootElement.GetProperty("BanList").EnumerateArray().Select(s => s.GetString()!).ToArray();
        }

        private PavlovRcon openConnection(string serverId)
        {
            if (!pavlovRconConnections.ContainsKey(serverId))
            {
                pavlovRconConnections.Add(serverId, new PavlovRcon(pterodactylService.GetHost(serverId), int.Parse(pterodactylService.GetStartupVariable(serverId, "RCON_PORT")), pterodactylService.GetStartupVariable(serverId, "RCON_PASSWORD")));
                //pavlovRconConnections[serverId].RconEvent += this.PavlovRcon_RconEvent;
            }

            if (!pavlovRconConnections[serverId].Connected)
            {
                pavlovRconConnections[serverId].Connect();
            }

            return pavlovRconConnections[serverId];
        }
    }
}
