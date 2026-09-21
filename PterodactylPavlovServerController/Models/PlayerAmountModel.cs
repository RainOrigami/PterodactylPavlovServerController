using System.ComponentModel.DataAnnotations;

namespace PterodactylPavlovServerController.Models;

public class PlayerAmountModel
{
    [Required]
    public ulong? UniqueId { get; set; }

    [Required]
    public int? Amount { get; set; }
}
