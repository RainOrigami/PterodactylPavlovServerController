using System.ComponentModel.DataAnnotations;

namespace PterodactylPavlovServerController.Models;

public class SpeedModel
{
    [Required]
    public ulong? UniqueId { get; set; }
    [Required]
    public float? Multiplier { get; set; }
}
