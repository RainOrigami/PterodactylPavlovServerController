using PavlovVR_Rcon.Models.Replies;
using PavlovVR_Rcon.Models.Commands;

namespace PterodactylPavlovServerDomain.Rcon.Commands;
public class DisablePickupCommand : BaseCommand<BaseReply>
{
    public DisablePickupCommand(string target, bool enable) : base("DisablePickup")
    {
        this.addParameter(target);
        this.addParameter(enable.ToString());
    }
}
