using PavlovVR_Rcon;
using PterodactylPavlovRconClient.Services;
using PterodactylPavlovServerDomain.Models;

namespace PterodactylPavlovRconClient
{
    public partial class PlayerCheatMenu : Form
    {
        private readonly PterodactylAPIService pterodactylAPIService;
        private readonly PterodactylServerModel server;
        private readonly PlayerModel targetPlayer;

        public PlayerCheatMenu(PterodactylAPIService pterodactylAPIService, PterodactylServerModel server, PlayerModel targetPlayer)
        {
            InitializeComponent();
            this.pterodactylAPIService = pterodactylAPIService;
            this.server = server;
            this.targetPlayer = targetPlayer;

            this.lblPlayerName.Text = targetPlayer.PlayerName;

            cbItems.Items.AddRange(Enum.GetNames<PavlovItemsAndGrenades>());
            cbSkins.Items.AddRange(Enum.GetNames<PavlovSkins>());
            cbVehicles.Items.AddRange(Enum.GetNames<PavlovVehicles>());
        }

        private void btnGiveItem_Click(object sender, EventArgs e)
        {
            if (cbItems.SelectedItem is null)
            {
                return;
            }

            PavlovItemsAndGrenades item = Enum.Parse<PavlovItemsAndGrenades>((string)cbItems.SelectedItem);

            ApiResponse giveItemResponse = this.pterodactylAPIService.DoGiveItem(server.ServerId, targetPlayer.UniqueId, item.ToString());
            if (!giveItemResponse.Success)
            {
                MessageBox.Show($"Failed to give item to player: {giveItemResponse.ErrorMessage}", "Cheat error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnGiveCash_Click(object sender, EventArgs e)
        {
            ApiResponse giveCashResponse = this.pterodactylAPIService.DoGiveCash(server.ServerId, targetPlayer.UniqueId, (int)nudCash.Value);
            if (!giveCashResponse.Success)
            {
                MessageBox.Show($"Failed to give cash to player: {giveCashResponse.ErrorMessage}", "Cheat error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnGiveVehicle_Click(object sender, EventArgs e)
        {
            if (cbVehicles.SelectedItem is null)
            {
                return;
            }

            PavlovVehicles vehicle = Enum.Parse<PavlovVehicles>((string)cbVehicles.SelectedItem);

            ApiResponse giveVehicleResponse = this.pterodactylAPIService.DoGiveVehicle(server.ServerId, targetPlayer.UniqueId, vehicle.ToString());
            if (!giveVehicleResponse.Success)
            {
                MessageBox.Show($"Failed to give vehicle to player: {giveVehicleResponse.ErrorMessage}", "Cheat error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnSetSkin_Click(object sender, EventArgs e)
        {
            if (cbSkins.SelectedItem is null)
            {
                return;
            }

            PavlovSkins skin = Enum.Parse<PavlovSkins>((string)cbSkins.SelectedItem);

            ApiResponse setSkinResponse = this.pterodactylAPIService.DoSetSkin(server.ServerId, targetPlayer.UniqueId, skin.ToString());
            if (!setSkinResponse.Success)
            {
                MessageBox.Show($"Failed to set skin to player: {setSkinResponse.ErrorMessage}", "Cheat error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnSlap_Click(object sender, EventArgs e)
        {
            ApiResponse slapResponse = this.pterodactylAPIService.DoSlap(server.ServerId, targetPlayer.UniqueId, (int)nudSlap.Value);
            if (!slapResponse.Success)
            {
                MessageBox.Show($"Failed to slap player: {slapResponse.ErrorMessage}", "Cheat error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnSwitchTeam_Click(object sender, EventArgs e)
        {
            ApiResponse<PlayerModel> playerDetail = this.pterodactylAPIService.GetPlayerInfo(server.ServerId, targetPlayer.UniqueId);
            if (!playerDetail.Success)
            {
                MessageBox.Show($"Failed to determine target player team: {playerDetail.ErrorMessage}", "Cheat error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            ApiResponse switchTeamResponse = this.pterodactylAPIService.DoSwitchTeam(server.ServerId, targetPlayer.UniqueId, playerDetail.Data!.TeamId == 0 ? 1 : 0);
            if (!switchTeamResponse.Success)
            {
                MessageBox.Show($"Failed to switch player team: {switchTeamResponse.ErrorMessage}", "Cheat error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
