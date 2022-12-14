using System.ComponentModel.DataAnnotations;

namespace PterodactylPavlovServerController.Models;

public class SetMapRotationModel
{
    [Required]
    [Display(Name = "Rotation")]
    public string? Name { get; set; }
}
