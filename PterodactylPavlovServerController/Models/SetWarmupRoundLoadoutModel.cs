using System.ComponentModel.DataAnnotations;

namespace PterodactylPavlovServerController.Models;

public class SetWarmupRoundLoadoutModel
{
    [Required]
    [Display(Name = "Loadout")]
    public string? Name { get; set; }
}
