using PavlovVR_Rcon;
using PavlovVR_Rcon.Commands;
using PavlovVR_Rcon.Models.Pavlov;

namespace PterodactylPavlovServerController.Services;

public class PavlovRconService
{
    private static readonly Dictionary<string, PavlovRcon> pavlovRconConnections = new();
    private readonly PterodactylService pterodactylService;

    public PavlovRconService(PterodactylService pterodactylService)
    {
        this.pterodactylService = pterodactylService;
    }

    public async Task<PavlovPlayer[]> GetActivePlayers(string serverId)
    {
        return (await new RefreshListCommand().ExecuteCommand(await this.openConnection(serverId))).PlayerList;
    }

    public async Task<PavlovPlayerDetail> GetActivePlayerDetail(string serverId, ulong uniqueId)
    {
        return (await new InspectPlayerCommand(uniqueId).ExecuteCommand(await this.openConnection(serverId))).PlayerInfo;
    }

    public async Task<PavlovServerInfo> GetServerInfo(string serverId)
    {
        return (await new ServerInfoCommand().ExecuteCommand(await this.openConnection(serverId))).ServerInfo;
    }

    public async Task SwitchMap(string serverId, string mapLabel, PavlovGameMode gameMode)
    {
        await new SwitchMapCommand(mapLabel, gameMode).ExecuteCommand(await this.openConnection(serverId));
    }

    public async Task RotateMap(string serverId)
    {
        await new RotateMapCommand().ExecuteCommand(await this.openConnection(serverId));
    }

    public async Task KickPlayer(string serverId, ulong uniqueId)
    {
        await new KickCommand(uniqueId).ExecuteCommand(await this.openConnection(serverId));
    }

    public async Task BanPlayer(string serverId, ulong uniqueId)
    {
        await new BanCommand(uniqueId).ExecuteCommand(await this.openConnection(serverId));
    }

    public async Task UnbanPlayer(string serverId, ulong uniqueId)
    {
        await new UnbanCommand(uniqueId).ExecuteCommand(await this.openConnection(serverId));
    }

    public async Task<string[]> Banlist(string serverId)
    {
        return (await new BanlistCommand().ExecuteCommand(await this.openConnection(serverId))).BanList;
    }

    private async Task<PavlovRcon> openConnection(string serverId)
    {
        if (!PavlovRconService.pavlovRconConnections.ContainsKey(serverId))
        {
            PavlovRconService.pavlovRconConnections.Add(serverId, new PavlovRcon(this.pterodactylService.GetHost(serverId), int.Parse(this.pterodactylService.GetStartupVariable(serverId, "RCON_PORT")), this.pterodactylService.GetStartupVariable(serverId, "RCON_PASSWORD"), true));
        }

        if (!PavlovRconService.pavlovRconConnections[serverId].Connected)
        {
            await PavlovRconService.pavlovRconConnections[serverId].Connect(new CancellationTokenSource(2000).Token);
        }

        return PavlovRconService.pavlovRconConnections[serverId];
    }

    public async Task GiveItem(string serverId, ulong uniqueId, string item)
    {
        await new GiveItemCommand(uniqueId, item).ExecuteCommand(await this.openConnection(serverId));
    }

    public async Task GiveCash(string serverId, ulong uniqueId, int amount)
    {
        await new GiveCashCommand(uniqueId, amount).ExecuteCommand(await this.openConnection(serverId));
    }

    public async Task GiveVehicle(string serverId, ulong uniqueId, string vehicle)
    {
        await new GiveVehicleCommand(uniqueId, vehicle).ExecuteCommand(await this.openConnection(serverId));
    }

    public async Task SetSkin(string serverId, ulong uniqueId, string skin)
    {
        await new SetPlayerSkinCommand(uniqueId, skin).ExecuteCommand(await this.openConnection(serverId));
    }

    public async Task Slap(string serverId, ulong uniqueId, int amount)
    {
        await new SlapCommand(uniqueId, amount).ExecuteCommand(await this.openConnection(serverId));
    }

    public async Task SwitchTeam(string serverId, ulong uniqueId, int team)
    {
        await new SwitchTeamCommand(uniqueId, team).ExecuteCommand(await this.openConnection(serverId));
    }
}
