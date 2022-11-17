using PavlovVR_Rcon;
using PavlovVR_Rcon.Exceptions;
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

    private readonly Dictionary<string, bool> commandRuning = new();

    private async Task delay()
    {
        if (this.lastCommand > DateTime.Now.AddMilliseconds(100))
        {
            await Task.Delay(100);
        }

        this.lastCommand = DateTime.Now;
    }

    private async Task<T> execute<T>(Func<PavlovRcon, Task<T>> action, string apiKey, string serverId, bool separateConnectionion)
    {
        if (!this.commandRuning.ContainsKey(serverId))
        {
            this.commandRuning.Add(serverId, false);
        }

        while (!separateConnectionion && this.commandRuning[serverId])
        {
            Console.WriteLine($"Command running for server {serverId}, delaying");
            await Task.Delay(50);
        }

        await this.delay();

        if (!separateConnectionion)
        {
            this.commandRuning[serverId] = true;
        }

        T result;
        try
        {
            PavlovRcon rcon = await openConnection(apiKey, serverId, separateConnectionion);
            result = await action(rcon);
            if (separateConnectionion)
            {
                try
                {
                    await rcon.SendTextCommand("Disconnect");
                }
                catch { }
            }
        }
        catch
        {
            if (!separateConnectionion)
            {
                this.commandRuning[serverId] = false;
            }

            throw;
        }

        if (!separateConnectionion)
        {
            this.commandRuning[serverId] = false;
        }

        return result;
    }

    public async Task<Player[]> GetActivePlayers(string apiKey, string serverId, bool separateConnection = false)
    {
        return await execute(async (rcon) => (await new RefreshListCommand().ExecuteCommand(rcon)).PlayerList, apiKey, serverId, separateConnection);
    }

    public async Task<PlayerDetail> GetActivePlayerDetail(string apiKey, string serverId, ulong uniqueId, bool separateConnection = false)
    {
        return await execute(async (rcon) => (await new InspectPlayerCommand(uniqueId).ExecuteCommand(rcon)).PlayerInfo, apiKey, serverId, separateConnection);

    }

    public async Task<ServerInfo> GetServerInfo(string apiKey, string serverId, bool separateConnection = false)
    {
        return await execute(async (rcon) => (await new ServerInfoCommand().ExecuteCommand(rcon)).ServerInfo, apiKey, serverId, separateConnection);
    }

    public async Task<bool> SwitchMap(string apiKey, string serverId, string mapLabel, GameMode gameMode, bool separateConnection = false)
    {
        try
        {
            return await execute(async (rcon) => (await new SwitchMapCommand(mapLabel, gameMode).ExecuteCommand(rcon)).SwitchMap, apiKey, serverId, separateConnection);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return false;
            }

            throw;
        }
    }

    public async Task RotateMap(string apiKey, string serverId, bool separateConnection = false)
    {
        await execute(async (rcon) => await new RotateMapCommand().ExecuteCommand(rcon), apiKey, serverId, separateConnection);
    }

    public async Task<bool> KickPlayer(string apiKey, string serverId, ulong uniqueId, bool separateConnection = false)
    {
        try
        {
            return await execute(async (rcon) => (await new KickCommand(uniqueId).ExecuteCommand(rcon)).Kick, apiKey, serverId, separateConnection);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return false;
            }

            throw;
        }
    }

    public async Task<bool> BanPlayer(string apiKey, string serverId, ulong uniqueId, bool separateConnection = false)
    {
        try
        {
            return await execute(async (rcon) => (await new BanCommand(uniqueId).ExecuteCommand(rcon)).Ban, apiKey, serverId, separateConnection);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return false;
            }

            throw;
        }
    }

    public async Task<bool> UnbanPlayer(string apiKey, string serverId, ulong uniqueId, bool separateConnection = false)
    {
        try
        {
            return await execute(async (rcon) => (await new UnbanCommand(uniqueId).ExecuteCommand(rcon)).Unban, apiKey, serverId, separateConnection);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return false;
            }

            throw;
        }
    }

    public async Task<ulong[]> Banlist(string apiKey, string serverId, bool separateConnection = false)
    {
        return await execute(async (rcon) => (await new BanListCommand().ExecuteCommand(rcon)).BanList, apiKey, serverId, separateConnection);
    }

    private async Task<PavlovRcon> openConnection(string apiKey, string serverId, bool separateConnectionion)
    {
        if (separateConnectionion)
        {
            PavlovRcon rcon = new PavlovRcon(this.pterodactylService.GetHost(apiKey, serverId), int.Parse(this.pterodactylService.GetStartupVariable(apiKey, serverId, "RCON_PORT")), this.pterodactylService.GetStartupVariable(apiKey, serverId, "RCON_PASSWORD"), true);
            await rcon.Connect(new CancellationTokenSource(2000).Token);
            return rcon;
        }

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

    public async Task<bool> GiveItem(string apiKey, string serverId, ulong uniqueId, string item, bool separateConnection = false)
    {
        try
        {
            return await execute(async (rcon) => (await new GiveItemCommand(uniqueId, item).ExecuteCommand(rcon)).GiveItem, apiKey, serverId, separateConnection);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return false;
            }

            throw;
        }
    }

    public async Task<bool> GiveCash(string apiKey, string serverId, ulong uniqueId, int amount, bool separateConnection = false)
    {
        try
        {
            return await execute(async (rcon) => (await new GiveCashCommand(uniqueId, amount).ExecuteCommand(rcon)).GiveCash, apiKey, serverId, separateConnection);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return false;
            }

            throw;
        }
    }

    public async Task<bool> GiveVehicle(string apiKey, string serverId, ulong uniqueId, string vehicle, bool separateConnection = false)
    {
        try
        {
            return await execute(async (rcon) => (await new GiveVehicleCommand(uniqueId, vehicle).ExecuteCommand(rcon)).GiveVehicle, apiKey, serverId, separateConnection);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return false;
            }

            throw;
        }
    }

    public async Task<bool> SetSkin(string apiKey, string serverId, ulong uniqueId, string skin, bool separateConnection = false)
    {
        try
        {
            return await execute(async (rcon) => (await new SetPlayerSkinCommand(uniqueId, skin).ExecuteCommand(rcon)).SetPlayerSkin, apiKey, serverId, separateConnection);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return false;
            }

            throw;
        }
    }

    public async Task Slap(string apiKey, string serverId, ulong uniqueId, int amount, bool separateConnection = false)
    {
        await execute(async (rcon) => await new SlapCommand(uniqueId, amount).ExecuteCommand(rcon), apiKey, serverId, separateConnection);
    }

    public async Task SwitchTeam(string apiKey, string serverId, ulong uniqueId, int team, bool separateConnection = false)
    {
        await execute(async (rcon) => await new SwitchTeamCommand(uniqueId, team).ExecuteCommand(rcon), apiKey, serverId, separateConnection);
    }

    public async Task<bool> AddMod(string apiKey, string serverId, ulong uniqueId, bool separateConnection = false)
    {
        try
        {
            return await execute(async (rcon) => (await new AddModCommand(uniqueId).ExecuteCommand(rcon)).AddMod, apiKey, serverId, separateConnection);

        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return false;
            }

            throw;
        }
    }

    public async Task<bool> RemoveMod(string apiKey, string serverId, ulong uniqueId, bool separateConnection = false)
    {
        try
        {
            return await execute(async (rcon) => (await new RemoveModCommand(uniqueId).ExecuteCommand(rcon)).RemoveMod, apiKey, serverId, separateConnection);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return false;
            }

            throw;
        }
    }

    public async Task<bool> GiveTeamCash(string apiKey, string serverId, int teamId, int amount, bool separateConnection = false)
    {
        try
        {
            return await execute(async (rcon) => (await new GiveTeamCashCommand(teamId, amount).ExecuteCommand(rcon)).GiveTeamCash, apiKey, serverId, separateConnection);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return false;
            }

            throw;
        }
    }

    public async Task<bool> SetLimitedAmmoType(string apiKey, string serverId, int ammoType, bool separateConnection = false)
    {
        try
        {
            return await execute(async (rcon) => (await new SetLimitedAmmoTypeCommand((AmmoType)ammoType).ExecuteCommand(rcon)).SetLimitedAmmoType, apiKey, serverId, separateConnection);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return false;
            }

            throw;
        }
    }

    public async Task<bool> ResetSND(string apiKey, string serverId, bool separateConnection = false)
    {
        try
        {
            return await execute(async (rcon) => (await new ResetSNDCommand().ExecuteCommand(rcon)).ResetSND, apiKey, serverId, separateConnection);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return false;
            }

            throw;
        }
    }

    public async Task SetPin(string apiKey, string serverId, int? pin, bool separateConnection = false)
    {
        await execute(async (rcon) => await new SetPinCommand(pin).ExecuteCommand(rcon), apiKey, serverId, separateConnection);
    }

    public async Task<bool> Shownametags(string apiKey, string serverId, bool show, bool separateConnection = false)
    {
        try
        {
            return await execute(async (rcon) =>
            {
                ShownametagsReply shownametagsReply = await new ShownametagsCommand(show).ExecuteCommand(rcon);
                return shownametagsReply.ShowNametags && shownametagsReply.NametagsEnabled == show;
            }, apiKey, serverId, separateConnection);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return false;
            }

            throw;
        }
    }

    public async Task<bool> TTTFlushKarma(string apiKey, string serverId, ulong uniqueId, bool separateConnection = false)
    {
        try
        {
            return await execute(async (rcon) => (await new TTTFlushKarmaCommand(uniqueId).ExecuteCommand(rcon)).TTTFlushKarma, apiKey, serverId, separateConnection);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return false;
            }

            throw;
        }
    }

    public async Task<bool> TTTSetKarma(string apiKey, string serverId, ulong uniqueId, int amount, bool separateConnection = false)
    {
        try
        {
            return await execute(async (rcon) => (await new TTTSetKarmaCommand(uniqueId, amount).ExecuteCommand(rcon)).TTTSetKarma, apiKey, serverId, separateConnection);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return false;
            }

            throw;
        }
    }

    public async Task<bool> TTTEndRound(string apiKey, string serverId, bool separateConnection = false)
    {
        try
        {
            return await execute(async (rcon) => (await new TTTEndRoundCommand().ExecuteCommand(rcon)).TTTEndRound, apiKey, serverId, separateConnection);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return false;
            }

            throw;
        }
    }

    public async Task<(bool success, bool state)> TTTPauseTimer(string apiKey, string serverId, bool separateConnection = false)
    {
        try
        {
            return await execute(async (rcon) =>
            {
                TTTPauseTimerReply pauseTimerReply = await new TTTPauseTimerCommand().ExecuteCommand(rcon);
                return (pauseTimerReply.TTTPauseTimer, pauseTimerReply.TTTPauseState);
            }, apiKey, serverId, separateConnection);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return (false, false);
            }

            throw;
        }
    }

    public async Task<(bool success, bool state)> TTTAlwaysEnableSkinMenu(string apiKey, string serverId, bool separateConnection = false)
    {
        try
        {
            return await execute(async (rcon) =>
            {
                TTTAlwaysEnableSkinMenuReply alwaysEnableSkinMenuReply = await new TTTAlwaysEnableSkinMenuCommand().ExecuteCommand(rcon);
                return (alwaysEnableSkinMenuReply.TTTAlwaysEnableSkinMenu, alwaysEnableSkinMenuReply.TTTSkinMenuState);
            }, apiKey, serverId, separateConnection);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return (false, false);
            }

            throw;
        }
    }

    public async Task<string> CustomCommand(string apiKey, string serverId, string customCommand, bool separateConnection = false)
    {
        string command;
        string[]? parameters = null;
        if (customCommand.Contains(' '))
        {
            string[] commandParts = customCommand.Split(' ');
            command = commandParts[0];
            parameters = commandParts[1..];
        }
        else
        {
            command = customCommand;
        }

        return await execute(async (rcon) => await rcon.SendTextCommand(customCommand, parameters), apiKey, serverId, separateConnection);
    }
}
