using PterodactylPavlovServerDomain.Rcon.Commands;
using System.ComponentModel.DataAnnotations;

namespace PterodactylPavlovServerController.Models;

public class CleanUpModel
{
    [Required]
    public RconObjectType? ObjectType { get; set; }
}
