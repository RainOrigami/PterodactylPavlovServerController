using PavlovVR_Rcon.Models.Replies;
using PavlovVR_Rcon.Models.Commands;

namespace PterodactylPavlovServerDomain.Rcon.Commands;
public class EnableBuyMenuCommand : BaseCommand<BaseReply>
{
    public EnableBuyMenuCommand(string target, bool enable) : base("EnableBuyMenu")
    {
        this.addParameter(target);
        this.addParameter(enable.ToString());
    }
}
