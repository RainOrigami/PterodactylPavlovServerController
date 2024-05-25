using PavlovVR_Rcon.Models.Pavlov;
using PterodactylPavlovServerDomain.Rcon.Commands;

namespace PterodactylPavlovServerController.Services.WarmupRoundLoadouts;

public class JohnWickLoadout : BaseLoadout
{
    public override async Task EnablePlayer(PavlovRconService rconService, string apiKey, string serverId, ulong playerId)
    {
        await rconService.SetCash(apiKey, serverId, playerId, 650);
        await Task.Delay(15);
        await rconService.GiveItem(apiKey, serverId, playerId, Item.flash);
        await Task.Delay(15);
        await rconService.GiveItem(apiKey, serverId, playerId, Item.flash);
        await Task.Delay(15);
        await rconService.GiveItem(apiKey, serverId, playerId, Item.flash);
        await Task.Delay(15);
        await rconService.GiveItem(apiKey, serverId, playerId, Item.supp_pistol);
        await Task.Delay(15);
        await rconService.GiveItem(apiKey, serverId, playerId, Item.reddot_pistol);
        await Task.Delay(15);
        await rconService.GiveItem(apiKey, serverId, playerId, Item.Laser_Pistol);
    }
    public override Task DisablePlayer(PavlovRconService rconService, string apiKey, string serverId, ulong playerId)
    {
        return Task.CompletedTask;
    }
    public override async Task EnableRound(PavlovRconService rconService, string apiKey, string serverId)
    {
        await rconService.MovementSpeed(apiKey, serverId, "All", 2.0f);
    }
    public override async Task DisableRound(PavlovRconService rconService, string apiKey, string serverId)
    {
        await rconService.MovementSpeed(apiKey, serverId, "All", 1.0f);
    }

    public override async Task EnablePlayers(PavlovRconService rconService, string apiKey, string serverId)
    {
        await rconService.EnableBuyMenu(apiKey, serverId, "All", true);
        await Task.Delay(10);
        await rconService.SetVitality(apiKey, serverId, "All", 100, 500, 500);
    }
    public override Task DisablePlayers(PavlovRconService rconService, string apiKey, string serverId) => Task.CompletedTask;

    public JohnWickLoadout()
    {
        this.Name = "John Wick (_BUY A PISTOL_!)";
    }
}
