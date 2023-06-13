using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using PavlovVR_Rcon.Models.Pavlov;
using PterodactylPavlovServerDomain.Exceptions;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace PterodactylPavlovServerController.Models;

public class MapChangeInputModel
{
    [Required]
    public string? Map { get; set; }

    [Required]
    public GameMode? GameMode { get; set; }

    public bool IsCustomMap => this.Map != null && long.TryParse(this.Map, out _);

    public long? MapId
    {
        get
        {
            if (!this.IsCustomMap)
            {
                return null;
            }

            if (!long.TryParse(this.Map, out long mapId))
            {
                return null;
            }

            return mapId;
        }
    }

    public string? MapLabel => this.IsCustomMap ? $"UGC{this.MapId}" : this.Map;
}
