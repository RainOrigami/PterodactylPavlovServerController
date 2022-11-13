using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PterodactylPavlovServerController.Exceptions;
using PterodactylPavlovServerController.Services;
using PterodactylPavlovServerDomain.Exceptions;
using PterodactylPavlovServerDomain.Models;

namespace PterodactylPavlovServerController.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MapsController : ControllerBase
{
    //internal static readonly Regex mapIdRegex = new(@"(?<id>\d+)");
    private readonly GoogleSheetService googleSheetService;
    private readonly PavlovServerService serverControlService;
    //private readonly SteamWorkshopService steamWorkshopService;

    public MapsController(PavlovServerService serverControlService, GoogleSheetService googleSheetService)
    {
        this.serverControlService = serverControlService;
        this.googleSheetService = googleSheetService;
    }

    [HttpPost("update")]
    public async Task<IActionResult> Update(string apiKey, string serverId, string spreadsheetId, string tabName)
    {
        GoogleSheetsMapRowModel[] mapRows;

        try
        {
            mapRows = this.googleSheetService.GetDocumentRows<GoogleSheetsMapRowModel>(typeof(MapRepository), spreadsheetId, tabName);
        }
        catch (GoogleSheetsHeaderMismatch)
        {
            return this.ValidationProblem("The sheet header names are not matching the model.");
        }
        catch (Exception e)
        {
            return this.Problem(e.Message, title: "Google Sheets Service Error");
        }

        try
        {
            mapRows = await this.serverControlService.UpdateMaps(apiKey, serverId, mapRows);
        }
        catch (InvalidMapsException ime)
        {
            return this.ValidationProblem(JsonConvert.SerializeObject(ime.InvalidMaps), title: "Invalid Maps Error");
        }
        catch (Exception e)
        {
            return this.Problem(e.Message, title: "Server Control Service Error");
        }

        return this.Ok(mapRows);
    }

    //[HttpGet("details")]
    //public IActionResult GetDetails(string mapId)
    //{
    //    Match mapIdMatch = MapsController.mapIdRegex.Match(mapId);
    //    if (!mapIdMatch.Success)
    //    {
    //        return this.ValidationProblem("Map id must be in format UGC0000000000 or 0000000000");
    //    }

    //    try
    //    {
    //        return this.Ok(this.steamWorkshopService.GetMapDetail(long.Parse(mapIdMatch.Groups["id"].Value)));
    //    }
    //    catch (SteamWorkshopException)
    //    {
    //        return this.Problem("Error while retrieving map details from steam workshop");
    //    }
    //    catch (Exception e)
    //    {
    //        return this.Problem(e.Message);
    //    }
    //}

    //[HttpGet("current")]
    //public IActionResult GetRotation(string serverId)
    //{
    //    try
    //    {
    //        return this.Ok(this.serverControlService.GetCurrentMapRotation(serverId));
    //    }
    //    catch (Exception e)
    //    {
    //        return this.Problem(e.Message);
    //    }
    //}
}
