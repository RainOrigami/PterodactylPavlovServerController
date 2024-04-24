using PavlovVR_Rcon.Models.Replies;
using PavlovVR_Rcon.Models.Commands;

namespace PterodactylPavlovServerDomain.Rcon.Commands;
public class FallDamageCommand : BaseCommand<BaseReply>
{
    public FallDamageCommand(bool enable) : base("FallDamage")
    {
        this.addParameter(enable.ToString());
    }
}
