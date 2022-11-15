using PavlovVR_Rcon.Models.Pavlov;
using System.ComponentModel.DataAnnotations;

namespace PterodactylPavlovServerController.Models;

public class GiveItemModel
{
    [Required]
    public ulong? UniqueId { get; set; }

    [Required]
    public Item? Item { get; set; }
}
