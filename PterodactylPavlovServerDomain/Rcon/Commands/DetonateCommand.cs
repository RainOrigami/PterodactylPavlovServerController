using PavlovVR_Rcon.Models.Replies;
using PavlovVR_Rcon.Models.Commands;

namespace PterodactylPavlovServerDomain.Rcon.Commands;
public class DetonateCommand : BaseCommand<BaseReply>
{
    public DetonateCommand(string target) : base("Detonate")
    {
        this.addParameter(target.ToString());
    }
}
