using Newtonsoft.Json;
using PavlovVR_Rcon;
using PterodactylPavlovServerController.Exceptions;
using PterodactylPavlovServerDomain.Models;
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

        internal void GiveItem(string serverId, string uniqueId, string item)
        {
            PavlovRcon pavlovRcon = openConnection(serverId);
            JsonElement result = JsonDocument.Parse(pavlovRcon.SendCommand(RconCommandType.GiveItem, uniqueId, item)).RootElement;
            if (!result.GetProperty("Successful").GetBoolean())
            {
                throw new RconException();

            }
        }

        internal void GiveCash(string serverId, string uniqueId, int amount)
        {
            PavlovRcon pavlovRcon = openConnection(serverId);
            JsonElement result = JsonDocument.Parse(pavlovRcon.SendCommand(RconCommandType.GiveCash, uniqueId, amount.ToString())).RootElement;
            if (!result.GetProperty("Successful").GetBoolean())
            {
                throw new RconException();

            }
        }
        internal void GiveVehicle(string serverId, string uniqueId, string vehicle)
        {
            PavlovRcon pavlovRcon = openConnection(serverId);
            JsonElement result = JsonDocument.Parse(pavlovRcon.SendCommand(RconCommandType.GiveVehicle, uniqueId, vehicle)).RootElement;
            if (!result.GetProperty("Successful").GetBoolean())
            {
                throw new RconException();

            }
        }
        internal void SetSkin(string serverId, string uniqueId, string skin)
        {
            PavlovRcon pavlovRcon = openConnection(serverId);
            JsonElement result = JsonDocument.Parse(pavlovRcon.SendCommand(RconCommandType.SetPlayerSkin, uniqueId, skin)).RootElement;
            if (!result.GetProperty("Successful").GetBoolean())
            {
                throw new RconException();

            }
        }
        internal void Slap(string serverId, string uniqueId, int amount)
        {
            PavlovRcon pavlovRcon = openConnection(serverId);
            JsonElement result = JsonDocument.Parse(pavlovRcon.SendCommand(RconCommandType.Slap, uniqueId, amount.ToString())).RootElement;
            if (!result.GetProperty("Successful").GetBoolean())
            {
                throw new RconException();

            }
        }
        internal void SwitchTeam(string serverId, string uniqueId, int team)
        {
            PavlovRcon pavlovRcon = openConnection(serverId);
            JsonElement result = JsonDocument.Parse(pavlovRcon.SendCommand(RconCommandType.SwitchTeam, uniqueId, team.ToString())).RootElement;
            if (!result.GetProperty("Successful").GetBoolean())
            {
                throw new RconException();

            }
        }
    }
}
