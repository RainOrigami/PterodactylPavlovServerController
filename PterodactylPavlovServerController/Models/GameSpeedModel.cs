using System.ComponentModel.DataAnnotations;

namespace PterodactylPavlovServerController.Models;

public class GenericFloatModel
{
    [Required]
    public float? Value { get; set; }
}
