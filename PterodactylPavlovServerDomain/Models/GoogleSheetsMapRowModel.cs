using GoogleSheetsWrapper;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace PterodactylPavlovServerDomain.Models;

public class GoogleSheetsMapRowModel : BaseRecord
{
    private static readonly Regex mapIdRegex = new("^https://steamcommunity\\.com/sharedfiles/filedetails/\\?id=(?<MapID>\\d+)");

    [JsonConstructor]
    public GoogleSheetsMapRowModel() { }

    public GoogleSheetsMapRowModel(IList<object> row, int rowId, int minColumnId = 1) : base(row, rowId, minColumnId) { }

    [SheetField(ColumnID = 1, DisplayName = "URL")]
    public string? URL { get; set; }

    [SheetField(ColumnID = 2, DisplayName = "Page Title")]
    public string? PageTitle { get; set; }

    [SheetField(ColumnID = 3, DisplayName = "Map Name")]
    public string? MapName { get; set; }

    [SheetField(ColumnID = 4, DisplayName = "Game Mode")]
    public string? GameMode { get; set; }

    public bool IsValid => this.URL != null && GoogleSheetsMapRowModel.mapIdRegex.IsMatch(this.URL) && this.PageTitle != null && !this.PageTitle.StartsWith("#N/A") && this.MapName != null && !this.MapName.StartsWith("#N/A") && !this.MapName.StartsWith("#VALUE") && this.GameMode != null;

    public long MapId => !this.IsValid ? throw new Exception("Invalid map URL") : long.Parse(GoogleSheetsMapRowModel.mapIdRegex.Match(this.URL!).Groups["MapID"].Value);

    public string MapRotationString => $"MapRotation=(MapId=\"UGC{this.MapId}\", GameMode=\"{this.GameMode}\")";
}

public class MapRepository : BaseRepository<GoogleSheetsMapRowModel>
{
    public MapRepository() { }

    public MapRepository(SheetHelper<GoogleSheetsMapRowModel> sheetHelper) : base(sheetHelper) { }
}
