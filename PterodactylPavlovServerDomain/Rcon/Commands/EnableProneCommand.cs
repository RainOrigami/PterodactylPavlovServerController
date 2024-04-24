using PavlovVR_Rcon.Models.Replies;
using PavlovVR_Rcon.Models.Commands;

namespace PterodactylPavlovServerDomain.Rcon.Commands;
public class EnableProneCommand : BaseCommand<BaseReply>
{
    public EnableProneCommand(bool enable) : base("EnableProne")
    {
        this.addParameter(enable.ToString());
    }
}
