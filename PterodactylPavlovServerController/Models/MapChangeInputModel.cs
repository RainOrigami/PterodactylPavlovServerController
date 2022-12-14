using PavlovVR_Rcon.Models.Pavlov;
using System.ComponentModel.DataAnnotations;

namespace PterodactylPavlovServerController.Models;

public class MapChangeInputModel
{
    [Required]
    public string? Map { get; set; }

    [Required]
    public GameMode? GameMode { get; set; }

    public bool IsWorkshopMap => this.Map != null && (this.Map.StartsWith("UGC") || this.Map.Contains("steamcommunity.com/sharedfiles/filedetails/?id=") || long.TryParse(this.Map, out _));

    public long? MapId
    {
        get
        {
            if (!this.IsWorkshopMap)
            {
                return null;
            }

            string mapIdString;
            if (this.Map!.StartsWith("UGC"))
            {
                mapIdString = this.Map[3..];
            }
            else if (this.Map.Contains("steamcommunity.com/sharedfiles/filedetails/?id="))
            {
                mapIdString = this.Map[(this.Map.IndexOf("?id=", StringComparison.Ordinal) + 4)..];
                if (mapIdString.Contains('&'))
                {
                    mapIdString = mapIdString[0..mapIdString.IndexOf("&")];
                }
            }
            else
            {
                mapIdString = this.Map;
            }

            if (!long.TryParse(mapIdString, out long mapId))
            {
                return null;
            }

            return mapId;
        }
    }

    public string? WorkshopUrl => this.MapId is { } mapId ? $"https://steamcommunity.com/sharedfiles/filedetails/?id={mapId}" : null;

    public string? MapLabel => this.IsWorkshopMap ? $"UGC{this.MapId}" : this.Map;
}
