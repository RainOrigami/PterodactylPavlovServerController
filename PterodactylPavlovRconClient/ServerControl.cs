using Newtonsoft.Json;
using PavlovVR_Rcon;
using PterodactylPavlovRconClient.Models;
using PterodactylPavlovServerController.Models;
using RestSharp;
using Timer = System.Windows.Forms.Timer;

namespace PterodactylPavlovRconClient
{
    public partial class ServerControl : UserControl
    {
        private readonly RestClient restClient;
        private readonly PterodactylServerModel server;

        public ServerControl(RestClient restClient, PterodactylServerModel server)
        {
            InitializeComponent();
            this.restClient = restClient;
            this.server = server;

            Timer serverRefresh = new Timer();
            serverRefresh.Interval = 2000;
            serverRefresh.Tick += this.ServerRefresh_Tick;
            serverRefresh.Start();
        }

        private void ServerRefresh_Tick(object? sender, EventArgs e)
        {
            refreshServerAndActivePlayers();
        }

        private void ServerControl_Load(object sender, EventArgs e)
        {
            cbGameMode.Items.AddRange(Enum.GetNames<PavlovGameMode>());
            refreshAll();
        }

        private void refreshAll()
        {
            refreshMaps();
            refreshServerAndActivePlayers();
            refreshBanlist();
        }

        private void refreshServerInfo()
        {
            RestRequest serverInfoRequest = new RestRequest($"rcon/serverinfo?serverId={server.ServerId}");
            RestResponse serverInfoResponse = restClient.Execute(serverInfoRequest);

            if (serverInfoResponse.StatusCode is not System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine(serverInfoResponse.Content ?? "Error");
                return;
            }

            ServerInfoModel serverInfo = JsonConvert.DeserializeObject<ServerInfoModel>(serverInfoResponse.Content!)!;

            cbMapList.SelectedItem = cbMapList.Items.OfType<MapModel>().FirstOrDefault(m => $"UGC{m.Id}" == serverInfo.MapLabel);
            if (cbMapList.SelectedItem is null)
            {
                cbMapList.Text = serverInfo.MapLabel;
                cbGameMode.SelectedItem = serverInfo.GameMode;
                if (cbGameMode.SelectedItem is null)
                {
                    cbGameMode.Text = serverInfo.GameMode;
                }
            }

            lblPlayerCount.Text = $"{playerControls.Count(c => !c.Value.Disconnected)} of {serverInfo.MaximumPlayerCount}";
        }

        private Dictionary<string, PlayerControl> playerControls = new();

        private void refreshPlayerControls()
        {
            RestRequest playerListRequest = new RestRequest($"rcon/playerlist?serverId={server.ServerId}");
            RestResponse<PlayerListPlayerModel[]> playerListResponse = restClient.Execute<PlayerListPlayerModel[]>(playerListRequest);

            if (!playerListResponse.IsSuccessful)
            {
                Console.WriteLine(playerListResponse.Content ?? "Error");
                return;
            }

            foreach (PlayerListPlayerModel playerListPlayer in playerListResponse.Data!)
            {
                PlayerControl playerControl;
                if (playerControls.ContainsKey(playerListPlayer.UniqueId))
                {
                    playerControl = playerControls[playerListPlayer.UniqueId];
                }
                else
                {
                    playerControl = new PlayerControl(restClient, server, playerListPlayer.UniqueId);
                    playerControl.Banned = false;
                    playerControls.Add(playerListPlayer.UniqueId, playerControl);
                }

                playerControl.Disconnected = false;
            }

            foreach (PlayerControl playerControl in playerControls.Values)
            {
                if (playerListResponse.Data!.All(p => p.UniqueId != playerControl.UniqueId))
                {
                    playerControl.Disconnected = true;
                }
                else
                {
                    playerControl.RefreshPlayer();
                }

                assignPlayerControlToCorrespondingPanel(playerControl);
            }
        }

        private void refreshServerAndActivePlayers()
        {
            refreshPlayerControls();
            refreshServerInfo();
        }

        private void refreshBanlist()
        {
            RestRequest banlistRequest = new RestRequest($"rcon/banlist?serverId={server.ServerId}");
            RestResponse<string[]> banlistResponse = restClient.Execute<string[]>(banlistRequest);
            if (!banlistResponse.IsSuccessful)
            {
                Console.WriteLine(banlistResponse.Content);
                return;
            }

            string[] bannedSteamIds = banlistResponse.Data!;
            foreach (string bannedSteamId in bannedSteamIds)
            {
                RestRequest steamUserNameRequest = new RestRequest($"steam/username?steamid={bannedSteamId}");
                RestResponse<string> steamUserNameResponse = restClient.Execute<string>(steamUserNameRequest);
                if (!steamUserNameResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine(banlistResponse.Content);
                    continue;
                }

                PlayerControl bannedPlayerControl;
                if (playerControls.ContainsKey(bannedSteamId))
                {
                    bannedPlayerControl = playerControls[bannedSteamId];
                }
                else
                {
                    bannedPlayerControl = new PlayerControl(restClient, server, bannedSteamId, new PlayerModel() { UniqueId = bannedSteamId, PlayerName = steamUserNameResponse.Data! });
                    playerControls.Add(bannedSteamId, bannedPlayerControl);
                }

                bannedPlayerControl.Banned = true;
                bannedPlayerControl.Disconnected = true;

                assignPlayerControlToCorrespondingPanel(bannedPlayerControl);
            }
        }

        private void assignPlayerControlToCorrespondingPanel(PlayerControl playerControl)
        {
            FlowLayoutPanel targetPanel;
            if (playerControl.Disconnected || playerControl.Banned)
            {
                targetPanel = flpOfflinePlayers;
            }
            else if (playerControl.Team == 0)
            {
                targetPanel = flpTeam0;
            }
            else if (playerControl.Team == 1)
            {
                targetPanel = flpTeam1;
            }
            else
            {
                Console.WriteLine($"Invalid state of player panel {playerControl.UniqueId}, not disconnected, not banned and team <0||>1");
                targetPanel = flpOfflinePlayers;
            }

            if (targetPanel == playerControl.Parent)
            {
                return;
            }

            playerControl.Parent?.Controls?.Remove(playerControl);
            targetPanel.Controls.Add(playerControl);
        }

        private void refreshMaps()
        {
            RestRequest mapRotationRequest = new RestRequest($"maps/current?serverId={server.ServerId}");
            RestResponse<MapRowModel[]> mapRotationResponse = restClient.Execute<MapRowModel[]>(mapRotationRequest);

            if (!mapRotationResponse.IsSuccessful)
            {
                MessageBox.Show(mapRotationResponse.Content ?? "Error", mapRotationResponse.StatusCode.ToString());
                return;
            }

            List<MapModel> maps = new List<MapModel>();

            foreach (MapRowModel mapRow in mapRotationResponse.Data!)
            {
                RestRequest mapDetailRequest = new RestRequest($"maps/details?mapId={mapRow.MapId}");
                RestResponse<MapDetailModel> mapDetailResponse = restClient.Execute<MapDetailModel>(mapDetailRequest);

                if (!mapDetailResponse.IsSuccessful || mapDetailResponse.Data is null)
                {
                    maps.Add(new MapModel()
                    {
                        Id = mapRow.MapId,
                        Name = $"{mapRow.MapId} ({mapRow.GameMode}) (Steam Down?)",
                        URL = $"steamcommunity.com/sharedfiles/filedetails/?id={mapRow.MapId}",
                        GameMode = mapRow.GameMode != null ? Enum.Parse<PavlovGameMode>(mapRow.GameMode) : PavlovGameMode.SND
                    });
                    continue;
                }

                maps.Add(new MapModel()
                {
                    Id = mapDetailResponse.Data.Id,
                    Name = $"{mapDetailResponse.Data.Name} ({mapRow.GameMode})",
                    ImageURL = mapDetailResponse.Data.ImageURL,
                    URL = mapDetailResponse.Data.URL,
                    GameMode = mapRow.GameMode != null ? Enum.Parse<PavlovGameMode>(mapRow.GameMode) : PavlovGameMode.SND
                });
            }

            cbMapList.Items.Clear();
            cbMapList.Items.AddRange(maps.ToArray());
        }

        private void btnSwitchMap_Click(object sender, EventArgs e)
        {
            string mapId;
            string gameMode;

            if (cbMapList.SelectedItem is MapModel mapValue)
            {
                mapId = mapValue.Id.ToString();
                if (!String.IsNullOrWhiteSpace(cbGameMode.Text))
                {
                    gameMode = cbGameMode.Text;
                }
                else
                {
                    gameMode = mapValue.GameMode.ToString();
                }
            }
            else if (!String.IsNullOrWhiteSpace(cbMapList.Text))
            {
                mapId = cbMapList.Text;
                if (String.IsNullOrWhiteSpace(cbGameMode.Text))
                {
                    MessageBox.Show("Select a game mode for a custom map to load!", "Game mode required", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
                else
                {
                    gameMode = cbGameMode.Text;
                }
            }
            else
            {
                MessageBox.Show("Select or type a map to change to!", "Map required", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            RestRequest mapChangeRequest = new RestRequest($"rcon/switchMap?serverId={server.ServerId}&mapId={mapId}&gamemode={gameMode}", Method.Post);
            RestResponse mapChangeResponse = restClient.Execute(mapChangeRequest);
            if (!mapChangeResponse.IsSuccessful)
            {
                MessageBox.Show($"Failed to change map:{Environment.NewLine}{mapChangeResponse.Content}", mapChangeResponse.StatusCode.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            refreshAll();
        }

        private void btnSkipToNextMap_Click(object sender, EventArgs e)
        {
            RestRequest skipMapRequest = new RestRequest($"rcon/rotateMap?serverId={server.ServerId}", Method.Post);
            RestResponse skipMapResponse = restClient.Execute(skipMapRequest);
            if (!skipMapResponse.IsSuccessful)
            {
                MessageBox.Show($"Failed to skip to next map:{Environment.NewLine}{skipMapResponse.Content}", skipMapResponse.StatusCode.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
        }
    }
}
