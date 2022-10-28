using Newtonsoft.Json;
using PavlovVR_Rcon;
using PterodactylPavlovRconClient.Models;
using PterodactylPavlovRconClient.Properties;
using PterodactylPavlovServerController.Models;
using RestSharp;
using System.Text.Json;

namespace PterodactylPavlovRconClient
{
    public partial class ServerControl : UserControl
    {
        private readonly RestClient restClient;
        private readonly PterodactylServerModel server;
        //Timer serverRefresh;

        private bool serverState = false;
        private bool ServerState
        {
            get => serverState; set
            {
                this.conditionalInvoke(() =>
                {
                    serverState = value;
                    pbServerStatus.Image = serverState ? Resources.signal_online : Resources.signal_offline;
                    btnSwitchMap.Enabled = serverState;
                    btnSkipToNextMap.Enabled = serverState;
                });
            }
        }

        public ServerControl(RestClient restClient, PterodactylServerModel server)
        {
            InitializeComponent();
            this.restClient = restClient;
            this.server = server;
        }

        private void refreshTimer()
        {
            while (!this.IsDisposed)
            {
                try
                {
                    refreshServerAndActivePlayers();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                Thread.Sleep((int)nudRefreshInterval.Value);
            }
        }

        //private async void ServerRefresh_Tick(object? sender, EventArgs e)
        //{
        //    //serverRefresh.Stop();
        //    pbLoading.Visible = true;
        //    await refreshServerAndActivePlayers();
        //    pbLoading.Visible = false;
        //    //serverRefresh.Start();
        //}

        private void ServerControl_Load(object sender, EventArgs e)
        {
            cbGameMode.Items.AddRange(Enum.GetNames<PavlovGameMode>());
            new Thread(new ThreadStart(firstLoad)).Start();
            //pbLoading.Visible = true;
            //await refreshAll();
            //pbLoading.Visible = false;
        }

        private void firstLoad()
        {
            try
            {
                refreshAll();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            refreshTimer();
        }

        private void refreshAll()
        {
            conditionalInvoke(() => pbLoading.Visible = true);
            refreshMaps();
            refreshServerAndActivePlayers();
            refreshBanlist();
            refreshPlayerBans();
            conditionalInvoke(() => pbLoading.Visible = false);
        }

        private void conditionalInvoke(Action method)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(method);
            }
            else
            {
                method();
            }
        }

        private void refreshPlayerBans()
        {
            foreach (PlayerControl playerControl in playerControls.Values)
            {
                playerControl.RefreshSteamBans();
            }
        }

        public static bool GetServerState(RestResponseBase restResponse)
        {
            if (restResponse.StatusCode != System.Net.HttpStatusCode.InternalServerError)
            {
                return true;
            }

            try
            {
                return JsonDocument.Parse(restResponse.Content!).RootElement.GetProperty("detail").GetString() != "No server connection available.";
            }
            catch (Exception)
            {
                return true;
            }
        }

        private bool isServerOffline(RestResponseBase restResponse)
        {
            ServerState = GetServerState(restResponse);
            return !ServerState;
        }

        private void refreshServerInfo()
        {
            RestResponse serverInfoResponse = restClient.Execute(new RestRequest($"rcon/serverinfo?serverId={server.ServerId}"));
            if (isServerOffline(serverInfoResponse))
            {
                return;
            }

            if (serverInfoResponse.StatusCode is not System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine(serverInfoResponse.Content ?? "Error");
                return;
            }

            ServerInfoModel serverInfo = JsonConvert.DeserializeObject<ServerInfoModel>(serverInfoResponse.Content!)!;
            conditionalInvoke(() =>
            {
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
            });
        }

        private Dictionary<string, PlayerControl> playerControls = new();

        private void refreshPlayerControls()
        {
            RestResponse<PlayerListPlayerModel[]> playerListResponse = restClient.Execute<PlayerListPlayerModel[]>(new RestRequest($"rcon/playerlist?serverId={server.ServerId}"));
            if (isServerOffline(playerListResponse))
            {
                return;
            }

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
            RestResponse<string[]> banlistResponse = restClient.Execute<string[]>(new RestRequest($"rcon/banlist?serverId={server.ServerId}"));
            if (isServerOffline(banlistResponse))
            {
                return;
            }

            if (!banlistResponse.IsSuccessful)
            {
                Console.WriteLine(banlistResponse.Content);
                return;
            }

            string[] bannedSteamIds = banlistResponse.Data!;
            foreach (string bannedSteamId in bannedSteamIds)
            {
                RestResponse<string> steamUserNameResponse = restClient.Execute<string>(new RestRequest($"steam/username?steamid={bannedSteamId}"));
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

            conditionalInvoke(() =>
            {
                playerControl.Parent?.Controls?.Remove(playerControl);
                targetPanel.Controls.Add(playerControl);
            });
        }

        private void refreshMaps()
        {
            RestResponse<MapRowModel[]> mapRotationResponse = restClient.Execute<MapRowModel[]>(new RestRequest($"maps/current?serverId={server.ServerId}"));

            if (!mapRotationResponse.IsSuccessful)
            {
                MessageBox.Show(mapRotationResponse.Content ?? "Error", mapRotationResponse.StatusCode.ToString());
                return;
            }

            List<MapModel> maps = new List<MapModel>();

            foreach (MapRowModel mapRow in mapRotationResponse.Data!)
            {
                RestResponse<MapDetailModel> mapDetailResponse = restClient.Execute<MapDetailModel>(new RestRequest($"maps/details?mapId={mapRow.MapId}"));

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

            conditionalInvoke(() =>
            {
                cbMapList.Items.Clear();
                cbMapList.Items.AddRange(maps.ToArray());
            });
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

            RestResponse mapChangeResponse = restClient.Execute(new RestRequest($"rcon/switchMap?serverId={server.ServerId}&mapId={mapId}&gamemode={gameMode}", Method.Post));
            if (isServerOffline(mapChangeResponse))
            {
                return;
            }
            if (!mapChangeResponse.IsSuccessful)
            {
                MessageBox.Show($"Failed to change map:{Environment.NewLine}{mapChangeResponse.Content}", mapChangeResponse.StatusCode.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            Task.Run(() => refreshAll());
        }

        private void btnSkipToNextMap_Click(object sender, EventArgs e)
        {
            RestResponse skipMapResponse = restClient.Execute(new RestRequest($"rcon/rotateMap?serverId={server.ServerId}", Method.Post));
            if (isServerOffline(skipMapResponse))
            {
                return;
            }
            if (!skipMapResponse.IsSuccessful)
            {
                MessageBox.Show($"Failed to skip to next map:{Environment.NewLine}{skipMapResponse.Content}", skipMapResponse.StatusCode.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
        }

        private void nudRefreshInterval_ValueChanged(object sender, EventArgs e)
        {
            //serverRefresh.Interval = (int)nudRefreshInterval.Value;
        }
    }
}
