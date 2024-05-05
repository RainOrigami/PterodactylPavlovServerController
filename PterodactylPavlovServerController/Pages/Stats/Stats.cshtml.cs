using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PterodactylPavlovServerController.Services;
using PterodactylPavlovServerDomain.Models;
using System.Text;
using System.Text.RegularExpressions;

namespace PterodactylPavlovServerController.Pages.Stats
{
    public class StatsModel : PageModel
    {
        public static readonly string ogdescriptionTag = "!!ogdescription!!: ";
        public static readonly string demosTag = "!!demos!!";
        public string StatsContent = "";

        public static readonly Regex ServerIdRegex = new Regex("^[a-z0-9]{8}$");
        private readonly PavlovServerService pavlovServerService;
        private readonly PterodactylService pterodactylService;
        private readonly IConfiguration configuration;

        public StatsModel(PavlovServerService pavlovServerService, PterodactylService pterodactylService, IConfiguration configuration)
        {
            this.pavlovServerService = pavlovServerService;
            this.pterodactylService = pterodactylService;
            this.configuration = configuration;
        }

        public async Task<IActionResult> OnGet(string? server)
        {
            if (server is not { Length: 8 } || !StatsModel.ServerIdRegex.IsMatch(server) || !System.IO.File.Exists($"stats/{server}.html"))
            {
                return this.LocalRedirect("/");
            }

            this.StatsContent = await System.IO.File.ReadAllTextAsync($"./stats/{server}.html");

            ViewData["title"] = $"{await pavlovServerService.GetServerName(this.configuration["pterodactyl_stats_apikey"]!, server)} server stats";
            int ogDescriptionStart = this.StatsContent.IndexOf(ogdescriptionTag) + ogdescriptionTag.Length;
            ViewData["ogdescription"] = this.StatsContent[ogDescriptionStart..];
            this.StatsContent = this.StatsContent[..(ogDescriptionStart - ogdescriptionTag.Length)];

            PterodactylFile[] demoFiles = pterodactylService.GetFileList(this.configuration["pterodactyl_stats_apikey"]!, server, this.configuration["pavlov_demospath"]!).OrderByDescending(d => d.ModifiedAt).Skip(1).Where(d => d.Name.Contains("SND") && d.Size < 300 * 1024 * 1024).Take(9).ToArray();
            StringBuilder demos = new();
            foreach (PterodactylFile demo in demoFiles)
            {
                string downloadPath = Url.Content($"~/Demos/{server}/{demo.Name}");
                demos.AppendLine(@$"<div class=""col-auto mt-3 me-3"" id=""demo-{demo.Name}"">
                    <div class=""card bg-dark h-100"" style=""width: 300px"">
                        <a href=""{downloadPath}"">
                            <img class=""card-img-top"" src=""https://bloodisgood.org/wp-content/uploads/2024/05/pavtv.png"" width=""300"" height=""300"" loading=""lazy"" />
                        </a>
                        <div class=""card-body px-0"">
                            <h5 class=""card-title px-3"">
                                <a href=""{downloadPath}"">
                                    {demo.Name}
                                </a>
                            </h5>
                        </div>
                    </div>
                </div>");
            }
            this.StatsContent = this.StatsContent.Replace(StatsModel.demosTag, demos.ToString());

            return this.Page();
        }
    }
}
