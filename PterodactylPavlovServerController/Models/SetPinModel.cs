using System.ComponentModel.DataAnnotations;

namespace PterodactylPavlovServerController.Models;

public class SetPinModel
{
    [StringLength(4, MinimumLength = 4)]
    public string? Pin { get; set; }

    public bool? Set { get; set; }
}
