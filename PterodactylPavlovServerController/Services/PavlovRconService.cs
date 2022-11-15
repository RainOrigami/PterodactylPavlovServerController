using PavlovVR_Rcon;
using PavlovVR_Rcon.Models.Commands;
using PavlovVR_Rcon.Models.Pavlov;
using PavlovVR_Rcon.Models.Replies;

namespace PterodactylPavlovServerController.Services;

public class PavlovRconService
{
    private static readonly Dictionary<string, PavlovRcon> pavlovRconConnections = new();
    private readonly PterodactylService pterodactylService;
    private DateTime lastCommand = DateTime.MinValue;

    public PavlovRconService(PterodactylService pterodactylService)
    {
        this.pterodactylService = pterodactylService;
    }

    private async Task delay()
    {
        if (this.lastCommand > DateTime.Now.AddMilliseconds(100))
        {
            await Task.Delay(100);
        }

        this.lastCommand = DateTime.Now;
    }

    public async Task<Player[]> GetActivePlayers(string apiKey, string serverId)
    {
        await this.delay();
        return (await new RefreshListCommand().ExecuteCommand(await this.openConnection(apiKey, serverId))).PlayerList;
    }

    public async Task<PlayerDetail> GetActivePlayerDetail(string apiKey, string serverId, ulong uniqueId)
    {
        await this.delay();
        return (await new InspectPlayerCommand(uniqueId).ExecuteCommand(await this.openConnection(apiKey, serverId))).PlayerInfo;
    }

    public async Task<ServerInfo> GetServerInfo(string apiKey, string serverId)
    {
        await this.delay();
        return (await new ServerInfoCommand().ExecuteCommand(await this.openConnection(apiKey, serverId))).ServerInfo;
    }

    public async Task<bool> SwitchMap(string apiKey, string serverId, string mapLabel, GameMode gameMode)
    {
        await this.delay();
        return (await new SwitchMapCommand(mapLabel, gameMode).ExecuteCommand(await this.openConnection(apiKey, serverId))).SwitchMap;
    }

    public async Task RotateMap(string apiKey, string serverId)
    {
        await this.delay();
        await new RotateMapCommand().ExecuteCommand(await this.openConnection(apiKey, serverId));
    }

    public async Task<bool> KickPlayer(string apiKey, string serverId, ulong uniqueId)
    {
        await this.delay();
        return (await new KickCommand(uniqueId).ExecuteCommand(await this.openConnection(apiKey, serverId))).Kick;
    }

    public async Task<bool> BanPlayer(string apiKey, string serverId, ulong uniqueId)
    {
        await this.delay();
        return (await new BanCommand(uniqueId).ExecuteCommand(await this.openConnection(apiKey, serverId))).Ban;
    }

    public async Task<bool> UnbanPlayer(string apiKey, string serverId, ulong uniqueId)
    {
        await this.delay();
        return (await new UnbanCommand(uniqueId).ExecuteCommand(await this.openConnection(apiKey, serverId))).Unban;
    }

    public async Task<ulong[]> Banlist(string apiKey, string serverId)
    {
        await this.delay();
        return (await new BanListCommand().ExecuteCommand(await this.openConnection(apiKey, serverId))).BanList;
    }

    private async Task<PavlovRcon> openConnection(string apiKey, string serverId)
    {
        if (!PavlovRconService.pavlovRconConnections.ContainsKey(serverId))
        {
            PavlovRconService.pavlovRconConnections.Add(serverId, new PavlovRcon(this.pterodactylService.GetHost(apiKey, serverId), int.Parse(this.pterodactylService.GetStartupVariable(apiKey, serverId, "RCON_PORT")), this.pterodactylService.GetStartupVariable(apiKey, serverId, "RCON_PASSWORD"), true));
        }

        if (!PavlovRconService.pavlovRconConnections[serverId].Connected)
        {
            await PavlovRconService.pavlovRconConnections[serverId].Connect(new CancellationTokenSource(2000).Token);
        }

        return PavlovRconService.pavlovRconConnections[serverId];
    }

    public async Task<bool> GiveItem(string apiKey, string serverId, ulong uniqueId, string item)
    {
        await this.delay();
        return (await new GiveItemCommand(uniqueId, item).ExecuteCommand(await this.openConnection(apiKey, serverId))).GiveItem;
    }

    public async Task<bool> GiveCash(string apiKey, string serverId, ulong uniqueId, int amount)
    {
        await this.delay();
        return (await new GiveCashCommand(uniqueId, amount).ExecuteCommand(await this.openConnection(apiKey, serverId))).GiveCash;
    }

    public async Task<bool> GiveVehicle(string apiKey, string serverId, ulong uniqueId, string vehicle)
    {
        await this.delay();
        return (await new GiveVehicleCommand(uniqueId, vehicle).ExecuteCommand(await this.openConnection(apiKey, serverId))).GiveVehicle;
    }

    public async Task<bool> SetSkin(string apiKey, string serverId, ulong uniqueId, string skin)
    {
        await this.delay();
        return (await new SetPlayerSkinCommand(uniqueId, skin).ExecuteCommand(await this.openConnection(apiKey, serverId))).SetPlayerSkin;
    }

    public async Task Slap(string apiKey, string serverId, ulong uniqueId, int amount)
    {
        await this.delay();
        await new SlapCommand(uniqueId, amount).ExecuteCommand(await this.openConnection(apiKey, serverId));
    }

    public async Task SwitchTeam(string apiKey, string serverId, ulong uniqueId, int team)
    {
        await this.delay();
        await new SwitchTeamCommand(uniqueId, team).ExecuteCommand(await this.openConnection(apiKey, serverId));
    }

    public async Task<bool> AddMod(string apiKey, string serverId, ulong uniqueId)
    {
        await this.delay();
        return (await new AddModCommand(uniqueId).ExecuteCommand(await this.openConnection(apiKey, serverId))).AddMod;
    }

    public async Task<bool> RemoveMod(string apiKey, string serverId, ulong uniqueId)
    {
        await this.delay();
        return (await new RemoveModCommand(uniqueId).ExecuteCommand(await this.openConnection(apiKey, serverId))).RemoveMod;
    }

    public async Task<bool> GiveTeamCash(string apiKey, string serverId, int teamId, int amount)
    {
        await this.delay();
        return (await new GiveTeamCashCommand(teamId, amount).ExecuteCommand(await this.openConnection(apiKey, serverId))).GiveTeamCash;
    }

    public async Task<bool> SetLimitedAmmoType(string apiKey, string serverId, int ammoType)
    {
        await this.delay();
        return (await new SetLimitedAmmoTypeCommand((AmmoType)ammoType).ExecuteCommand(await this.openConnection(apiKey, serverId))).SetLimitedAmmoType;
    }

    public async Task<bool> ResetSND(string apiKey, string serverId)
    {
        await this.delay();
        return (await new ResetSNDCommand().ExecuteCommand(await this.openConnection(apiKey, serverId))).ResetSND;
    }

    public async Task Shutdown(string apiKey, string serverId)
    {
        await this.delay();
        await new ShutdownCommand().ExecuteCommand(await this.openConnection(apiKey, serverId));
    }

    public async Task SetPin(string apiKey, string serverId, int? pin)
    {
        await this.delay();
        await new SetPinCommand(pin).ExecuteCommand(await this.openConnection(apiKey, serverId));
    }

    public async Task<bool> Shownametags(string apiKey, string serverId, bool show)
    {
        await this.delay();
        ShownametagsReply shownametagsReply = await new ShownametagsCommand(show).ExecuteCommand(await this.openConnection(apiKey, serverId));
        return shownametagsReply.ShowNametags && shownametagsReply.NametagsEnabled == show;
    }

    public async Task<bool> TTTFlushKarma(string apiKey, string serverId, ulong uniqueId)
    {
        await this.delay();
        return (await new TTTFlushKarmaCommand(uniqueId).ExecuteCommand(await this.openConnection(apiKey, serverId))).TTTFlushKarma;
    }

    public async Task<bool> TTTSetKarma(string apiKey, string serverId, ulong uniqueId, int amount)
    {
        await this.delay();
        return (await new TTTSetKarmaCommand(uniqueId, amount).ExecuteCommand(await this.openConnection(apiKey, serverId))).TTTSetKarma;
    }

    public async Task<bool> TTTEndRound(string apiKey, string serverId)
    {
        await this.delay();
        return (await new TTTEndRoundCommand().ExecuteCommand(await this.openConnection(apiKey, serverId))).TTTEndRound;
    }

    public async Task<(bool success, bool state)> TTTPauseTimer(string apiKey, string serverId)
    {
        await this.delay();
        TTTPauseTimerReply pauseTimerReply = await new TTTPauseTimer().ExecuteCommand(await this.openConnection(apiKey, serverId));
        return (pauseTimerReply.TTTPauseTimer, pauseTimerReply.TTTPauseState);
    }

    public async Task<(bool success, bool state)> TTTAlwaysEnableSkinMenu(string apiKey, string serverId)
    {
        await this.delay();
        TTTAlwaysEnableSkinMenuReply alwaysEnableSkinMenuReply = await new TTTAlwaysEnableSkinMenuCommand().ExecuteCommand(await this.openConnection(apiKey, serverId));
        return (alwaysEnableSkinMenuReply.TTTAlwaysEnableSkinMenu, alwaysEnableSkinMenuReply.TTTSkinMenuState);
    }
}
