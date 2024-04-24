using PavlovVR_Rcon.Models.Replies;
using PavlovVR_Rcon.Models.Commands;

namespace PterodactylPavlovServerDomain.Rcon.Commands;
public class RemoveBotCommand : BaseCommand<BaseReply>
{
    public RemoveBotCommand(int amount, int? teamId = null) : base("RemoveBot")
    {
        this.addParameter(amount.ToString());
        if (teamId.HasValue)
        {
            this.addParameter(teamId.Value.ToString());
        }
    }
}
