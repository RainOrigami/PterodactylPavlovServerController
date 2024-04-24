using PavlovVR_Rcon.Models.Replies;
using PavlovVR_Rcon.Models.Commands;

namespace PterodactylPavlovServerDomain.Rcon.Commands;
public class AttachmentModeCommand : BaseCommand<BaseReply>
{
    public AttachmentModeCommand(bool enable) : base("AttachmentMode")
    {
        this.addParameter(enable.ToString());
    }
}
