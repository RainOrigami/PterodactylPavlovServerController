using PavlovVR_Rcon.Models.Replies;
using PavlovVR_Rcon.Models.Commands;

namespace PterodactylPavlovServerDomain.Rcon.Commands;
public class UtilityTrailsCommand : BaseCommand<BaseReply>
{
    public UtilityTrailsCommand(bool enable) : base("UtilityTrails")
    {
        this.addParameter(enable.ToString());
    }
}
