using PavlovVR_Rcon.Models.Replies;
using PavlovVR_Rcon.Models.Commands;

namespace PterodactylPavlovServerDomain.Rcon.Commands;
public class RemoveMenuCommand : BaseCommand<BaseReply>
{
  public RemoveMenuCommand(string target) : base("RemoveMenu")
  {
        this.addParameter(target);
  }
}
