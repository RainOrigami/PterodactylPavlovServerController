using System.ComponentModel.DataAnnotations;

namespace PterodactylPavlovServerController.Models;

public class TeleportModel
{
    [Required]
    public ulong? SourceUniqueId { get; set; }

    [Required]
    public ulong? TargetUniqueId { get; set; }
}
