using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PterodactylPavlovServerController.Controllers;

[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class StatsController : ControllerBase
{
    [HttpGet]
    public IActionResult Index(string serverId)
    {
        if (!System.IO.File.Exists($"stats/{serverId}.html"))
        {
            return this.BadRequest("Server does not exist.");
        }

        return this.File(System.IO.File.ReadAllBytes($"stats/{serverId}.html"), "text/html; charset=utf-8");
    }
}
