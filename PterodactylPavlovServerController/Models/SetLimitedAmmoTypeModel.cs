using PavlovVR_Rcon.Models.Pavlov;
using System.ComponentModel.DataAnnotations;

namespace PterodactylPavlovServerController.Models;

public class SetLimitedAmmoTypeModel
{
    [Required]
    public AmmoType? AmmoType { get; set; }
}
