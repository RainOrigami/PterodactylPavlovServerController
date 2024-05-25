
using PavlovVR_Rcon.Models.Pavlov;

namespace PterodactylPavlovServerController.Services.WarmupRoundLoadouts;

public class GoldenGunLoadout : BaseLoadout
{
    public override Task DisablePlayer(PavlovRconService rconService, string apiKey, string serverId, ulong playerId) => Task.CompletedTask;
    public override Task DisablePlayers(PavlovRconService rconService, string apiKey, string serverId) => Task.CompletedTask;
    public override Task DisableRound(PavlovRconService rconService, string apiKey, string serverId) => Task.CompletedTask;
    public override async Task EnablePlayer(PavlovRconService rconService, string apiKey, string serverId, ulong playerId)
    {
        await rconService.GiveItem(apiKey, serverId, playerId, Item.goldengun);
    }
    public override Task EnablePlayers(PavlovRconService rconService, string apiKey, string serverId) => Task.CompletedTask;
    public override Task EnableRound(PavlovRconService rconService, string apiKey, string serverId) => Task.CompletedTask;

    public GoldenGunLoadout()
    {
        this.Name = "Golden Gun";
    }
}
