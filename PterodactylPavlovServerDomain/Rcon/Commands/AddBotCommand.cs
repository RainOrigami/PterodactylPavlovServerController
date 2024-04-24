using PavlovVR_Rcon.Models.Replies;
using PavlovVR_Rcon.Models.Commands;

namespace PterodactylPavlovServerDomain.Rcon.Commands;
public class AddBotCommand : BaseCommand<BaseReply>
{
    public AddBotCommand(int amount, int? teamId = null) : base("AddBot")
    {
        this.addParameter(amount.ToString());
        if (teamId.HasValue)
        {
            this.addParameter(teamId.Value.ToString());
        }
    }
}
