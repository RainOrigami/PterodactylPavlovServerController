using System.ComponentModel.DataAnnotations;

namespace PterodactylPavlovServerController.Models;

public class WarpModel
{
    [Required]
    public ulong? SourceId { get; set; }
    [Required]
    public ulong? TargetId { get; set; }
}
