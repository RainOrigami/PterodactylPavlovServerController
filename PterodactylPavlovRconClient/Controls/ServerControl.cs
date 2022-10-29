using Microsoft.Extensions.Logging;
using PavlovVR_Rcon;
using PterodactylPavlovRconClient.Models;
using PterodactylPavlovRconClient.Properties;
using PterodactylPavlovRconClient.Services;
using PterodactylPavlovServerController.Models;
using Control = System.Windows.Forms.Control;

namespace PterodactylPavlovRconClient.Controls
{
    public partial class ServerControl : UserControl
    {
        private readonly PterodactylAPIService pterodactylAPIService;
        private readonly PterodactylServerModel server;
        private readonly ILogger logger;
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

        private MapModel? currentMap;
        int currentRound = -1;

        public ServerControl(PterodactylAPIService pterodactylAPIService, PterodactylServerModel server, ILogger logger)
        {
            InitializeComponent();
            this.pterodactylAPIService = pterodactylAPIService;
            this.server = server;
            this.logger = logger;
        }

        private void refreshTimer()
        {
            while (!this.IsDisposed)
            {
                try
                {
                    conditionalInvoke(() => pbLoading.Visible = true);
                    refreshServerAndActivePlayers();
                    conditionalInvoke(() => pbLoading.Visible = false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                Thread.Sleep((int)nudRefreshInterval.Value);
            }
        }

        private void ServerControl_Load(object sender, EventArgs e)
        {
            cbGameMode.Items.AddRange(Enum.GetNames<PavlovGameMode>());
            new Thread(new ThreadStart(firstLoad)).Start();
        }

        private void firstLoad()
        {
            try
            {
                refreshAll();
            }
            catch (Exception e)
            {
                logger.LogError($"Failed first load refresh all: {e.Message}{Environment.NewLine}{e}");
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

        public static bool GetServerState(ApiResponse response)
        {
            if (response.Success)
            {
                return true;
            }

            try
            {
                return response.ErrorMessage == "No server connection available.";
            }
            catch (Exception)
            {
                return true;
            }
        }

        private bool isServerOffline(ApiResponse response)
        {
            ServerState = GetServerState(response);
            return !ServerState;
        }

        private void refreshServerInfo()
        {
            ApiResponse<ServerInfoModel> serverInfoResponse = this.pterodactylAPIService.GetServerInfo(server.ServerId);
            if (isServerOffline(serverInfoResponse) || !serverInfoResponse.Success)
            {
                return;
            }

            ServerInfoModel serverInfo = serverInfoResponse.Data!;

            conditionalInvoke(() =>
            {
                currentMap = cbMapList.Items.OfType<MapModel>().FirstOrDefault(m => $"UGC{m.Id}" == serverInfo.MapLabel);

                if ((currentRound != 0 && serverInfo.Round == 0) || currentRound == -1)
                {
                    cbMapList.SelectedItem = currentMap;
                    if (cbMapList.SelectedItem is null)
                    {
                        cbMapList.Text = serverInfo.MapLabel;
                    }
                    cbGameMode.SelectedItem = serverInfo.GameMode;
                    if (cbGameMode.SelectedItem is null)
                    {
                        cbGameMode.Text = serverInfo.GameMode;
                    }
                }

                currentRound = serverInfo.Round;

                lblPlayerCount.Text = $"{playerControls.Count(c => !c.Value.Disconnected)} of {serverInfo.MaximumPlayerCount}";

                lblRound.Text = $"{serverInfo.Round} ({serverInfo.RoundState})";

                lblBlueTeam.Text = $"Blue Team ({serverInfo.Team0Score})";
                lblRedTeam.Text = $"Red Team ({serverInfo.Team1Score})";
            });
        }

        private Dictionary<string, PlayerControl> playerControls = new();

        private void refreshPlayerControls()
        {
            ApiResponse<PlayerListPlayerModel[]> playerListResponse = this.pterodactylAPIService.GetPlayerList(server.ServerId);
            if (isServerOffline(playerListResponse) || !playerListResponse.Success)
            {
                return;
            }

            PlayerListPlayerModel[] playerListPlayerModels = playerListResponse.Data!;

            foreach (PlayerListPlayerModel playerListPlayer in playerListPlayerModels)
            {
                PlayerControl playerControl;
                if (playerControls.ContainsKey(playerListPlayer.UniqueId))
                {
                    playerControl = playerControls[playerListPlayer.UniqueId];
                }
                else
                {
                    playerControl = new PlayerControl(this.pterodactylAPIService, server, playerListPlayer.UniqueId);
                    playerControl.Banned = false;
                    playerControls.Add(playerListPlayer.UniqueId, playerControl);
                }

                playerControl.Disconnected = false;
            }

            foreach (PlayerControl playerControl in playerControls.Values)
            {
                if (playerListPlayerModels.All(p => p.UniqueId != playerControl.UniqueId))
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
            if (ServerState)
            {
                refreshPlayerControls();
            }

            refreshServerInfo();
        }

        private void refreshBanlist()
        {
            ApiResponse<string[]> banlistResponse = this.pterodactylAPIService.GetBanlist(server.ServerId);
            if (isServerOffline(banlistResponse) || !banlistResponse.Success)
            {
                return;
            }

            string[] bannedSteamIds = banlistResponse.Data!;
            foreach (string bannedSteamId in bannedSteamIds)
            {
                ApiResponse<string> steamUserNameResponse = this.pterodactylAPIService.GetSteamUsername(bannedSteamId);

                PlayerControl bannedPlayerControl;
                if (playerControls.ContainsKey(bannedSteamId))
                {
                    bannedPlayerControl = playerControls[bannedSteamId];
                    if (steamUserNameResponse.Success)
                    {
                        bannedPlayerControl.UpdateUsername(steamUserNameResponse.Data!);
                    }
                }
                else
                {
                    bannedPlayerControl = new PlayerControl(pterodactylAPIService, server, bannedSteamId, new PlayerModel() { UniqueId = bannedSteamId, PlayerName = steamUserNameResponse.Success ? steamUserNameResponse.Data! : "Steam unavailable (try refresh all)" });
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
            ApiResponse<MapRowModel[]> mapRotationResponse = this.pterodactylAPIService.GetMapRotation(server.ServerId);

            if (!mapRotationResponse.Success)
            {
                MessageBox.Show(mapRotationResponse.ErrorMessage, $"Failed to retrieve map rotation for server {server.Name}", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            MapRowModel[] mapRowModels = mapRotationResponse.Data!;
            List<MapModel> maps = new List<MapModel>();

            foreach (MapRowModel mapRow in mapRowModels)
            {
                ApiResponse<MapDetailModel> mapDetailResponse = this.pterodactylAPIService.GetMapDetails(mapRow.MapId);

                if (!mapDetailResponse.Success)
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

                MapDetailModel mapDetailModel = mapDetailResponse.Data!;

                maps.Add(new MapModel()
                {
                    Id = mapDetailModel.Id,
                    Name = $"{mapDetailModel.Name} ({mapRow.GameMode})",
                    ImageURL = mapDetailModel.ImageURL,
                    URL = mapDetailModel.URL,
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

            ApiResponse mapChangeResponse = this.pterodactylAPIService.DoSwitchMap(server.ServerId, mapId, gameMode); //HttpMethod.Post, $"rcon/switchMap?serverId={server.ServerId}&mapId={mapId}&gamemode={gameMode}";
            if (isServerOffline(mapChangeResponse) || !mapChangeResponse.Success)
            {
                MessageBox.Show($"Failed to change map: {mapChangeResponse.ErrorMessage}", "Map change failed", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            Task.Run(() => refreshAll());
        }

        private void btnSkipToNextMap_Click(object sender, EventArgs e)
        {
            ApiResponse skipMapResponse = this.pterodactylAPIService.DoRotateMap(server.ServerId);
            if (isServerOffline(skipMapResponse) || !skipMapResponse.Success)
            {
                MessageBox.Show($"Failed to skip to next map: {skipMapResponse.ErrorMessage}", "Map rotation failed", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
        }

        private void btnToggleOfflinePlayers_Click(object sender, EventArgs e)
        {
            this.scTeamsAndOffline.Panel2Collapsed = !this.scTeamsAndOffline.Panel2Collapsed;
        }

        private void cbMapList_DrawItem(object sender, DrawItemEventArgs e)
        {
            MapModel item = (MapModel)((ComboBox)sender).Items[e.Index];
            Brush backBrush = currentMap != null && currentMap.Id == item.Id && currentMap.GameMode == item.GameMode ? Brushes.DarkOliveGreen : new SolidBrush(e.BackColor);
            e.Graphics.FillRectangle(backBrush, e.Bounds);
            e.Graphics.DrawString(((ComboBox)sender).Items[e.Index].ToString(), ((Control)sender).Font, new SolidBrush(e.ForeColor), e.Bounds.X, e.Bounds.Y);
        }

        private void cbMapList_SelectedIndexChanged(object sender, EventArgs e)
        {
            cbGameMode.SelectedItem = (((ComboBox)sender).SelectedItem as MapModel)?.GameMode.ToString();
        }
    }
}
