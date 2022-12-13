namespace PterodactylPavlovServerDomain.Models;
public class AuditActionModel
{
    public string User { get; set; }
    public string Server { get; set; }
    public string Action { get; set; }
    public DateTime Time { get; set; }
}