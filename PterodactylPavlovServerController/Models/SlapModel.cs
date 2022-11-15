using System.ComponentModel.DataAnnotations;

namespace PterodactylPavlovServerController.Models;

public class SlapModel
{
    [Required]
    public ulong? UniqueId { get; set; }

    [Required]
    public int? Amount { get; set; }
}
