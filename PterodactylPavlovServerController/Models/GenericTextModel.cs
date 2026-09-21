using System.ComponentModel.DataAnnotations;

namespace PterodactylPavlovServerController.Models;

public class GenericTextModel
{
    [Required]
    public string? Value { get; set; }
}
