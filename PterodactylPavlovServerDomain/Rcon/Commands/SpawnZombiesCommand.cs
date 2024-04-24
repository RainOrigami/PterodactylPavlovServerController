using PavlovVR_Rcon.Models.Replies;
using PavlovVR_Rcon.Models.Commands;

namespace PterodactylPavlovServerDomain.Rcon.Commands;
public class SpawnZombiesCommand : BaseCommand<BaseReply>
{
    public SpawnZombiesCommand(int amount) : base("SpawnZombies")
    {
        this.addParameter(amount.ToString());
    }
}
