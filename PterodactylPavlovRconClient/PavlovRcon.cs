using Newtonsoft.Json;
using PterodactylPavlovServerController.Models;
using RestSharp;

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
        }

        RestClient restClient;

        private void PavlovRcon_Load(object sender, EventArgs e)
        {
            RestRequest serverListRequest = new RestRequest("server/list");
            RestResponse serverListResponse = restClient.Execute(serverListRequest);
            if (serverListResponse.StatusCode is not System.Net.HttpStatusCode.OK)
            {
                MessageBox.Show(serverListResponse.Content, serverListResponse.StatusCode.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

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
    }
}