using PavlovVR_Rcon.Models.Replies;
using PavlovVR_Rcon.Models.Commands;

namespace PterodactylPavlovServerDomain.Rcon.Commands;
public class RemoveZombiesCommand : BaseCommand<BaseReply>
{
    public RemoveZombiesCommand() : base("RemoveZombies")
    {
    }
}
