using System.ComponentModel.DataAnnotations;

namespace PterodactylPavlovServerController.Models;

public class VitalityModel
{
    [Required]
    public ulong? UniqueId { get; set; }

    [Required]
    public int Health { get; set; } = -1;

    [Required]
    public int Armor { get; set; } = -1;

    [Required]
    public int Helmet { get; set; } = -1;
}
