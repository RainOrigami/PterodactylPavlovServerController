using PavlovVR_Rcon.Models.Replies;
using PavlovVR_Rcon.Models.Commands;

namespace PterodactylPavlovServerDomain.Rcon.Commands;
public class MovementSpeedCommand : BaseCommand<BaseReply>
{
    public MovementSpeedCommand(string target, float multiplier) : base("MovementSpeed")
    {
        this.addParameter(target);
        this.addParameter(multiplier.ToString());
    }
}
