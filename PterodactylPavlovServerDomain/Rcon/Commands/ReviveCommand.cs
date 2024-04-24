using PavlovVR_Rcon.Models.Replies;
using PavlovVR_Rcon.Models.Commands;

namespace PterodactylPavlovServerDomain.Rcon.Commands;
public class ReviveCommand : BaseCommand<BaseReply>
{
    public ReviveCommand(string target) : base("Revive")
    {
        this.addParameter(target);
    }
}
