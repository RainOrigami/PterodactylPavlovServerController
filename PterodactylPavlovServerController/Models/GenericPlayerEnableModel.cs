using System.ComponentModel.DataAnnotations;

namespace PterodactylPavlovServerController.Models;

public class GenericPlayerEnableModel
{
    [Required]
    public ulong? UniqueId { get; set; }
    [Required]
    public bool? Enable { get; set; } = false;
}
