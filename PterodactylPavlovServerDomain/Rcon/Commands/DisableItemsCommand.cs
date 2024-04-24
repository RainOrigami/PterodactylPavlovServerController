using PavlovVR_Rcon.Models.Replies;
using PavlovVR_Rcon.Models.Commands;

namespace PterodactylPavlovServerDomain.Rcon.Commands;
public class DisableItemsCommand : BaseCommand<BaseReply>
{
    public DisableItemsCommand(string target, bool enable) : base("DisableItems")
    {
        this.addParameter(target);
        this.addParameter(enable.ToString());
    }
}
