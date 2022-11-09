﻿using Microsoft.AspNetCore.Mvc;
using PterodactylPavlovServerController.Services;

namespace PterodactylPavlovServerController.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SteamController : ControllerBase
{
    private readonly SteamService steamService;

    public SteamController(SteamService steamService)
    {
        this.steamService = steamService;
    }

    [HttpGet("username")]
    public IActionResult GetUsername(ulong steamId)
    {
        try
        {
            return this.Ok(this.steamService.GetUsername(steamId));
        }
        catch (Exception)
        {
            return this.Problem("Failed to retrieve user details");
        }
    }

    [HttpGet("bans")]
    public IActionResult GetUserBans(ulong steamId)
    {
        try
        {
            return this.Ok(this.steamService.GetBans(steamId));
        }
        catch (Exception)
        {
            return this.Problem("Failed to retrieve user bans");
        }
    }

    [HttpGet("summary")]
    public IActionResult GetUserSummary(ulong steamId)
    {
        try
        {
            return this.Ok(this.steamService.GetPlayerSummary(steamId));
        }
        catch (Exception)
        {
            return this.Problem("Failed to retrieve user summary");
        }
    }
}
