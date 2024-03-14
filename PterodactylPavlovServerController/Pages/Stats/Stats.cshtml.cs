using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PterodactylPavlovServerController.Services;
using System.Text.RegularExpressions;

namespace PterodactylPavlovServerController.Pages.Stats
{
    public class StatsModel : PageModel
    {
        public static readonly string ogdescriptionTag = "!!ogdescription!!: ";
        public string StatsContent = "";

        private static readonly Regex serverIdRegex = new Regex("^[a-z0-9]{8}$");
        private readonly PavlovServerService pavlovServerService;
        private readonly IConfiguration configuration;

        public StatsModel(PavlovServerService pavlovServerService, IConfiguration configuration)
        {
            this.pavlovServerService = pavlovServerService;
            this.configuration = configuration;
        }

        public async Task<IActionResult> OnGet(string? server)
        {
            if (server is not { Length: 8 } || !StatsModel.serverIdRegex.IsMatch(server) || !System.IO.File.Exists($"stats/{server}.html"))
            {
                return this.LocalRedirect("/");
            }

            this.StatsContent = await System.IO.File.ReadAllTextAsync($"./stats/{server}.html");

            ViewData["title"] = $"{await pavlovServerService.GetServerName(this.configuration["pterodactyl_stats_apikey"], server)} server stats";
            int ogDescriptionStart = this.StatsContent.IndexOf(ogdescriptionTag) + ogdescriptionTag.Length;
            ViewData["ogdescription"] = this.StatsContent[ogDescriptionStart..];

            this.StatsContent = this.StatsContent[..(ogDescriptionStart - ogdescriptionTag.Length)];

            return this.Page();
        }
    }
}
