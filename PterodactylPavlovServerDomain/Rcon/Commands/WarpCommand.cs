using PavlovVR_Rcon.Models.Replies;
using PavlovVR_Rcon.Models.Commands;

namespace PterodactylPavlovServerDomain.Rcon.Commands;
public class WarpCommand : BaseCommand<BaseReply>
{
    public WarpCommand(string player, ulong targetId) : base("Warp")
    {
        this.addParameter(player);
        this.addParameter(targetId.ToString());
    }
}
