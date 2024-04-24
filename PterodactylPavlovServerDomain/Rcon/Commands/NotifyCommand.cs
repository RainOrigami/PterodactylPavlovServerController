using PavlovVR_Rcon.Models.Replies;
using PavlovVR_Rcon.Models.Commands;
using PavlovVR_Rcon;

namespace PterodactylPavlovServerDomain.Rcon.Commands;
public class NotifyCommand : BaseCommand<BaseReply>
{
    private readonly int? duration;

    public NotifyCommand(string target, string message, int? duration = null) : base("Notify")
    {
        this.addParameter(target);
        this.addParameter(message);
        this.duration = duration;
    }

    public override Task<BaseReply> ExecuteCommand(PavlovRcon connection)
    {
        if (!this.duration.HasValue)
        {
            return base.ExecuteCommand(connection);
        }

        Task.Run(async () =>
        {
            DateTime displayUntil = DateTime.Now.AddSeconds(this.duration.Value);
            while (DateTime.Now < displayUntil)
            {
                try
                {
                    await base.ExecuteCommand(connection);
                }
                finally
                {
                    await Task.Delay(1000);
                }
            }
        });

        return Task.FromResult(new BaseReply() { Command = this.Command, Successful = true });
    }
}
