using PavlovVR_Rcon.Models.Replies;
using PavlovVR_Rcon.Models.Commands;

namespace PterodactylPavlovServerDomain.Rcon.Commands;
public class SpawnChickensCommand : BaseCommand<BaseReply>
{
    public SpawnChickensCommand(int amount) : base("SpawnChickens")
    {
        this.addParameter(amount.ToString());
    }
}
