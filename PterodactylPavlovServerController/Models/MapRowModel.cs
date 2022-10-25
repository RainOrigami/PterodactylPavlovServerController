using GoogleSheetsWrapper;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace PterodactylPavlovServerController.Models
{
    public class MapRowModel : BaseRecord
    {
        [SheetField(ColumnID = 1, DisplayName = "URL")]
        public string? URL { get; set; }
        [SheetField(ColumnID = 2, DisplayName = "Page Title")]
        public string? PageTitle { get; set; }
        [SheetField(ColumnID = 3, DisplayName = "Map Name")]
        public string? MapName { get; set; }
        [SheetField(ColumnID = 4, DisplayName = "Game Mode")]
        public string? GameMode { get; set; }

        public bool IsValid => this.URL != null && mapIdRegex.IsMatch(this.URL) && this.PageTitle != null && !this.PageTitle.StartsWith("#N/A") && this.MapName != null && !this.MapName.StartsWith("#N/A") && !this.MapName.StartsWith("#VALUE") && this.GameMode != null;

        private static readonly Regex mapIdRegex = new Regex("^https://steamcommunity\\.com/sharedfiles/filedetails/\\?id=(?<MapID>\\d+)");

        public long MapId => !this.IsValid ? throw new Exception("Invalid map URL") : long.Parse(mapIdRegex.Match(this.URL!).Groups["MapID"].Value);

        public string MapRotationString => $"MapRotation=(MapId=\"UGC{this.MapId}\", GameMode=\"{this.GameMode}\")";

        [JsonConstructor]
        public MapRowModel()
        {

        }

        public MapRowModel(IList<object> row, int rowId, int minColumnId = 1)
            : base(row, rowId, minColumnId)
        {

        }
    }

    public class MapRepository : BaseRepository<MapRowModel>
    {
        public MapRepository()
        {

        }

        public MapRepository(SheetHelper<MapRowModel> sheetHelper) : base(sheetHelper)
        {

        }
    }
}
