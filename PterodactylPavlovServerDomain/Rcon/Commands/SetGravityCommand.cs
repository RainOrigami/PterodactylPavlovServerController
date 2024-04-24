using PavlovVR_Rcon.Models.Replies;
using PavlovVR_Rcon.Models.Commands;

namespace PterodactylPavlovServerDomain.Rcon.Commands;
public class SetGravityCommand : BaseCommand<BaseReply>
{
    public SetGravityCommand(float multiplier) : base("SetGravity")
    {
        this.addParameter(multiplier.ToString());
    }
}
