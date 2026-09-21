using System.ComponentModel.DataAnnotations;

namespace PterodactylPavlovServerController.Models;

public class GenericAmountModel
{
    [Required]
    public int? Amount { get; set; }
}
