
using PavlovVR_Rcon.Models.Pavlov;

namespace PterodactylPavlovServerController.Services.WarmupRoundLoadouts;

public class TranquillizerLoadout : BaseLoadout
{
    public override Task DisablePlayer(PavlovRconService rconService, string apiKey, string serverId, ulong playerId) => Task.CompletedTask;
    public override Task DisablePlayers(PavlovRconService rconService, string apiKey, string serverId) => Task.CompletedTask;
    public override Task DisableRound(PavlovRconService rconService, string apiKey, string serverId) => Task.CompletedTask;
    public override async Task EnablePlayer(PavlovRconService rconService, string apiKey, string serverId, ulong playerId)
    {
        await rconService.GiveItem(apiKey, serverId, playerId, Item.tranqgun);
        await Task.Delay(15);
        await rconService.GiveItem(apiKey, serverId, playerId, Item.Syringe);
        await Task.Delay(15);
        await rconService.GiveItem(apiKey, serverId, playerId, Item.Painkillers);
    }
    public override async Task EnablePlayers(PavlovRconService rconService, string apiKey, string serverId)
    {
        await rconService.SetVitality(apiKey, serverId, "All", 100, 100, 100);
    }
    public override Task EnableRound(PavlovRconService rconService, string apiKey, string serverId) => Task.CompletedTask;

    public TranquillizerLoadout()
    {
        this.Name = "Keep Calm and...";
    }
}
