using System.ComponentModel.DataAnnotations;

namespace PterodactylPavlovServerController.Models;

public class AddOrRemoveModModel
{
    [Required]
    [Display(Name = "Player")]
    public ulong? UniqueId { get; set; }

    [Required]
    public bool? Add { get; set; }
}
