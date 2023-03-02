using Microsoft.Build.Framework;
using PavlovVR_Rcon.Models.Pavlov;

namespace PterodactylPavlovServerController.Models;

public class AddWarmupItemModel
{
    [Required]
    public Item? Item { get; set; }
}
