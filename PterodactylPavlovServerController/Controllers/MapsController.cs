using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PterodactylPavlovServerController.Exceptions;
using PterodactylPavlovServerController.Models;
using PterodactylPavlovServerController.Services;

namespace PterodactylPavlovServerController.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MapsController : ControllerBase
    {
        private readonly ServerControlService serverControlService;
        private readonly GoogleSheetService googleSheetService;

        public MapsController(ServerControlService serverControlService, GoogleSheetService googleSheetService)
        {
            this.serverControlService = serverControlService;
            this.googleSheetService = googleSheetService;
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
    }
}
