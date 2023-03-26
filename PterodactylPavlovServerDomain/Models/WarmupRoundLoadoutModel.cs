using PavlovVR_Rcon.Models.Pavlov;
using System.ComponentModel.DataAnnotations;

namespace PterodactylPavlovServerDomain.Models;
public class WarmupRoundLoadoutModel
{
    [MaxLength(64)]
    public string ServerId { get; set; }

    [MaxLength(64)]
    [Required]
    public string Name { get; set; }

    public Item? Gun { get; set; }
    public Item? Item { get; set; }
    public Item? Attachment { get; set; }
}
