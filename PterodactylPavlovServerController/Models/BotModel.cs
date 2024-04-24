using System.ComponentModel.DataAnnotations;

namespace PterodactylPavlovServerController.Models;

public class BotModel
{
    [Required]
    public int? Amount { get; set; }

    [Required]
    public int? TeamId { get; set; }

    [Required]
    public bool? Add { get; set; }
}
