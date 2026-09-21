using System.ComponentModel.DataAnnotations;

namespace PterodactylPavlovServerController.Models;

public class GiveAllModel
{
    [Required]
    public int? TeamId { get; set; }

    [Required]
    public string? Item { get; set; }
}
