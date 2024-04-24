using PavlovVR_Rcon.Models.Replies;
using PavlovVR_Rcon.Models.Commands;

namespace PterodactylPavlovServerDomain.Rcon.Commands;
public class SpawnLootCrateCommand : BaseCommand<BaseReply>
{
    public SpawnLootCrateCommand(int? lootCrateId = null) : base("SpawnLootCrate")
    {
        if (lootCrateId.HasValue)
        {
            this.addParameter(lootCrateId.Value.ToString());
        }
    }
}
