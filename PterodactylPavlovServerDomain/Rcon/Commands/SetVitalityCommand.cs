using PavlovVR_Rcon.Models.Commands;
using PavlovVR_Rcon.Models.Replies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PterodactylPavlovServerDomain.Rcon.Commands;
public class SetVitalityCommand : BaseCommand<BaseReply>
{
    public SetVitalityCommand(string target, int? health = null, int? armor = null, int? helmet = null) : base("SetVitality")
    {
        this.addParameter(target);
        if (health is not null)
        {
            this.addParameter(health.Value.ToString());
        }
        else
        {
            this.addParameter("-1");
        }
        if (armor is not null)
        {
            this.addParameter(armor.Value.ToString());
        }
        else
        {
            this.addParameter("-1");
        }
        if (helmet is not null)
        {
            this.addParameter(helmet.Value.ToString());
        }
        else
        {
            this.addParameter("-1");
        }
    }
}
