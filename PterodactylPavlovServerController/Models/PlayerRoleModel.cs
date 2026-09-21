using System.ComponentModel.DataAnnotations;

namespace PterodactylPavlovServerController.Models;

public class PlayerRoleModel
{
    [Required]
    public ulong? UniqueId { get; set; }

    [Required]
    public string? RoleId { get; set; }
}
