namespace PterodactylPavlovRconClient.Controls
{
    partial class PlayerControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblPlayerName = new System.Windows.Forms.Label();
            this.btnKick = new System.Windows.Forms.Button();
            this.btnBan = new System.Windows.Forms.Button();
            this.lblKills = new System.Windows.Forms.Label();
            this.lblDeaths = new System.Windows.Forms.Label();
            this.lblAssists = new System.Windows.Forms.Label();
            this.pbConnection = new System.Windows.Forms.PictureBox();
            this.pbPlayerState = new System.Windows.Forms.PictureBox();
            this.lblCash = new System.Windows.Forms.Label();
            this.lblScore = new System.Windows.Forms.Label();
            this.lblCashInfo = new System.Windows.Forms.Label();
            this.lblScoreInfo = new System.Windows.Forms.Label();
            this.lblKillsInfo = new System.Windows.Forms.Label();
            this.lblDeathsInfo = new System.Windows.Forms.Label();
            this.lblAssistsInfo = new System.Windows.Forms.Label();
            this.lblSteamIdInfo = new System.Windows.Forms.Label();
            this.lblSteamId = new System.Windows.Forms.Label();
            this.btnOpenProfile = new System.Windows.Forms.Button();
            this.pbBanned = new System.Windows.Forms.PictureBox();
            this.lblBansInfo = new System.Windows.Forms.Label();
            this.lblVacBans = new System.Windows.Forms.Label();
            this.lblGameBans = new System.Windows.Forms.Label();
            this.lblDaysSinceLastBan = new System.Windows.Forms.Label();
            this.pbLoading = new System.Windows.Forms.PictureBox();
            this.btnCheatsMenu = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pbConnection)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbPlayerState)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbBanned)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbLoading)).BeginInit();
            this.SuspendLayout();
            // 
            // lblPlayerName
            // 
            this.lblPlayerName.AutoSize = true;
            this.lblPlayerName.Font = new System.Drawing.Font("Segoe UI", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblPlayerName.Location = new System.Drawing.Point(0, 44);
            this.lblPlayerName.Name = "lblPlayerName";
            this.lblPlayerName.Size = new System.Drawing.Size(429, 40);
            this.lblPlayerName.TabIndex = 0;
            this.lblPlayerName.Text = "Some Guy with Very Long Name";
            // 
            // btnKick
            // 
            this.btnKick.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.btnKick.Location = new System.Drawing.Point(541, 3);
            this.btnKick.Name = "btnKick";
            this.btnKick.Size = new System.Drawing.Size(135, 45);
            this.btnKick.TabIndex = 1;
            this.btnKick.Text = "Kick";
            this.btnKick.UseVisualStyleBackColor = false;
            this.btnKick.Click += new System.EventHandler(this.btnKick_Click);
            // 
            // btnBan
            // 
            this.btnBan.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.btnBan.Location = new System.Drawing.Point(541, 54);
            this.btnBan.Name = "btnBan";
            this.btnBan.Size = new System.Drawing.Size(135, 45);
            this.btnBan.TabIndex = 2;
            this.btnBan.Text = "Ban";
            this.btnBan.UseVisualStyleBackColor = false;
            this.btnBan.Click += new System.EventHandler(this.btnBan_Click);
            // 
            // lblKills
            // 
            this.lblKills.AutoSize = true;
            this.lblKills.Location = new System.Drawing.Point(480, 44);
            this.lblKills.Name = "lblKills";
            this.lblKills.Size = new System.Drawing.Size(28, 15);
            this.lblKills.TabIndex = 3;
            this.lblKills.Text = "Kills";
            // 
            // lblDeaths
            // 
            this.lblDeaths.AutoSize = true;
            this.lblDeaths.Location = new System.Drawing.Point(480, 60);
            this.lblDeaths.Name = "lblDeaths";
            this.lblDeaths.Size = new System.Drawing.Size(43, 15);
            this.lblDeaths.TabIndex = 4;
            this.lblDeaths.Text = "Deaths";
            // 
            // lblAssists
            // 
            this.lblAssists.AutoSize = true;
            this.lblAssists.Location = new System.Drawing.Point(480, 75);
            this.lblAssists.Name = "lblAssists";
            this.lblAssists.Size = new System.Drawing.Size(42, 15);
            this.lblAssists.TabIndex = 5;
            this.lblAssists.Text = "Assists";
            // 
            // pbConnection
            // 
            this.pbConnection.Image = global::PterodactylPavlovRconClient.Properties.Resources.signal_offline;
            this.pbConnection.Location = new System.Drawing.Point(3, 3);
            this.pbConnection.Name = "pbConnection";
            this.pbConnection.Size = new System.Drawing.Size(32, 32);
            this.pbConnection.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbConnection.TabIndex = 6;
            this.pbConnection.TabStop = false;
            // 
            // pbPlayerState
            // 
            this.pbPlayerState.Image = global::PterodactylPavlovRconClient.Properties.Resources.skull;
            this.pbPlayerState.Location = new System.Drawing.Point(41, 3);
            this.pbPlayerState.Name = "pbPlayerState";
            this.pbPlayerState.Size = new System.Drawing.Size(32, 32);
            this.pbPlayerState.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbPlayerState.TabIndex = 7;
            this.pbPlayerState.TabStop = false;
            // 
            // lblCash
            // 
            this.lblCash.AutoSize = true;
            this.lblCash.Location = new System.Drawing.Point(480, 14);
            this.lblCash.Name = "lblCash";
            this.lblCash.Size = new System.Drawing.Size(33, 15);
            this.lblCash.TabIndex = 8;
            this.lblCash.Text = "Cash";
            // 
            // lblScore
            // 
            this.lblScore.AutoSize = true;
            this.lblScore.Location = new System.Drawing.Point(480, 29);
            this.lblScore.Name = "lblScore";
            this.lblScore.Size = new System.Drawing.Size(36, 15);
            this.lblScore.TabIndex = 9;
            this.lblScore.Text = "Score";
            // 
            // lblCashInfo
            // 
            this.lblCashInfo.AutoSize = true;
            this.lblCashInfo.Location = new System.Drawing.Point(438, 14);
            this.lblCashInfo.Name = "lblCashInfo";
            this.lblCashInfo.Size = new System.Drawing.Size(36, 15);
            this.lblCashInfo.TabIndex = 10;
            this.lblCashInfo.Text = "Cash:";
            // 
            // lblScoreInfo
            // 
            this.lblScoreInfo.AutoSize = true;
            this.lblScoreInfo.Location = new System.Drawing.Point(435, 29);
            this.lblScoreInfo.Name = "lblScoreInfo";
            this.lblScoreInfo.Size = new System.Drawing.Size(39, 15);
            this.lblScoreInfo.TabIndex = 11;
            this.lblScoreInfo.Text = "Score:";
            // 
            // lblKillsInfo
            // 
            this.lblKillsInfo.AutoSize = true;
            this.lblKillsInfo.Location = new System.Drawing.Point(443, 44);
            this.lblKillsInfo.Name = "lblKillsInfo";
            this.lblKillsInfo.Size = new System.Drawing.Size(31, 15);
            this.lblKillsInfo.TabIndex = 12;
            this.lblKillsInfo.Text = "Kills:";
            // 
            // lblDeathsInfo
            // 
            this.lblDeathsInfo.AutoSize = true;
            this.lblDeathsInfo.Location = new System.Drawing.Point(428, 59);
            this.lblDeathsInfo.Name = "lblDeathsInfo";
            this.lblDeathsInfo.Size = new System.Drawing.Size(46, 15);
            this.lblDeathsInfo.TabIndex = 13;
            this.lblDeathsInfo.Text = "Deaths:";
            // 
            // lblAssistsInfo
            // 
            this.lblAssistsInfo.AutoSize = true;
            this.lblAssistsInfo.Location = new System.Drawing.Point(429, 75);
            this.lblAssistsInfo.Name = "lblAssistsInfo";
            this.lblAssistsInfo.Size = new System.Drawing.Size(45, 15);
            this.lblAssistsInfo.TabIndex = 14;
            this.lblAssistsInfo.Text = "Assists:";
            // 
            // lblSteamIdInfo
            // 
            this.lblSteamIdInfo.AutoSize = true;
            this.lblSteamIdInfo.Location = new System.Drawing.Point(3, 84);
            this.lblSteamIdInfo.Name = "lblSteamIdInfo";
            this.lblSteamIdInfo.Size = new System.Drawing.Size(53, 15);
            this.lblSteamIdInfo.TabIndex = 15;
            this.lblSteamIdInfo.Text = "SteamId:";
            // 
            // lblSteamId
            // 
            this.lblSteamId.AutoSize = true;
            this.lblSteamId.Location = new System.Drawing.Point(62, 84);
            this.lblSteamId.Name = "lblSteamId";
            this.lblSteamId.Size = new System.Drawing.Size(103, 15);
            this.lblSteamId.TabIndex = 16;
            this.lblSteamId.Text = "0000000000000000";
            // 
            // btnOpenProfile
            // 
            this.btnOpenProfile.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(35)))));
            this.btnOpenProfile.ForeColor = System.Drawing.Color.LightGray;
            this.btnOpenProfile.Location = new System.Drawing.Point(312, 3);
            this.btnOpenProfile.Name = "btnOpenProfile";
            this.btnOpenProfile.Size = new System.Drawing.Size(117, 23);
            this.btnOpenProfile.TabIndex = 17;
            this.btnOpenProfile.Text = "Open steam profile";
            this.btnOpenProfile.UseVisualStyleBackColor = false;
            this.btnOpenProfile.Click += new System.EventHandler(this.btnOpenProfile_Click);
            // 
            // pbBanned
            // 
            this.pbBanned.Image = global::PterodactylPavlovRconClient.Properties.Resources.ban;
            this.pbBanned.Location = new System.Drawing.Point(79, 3);
            this.pbBanned.Name = "pbBanned";
            this.pbBanned.Size = new System.Drawing.Size(32, 32);
            this.pbBanned.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbBanned.TabIndex = 18;
            this.pbBanned.TabStop = false;
            // 
            // lblBansInfo
            // 
            this.lblBansInfo.AutoSize = true;
            this.lblBansInfo.Location = new System.Drawing.Point(117, 3);
            this.lblBansInfo.Name = "lblBansInfo";
            this.lblBansInfo.Size = new System.Drawing.Size(35, 15);
            this.lblBansInfo.TabIndex = 19;
            this.lblBansInfo.Text = "Bans:";
            // 
            // lblVacBans
            // 
            this.lblVacBans.AutoSize = true;
            this.lblVacBans.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblVacBans.Location = new System.Drawing.Point(158, 3);
            this.lblVacBans.Name = "lblVacBans";
            this.lblVacBans.Size = new System.Drawing.Size(59, 15);
            this.lblVacBans.TabIndex = 20;
            this.lblVacBans.Text = "VAC: 100x";
            // 
            // lblGameBans
            // 
            this.lblGameBans.AutoSize = true;
            this.lblGameBans.Location = new System.Drawing.Point(158, 18);
            this.lblGameBans.Name = "lblGameBans";
            this.lblGameBans.Size = new System.Drawing.Size(73, 15);
            this.lblGameBans.TabIndex = 21;
            this.lblGameBans.Text = "Games: 100x";
            // 
            // lblDaysSinceLastBan
            // 
            this.lblDaysSinceLastBan.AutoSize = true;
            this.lblDaysSinceLastBan.Location = new System.Drawing.Point(158, 33);
            this.lblDaysSinceLastBan.Name = "lblDaysSinceLastBan";
            this.lblDaysSinceLastBan.Size = new System.Drawing.Size(114, 15);
            this.lblDaysSinceLastBan.TabIndex = 22;
            this.lblDaysSinceLastBan.Text = "Last: 10000 days ago";
            // 
            // pbLoading
            // 
            this.pbLoading.Image = global::PterodactylPavlovRconClient.Properties.Resources.spin;
            this.pbLoading.Location = new System.Drawing.Point(3, 3);
            this.pbLoading.Name = "pbLoading";
            this.pbLoading.Size = new System.Drawing.Size(32, 32);
            this.pbLoading.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbLoading.TabIndex = 23;
            this.pbLoading.TabStop = false;
            // 
            // btnCheatsMenu
            // 
            this.btnCheatsMenu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(35)))));
            this.btnCheatsMenu.Location = new System.Drawing.Point(312, 29);
            this.btnCheatsMenu.Name = "btnCheatsMenu";
            this.btnCheatsMenu.Size = new System.Drawing.Size(117, 23);
            this.btnCheatsMenu.TabIndex = 24;
            this.btnCheatsMenu.Text = "Cheats";
            this.btnCheatsMenu.UseVisualStyleBackColor = false;
            this.btnCheatsMenu.Click += new System.EventHandler(this.btnCheatsMenu_Click);
            // 
            // PlayerControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(35)))));
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.btnCheatsMenu);
            this.Controls.Add(this.pbLoading);
            this.Controls.Add(this.lblDaysSinceLastBan);
            this.Controls.Add(this.lblGameBans);
            this.Controls.Add(this.lblVacBans);
            this.Controls.Add(this.lblBansInfo);
            this.Controls.Add(this.pbBanned);
            this.Controls.Add(this.btnOpenProfile);
            this.Controls.Add(this.lblSteamId);
            this.Controls.Add(this.lblSteamIdInfo);
            this.Controls.Add(this.lblAssistsInfo);
            this.Controls.Add(this.lblDeathsInfo);
            this.Controls.Add(this.lblKillsInfo);
            this.Controls.Add(this.lblScoreInfo);
            this.Controls.Add(this.lblCashInfo);
            this.Controls.Add(this.lblScore);
            this.Controls.Add(this.lblCash);
            this.Controls.Add(this.pbPlayerState);
            this.Controls.Add(this.pbConnection);
            this.Controls.Add(this.lblAssists);
            this.Controls.Add(this.lblDeaths);
            this.Controls.Add(this.lblKills);
            this.Controls.Add(this.btnBan);
            this.Controls.Add(this.btnKick);
            this.Controls.Add(this.lblPlayerName);
            this.ForeColor = System.Drawing.Color.LightGray;
            this.Name = "PlayerControl";
            this.Size = new System.Drawing.Size(679, 102);
            this.Load += new System.EventHandler(this.PlayerControl_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pbConnection)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbPlayerState)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbBanned)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbLoading)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Label lblPlayerName;
        private Button btnKick;
        private Button btnBan;
        private Label lblKills;
        private Label lblDeaths;
        private Label lblAssists;
        private PictureBox pbConnection;
        private PictureBox pbPlayerState;
        private Label lblCash;
        private Label lblScore;
        private Label lblCashInfo;
        private Label lblScoreInfo;
        private Label lblKillsInfo;
        private Label lblDeathsInfo;
        private Label lblAssistsInfo;
        private Label lblSteamIdInfo;
        private Label lblSteamId;
        private Button btnOpenProfile;
        private PictureBox pbBanned;
        private Label lblBansInfo;
        private Label lblVacBans;
        private Label lblGameBans;
        private Label lblDaysSinceLastBan;
        private PictureBox pbLoading;
        private Button btnCheatsMenu;
    }
}
