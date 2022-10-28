namespace PterodactylPavlovRconClient
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.flpTeam0 = new PterodactylPavlovRconClient.ForceVerticalScrollFlowLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.flpTeam1 = new PterodactylPavlovRconClient.ForceVerticalScrollFlowLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.lblPlayers = new System.Windows.Forms.Label();
            this.lblPlayerCount = new System.Windows.Forms.Label();
            this.cbGameMode = new System.Windows.Forms.ComboBox();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.flpOfflinePlayers = new PterodactylPavlovRconClient.ForceVerticalScrollFlowLayoutPanel();
            this.label3 = new System.Windows.Forms.Label();
            this.btnSkipToNextMap = new System.Windows.Forms.Button();
            this.pbLoading = new System.Windows.Forms.PictureBox();
            this.nudRefreshInterval = new System.Windows.Forms.NumericUpDown();
            this.lblRefreshInterval = new System.Windows.Forms.Label();
            this.pbServerStatus = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbLoading)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRefreshInterval)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbServerStatus)).BeginInit();
            this.SuspendLayout();
            // 
            // cbMapList
            // 
            this.cbMapList.FormattingEnabled = true;
            this.cbMapList.Location = new System.Drawing.Point(3, 3);
            this.cbMapList.Name = "cbMapList";
            this.cbMapList.Size = new System.Drawing.Size(159, 23);
            this.cbMapList.TabIndex = 0;
            // 
            // btnSwitchMap
            // 
            this.btnSwitchMap.Location = new System.Drawing.Point(321, 3);
            this.btnSwitchMap.Name = "btnSwitchMap";
            this.btnSwitchMap.Size = new System.Drawing.Size(91, 23);
            this.btnSwitchMap.TabIndex = 1;
            this.btnSwitchMap.Text = "Switch map";
            this.btnSwitchMap.UseVisualStyleBackColor = true;
            this.btnSwitchMap.Click += new System.EventHandler(this.btnSwitchMap_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.flpTeam0);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.flpTeam1);
            this.splitContainer1.Panel2.Controls.Add(this.label2);
            this.splitContainer1.Size = new System.Drawing.Size(1714, 557);
            this.splitContainer1.SplitterDistance = 830;
            this.splitContainer1.TabIndex = 2;
            // 
            // flpTeam0
            // 
            this.flpTeam0.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flpTeam0.AutoScroll = true;
            this.flpTeam0.Location = new System.Drawing.Point(-2, 77);
            this.flpTeam0.Name = "flpTeam0";
            this.flpTeam0.Size = new System.Drawing.Size(828, 475);
            this.flpTeam0.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.RoyalBlue;
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(826, 74);
            this.label1.TabIndex = 1;
            this.label1.Text = "Blue Team";
            // 
            // flpTeam1
            // 
            this.flpTeam1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flpTeam1.AutoScroll = true;
            this.flpTeam1.Location = new System.Drawing.Point(0, 77);
            this.flpTeam1.Name = "flpTeam1";
            this.flpTeam1.Size = new System.Drawing.Size(878, 475);
            this.flpTeam1.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.Color.Firebrick;
            this.label2.Dock = System.Windows.Forms.DockStyle.Top;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(876, 74);
            this.label2.TabIndex = 2;
            this.label2.Text = "Red Team";
            // 
            // lblPlayers
            // 
            this.lblPlayers.AutoSize = true;
            this.lblPlayers.Location = new System.Drawing.Point(3, 29);
            this.lblPlayers.Name = "lblPlayers";
            this.lblPlayers.Size = new System.Drawing.Size(47, 15);
            this.lblPlayers.TabIndex = 3;
            this.lblPlayers.Text = "Players:";
            // 
            // lblPlayerCount
            // 
            this.lblPlayerCount.AutoSize = true;
            this.lblPlayerCount.Location = new System.Drawing.Point(56, 29);
            this.lblPlayerCount.Name = "lblPlayerCount";
            this.lblPlayerCount.Size = new System.Drawing.Size(36, 15);
            this.lblPlayerCount.TabIndex = 4;
            this.lblPlayerCount.Text = "0 of 0";
            // 
            // cbGameMode
            // 
            this.cbGameMode.FormattingEnabled = true;
            this.cbGameMode.Location = new System.Drawing.Point(168, 3);
            this.cbGameMode.Name = "cbGameMode";
            this.cbGameMode.Size = new System.Drawing.Size(147, 23);
            this.cbGameMode.TabIndex = 5;
            // 
            // btnRefresh
            // 
            this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefresh.Location = new System.Drawing.Point(1636, 3);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(78, 23);
            this.btnRefresh.TabIndex = 6;
            this.btnRefresh.Text = "Refresh all";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // splitContainer2
            // 
            this.splitContainer2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer2.Location = new System.Drawing.Point(3, 47);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.splitContainer1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.flpOfflinePlayers);
            this.splitContainer2.Panel2.Controls.Add(this.label3);
            this.splitContainer2.Size = new System.Drawing.Size(1714, 894);
            this.splitContainer2.SplitterDistance = 557;
            this.splitContainer2.TabIndex = 7;
            // 
            // flpOfflinePlayers
            // 
            this.flpOfflinePlayers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flpOfflinePlayers.AutoScroll = true;
            this.flpOfflinePlayers.Location = new System.Drawing.Point(0, 77);
            this.flpOfflinePlayers.Name = "flpOfflinePlayers";
            this.flpOfflinePlayers.Size = new System.Drawing.Size(1711, 256);
            this.flpOfflinePlayers.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.BackColor = System.Drawing.Color.DimGray;
            this.label3.Dock = System.Windows.Forms.DockStyle.Top;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(0, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(1714, 74);
            this.label3.TabIndex = 2;
            this.label3.Text = "Banned / Previous players";
            // 
            // btnSkipToNextMap
            // 
            this.btnSkipToNextMap.Location = new System.Drawing.Point(418, 3);
            this.btnSkipToNextMap.Name = "btnSkipToNextMap";
            this.btnSkipToNextMap.Size = new System.Drawing.Size(109, 23);
            this.btnSkipToNextMap.TabIndex = 8;
            this.btnSkipToNextMap.Text = "Skip to next map";
            this.btnSkipToNextMap.UseVisualStyleBackColor = true;
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
            this.nudRefreshInterval.ValueChanged += new System.EventHandler(this.nudRefreshInterval_ValueChanged);
            // 
            // lblRefreshInterval
            // 
            this.lblRefreshInterval.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblRefreshInterval.AutoSize = true;
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
            // ServerControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pbServerStatus);
            this.Controls.Add(this.lblRefreshInterval);
            this.Controls.Add(this.nudRefreshInterval);
            this.Controls.Add(this.pbLoading);
            this.Controls.Add(this.btnSkipToNextMap);
            this.Controls.Add(this.splitContainer2);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.cbGameMode);
            this.Controls.Add(this.lblPlayerCount);
            this.Controls.Add(this.lblPlayers);
            this.Controls.Add(this.btnSwitchMap);
            this.Controls.Add(this.cbMapList);
            this.Name = "ServerControl";
            this.Size = new System.Drawing.Size(1717, 941);
            this.Load += new System.EventHandler(this.ServerControl_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbLoading)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRefreshInterval)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbServerStatus)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ComboBox cbMapList;
        private Button btnSwitchMap;
        private SplitContainer splitContainer1;
        private Label lblPlayers;
        private Label lblPlayerCount;
        private ComboBox cbGameMode;
        private Button btnRefresh;
        private SplitContainer splitContainer2;
        private Label label1;
        private Label label2;
        private Label label3;
        private ForceVerticalScrollFlowLayoutPanel flpTeam0;
        private ForceVerticalScrollFlowLayoutPanel flpTeam1;
        private ForceVerticalScrollFlowLayoutPanel flpOfflinePlayers;
        private Button btnSkipToNextMap;
        private PictureBox pbLoading;
        private NumericUpDown nudRefreshInterval;
        private Label lblRefreshInterval;
        private PictureBox pbServerStatus;
    }
}
