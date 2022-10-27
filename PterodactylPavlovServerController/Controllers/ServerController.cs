using Microsoft.AspNetCore.Mvc;
using PterodactylPavlovServerController.Services;

namespace PterodactylPavlovServerController.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServerController : ControllerBase
    {
        private readonly PterodactylService pterodactylService;

        public ServerController(PterodactylService pterodactylService)
        {
            this.pterodactylService = pterodactylService;
        }

        [HttpGet("list")]
        public IActionResult GetServerList()
        {
            try
            {
                return Ok(pterodactylService.GetServers());
            }
            catch (Exception e)
            {
                return Problem(e.Message);
            }
        }
    }
}
