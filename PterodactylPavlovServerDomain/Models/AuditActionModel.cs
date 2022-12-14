using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PterodactylPavlovServerDomain.Models;
public class AuditActionModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string User { get; set; }
    public string Server { get; set; }
    public string Action { get; set; }
    public DateTime Time { get; set; }
}