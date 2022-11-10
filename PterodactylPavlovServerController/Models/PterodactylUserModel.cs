using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PterodactylPavlovServerController.Models;

[Table("users")]
public class PterodactylUserModel
{
    [Required]
    [MinLength(2)]
    [Column("username")]
    public string Username { get; set; } = String.Empty;

    [Required]
    [MinLength(3)]
    [Column("password")]
    public string Password { get; set; } = String.Empty;
}
