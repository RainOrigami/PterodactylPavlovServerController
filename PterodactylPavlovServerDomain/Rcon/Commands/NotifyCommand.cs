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

    public override async Task<BaseReply> ExecuteCommand(PavlovRcon connection, bool readReply = true)
    {
        if (!this.duration.HasValue)
        {
            await base.ExecuteCommand(connection);
            return new BaseReply() { Command = this.Command, Successful = true };
        }


        DateTime displayUntil = DateTime.Now.AddSeconds(this.duration.Value);
        while (DateTime.Now < displayUntil)
        {
            await Console.Out.WriteLineAsync("Sending notify...");
            try
            {
                await base.ExecuteCommand(connection, false);
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.ToString());
            }
            finally
            {
                await Task.Delay(1000);
            }
        }
        await Console.Out.WriteLineAsync("No more notify");

        return new BaseReply() { Command = this.Command, Successful = true };
    }
}
