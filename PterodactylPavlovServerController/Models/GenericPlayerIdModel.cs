using System.ComponentModel.DataAnnotations;

namespace PterodactylPavlovServerController.Models;

public class GenericPlayerIdModel
{
    [Required]
    public ulong? UniqueId { get; set; }
}
