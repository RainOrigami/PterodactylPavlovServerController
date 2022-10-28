using Newtonsoft.Json;
using PterodactylPavlovServerController.Models;
using RestSharp;
using Timer = System.Windows.Forms.Timer;

namespace PterodactylPavlovRconClient
{
    public partial class PavlovRcon : Form
    {
        public PavlovRcon()
        {
            InitializeComponent();

            restClient = new RestClient(Properties.Settings.Default.ppsc_basepath);
            restClient.AddDefaultHeader("x-api-key", Properties.Settings.Default.ppsc_apikey);
            restClient.AddDefaultHeader("x-pterodactyl-api-key", Properties.Settings.Default.ppsc_pterodactyl_key);

            Timer garbageCollectHttpClients = new Timer();
            garbageCollectHttpClients.Interval = 5000;
            garbageCollectHttpClients.Tick += (s, e) => GC.Collect();
            garbageCollectHttpClients.Start();
        }

        RestClient restClient;

        private async void PavlovRcon_Load(object sender, EventArgs e)
        {
            PavlovRcon_Resize(sender, e);

            RestResponse serverListResponse = await restClient.ExecuteAsync(new RestRequest("server/list"));
            if (serverListResponse.StatusCode is not System.Net.HttpStatusCode.OK)
            {
                MessageBox.Show(serverListResponse.Content, serverListResponse.StatusCode.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            pbLoading.Visible = false;
            this.Controls.Remove(pbLoading);
            pbLoading = null;

            foreach (PterodactylServerModel server in JsonConvert.DeserializeObject<PterodactylServerModel[]>(serverListResponse.Content!)!)
            {
                TabPage tabPage = new TabPage(server.Name);
                tabPage.Controls.Add(new ServerControl(restClient, server)
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