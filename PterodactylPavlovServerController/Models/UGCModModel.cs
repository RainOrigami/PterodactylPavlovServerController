using System.ComponentModel.DataAnnotations;

namespace PterodactylPavlovServerController.Models;

public class UGCModModel
{
    [Required]
    public long? ModId { get; set; }
}
