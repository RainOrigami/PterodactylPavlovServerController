using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PterodactylPavlovServerController.Pages.Rcon;
using PterodactylPavlovServerController.Pages.Stats;
using PterodactylPavlovServerController.Services;
using System.Text;
using System.Text.RegularExpressions;

namespace PterodactylPavlovServerController.Pages.Demos
{
    public partial class DemosModel : PageModel
    {
        private readonly PavlovServerService pavlovServerService;
        private readonly PterodactylService pterodactylService;
        private readonly IConfiguration configuration;

        public DemosModel(PavlovServerService pavlovServerService, PterodactylService pterodactylService, IConfiguration configuration)
        {
            this.pavlovServerService = pavlovServerService;
            this.pterodactylService = pterodactylService;
            this.configuration = configuration;
        }

        public async Task<IActionResult> OnGet(string server, string name)
        {
            if (server is not { Length: 8 } || !StatsModel.ServerIdRegex.IsMatch(server)
                || name is null || name.Length > 96 || !DemosNameRegex().IsMatch(name))
            {
                return this.LocalRedirect("/");
            }

            if (!Directory.Exists("demos"))
            {
                Directory.CreateDirectory("demos");
            }

            if (!System.IO.File.Exists($"demos/{name}"))
            {
                if (!this.pterodactylService.FileList(this.configuration["pterodactyl_stats_apikey"]!, server, this.configuration["pavlov_demospath"]!).Contains(name))
                {
                    return this.LocalRedirect("/");
                }

                try
                {
                    byte[] demoFileContent = await this.pterodactylService.ReadFileUnsafe(this.configuration["pterodactyl_stats_apikey"]!, server, $"{this.configuration["pavlov_demospath"]!}/{name}");

                    await System.IO.File.WriteAllBytesAsync($"demos/{name}", demoFileContent);
                }
                catch
                {
                    return this.LocalRedirect("/");
                }
            }

            return this.PhysicalFile(Path.GetFullPath($"demos/{name}"), "application/octet-stream", name);
        }

        [GeneratedRegex("^[a-zA-Z0-9\\-\\.]+$", RegexOptions.Compiled)]
        private static partial Regex DemosNameRegex();
    }
}
