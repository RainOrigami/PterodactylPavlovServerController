using PavlovVR_Rcon.Models.Pavlov;
using System.ComponentModel.DataAnnotations;

namespace PterodactylPavlovServerController.Models;

public class SetTeamSkinModel
{
    [Required]
    public int? TeamId { get; set; }

    [Required]
    public Skin? Skin { get; set; }
}
