using PavlovVR_Rcon.Models.Pavlov;

namespace PterodactylPavlovServerDomain.Models;
public class ServerWarmupItemModel
{
    public string ServerId { get; set; } = string.Empty;
    public Item Item { get; set; }
}
