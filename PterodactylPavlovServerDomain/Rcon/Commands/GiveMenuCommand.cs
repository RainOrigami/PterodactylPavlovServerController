using PavlovVR_Rcon.Models.Replies;
using PavlovVR_Rcon.Models.Commands;

namespace PterodactylPavlovServerDomain.Rcon.Commands;
public class GiveMenuCommand : BaseCommand<BaseReply>
{
    public GiveMenuCommand(string target) : base("GiveMenu")
    {
        this.addParameter(target);
    }
}
