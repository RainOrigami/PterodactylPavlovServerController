using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PterodactylPavlovServerController.Controllers.Api
{
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
                return BadRequest("Server does not exist.");
            }

            return File(System.IO.File.ReadAllBytes($"stats/{serverId}.html"), "text/html; charset=utf-8");
        }
    }
}
