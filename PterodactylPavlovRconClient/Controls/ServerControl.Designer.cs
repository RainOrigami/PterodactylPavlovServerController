namespace PterodactylPavlovRconClient.Controls
{
    partial class ServerControl
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
            this.cbMapList = new System.Windows.Forms.ComboBox();
            this.btnSwitchMap = new System.Windows.Forms.Button();
            this.scTeam0AndTeam1 = new System.Windows.Forms.SplitContainer();
            this.flpTeam0 = new PterodactylPavlovRconClient.Controls.ForceVerticalScrollFlowLayoutPanel();
            this.lblBlueTeam = new System.Windows.Forms.Label();
            this.flpTeam1 = new PterodactylPavlovRconClient.Controls.ForceVerticalScrollFlowLayoutPanel();
            this.lblRedTeam = new System.Windows.Forms.Label();
            this.lblPlayers = new System.Windows.Forms.Label();
            this.lblPlayerCount = new System.Windows.Forms.Label();
            this.cbGameMode = new System.Windows.Forms.ComboBox();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.scTeamsAndOffline = new System.Windows.Forms.SplitContainer();
            this.flpOfflinePlayers = new PterodactylPavlovRconClient.Controls.ForceVerticalScrollFlowLayoutPanel();
            this.label3 = new System.Windows.Forms.Label();
            this.btnSkipToNextMap = new System.Windows.Forms.Button();
            this.pbLoading = new System.Windows.Forms.PictureBox();
            this.nudRefreshInterval = new System.Windows.Forms.NumericUpDown();
            this.lblRefreshInterval = new System.Windows.Forms.Label();
            this.pbServerStatus = new System.Windows.Forms.PictureBox();
            this.lblRoundInfo = new System.Windows.Forms.Label();
            this.lblRound = new System.Windows.Forms.Label();
            this.btnToggleOfflinePlayers = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.scTeam0AndTeam1)).BeginInit();
            this.scTeam0AndTeam1.Panel1.SuspendLayout();
            this.scTeam0AndTeam1.Panel2.SuspendLayout();
            this.scTeam0AndTeam1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scTeamsAndOffline)).BeginInit();
            this.scTeamsAndOffline.Panel1.SuspendLayout();
            this.scTeamsAndOffline.Panel2.SuspendLayout();
            this.scTeamsAndOffline.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbLoading)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRefreshInterval)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbServerStatus)).BeginInit();
            this.SuspendLayout();
            // 
            // cbMapList
            // 
            this.cbMapList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(35)))));
            this.cbMapList.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.cbMapList.DropDownHeight = 726;
            this.cbMapList.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.cbMapList.ForeColor = System.Drawing.Color.LightGray;
            this.cbMapList.FormattingEnabled = true;
            this.cbMapList.IntegralHeight = false;
            this.cbMapList.Location = new System.Drawing.Point(3, 3);
            this.cbMapList.Name = "cbMapList";
            this.cbMapList.Size = new System.Drawing.Size(448, 36);
            this.cbMapList.TabIndex = 0;
            this.cbMapList.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.cbMapList_DrawItem);
            this.cbMapList.SelectedIndexChanged += new System.EventHandler(this.cbMapList_SelectedIndexChanged);
            // 
            // btnSwitchMap
            // 
            this.btnSwitchMap.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(35)))));
            this.btnSwitchMap.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnSwitchMap.ForeColor = System.Drawing.Color.LightGray;
            this.btnSwitchMap.Location = new System.Drawing.Point(634, 3);
            this.btnSwitchMap.Name = "btnSwitchMap";
            this.btnSwitchMap.Size = new System.Drawing.Size(104, 38);
            this.btnSwitchMap.TabIndex = 1;
            this.btnSwitchMap.Text = "Switch map";
            this.btnSwitchMap.UseVisualStyleBackColor = false;
            this.btnSwitchMap.Click += new System.EventHandler(this.btnSwitchMap_Click);
            // 
            // scTeam0AndTeam1
            // 
            this.scTeam0AndTeam1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.scTeam0AndTeam1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scTeam0AndTeam1.Location = new System.Drawing.Point(0, 0);
            this.scTeam0AndTeam1.Name = "scTeam0AndTeam1";
            // 
            // scTeam0AndTeam1.Panel1
            // 
            this.scTeam0AndTeam1.Panel1.Controls.Add(this.flpTeam0);
            this.scTeam0AndTeam1.Panel1.Controls.Add(this.lblBlueTeam);
            // 
            // scTeam0AndTeam1.Panel2
            // 
            this.scTeam0AndTeam1.Panel2.Controls.Add(this.flpTeam1);
            this.scTeam0AndTeam1.Panel2.Controls.Add(this.lblRedTeam);
            this.scTeam0AndTeam1.Size = new System.Drawing.Size(1714, 864);
            this.scTeam0AndTeam1.SplitterDistance = 830;
            this.scTeam0AndTeam1.TabIndex = 2;
            // 
            // flpTeam0
            // 
            this.flpTeam0.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flpTeam0.AutoScroll = true;
            this.flpTeam0.Location = new System.Drawing.Point(-2, 77);
            this.flpTeam0.Name = "flpTeam0";
            this.flpTeam0.Size = new System.Drawing.Size(828, 782);
            this.flpTeam0.TabIndex = 2;
            // 
            // lblBlueTeam
            // 
            this.lblBlueTeam.BackColor = System.Drawing.Color.Navy;
            this.lblBlueTeam.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblBlueTeam.Font = new System.Drawing.Font("Segoe UI", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblBlueTeam.ForeColor = System.Drawing.Color.LightGray;
            this.lblBlueTeam.Location = new System.Drawing.Point(0, 0);
            this.lblBlueTeam.Name = "lblBlueTeam";
            this.lblBlueTeam.Size = new System.Drawing.Size(826, 74);
            this.lblBlueTeam.TabIndex = 1;
            this.lblBlueTeam.Text = "Blue Team";
            // 
            // flpTeam1
            // 
            this.flpTeam1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flpTeam1.AutoScroll = true;
            this.flpTeam1.Location = new System.Drawing.Point(0, 77);
            this.flpTeam1.Name = "flpTeam1";
            this.flpTeam1.Size = new System.Drawing.Size(878, 782);
            this.flpTeam1.TabIndex = 3;
            // 
            // lblRedTeam
            // 
            this.lblRedTeam.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.lblRedTeam.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblRedTeam.Font = new System.Drawing.Font("Segoe UI", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblRedTeam.ForeColor = System.Drawing.Color.LightGray;
            this.lblRedTeam.Location = new System.Drawing.Point(0, 0);
            this.lblRedTeam.Name = "lblRedTeam";
            this.lblRedTeam.Size = new System.Drawing.Size(876, 74);
            this.lblRedTeam.TabIndex = 2;
            this.lblRedTeam.Text = "Red Team";
            // 
            // lblPlayers
            // 
            this.lblPlayers.AutoSize = true;
            this.lblPlayers.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblPlayers.Location = new System.Drawing.Point(5, 44);
            this.lblPlayers.Name = "lblPlayers";
            this.lblPlayers.Size = new System.Drawing.Size(83, 30);
            this.lblPlayers.TabIndex = 3;
            this.lblPlayers.Text = "Players:";
            // 
            // lblPlayerCount
            // 
            this.lblPlayerCount.AutoSize = true;
            this.lblPlayerCount.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblPlayerCount.Location = new System.Drawing.Point(94, 44);
            this.lblPlayerCount.Name = "lblPlayerCount";
            this.lblPlayerCount.Size = new System.Drawing.Size(66, 30);
            this.lblPlayerCount.TabIndex = 4;
            this.lblPlayerCount.Text = "0 of 0";
            // 
            // cbGameMode
            // 
            this.cbGameMode.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(35)))));
            this.cbGameMode.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.cbGameMode.ForeColor = System.Drawing.Color.LightGray;
            this.cbGameMode.FormattingEnabled = true;
            this.cbGameMode.Location = new System.Drawing.Point(457, 3);
            this.cbGameMode.Name = "cbGameMode";
            this.cbGameMode.Size = new System.Drawing.Size(171, 38);
            this.cbGameMode.TabIndex = 5;
            // 
            // btnRefresh
            // 
            this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefresh.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(35)))));
            this.btnRefresh.ForeColor = System.Drawing.Color.LightGray;
            this.btnRefresh.Location = new System.Drawing.Point(1636, 3);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(78, 23);
            this.btnRefresh.TabIndex = 6;
            this.btnRefresh.Text = "Refresh all";
            this.btnRefresh.UseVisualStyleBackColor = false;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // scTeamsAndOffline
            // 
            this.scTeamsAndOffline.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scTeamsAndOffline.Location = new System.Drawing.Point(3, 77);
            this.scTeamsAndOffline.Name = "scTeamsAndOffline";
            this.scTeamsAndOffline.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // scTeamsAndOffline.Panel1
            // 
            this.scTeamsAndOffline.Panel1.Controls.Add(this.scTeam0AndTeam1);
            // 
            // scTeamsAndOffline.Panel2
            // 
            this.scTeamsAndOffline.Panel2.Controls.Add(this.flpOfflinePlayers);
            this.scTeamsAndOffline.Panel2.Controls.Add(this.label3);
            this.scTeamsAndOffline.Panel2Collapsed = true;
            this.scTeamsAndOffline.Size = new System.Drawing.Size(1714, 864);
            this.scTeamsAndOffline.SplitterDistance = 537;
            this.scTeamsAndOffline.TabIndex = 7;
            // 
            // flpOfflinePlayers
            // 
            this.flpOfflinePlayers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flpOfflinePlayers.AutoScroll = true;
            this.flpOfflinePlayers.Location = new System.Drawing.Point(0, 77);
            this.flpOfflinePlayers.Name = "flpOfflinePlayers";
            this.flpOfflinePlayers.Size = new System.Drawing.Size(1711, 246);
            this.flpOfflinePlayers.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.label3.Dock = System.Windows.Forms.DockStyle.Top;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label3.ForeColor = System.Drawing.Color.LightGray;
            this.label3.Location = new System.Drawing.Point(0, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(150, 74);
            this.label3.TabIndex = 2;
            this.label3.Text = "Banned / Previous players";
            // 
            // btnSkipToNextMap
            // 
            this.btnSkipToNextMap.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(35)))));
            this.btnSkipToNextMap.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnSkipToNextMap.ForeColor = System.Drawing.Color.LightGray;
            this.btnSkipToNextMap.Location = new System.Drawing.Point(744, 3);
            this.btnSkipToNextMap.Name = "btnSkipToNextMap";
            this.btnSkipToNextMap.Size = new System.Drawing.Size(184, 38);
            this.btnSkipToNextMap.TabIndex = 8;
            this.btnSkipToNextMap.Text = "Skip to next map";
            this.btnSkipToNextMap.UseVisualStyleBackColor = false;
            this.btnSkipToNextMap.Click += new System.EventHandler(this.btnSkipToNextMap_Click);
            // 
            // pbLoading
            // 
            this.pbLoading.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pbLoading.Image = global::PterodactylPavlovRconClient.Properties.Resources.spin;
            this.pbLoading.Location = new System.Drawing.Point(1392, 3);
            this.pbLoading.Name = "pbLoading";
            this.pbLoading.Size = new System.Drawing.Size(23, 23);
            this.pbLoading.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbLoading.TabIndex = 9;
            this.pbLoading.TabStop = false;
            // 
            // nudRefreshInterval
            // 
            this.nudRefreshInterval.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.nudRefreshInterval.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(35)))));
            this.nudRefreshInterval.ForeColor = System.Drawing.Color.LightGray;
            this.nudRefreshInterval.Location = new System.Drawing.Point(1545, 3);
            this.nudRefreshInterval.Maximum = new decimal(new int[] {
            30000,
            0,
            0,
            0});
            this.nudRefreshInterval.Minimum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.nudRefreshInterval.Name = "nudRefreshInterval";
            this.nudRefreshInterval.Size = new System.Drawing.Size(85, 23);
            this.nudRefreshInterval.TabIndex = 10;
            this.nudRefreshInterval.Value = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            // 
            // lblRefreshInterval
            // 
            this.lblRefreshInterval.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblRefreshInterval.AutoSize = true;
            this.lblRefreshInterval.ForeColor = System.Drawing.Color.LightGray;
            this.lblRefreshInterval.Location = new System.Drawing.Point(1421, 7);
            this.lblRefreshInterval.Name = "lblRefreshInterval";
            this.lblRefreshInterval.Size = new System.Drawing.Size(118, 15);
            this.lblRefreshInterval.TabIndex = 11;
            this.lblRefreshInterval.Text = "Refresh interval (ms):";
            // 
            // pbServerStatus
            // 
            this.pbServerStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pbServerStatus.Image = global::PterodactylPavlovRconClient.Properties.Resources.signal_online;
            this.pbServerStatus.Location = new System.Drawing.Point(1363, 3);
            this.pbServerStatus.Name = "pbServerStatus";
            this.pbServerStatus.Size = new System.Drawing.Size(23, 23);
            this.pbServerStatus.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbServerStatus.TabIndex = 12;
            this.pbServerStatus.TabStop = false;
            // 
            // lblRoundInfo
            // 
            this.lblRoundInfo.AutoSize = true;
            this.lblRoundInfo.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblRoundInfo.Location = new System.Drawing.Point(234, 44);
            this.lblRoundInfo.Name = "lblRoundInfo";
            this.lblRoundInfo.Size = new System.Drawing.Size(78, 30);
            this.lblRoundInfo.TabIndex = 13;
            this.lblRoundInfo.Text = "Round:";
            // 
            // lblRound
            // 
            this.lblRound.AutoSize = true;
            this.lblRound.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblRound.Location = new System.Drawing.Point(318, 44);
            this.lblRound.Name = "lblRound";
            this.lblRound.Size = new System.Drawing.Size(24, 30);
            this.lblRound.TabIndex = 14;
            this.lblRound.Text = "0";
            // 
            // btnToggleOfflinePlayers
            // 
            this.btnToggleOfflinePlayers.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnToggleOfflinePlayers.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(35)))));
            this.btnToggleOfflinePlayers.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnToggleOfflinePlayers.Location = new System.Drawing.Point(1363, 32);
            this.btnToggleOfflinePlayers.Name = "btnToggleOfflinePlayers";
            this.btnToggleOfflinePlayers.Size = new System.Drawing.Size(351, 38);
            this.btnToggleOfflinePlayers.TabIndex = 15;
            this.btnToggleOfflinePlayers.Text = "Toggle offline/banned players";
            this.btnToggleOfflinePlayers.UseVisualStyleBackColor = false;
            this.btnToggleOfflinePlayers.Click += new System.EventHandler(this.btnToggleOfflinePlayers_Click);
            // 
            // ServerControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(35)))));
            this.Controls.Add(this.btnToggleOfflinePlayers);
            this.Controls.Add(this.lblRound);
            this.Controls.Add(this.lblRoundInfo);
            this.Controls.Add(this.pbServerStatus);
            this.Controls.Add(this.lblRefreshInterval);
            this.Controls.Add(this.nudRefreshInterval);
            this.Controls.Add(this.pbLoading);
            this.Controls.Add(this.btnSkipToNextMap);
            this.Controls.Add(this.scTeamsAndOffline);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.cbGameMode);
            this.Controls.Add(this.lblPlayerCount);
            this.Controls.Add(this.lblPlayers);
            this.Controls.Add(this.btnSwitchMap);
            this.Controls.Add(this.cbMapList);
            this.ForeColor = System.Drawing.Color.LightGray;
            this.Name = "ServerControl";
            this.Size = new System.Drawing.Size(1717, 941);
            this.Load += new System.EventHandler(this.ServerControl_Load);
            this.scTeam0AndTeam1.Panel1.ResumeLayout(false);
            this.scTeam0AndTeam1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scTeam0AndTeam1)).EndInit();
            this.scTeam0AndTeam1.ResumeLayout(false);
            this.scTeamsAndOffline.Panel1.ResumeLayout(false);
            this.scTeamsAndOffline.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scTeamsAndOffline)).EndInit();
            this.scTeamsAndOffline.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbLoading)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRefreshInterval)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbServerStatus)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ComboBox cbMapList;
        private Button btnSwitchMap;
        private SplitContainer scTeam0AndTeam1;
        private Label lblPlayers;
        private Label lblPlayerCount;
        private ComboBox cbGameMode;
        private Button btnRefresh;
        private SplitContainer scTeamsAndOffline;
        private Label lblBlueTeam;
        private Label lblRedTeam;
        private Label label3;
        private ForceVerticalScrollFlowLayoutPanel flpTeam0;
        private ForceVerticalScrollFlowLayoutPanel flpTeam1;
        private ForceVerticalScrollFlowLayoutPanel flpOfflinePlayers;
        private Button btnSkipToNextMap;
        private PictureBox pbLoading;
        private NumericUpDown nudRefreshInterval;
        private Label lblRefreshInterval;
        private PictureBox pbServerStatus;
        private Label lblRoundInfo;
        private Label lblRound;
        private Button btnToggleOfflinePlayers;
    }
}
