using Microsoft.AspNetCore.Mvc;
using PavlovVR_Rcon.Exceptions;
using PavlovVR_Rcon.Models.Pavlov;
using PterodactylPavlovServerController.Services;

namespace PterodactylPavlovServerController.Controllers;

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
            return this.Ok(this.pavlovRconService.GetServerInfo(serverId));
        }
        catch (RconException)
        {
            return this.Problem("Error while trying to retrieve RCON data from server");
        }
        catch (Exception e)
        {
            return this.Problem(e.Message);
        }
    }

    [HttpGet("playerList")]
    public IActionResult GetPlayerList(string serverId)
    {
        try
        {
            return this.Ok(this.pavlovRconService.GetActivePlayers(serverId));
        }
        catch (RconException)
        {
            return this.Problem("Error while trying to retrieve RCON data from server");
        }
        catch (Exception e)
        {
            return this.Problem(e.Message);
        }
    }

    [HttpGet("player")]
    public async Task<IActionResult> GetPlayer(string serverId, ulong uniqueId)
    {
        try
        {
            PavlovPlayerDetail activePlayer = await this.pavlovRconService.GetActivePlayerDetail(serverId, uniqueId);
            return this.Ok(activePlayer);
        }
        catch (RconException)
        {
            return this.Problem("Error while trying to retrieve RCON data from server");
        }
        catch (Exception e)
        {
            return this.Problem(e.Message);
        }
    }

    [HttpPost("switchMap")]
    public async Task<IActionResult> SwitchMap(string serverId, string mapId, string gameMode)
    {
        try
        {
            await this.pavlovRconService.SwitchMap(serverId, mapId, gameMode);
            return this.Ok();
        }
        catch (RconException)
        {
            return this.Problem("Failed to execute RCON command.");
        }
        catch (ArgumentException e)
        {
            return this.ValidationProblem(e.Message);
        }
        catch (Exception e)
        {
            return this.Problem(e.Message);
        }
    }

    [HttpPost("rotateMap")]
    public IActionResult RotateMap(string serverId)
    {
        try
        {
            this.pavlovRconService.RotateMap(serverId);
            return this.Ok();
        }
        catch (RconException)
        {
            return this.Problem("Failed to execute RCON command.");
        }
        catch (Exception e)
        {
            return this.Problem(e.Message);
        }
    }

    [HttpPost("kick")]
    public IActionResult KickPlayer(string serverId, string uniqueId)
    {
        try
        {
            this.pavlovRconService.KickPlayer(serverId, uniqueId);
            return this.Ok();
        }
        catch (RconException)
        {
            return this.Problem("Failed to execute RCON command.");
        }
        catch (Exception e)
        {
            return this.Problem(e.Message);
        }
    }

    [HttpPost("ban")]
    public IActionResult BanPlayer(string serverId, string uniqueId)
    {
        try
        {
            this.pavlovRconService.BanPlayer(serverId, uniqueId);
            return this.Ok();
        }
        catch (RconException)
        {
            return this.Problem("Failed to execute RCON command.");
        }
        catch (Exception e)
        {
            return this.Problem(e.Message);
        }
    }

    [HttpPost("unban")]
    public IActionResult UnbanPlayer(string serverId, string uniqueId)
    {
        try
        {
            this.pavlovRconService.UnbanPlayer(serverId, uniqueId);
            return this.Ok();
        }
        catch (RconException)
        {
            return this.Problem("Failed to execute RCON command.");
        }
        catch (Exception e)
        {
            return this.Problem(e.Message);
        }
    }

    [HttpGet("banlist")]
    public IActionResult Banlist(string serverId)
    {
        try
        {
            return this.Ok(this.pavlovRconService.Banlist(serverId));
        }
        catch (RconException)
        {
            return this.Problem("Error while trying to retrieve RCON data from server");
        }
        catch (Exception e)
        {
            return this.Problem(e.Message);
        }
    }

    [HttpPost("giveItem")]
    public IActionResult GiveItem(string serverId, string uniqueId, string item)
    {
        try
        {
            this.pavlovRconService.GiveItem(serverId, uniqueId, item);
            return this.Ok();
        }
        catch (RconException)
        {
            return this.Problem("Failed to execute RCON command.");
        }
        catch (Exception e)
        {
            return this.Problem(e.Message);
        }
    }

    [HttpPost("giveCash")]
    public IActionResult GiveCash(string serverId, string uniqueId, int amount)
    {
        try
        {
            this.pavlovRconService.GiveCash(serverId, uniqueId, amount);
            return this.Ok();
        }
        catch (RconException)
        {
            return this.Problem("Failed to execute RCON command.");
        }
        catch (Exception e)
        {
            return this.Problem(e.Message);
        }
    }

    [HttpPost("giveVehicle")]
    public IActionResult GiveVehicle(string serverId, string uniqueId, string vehicle)
    {
        try
        {
            this.pavlovRconService.GiveVehicle(serverId, uniqueId, vehicle);
            return this.Ok();
        }
        catch (RconException)
        {
            return this.Problem("Failed to execute RCON command.");
        }
        catch (Exception e)
        {
            return this.Problem(e.Message);
        }
    }

    [HttpPost("setSkin")]
    public IActionResult SetSkin(string serverId, string uniqueId, string skin)
    {
        try
        {
            this.pavlovRconService.SetSkin(serverId, uniqueId, skin);
            return this.Ok();
        }
        catch (RconException)
        {
            return this.Problem("Failed to execute RCON command.");
        }
        catch (Exception e)
        {
            return this.Problem(e.Message);
        }
    }

    [HttpPost("slap")]
    public IActionResult DoSlap(string serverId, string uniqueId, int amount)
    {
        try
        {
            this.pavlovRconService.Slap(serverId, uniqueId, amount);
            return this.Ok();
        }
        catch (RconException)
        {
            return this.Problem("Failed to execute RCON command.");
        }
        catch (Exception e)
        {
            return this.Problem(e.Message);
        }
    }

    [HttpPost("switchTeam")]
    public IActionResult SwitchTeam(string serverId, string uniqueId, int team)
    {
        try
        {
            this.pavlovRconService.SwitchTeam(serverId, uniqueId, team);
            return this.Ok();
        }
        catch (RconException)
        {
            return this.Problem("Failed to execute RCON command.");
        }
        catch (Exception e)
        {
            return this.Problem(e.Message);
        }
    }
}
