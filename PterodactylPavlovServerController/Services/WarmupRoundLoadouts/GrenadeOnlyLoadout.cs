using PavlovVR_Rcon.Models.Pavlov;
using PterodactylPavlovServerDomain.Rcon.Commands;

namespace PterodactylPavlovServerController.Services.WarmupRoundLoadouts;

public class GrenadeOnlyLoadout : BaseLoadout
{
    public override Task<bool> EnablePlayer(PavlovRconService rconService, string apiKey, string serverId, ulong playerId) => Task.FromResult(true);
    public override Task DisablePlayer(PavlovRconService rconService, string apiKey, string serverId, ulong playerId) => Task.CompletedTask;
    public override async Task EnableRound(PavlovRconService rconService, string apiKey, string serverId)
    {
        await rconService.UtilityTrails(apiKey, serverId, true);
        await Task.Delay(15);
        await rconService.SetGravity(apiKey, serverId, 0.75f);
    }
    public override async Task DisableRound(PavlovRconService rconService, string apiKey, string serverId)
    {
        await rconService.UtilityTrails(apiKey, serverId, false);
        await Task.Delay(15);
        await rconService.SetGravity(apiKey, serverId, 1.0f);
    }

    public override async Task EnablePlayers(PavlovRconService rconService, string apiKey, string serverId)
    {
        await rconService.Supply(apiKey, serverId, "All", Item.grenade_us.ToString());
    }
    public override async Task DisablePlayers(PavlovRconService rconService, string apiKey, string serverId)
    {
        await rconService.Supply(apiKey, serverId, "All");
    }

    public GrenadeOnlyLoadout()
    {
        this.Name = "Grenade Only";
    }
}
