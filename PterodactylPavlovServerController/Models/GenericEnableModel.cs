using System.ComponentModel.DataAnnotations;

namespace PterodactylPavlovServerController.Models;

public class GenericEnableModel
{
    [Required]
    public bool? Enable { get; set; }
}
