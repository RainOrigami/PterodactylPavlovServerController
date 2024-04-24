using System.ComponentModel.DataAnnotations;

namespace PterodactylPavlovServerController.Models;

public class NotifyModel
{
    [Required]
    public ulong? UniqueId { get; set; }
    [Required]
    public string? Message { get; set; }
    public int? Duration { get; set; }
}
