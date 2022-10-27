using Microsoft.AspNetCore.Mvc;
using PterodactylPavlovServerController.Exceptions;
using PterodactylPavlovServerController.Models;
using PterodactylPavlovServerController.Services;
using System.Text.RegularExpressions;

namespace PterodactylPavlovServerController.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RconController : Controller
    {
        private readonly PavlovRconService pavlovRconService;

        public RconController(PavlovRconService pavlovRconService)
        {
            this.pavlovRconService = pavlovRconService;
        }

        [HttpGet("serverInfo")]
        public IActionResult GetServerInfo(string serverId)
        {
            try
            {
                return Ok(pavlovRconService.GetServerInfo(serverId));
            }
            catch (RconException)
            {
                return Problem("Error while trying to retrieve RCON data from server");
            }
            catch (Exception e)
            {
                return Problem(e.Message);
            }
        }

        [HttpGet("playerList")]
        public IActionResult GetPlayerList(string serverId)
        {
            try
            {
                return Ok(pavlovRconService.GetActivePlayers(serverId));
            }
            catch (RconException)
            {
                return Problem("Error while trying to retrieve RCON data from server");
            }
            catch (Exception e)
            {
                return Problem(e.Message);
            }
        }

        [HttpGet("player")]
        public IActionResult GetPlayer(string serverId, string uniqueId)
        {
            try
            {
                PlayerDetailModel? activePlayer = pavlovRconService.GetActivePlayerDetail(serverId, uniqueId);
                if (activePlayer is null)
                {
                    return ValidationProblem("Player is not active on this server");
                }
                return Ok(activePlayer);
            }
            catch (RconException)
            {
                return Problem("Error while trying to retrieve RCON data from server");
            }
            catch (Exception e)
            {
                return Problem(e.Message);
            }
        }

        [HttpPost("switchMap")]
        public IActionResult SwitchMap(string serverId, string mapId, string gameMode)
        {
            Match mapIdMatch = MapsController.mapIdRegex.Match(mapId);
            if (!mapIdMatch.Success)
            {
                return ValidationProblem("Map id must be in format UGC0000000000 or 0000000000");
            }

            try
            {
                pavlovRconService.SwitchMap(serverId, long.Parse(mapIdMatch.Groups["id"].Value), gameMode);
                return Ok();
            }
            catch (RconException)
            {
                return Problem("Failed to execute RCON command.");
            }
            catch (ArgumentException e)
            {
                return ValidationProblem(e.Message);
            }
            catch (Exception e)
            {
                return Problem(e.Message);
            }
        }

        [HttpPost("rotateMap")]
        public IActionResult RotateMap(string serverId)
        {
            try
            {
                pavlovRconService.RotateMap(serverId);
                return Ok();
            }
            catch (RconException)
            {
                return Problem("Failed to execute RCON command.");
            }
            catch (Exception e)
            {
                return Problem(e.Message);
            }
        }

        [HttpPost("kick")]
        public IActionResult KickPlayer(string serverId, string uniqueId)
        {
            try
            {
                pavlovRconService.KickPlayer(serverId, uniqueId);
                return Ok();
            }
            catch (RconException)
            {
                return Problem("Failed to execute RCON command.");
            }
            catch (Exception e)
            {
                return Problem(e.Message);
            }
        }

        [HttpPost("ban")]
        public IActionResult BanPlayer(string serverId, string uniqueId)
        {
            try
            {
                pavlovRconService.BanPlayer(serverId, uniqueId);
                return Ok();
            }
            catch (RconException)
            {
                return Problem("Failed to execute RCON command.");
            }
            catch (Exception e)
            {
                return Problem(e.Message);
            }
        }

        [HttpPost("unban")]
        public IActionResult UnbanPlayer(string serverId, string uniqueId)
        {
            try
            {
                pavlovRconService.UnbanPlayer(serverId, uniqueId);
                return Ok();
            }
            catch (RconException)
            {
                return Problem("Failed to execute RCON command.");
            }
            catch (Exception e)
            {
                return Problem(e.Message);
            }
        }

        [HttpGet("banlist")]
        public IActionResult Banlist(string serverId)
        {
            try
            {
                return Ok(pavlovRconService.Banlist(serverId));
            }
            catch (RconException)
            {
                return Problem("Error while trying to retrieve RCON data from server");
            }
            catch (Exception e)
            {
                return Problem(e.Message);
            }
        }
    }
}
