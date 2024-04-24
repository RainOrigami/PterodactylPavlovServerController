using PavlovVR_Rcon.Models.Replies;
using PavlovVR_Rcon.Models.Commands;

namespace PterodactylPavlovServerDomain.Rcon.Commands;
public class CleanUpCommand : BaseCommand<BaseReply>
{
  public CleanUpCommand(RconObjectType objectType) : base("CleanUp")
  {
        this.addParameter(objectType.ToString());
  }
}

public enum RconObjectType
{
    All,
    Bodies,
    Items,
    Vehicles
}
