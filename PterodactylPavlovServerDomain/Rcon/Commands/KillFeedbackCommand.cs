using PavlovVR_Rcon.Models.Replies;
using PavlovVR_Rcon.Models.Commands;

namespace PterodactylPavlovServerDomain.Rcon.Commands;
public class KillFeedbackCommand : BaseCommand<BaseReply>
{
    public KillFeedbackCommand(bool enable) : base("KillFeedback")
    {
        this.addParameter(enable.ToString());
    }
}
