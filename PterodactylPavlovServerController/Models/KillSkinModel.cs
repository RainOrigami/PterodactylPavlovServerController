using Microsoft.Build.Framework;
using PavlovVR_Rcon.Models.Pavlov;

namespace PterodactylPavlovServerController.Models;

public class KillSkinModel
{
    [Required]
    public bool Enabled { get; set; } = false;
    [Required]
    public int Threshold { get; set; } = 15;
    [Required]
    public Skin? Skin { get; set; } = null;
}
