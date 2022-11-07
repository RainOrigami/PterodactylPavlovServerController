using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PterodactylPavlovServerController.Exceptions;
using PterodactylPavlovServerController.Services;
using PterodactylPavlovServerDomain.Exceptions;
using PterodactylPavlovServerDomain.Models;
using System.Text.RegularExpressions;

namespace PterodactylPavlovServerController.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class MapsController : ControllerBase
    {
        private readonly PavlovServerService serverControlService;
        private readonly GoogleSheetService googleSheetService;
        private readonly SteamWorkshopService steamWorkshopService;

        public MapsController(PavlovServerService serverControlService, GoogleSheetService googleSheetService, SteamWorkshopService steamWorkshopService)
        {
            this.serverControlService = serverControlService;
            this.googleSheetService = googleSheetService;
            this.steamWorkshopService = steamWorkshopService;
        }

        [HttpPost("update")]
        public IActionResult Update(string serverId, string spreadsheetId, string tabName)
        {
            MapRowModel[] mapRows;

            try
            {
                mapRows = googleSheetService.GetDocumentRows<MapRowModel>(typeof(MapRepository), spreadsheetId, tabName);
            }
            catch (GoogleSheetsHeaderMismatch)
            {
                return ValidationProblem("The sheet header names are not matching the model.");
            }
            catch (Exception e)
            {
                return Problem(e.Message, title: "Google Sheets Service Error");
            }

            try
            {
                mapRows = serverControlService.UpdateMaps(serverId, mapRows);
            }
            catch (InvalidMapsException ime)
            {
                return ValidationProblem(JsonConvert.SerializeObject(ime.InvalidMaps), title: "Invalid Maps Error");
            }
            catch (Exception e)
            {
                return Problem(e.Message, title: "Server Control Service Error");
            }

            return Ok(mapRows);
        }

        internal static readonly Regex mapIdRegex = new Regex(@"(?<id>\d+)");

        [HttpGet("details")]
        public IActionResult GetDetails(string mapId)
        {
            Match mapIdMatch = mapIdRegex.Match(mapId);
            if (!mapIdMatch.Success)
            {
                return ValidationProblem("Map id must be in format UGC0000000000 or 0000000000");
            }

            try
            {
                return Ok(steamWorkshopService.GetMapDetail(long.Parse(mapIdMatch.Groups["id"].Value)));
            }
            catch (SteamWorkshopException)
            {
                return Problem("Error while retrieving map details from steam workshop");
            }
            catch (Exception e)
            {
                return Problem(e.Message);
            }
        }

        [HttpGet("current")]
        public IActionResult GetRotation(string serverId)
        {
            try
            {
                return Ok(serverControlService.GetCurrentMapRotation(serverId));
            }
            catch (Exception e)
            {
                return Problem(e.Message);
            }
        }
    }
}
