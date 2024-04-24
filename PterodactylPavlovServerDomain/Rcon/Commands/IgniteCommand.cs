using PavlovVR_Rcon.Models.Replies;
using PavlovVR_Rcon.Models.Commands;

namespace PterodactylPavlovServerDomain.Rcon.Commands;
public class IgniteCommand : BaseCommand<BaseReply>
{
    public IgniteCommand(string target) : base("Ignite")
    {
        this.addParameter(target);
    }
}
