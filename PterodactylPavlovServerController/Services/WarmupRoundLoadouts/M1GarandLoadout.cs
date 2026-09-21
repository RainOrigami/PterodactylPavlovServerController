
using PavlovVR_Rcon.Models.Pavlov;

namespace PterodactylPavlovServerController.Services.WarmupRoundLoadouts;

public class M1GarandLoadout : BaseLoadout
{
    public override Task DisablePlayer(PavlovRconService rconService, string apiKey, string serverId, ulong playerId) => Task.CompletedTask;
    public override Task DisablePlayers(PavlovRconService rconService, string apiKey, string serverId) => Task.CompletedTask;
    public override Task DisableRound(PavlovRconService rconService, string apiKey, string serverId) => Task.CompletedTask;
    public override async Task<bool> EnablePlayer(PavlovRconService rconService, string apiKey, string serverId, ulong playerId)
    {
        bool equipped = await BaseLoadout.GiveItem(rconService, apiKey, serverId, playerId, Item.m1garand);
        equipped &= await BaseLoadout.GiveItem(rconService, apiKey, serverId, playerId, Item.bayonet_m1garand);
        equipped &= await BaseLoadout.GiveItem(rconService, apiKey, serverId, playerId, Item.antipersonnelmine);
        equipped &= await BaseLoadout.GiveItem(rconService, apiKey, serverId, playerId, Item.antipersonnelmine);
        equipped &= await BaseLoadout.GiveItem(rconService, apiKey, serverId, playerId, Item.smoke_svt);
        equipped &= await BaseLoadout.GiveItem(rconService, apiKey, serverId, playerId, Item.smoke_svt);
        return equipped;
    }
    public override async Task EnablePlayers(PavlovRconService rconService, string apiKey, string serverId)
    {
        await rconService.SetVitality(apiKey, serverId, "All", 100, 100, 100);
    }
    public override Task EnableRound(PavlovRconService rconService, string apiKey, string serverId) => Task.CompletedTask;

    public M1GarandLoadout()
    {
        this.Name = "Pling!";
    }
}
