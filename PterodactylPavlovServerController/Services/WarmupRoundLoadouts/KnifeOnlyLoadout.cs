using PavlovVR_Rcon.Models.Pavlov;
using PterodactylPavlovServerDomain.Rcon.Commands;

namespace PterodactylPavlovServerController.Services.WarmupRoundLoadouts;

public class KnifeOnlyLoadout : BaseLoadout
{
    public override Task EnablePlayer(PavlovRconService rconService, string apiKey, string serverId, ulong playerId) => Task.CompletedTask;
    public override Task DisablePlayer(PavlovRconService rconService, string apiKey, string serverId, ulong playerId) => Task.CompletedTask;
    public override async Task EnableRound(PavlovRconService rconService, string apiKey, string serverId)
    {
        await rconService.UtilityTrails(apiKey, serverId, true);
    }
    public override async Task DisableRound(PavlovRconService rconService, string apiKey, string serverId)
    {
        await rconService.UtilityTrails(apiKey, serverId, false);
    }

    public override async Task EnablePlayers(PavlovRconService rconService, string apiKey, string serverId) => await rconService.Supply(apiKey, serverId, "All", Item.Knife.ToString());
    public override async Task DisablePlayers(PavlovRconService rconService, string apiKey, string serverId) => await rconService.Supply(apiKey, serverId, "All");

    public KnifeOnlyLoadout()
    {
        this.Name = "Knife Only";
    }
}
