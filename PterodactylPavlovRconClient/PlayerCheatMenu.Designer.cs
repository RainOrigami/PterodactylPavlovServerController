namespace PterodactylPavlovRconClient
{
    partial class PlayerCheatMenu
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PlayerCheatMenu));
            this.lblPlayerName = new System.Windows.Forms.Label();
            this.cbItems = new System.Windows.Forms.ComboBox();
            this.btnGiveItem = new System.Windows.Forms.Button();
            this.nudCash = new System.Windows.Forms.NumericUpDown();
            this.btnGiveCash = new System.Windows.Forms.Button();
            this.cbVehicles = new System.Windows.Forms.ComboBox();
            this.btnGiveVehicle = new System.Windows.Forms.Button();
            this.nudSlap = new System.Windows.Forms.NumericUpDown();
            this.btnSlap = new System.Windows.Forms.Button();
            this.cbSkins = new System.Windows.Forms.ComboBox();
            this.btnSetSkin = new System.Windows.Forms.Button();
            this.btnSwitchTeam = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.nudCash)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSlap)).BeginInit();
            this.SuspendLayout();
            // 
            // lblPlayerName
            // 
            this.lblPlayerName.AutoSize = true;
            this.lblPlayerName.Font = new System.Drawing.Font("Segoe UI", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblPlayerName.Location = new System.Drawing.Point(12, 9);
            this.lblPlayerName.Name = "lblPlayerName";
            this.lblPlayerName.Size = new System.Drawing.Size(429, 40);
            this.lblPlayerName.TabIndex = 0;
            this.lblPlayerName.Text = "Some Guy with Very Long Name";
            // 
            // cbItems
            // 
            this.cbItems.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbItems.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbItems.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.cbItems.FormattingEnabled = true;
            this.cbItems.Location = new System.Drawing.Point(12, 65);
            this.cbItems.Name = "cbItems";
            this.cbItems.Size = new System.Drawing.Size(262, 38);
            this.cbItems.TabIndex = 1;
            // 
            // btnGiveItem
            // 
            this.btnGiveItem.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGiveItem.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnGiveItem.Location = new System.Drawing.Point(280, 65);
            this.btnGiveItem.Name = "btnGiveItem";
            this.btnGiveItem.Size = new System.Drawing.Size(161, 38);
            this.btnGiveItem.TabIndex = 2;
            this.btnGiveItem.Text = "Give Item";
            this.btnGiveItem.UseVisualStyleBackColor = true;
            this.btnGiveItem.Click += new System.EventHandler(this.btnGiveItem_Click);
            // 
            // nudCash
            // 
            this.nudCash.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudCash.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.nudCash.Location = new System.Drawing.Point(12, 109);
            this.nudCash.Maximum = new decimal(new int[] {
            15000,
            0,
            0,
            0});
            this.nudCash.Name = "nudCash";
            this.nudCash.Size = new System.Drawing.Size(262, 35);
            this.nudCash.TabIndex = 3;
            this.nudCash.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // btnGiveCash
            // 
            this.btnGiveCash.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGiveCash.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnGiveCash.Location = new System.Drawing.Point(280, 109);
            this.btnGiveCash.Name = "btnGiveCash";
            this.btnGiveCash.Size = new System.Drawing.Size(161, 35);
            this.btnGiveCash.TabIndex = 2;
            this.btnGiveCash.Text = "Give cash";
            this.btnGiveCash.UseVisualStyleBackColor = true;
            this.btnGiveCash.Click += new System.EventHandler(this.btnGiveCash_Click);
            // 
            // cbVehicles
            // 
            this.cbVehicles.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbVehicles.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbVehicles.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.cbVehicles.FormattingEnabled = true;
            this.cbVehicles.Location = new System.Drawing.Point(12, 150);
            this.cbVehicles.Name = "cbVehicles";
            this.cbVehicles.Size = new System.Drawing.Size(262, 38);
            this.cbVehicles.TabIndex = 1;
            // 
            // btnGiveVehicle
            // 
            this.btnGiveVehicle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGiveVehicle.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnGiveVehicle.Location = new System.Drawing.Point(280, 150);
            this.btnGiveVehicle.Name = "btnGiveVehicle";
            this.btnGiveVehicle.Size = new System.Drawing.Size(161, 38);
            this.btnGiveVehicle.TabIndex = 2;
            this.btnGiveVehicle.Text = "Give vehicle";
            this.btnGiveVehicle.UseVisualStyleBackColor = true;
            this.btnGiveVehicle.Click += new System.EventHandler(this.btnGiveVehicle_Click);
            // 
            // nudSlap
            // 
            this.nudSlap.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudSlap.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.nudSlap.Location = new System.Drawing.Point(12, 238);
            this.nudSlap.Name = "nudSlap";
            this.nudSlap.Size = new System.Drawing.Size(262, 35);
            this.nudSlap.TabIndex = 3;
            this.nudSlap.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            // 
            // btnSlap
            // 
            this.btnSlap.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSlap.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnSlap.Location = new System.Drawing.Point(280, 238);
            this.btnSlap.Name = "btnSlap";
            this.btnSlap.Size = new System.Drawing.Size(161, 35);
            this.btnSlap.TabIndex = 2;
            this.btnSlap.Text = "Slap";
            this.btnSlap.UseVisualStyleBackColor = true;
            this.btnSlap.Click += new System.EventHandler(this.btnSlap_Click);
            // 
            // cbSkins
            // 
            this.cbSkins.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbSkins.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbSkins.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.cbSkins.FormattingEnabled = true;
            this.cbSkins.Location = new System.Drawing.Point(12, 194);
            this.cbSkins.Name = "cbSkins";
            this.cbSkins.Size = new System.Drawing.Size(262, 38);
            this.cbSkins.TabIndex = 1;
            // 
            // btnSetSkin
            // 
            this.btnSetSkin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSetSkin.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnSetSkin.Location = new System.Drawing.Point(280, 194);
            this.btnSetSkin.Name = "btnSetSkin";
            this.btnSetSkin.Size = new System.Drawing.Size(161, 38);
            this.btnSetSkin.TabIndex = 2;
            this.btnSetSkin.Text = "Set skin";
            this.btnSetSkin.UseVisualStyleBackColor = true;
            this.btnSetSkin.Click += new System.EventHandler(this.btnSetSkin_Click);
            // 
            // btnSwitchTeam
            // 
            this.btnSwitchTeam.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSwitchTeam.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnSwitchTeam.Location = new System.Drawing.Point(12, 279);
            this.btnSwitchTeam.Name = "btnSwitchTeam";
            this.btnSwitchTeam.Size = new System.Drawing.Size(429, 38);
            this.btnSwitchTeam.TabIndex = 4;
            this.btnSwitchTeam.Text = "Switch team";
            this.btnSwitchTeam.UseVisualStyleBackColor = true;
            this.btnSwitchTeam.Click += new System.EventHandler(this.btnSwitchTeam_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnClose.Location = new System.Drawing.Point(278, 323);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(161, 38);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // PlayerCheatMenu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(35)))));
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(451, 369);
            this.Controls.Add(this.btnSwitchTeam);
            this.Controls.Add(this.nudSlap);
            this.Controls.Add(this.nudCash);
            this.Controls.Add(this.btnGiveCash);
            this.Controls.Add(this.btnSlap);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnSetSkin);
            this.Controls.Add(this.btnGiveVehicle);
            this.Controls.Add(this.btnGiveItem);
            this.Controls.Add(this.cbSkins);
            this.Controls.Add(this.cbVehicles);
            this.Controls.Add(this.cbItems);
            this.Controls.Add(this.lblPlayerName);
            this.ForeColor = System.Drawing.Color.LightGray;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PlayerCheatMenu";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Player Cheat Menu";
            ((System.ComponentModel.ISupportInitialize)(this.nudCash)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSlap)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Label lblPlayerName;
        private ComboBox cbItems;
        private Button btnGiveItem;
        private NumericUpDown nudCash;
        private Button btnGiveCash;
        private ComboBox cbVehicles;
        private Button btnGiveVehicle;
        private NumericUpDown nudSlap;
        private Button btnSlap;
        private ComboBox cbSkins;
        private Button btnSetSkin;
        private Button btnSwitchTeam;
        private Button btnClose;
    }
}