using PavlovVR_Rcon;
using PavlovVR_Rcon.Models.Commands;
using PavlovVR_Rcon.Models.Pavlov;

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

    public async Task SwitchMap(string apiKey, string serverId, string mapLabel, GameMode gameMode)
    {
        await this.delay();
        await new SwitchMapCommand(mapLabel, gameMode).ExecuteCommand(await this.openConnection(apiKey, serverId));
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

    public async Task GiveItem(string apiKey, string serverId, ulong uniqueId, string item)
    {
        await this.delay();
        await new GiveItemCommand(uniqueId, item).ExecuteCommand(await this.openConnection(apiKey, serverId));
    }

    public async Task GiveCash(string apiKey, string serverId, ulong uniqueId, int amount)
    {
        await this.delay();
        await new GiveCashCommand(uniqueId, amount).ExecuteCommand(await this.openConnection(apiKey, serverId));
    }

    public async Task GiveVehicle(string apiKey, string serverId, ulong uniqueId, string vehicle)
    {
        await this.delay();
        await new GiveVehicleCommand(uniqueId, vehicle).ExecuteCommand(await this.openConnection(apiKey, serverId));
    }

    public async Task SetSkin(string apiKey, string serverId, ulong uniqueId, string skin)
    {
        await this.delay();
        await new SetPlayerSkinCommand(uniqueId, skin).ExecuteCommand(await this.openConnection(apiKey, serverId));
    }

    public async Task Slap(string apiKey, string serverId, ulong uniqueId, int amount)
    {
        await this.delay();
        await new SlapCommand(uniqueId, amount).ExecuteCommand(await this.openConnection(apiKey, serverId));
    }

    public async Task<bool> SwitchTeam(string apiKey, string serverId, ulong uniqueId, int team)
    {
        await this.delay();
        return (await new SwitchTeamCommand(uniqueId, team).ExecuteCommand(await this.openConnection(apiKey, serverId))).Successful;
    }
}
