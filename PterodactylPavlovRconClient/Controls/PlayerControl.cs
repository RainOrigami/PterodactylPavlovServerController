using PterodactylPavlovRconClient.Models;
using PterodactylPavlovRconClient.Properties;
using PterodactylPavlovRconClient.Services;
using PterodactylPavlovServerController.Models;
using Steam.Models.SteamCommunity;
using System.Diagnostics;

namespace PterodactylPavlovRconClient.Controls
{
    public partial class PlayerControl : UserControl
    {
        private readonly PterodactylAPIService pterodactylAPIService;
        private readonly PterodactylServerModel server;
        private readonly string uniqueId;
        private PlayerModel playerModel;

        private bool disconnected = false;
        public bool Disconnected
        {
            get => disconnected; set
            {
                conditionalInvoke(() =>
                {
                    disconnected = value;

                    btnKick.Visible = !disconnected;
                    btnCheatsMenu.Visible = !disconnected;

                    if (disconnected)
                    {
                        this.pbConnection.Image = Resources.signal_offline;
                        this.pbPlayerState.Image = null;
                    }
                });
            }
        }
        public string UniqueId => uniqueId;
        public int Team => playerModel.TeamId;

        private bool banned = false;
        public bool Banned
        {
            get => banned; set
            {
                conditionalInvoke(() =>
                {

                    banned = value;

                    pbBanned.Image = banned ? Resources.ban : null;
                    btnBan.Text = banned ? "Unban" : "Ban";
                });
            }
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

        public PlayerControl(PterodactylAPIService pterodactylAPIService, PterodactylServerModel server, string uniqueId, PlayerModel? playerModel = null)
        {
            InitializeComponent();
            this.pterodactylAPIService = pterodactylAPIService;
            this.server = server;
            this.uniqueId = uniqueId;

            this.playerModel = playerModel ?? new PlayerModel()
            {
                UniqueId = this.UniqueId
            };
            updateValues(this.playerModel);
        }

        private void PlayerControl_Load(object sender, EventArgs e)
        {
            //RefreshPlayer();
            Task.Run(() => RefreshSteamBans());
        }

        public void RefreshSteamBans()
        {
            conditionalInvoke(() => pbLoading.Visible = true);
            ApiResponse<PlayerBansModel[]> steamBanResponse = this.pterodactylAPIService.GetPlayerBans(uniqueId);
            if (!steamBanResponse.Success)
            {
                lblVacBans.Text = "Steam unreachable";
                lblGameBans.Text = "Steam unreachable";
                lblDaysSinceLastBan.Text = "Steam unreachable";
                return;
            }

            int gameBans = 0;
            int vacBans = 0;
            int daysSinceLastBan = -1;
            bool currentlyVacBanned = false;

            PlayerBansModel[] playerBansModels = steamBanResponse.Data!;

            foreach (PlayerBansModel playerBans in playerBansModels)
            {
                if ((playerBans.NumberOfVACBans > 0 || playerBans.NumberOfGameBans > 0 || playerBans.DaysSinceLastBan > 0) && (daysSinceLastBan == -1 || daysSinceLastBan < playerBans.DaysSinceLastBan))
                {
                    daysSinceLastBan = (int)playerBans.DaysSinceLastBan;
                }

                vacBans += (int)playerBans.NumberOfVACBans;
                gameBans += (int)playerBans.NumberOfGameBans;

                if (playerBans.VACBanned)
                {
                    currentlyVacBanned = true;
                }
            }

            conditionalInvoke(() =>
            {
                lblVacBans.Text = $"VAC: {vacBans}x";
                lblGameBans.Text = $"Games: {gameBans}x";
                lblDaysSinceLastBan.Text = $"Last: {(daysSinceLastBan == -1 ? "never" : $"{daysSinceLastBan} days ago")}";

                if (currentlyVacBanned)
                {
                    lblVacBans.ForeColor = Color.Red;
                }
                else if (vacBans > 0)
                {
                    lblVacBans.ForeColor = Color.Blue;
                }

                if (gameBans > 0)
                {
                    lblGameBans.ForeColor = Color.Red;
                }

                if (daysSinceLastBan != -1)
                {
                    lblDaysSinceLastBan.ForeColor = Color.Red;
                }
                pbLoading.Visible = false;
            });
        }

        public void RefreshPlayer()
        {
            if (Banned || Disconnected)
            {
                return;
            }

            conditionalInvoke(() => pbLoading.Visible = true);

            ApiResponse<PlayerModel> playerInfoResponse = this.pterodactylAPIService.GetPlayerInfo(server.ServerId, uniqueId);
            if (!playerInfoResponse.Success)
            {
                updateValues(null);
            }
            else
            {
                playerModel = playerInfoResponse.Data!;
                updateValues(playerModel);
            }

            conditionalInvoke(() => pbLoading.Visible = false);
        }

        private void updateValues(PlayerModel? player)
        {
            conditionalInvoke(() =>
            {

                if (player is null)
                {
                    this.pbConnection.Image = Resources.signal_unstable;
                    this.pbPlayerState.Image = null;
                    return;
                }

                this.lblPlayerName.Text = player.PlayerName;
                this.lblKills.Text = player.Kills.ToString();
                this.lblDeaths.Text = player.Deaths.ToString();
                this.lblAssists.Text = player.Assists.ToString();
                this.lblCash.Text = $"${player.Cash}";
                this.lblScore.Text = player.Score.ToString();
                this.lblSteamId.Text = player.UniqueId.ToString();

                this.pbPlayerState.Image = player.Dead ? Resources.skull : Resources.heart;
                this.pbConnection.Image = Resources.signal_online;
            });
        }

        private void btnOpenProfile_Click(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo($"http://steamcommunity.com/profiles/{uniqueId}/") { UseShellExecute = true });
        }

        private void btnKick_Click(object sender, EventArgs e)
        {
            ApiResponse kickResponse = this.pterodactylAPIService.DoKickPlayer(server.ServerId, uniqueId);
            if (!kickResponse.Success)
            {
                MessageBox.Show($"Kick failed: {kickResponse.ErrorMessage}", "Could not kick player", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
        }

        private void btnBan_Click(object sender, EventArgs e)
        {
            if (Banned)
            {
                ApiResponse unbanResponse = this.pterodactylAPIService.DoUnbanPlayer(server.ServerId, uniqueId);
                if (!unbanResponse.Success)
                {
                    MessageBox.Show($"Unban failed: {unbanResponse.ErrorMessage}", "Could not unban player", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
                Banned = false;
            }
            else
            {
                ApiResponse banResponse = this.pterodactylAPIService.DoBanPlayer(server.ServerId, uniqueId);
                if (!banResponse.Success)
                {
                    MessageBox.Show($"Ban failed: {banResponse.ErrorMessage}", "Could not ban player", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
                Banned = true;
            }
        }

        internal void UpdateUsername(string username) => this.playerModel.PlayerName = username;

        private void btnCheatsMenu_Click(object sender, EventArgs e)
        {
            new PlayerCheatMenu(pterodactylAPIService, server, playerModel).Show();
        }
    }
}
