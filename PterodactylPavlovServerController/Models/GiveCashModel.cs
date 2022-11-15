using System.ComponentModel.DataAnnotations;

namespace PterodactylPavlovServerController.Models;

public class GiveCashModel
{
    [Required]
    public ulong? UniqueId { get; set; }

    [Required]
    public int? Amount { get; set; }
}
