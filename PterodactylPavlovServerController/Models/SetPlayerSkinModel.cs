using PavlovVR_Rcon.Models.Pavlov;
using System.ComponentModel.DataAnnotations;

namespace PterodactylPavlovServerController.Models;

public class SetPlayerSkinModel
{
    [Required]
    public ulong? UniqueId { get; set; }

    [Required]
    public Skin? Skin { get; set; }
}
