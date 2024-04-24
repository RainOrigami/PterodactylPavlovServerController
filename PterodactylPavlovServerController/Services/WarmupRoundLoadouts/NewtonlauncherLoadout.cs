using PavlovVR_Rcon.Models.Pavlov;
using PterodactylPavlovServerDomain.Rcon.Commands;

namespace PterodactylPavlovServerController.Services.WarmupRoundLoadouts;

public class NewtonlauncherLoadout : BaseLoadout
{
    public override async Task EnablePlayer(PavlovRconService rconService, string apiKey, string serverId, ulong playerId)
    {
        await rconService.GiveItem(apiKey, serverId, playerId, Item.newtonlauncher.ToString());
    }
    public override Task DisablePlayer(PavlovRconService rconService, string apiKey, string serverId, ulong playerId)
    {
        return Task.CompletedTask;
    }
    public override async Task EnableRound(PavlovRconService rconService, string apiKey, string serverId)
    {
        await rconService.SetGravity(apiKey, serverId, 0.25f);
        await Task.Delay(15);
        await rconService.FallDamage(apiKey, serverId, false);
    }
    public override async Task DisableRound(PavlovRconService rconService, string apiKey, string serverId)
    {
        await rconService.SetGravity(apiKey, serverId, 1.0f);
        await Task.Delay(15);
        await rconService.FallDamage(apiKey, serverId, true);
    }
}
