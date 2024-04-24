using PavlovVR_Rcon.Models.Replies;
using PavlovVR_Rcon.Models.Commands;

namespace PterodactylPavlovServerDomain.Rcon.Commands;
public class DropItemsCommand : BaseCommand<BaseReply>
{
    public DropItemsCommand(string target) : base("DropItems")
    {
        this.addParameter(target);
    }
}
