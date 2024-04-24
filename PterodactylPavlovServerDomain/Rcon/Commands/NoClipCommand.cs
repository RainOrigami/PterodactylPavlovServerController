using PavlovVR_Rcon.Models.Replies;
using PavlovVR_Rcon.Models.Commands;

namespace PterodactylPavlovServerDomain.Rcon.Commands;
public class NoClipCommand : BaseCommand<BaseReply>
{
    public NoClipCommand(string target, bool enable) : base("NoClip")
    {
        this.addParameter(target);
        this.addParameter(enable.ToString());
    }
}
