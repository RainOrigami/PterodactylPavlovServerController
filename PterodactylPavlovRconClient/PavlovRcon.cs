using Microsoft.Extensions.Logging;
using PterodactylPavlovRconClient.Services;
using PterodactylPavlovServerDomain.Models;
using Timer = System.Windows.Forms.Timer;

namespace PterodactylPavlovRconClient
{
    public partial class PavlovRcon : Form
    {
        public PavlovRcon(PterodactylAPIService pterodactylAPIService, ILogger logger)
        {
            InitializeComponent();

            this.pterodactylAPIService = pterodactylAPIService;
            this.logger = logger;

            Timer garbageCollectHttpClients = new Timer();
            garbageCollectHttpClients.Interval = 5000;
            garbageCollectHttpClients.Tick += (s, e) => GC.Collect();
            garbageCollectHttpClients.Start();
        }

        private readonly PterodactylAPIService pterodactylAPIService;
        private readonly ILogger logger;

        private void PavlovRcon_Load(object sender, EventArgs e)
        {
            PavlovRcon_Resize(sender, e);

            ApiResponse<PterodactylServerModel[]> serverListResponse = pterodactylAPIService.GetServers();

            if (!serverListResponse.Success)
            {
                MessageBox.Show(serverListResponse.ErrorMessage, "Failed to retrieve server list", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            pbLoading.Visible = false;
            this.Controls.Remove(pbLoading);
            pbLoading = null;

            foreach (PterodactylServerModel server in serverListResponse.Data!)
            {
                TabPage tabPage = new TabPage(server.Name);
                tabPage.Controls.Add(new Controls.ServerControl(pterodactylAPIService, server, logger)
                {
                    Dock = DockStyle.Fill
                });
                tabControl1.TabPages.Add(tabPage);
            }
        }

        private void PavlovRcon_Resize(object sender, EventArgs e)
        {
            if (pbLoading is null)
            {
                return;
            }

            pbLoading.Location = new Point((this.Width / 2) - (pbLoading.Width / 2), (this.Height / 2) - (pbLoading.Height / 2));
        }
    }
}