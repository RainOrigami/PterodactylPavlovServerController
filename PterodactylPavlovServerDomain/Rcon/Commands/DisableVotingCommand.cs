using PavlovVR_Rcon.Models.Replies;
using PavlovVR_Rcon.Models.Commands;

namespace PterodactylPavlovServerDomain.Rcon.Commands;
public class DisableVotingCommand : BaseCommand<BaseReply>
{
    public DisableVotingCommand(bool enable) : base("DisableVoting")
    {
        this.addParameter(enable.ToString());
    }
}
