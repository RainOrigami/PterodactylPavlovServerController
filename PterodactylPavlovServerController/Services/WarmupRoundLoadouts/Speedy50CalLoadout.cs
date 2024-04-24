using PavlovVR_Rcon.Models.Pavlov;
using PterodactylPavlovServerDomain.Rcon.Commands;

namespace PterodactylPavlovServerController.Services.WarmupRoundLoadouts;

public class Speedy50CalLoadout : BaseLoadout
{
    public override async Task EnablePlayer(PavlovRconService rconService, string apiKey, string serverId, ulong playerId)
    {
        await rconService.GiveItem(apiKey, serverId, playerId, Item.AntiTank.ToString());
    }
    public override Task DisablePlayer(PavlovRconService rconService, string apiKey, string serverId, ulong playerId)
    {
        return Task.CompletedTask;
    }
    public override async Task EnableRound(PavlovRconService rconService, string apiKey, string serverId)
    {
        await rconService.GameSpeed(apiKey, serverId, 2.0f);
        await Task.Delay(15);
        await rconService.AttachmentMode(apiKey, serverId, false);
    }
    public override async Task DisableRound(PavlovRconService rconService, string apiKey, string serverId)
    {
        await rconService.GameSpeed(apiKey, serverId, 1.0f);
        await Task.Delay(15);
        await rconService.AttachmentMode(apiKey, serverId, true);
    }
}
