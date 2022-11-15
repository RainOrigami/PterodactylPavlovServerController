using System.ComponentModel.DataAnnotations;

namespace PterodactylPavlovServerController.Models;

public class GiveTeamCashModel
{
    [Required]
    public int? TeamId { get; set; }

    [Required]
    public int? Amount { get; set; }
}
