using PavlovVR_Rcon.Models.Replies;
using PavlovVR_Rcon.Models.Commands;

namespace PterodactylPavlovServerDomain.Rcon.Commands;
public class SetTeamSkinCommand : BaseCommand<BaseReply>
{
    public SetTeamSkinCommand(int teamId, string? skinId = null) : base("SetTeamSkin")
    {
        this.addParameter(teamId.ToString());
        if (skinId != null)
        {
            this.addParameter(skinId);
        }
    }
}
