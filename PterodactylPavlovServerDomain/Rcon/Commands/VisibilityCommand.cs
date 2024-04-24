using PavlovVR_Rcon.Models.Replies;
using PavlovVR_Rcon.Models.Commands;

namespace PterodactylPavlovServerDomain.Rcon.Commands;
public class VisibilityCommand : BaseCommand<BaseReply>
{
    public VisibilityCommand(string target, bool visible) : base("Visibility")
    {
        this.addParameter(target);
        this.addParameter(visible.ToString());
    }
}
