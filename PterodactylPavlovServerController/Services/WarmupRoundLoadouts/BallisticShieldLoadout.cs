
using PavlovVR_Rcon.Models.Pavlov;

namespace PterodactylPavlovServerController.Services.WarmupRoundLoadouts;

public class BallisticShieldLoadout : BaseLoadout
{
    public override Task DisablePlayer(PavlovRconService rconService, string apiKey, string serverId, ulong playerId) => Task.CompletedTask;
    public override Task DisablePlayers(PavlovRconService rconService, string apiKey, string serverId) => Task.CompletedTask;
    public override Task DisableRound(PavlovRconService rconService, string apiKey, string serverId) => Task.CompletedTask;
    public override async Task<bool> EnablePlayer(PavlovRconService rconService, string apiKey, string serverId, ulong playerId)
    {
        bool equipped = await BaseLoadout.GiveItem(rconService, apiKey, serverId, playerId, Item.BallisticsShield);
        equipped &= await BaseLoadout.GiveItem(rconService, apiKey, serverId, playerId, Item.p90);
        equipped &= await BaseLoadout.GiveItem(rconService, apiKey, serverId, playerId, Item.Flashlight_Rifle);
        return equipped;
    }
    public override Task EnablePlayers(PavlovRconService rconService, string apiKey, string serverId) => Task.CompletedTask;
    public override Task EnableRound(PavlovRconService rconService, string apiKey, string serverId) => Task.CompletedTask;

    public BallisticShieldLoadout()
    {
        this.Name = "CS 1.6 Tactical Shield";
    }
}
