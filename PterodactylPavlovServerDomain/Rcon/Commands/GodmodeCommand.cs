using PavlovVR_Rcon.Models.Replies;
using PavlovVR_Rcon.Models.Commands;

namespace PterodactylPavlovServerDomain.Rcon.Commands;
public class GodmodeCommand : BaseCommand<BaseReply>
{
    public GodmodeCommand(string target, bool enable) : base("Godmode")
    {
        this.addParameter(target);
        this.addParameter(enable.ToString());
    }
}
