using PavlovVR_Rcon.Models.Pavlov;
using PterodactylPavlovServerDomain.Rcon.Commands;

namespace PterodactylPavlovServerController.Services.WarmupRoundLoadouts;

public class JohnWickLoadout : BaseLoadout
{
    public override async Task EnablePlayer(PavlovRconService rconService, string apiKey, string serverId, ulong playerId)
    {
        await rconService.SetCash(apiKey, serverId, playerId, 650);
        await Task.Delay(30);
        await rconService.GiveItem(apiKey, serverId, playerId, Item.supp_pistol);
        await Task.Delay(30);
        await rconService.GiveItem(apiKey, serverId, playerId, Item.reddot_pistol);
        await Task.Delay(30);
        await rconService.GiveItem(apiKey, serverId, playerId, Item.Laser_Pistol);
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

    public override async Task EnablePlayers(PavlovRconService rconService, string apiKey, string serverId)
    {
        await rconService.EnableBuyMenu(apiKey, serverId, "All", true);
        await Task.Delay(10);
        await rconService.SetVitality(apiKey, serverId, "All", 100, 100, 100);
    }
    public override Task DisablePlayers(PavlovRconService rconService, string apiKey, string serverId) => Task.CompletedTask;

    public JohnWickLoadout()
    {
        this.Name = "John Wick (buy a pistol!)";
    }
}
