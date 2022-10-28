namespace PterodactylPavlovRconClient
{
    partial class PavlovRcon
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PavlovRcon));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.pbLoading = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pbLoading)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1450, 903);
            this.tabControl1.TabIndex = 0;
            // 
            // pbLoading
            // 
            this.pbLoading.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.pbLoading.Image = global::PterodactylPavlovRconClient.Properties.Resources.spin;
            this.pbLoading.Location = new System.Drawing.Point(613, 344);
            this.pbLoading.Name = "pbLoading";
            this.pbLoading.Size = new System.Drawing.Size(192, 189);
            this.pbLoading.TabIndex = 1;
            this.pbLoading.TabStop = false;
            // 
            // PavlovRcon
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1450, 903);
            this.Controls.Add(this.pbLoading);
            this.Controls.Add(this.tabControl1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "PavlovRcon";
            this.Text = "Pavlov VR Rcon";
            this.Load += new System.EventHandler(this.PavlovRcon_Load);
            this.Resize += new System.EventHandler(this.PavlovRcon_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.pbLoading)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private TabControl tabControl1;
        private PictureBox pbLoading;
    }
}