
using PavlovVR_Rcon.Models.Pavlov;

namespace PterodactylPavlovServerController.Services.WarmupRoundLoadouts;

public class TranquillizerLoadout : BaseLoadout
{
    public override Task DisablePlayer(PavlovRconService rconService, string apiKey, string serverId, ulong playerId) => Task.CompletedTask;
    public override Task DisablePlayers(PavlovRconService rconService, string apiKey, string serverId) => Task.CompletedTask;
    public override Task DisableRound(PavlovRconService rconService, string apiKey, string serverId) => Task.CompletedTask;
    public override async Task<bool> EnablePlayer(PavlovRconService rconService, string apiKey, string serverId, ulong playerId)
    {
        bool equipped = await BaseLoadout.GiveItem(rconService, apiKey, serverId, playerId, Item.tranqgun);
        equipped &= await BaseLoadout.GiveItem(rconService, apiKey, serverId, playerId, Item.Syringe);
        equipped &= await BaseLoadout.GiveItem(rconService, apiKey, serverId, playerId, Item.Painkillers);
        return equipped;
    }
    public override async Task EnablePlayers(PavlovRconService rconService, string apiKey, string serverId)
    {
        //await rconService.SetVitality(apiKey, serverId, "All", 100, 0, 0);
        await Task.CompletedTask;
    }
    public override Task EnableRound(PavlovRconService rconService, string apiKey, string serverId) => Task.CompletedTask;

    public TranquillizerLoadout()
    {
        this.Name = "Keep Calm and...";
    }
}
