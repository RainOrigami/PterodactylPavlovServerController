using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.RegularExpressions;

namespace PterodactylPavlovServerController.Pages.Stats
{
    public class StatsModel : PageModel
    {
        public string StatsContent = "";

        private static readonly Regex serverIdRegex = new Regex("^[a-z0-9]{8}$");

        public async Task<IActionResult> OnGet(string? server)
        {
            if (server is not { Length: 8 } || !StatsModel.serverIdRegex.IsMatch(server) || !System.IO.File.Exists($"stats/{server}.html"))
            {
                return this.LocalRedirect("/");
            }

            this.StatsContent = await System.IO.File.ReadAllTextAsync($"./stats/{server}.html");

            return this.Page();
        }
    }
}
