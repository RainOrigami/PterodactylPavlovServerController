using PavlovVR_Rcon.Models.Pavlov;
using System.ComponentModel.DataAnnotations;

namespace PterodactylPavlovServerController.Models;

public class GiveVehicleModel
{
    [Required]
    public ulong? UniqueId { get; set; }

    [Required]
    public string? Vehicle { get; set; }
}
