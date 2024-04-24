using PavlovVR_Rcon.Models.Replies;
using PavlovVR_Rcon.Models.Commands;
using PavlovVR_Rcon.Models.Pavlov;

namespace PterodactylPavlovServerDomain.Rcon.Commands;
public class SupplyCommand : BaseCommand<BaseReply>
{
    public SupplyCommand(string target, string? item) : base("Supply")
    {
        this.addParameter(target);
        if (item is not null)
        {
            this.addParameter(item);
        }
    }
}
