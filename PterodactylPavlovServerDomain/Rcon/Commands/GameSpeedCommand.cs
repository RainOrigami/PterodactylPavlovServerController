using PavlovVR_Rcon.Models.Replies;
using PavlovVR_Rcon.Models.Commands;

namespace PterodactylPavlovServerDomain.Rcon.Commands;
public class GameSpeedCommand : BaseCommand<BaseReply>
{
    public GameSpeedCommand(float multiplier) : base("GameSpeed")
    {
        this.addParameter(multiplier.ToString());
    }
}
