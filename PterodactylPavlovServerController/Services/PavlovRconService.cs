using PavlovVR_Rcon;
using PavlovVR_Rcon.Exceptions;
using PavlovVR_Rcon.Models.Commands;
using PavlovVR_Rcon.Models.Pavlov;
using PavlovVR_Rcon.Models.Replies;
using PterodactylPavlovServerDomain.Rcon.Commands;

namespace PterodactylPavlovServerController.Services;

public class PavlovRconService
{
    private static readonly Dictionary<string, PavlovRcon> pavlovRconConnections = new();
    private readonly PterodactylService pterodactylService;
    private readonly IConfiguration configuration;
    private DateTime lastCommand = DateTime.MinValue;

    public PavlovRconService(PterodactylService pterodactylService, IConfiguration configuration)
    {
        this.pterodactylService = pterodactylService;
        this.configuration = configuration;
    }

    private readonly Dictionary<string, bool> commandRunning = new();

    private async Task delay()
    {
        if (this.lastCommand > DateTime.Now.AddMilliseconds(100))
        {
            await Task.Delay(100);
        }

        this.lastCommand = DateTime.Now;
    }

    private async Task<T> execute<T>(Func<PavlovRcon, Task<T>> action, string apiKey, string serverId, bool separateConnection, bool awaitResponse = true)
    {
        lock (this.commandRunning)
        {
            if (!this.commandRunning.ContainsKey(serverId))
            {
                this.commandRunning.Add(serverId, false);
            }
        }

        while (!separateConnection && this.commandRunning[serverId])
        {
            Console.WriteLine($"Command running for server {serverId}, delaying");
            await Task.Delay(50);
        }

        await this.delay();

        if (!separateConnection)
        {
            this.commandRunning[serverId] = true;
        }

        T result;
        int commandTimeout = 2000;
        PavlovRcon? rcon = null;
        try
        {
            rcon = await openConnection(apiKey, serverId, separateConnection);
            commandTimeout = rcon.CommandTimeout;
            if (!awaitResponse)
            {
                rcon.CommandTimeout = 5;
            }
            result = await action(rcon);
            if (!awaitResponse)
            {
                rcon.CommandTimeout = commandTimeout;
            }
            if (separateConnection)
            {
                try
                {
                    await rcon.SendTextCommand("Disconnect");
                }
                catch { }
            }
        }
        catch (Exception ex)
        {
            if (rcon != null)
            {
                rcon.CommandTimeout = commandTimeout;
            }

            if (!awaitResponse && ex.InnerException is CommandTimeoutException and not null)
            {
                // RCON Plus does not send a response but other errors must still be thrown
                return default(T)!;
            }

            if (!separateConnection)
            {
                this.commandRunning[serverId] = false;
            }

            throw;
        }

        if (!separateConnection)
        {
            this.commandRunning[serverId] = false;
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

    public async Task<PlayerDetail[]> GetActivePlayerDetails(string apiKey, string serverId, bool separateConnection = false)
    {
        return await execute(async (rcon) => (await new InspectAllCommand().ExecuteCommand(rcon)).InspectList, apiKey, serverId, separateConnection);
    }

    public async Task<bool> SetBalanceTableURL(string apiKey, string serverId, string balanceTableUrl, bool separateConnection = false)
    {
        return await execute(async (rcon) => (await new SetBalanceTableURLCommand(balanceTableUrl).ExecuteCommand(rcon)).SetBalanceTableURL, apiKey, serverId, separateConnection);
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

    private async Task<PavlovRcon> openConnection(string apiKey, string serverId, bool separateConnection)
    {
        if (this.configuration.GetValue<bool>("logRconToFile") && !Directory.Exists("./logs"))
        {
            Directory.CreateDirectory("./logs");
        }

        if (separateConnection)
        {
            PavlovRcon rcon = new PavlovRcon(this.pterodactylService.GetHost(apiKey, serverId), int.Parse(this.pterodactylService.GetStartupVariable(apiKey, serverId, "RCON_PORT")), this.pterodactylService.GetStartupVariable(apiKey, serverId, "RCON_PASSWORD"), true, this.configuration.GetValue<bool>("logRconToFile") ? $"./logs/{DateTime.Now:yyyy-MM-dd-HH-mm-ss}_{serverId}_rcon.log" : null);
            await rcon.Connect(new CancellationTokenSource(2000).Token);
            return rcon;
        }

        if (!PavlovRconService.pavlovRconConnections.ContainsKey(serverId))
        {
            PavlovRconService.pavlovRconConnections.Add(serverId, new PavlovRcon(this.pterodactylService.GetHost(apiKey, serverId), int.Parse(this.pterodactylService.GetStartupVariable(apiKey, serverId, "RCON_PORT")), this.pterodactylService.GetStartupVariable(apiKey, serverId, "RCON_PASSWORD"), true, this.configuration.GetValue<bool>("logRconToFile") ? $"./logs/{DateTime.Now:yyyy-MM-dd-HH-mm-ss}_{serverId}_rcon.log" : null));
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

    public async Task<bool> ClearEmptyVehicles(string apiKey, string serverId, bool seperateConnection = false)
    {
        try
        {
            return await execute(async (rcon) => (await new ClearEmptyVehiclesCommand().ExecuteCommand(rcon)).Successful, apiKey, serverId, seperateConnection);
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

    public async Task<bool> GagPlayer(string apiKey, string serverId, ulong uniqueId, bool separateConnection = false)
    {
        try
        {
            return await execute(async (rcon) => (await new GagCommand(uniqueId).ExecuteCommand(rcon)).Gag, apiKey, serverId, separateConnection);
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

    public async Task<bool> SetCash(string apiKey, string serverId, ulong uniqueId, int amount, bool seperateConnection = false)
    {
        try
        {
            return await execute(async (rcon) => (await new SetCashCommand(uniqueId, amount).ExecuteCommand(rcon)).SetCash, apiKey, serverId, seperateConnection);
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

    public async Task<bool> PauseMatch(string apiKey, string serverId, int amount, bool seperateConnection = false)
    {
        try
        {
            return await execute(async (rcon) => (await new PauseMatchCommand(amount).ExecuteCommand(rcon)).PauseMatch, apiKey, serverId, seperateConnection);
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

        return await execute(async (rcon) => await rcon.SendTextCommand(command, parameters), apiKey, serverId, separateConnection);
    }

    #region Rcon Plus
    public async Task GiveMenu(string apiKey, string serverId, string target)
    {
        try
        {
            await execute(async (rcon) => (await new GiveMenuCommand(target).ExecuteCommand(rcon)).Successful, apiKey, serverId, true, false);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return;
            }

            throw;
        }
    }

    public async Task RemoveMenu(string apiKey, string serverId, string target)
    {
        try
        {
            await execute(async (rcon) => (await new RemoveMenuCommand(target).ExecuteCommand(rcon)).Successful, apiKey, serverId, true, false);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return;
            }

            throw;
        }
    }

    public async Task Notify(string apiKey, string serverId, string target, string message, int? duration = null)
    {
        try
        {
            await execute(async (rcon) => (await new NotifyCommand(target, message, duration).ExecuteCommand(rcon)).Successful, apiKey, serverId, true, false);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return;
            }

            throw;
        }
    }

    public async Task DropItems(string apiKey, string serverId, string target)
    {
        try
        {
            await execute(async (rcon) => (await new DropItemsCommand(target).ExecuteCommand(rcon)).Successful, apiKey, serverId, true, false);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return;
            }

            throw;
        }
    }

    public async Task DisablePickup(string apiKey, string serverId, string target, bool enable)
    {
        try
        {
            await execute(async (rcon) => (await new DisablePickupCommand(target, enable).ExecuteCommand(rcon)).Successful, apiKey, serverId, true, false);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return;
            }

            throw;
        }
    }

    public async Task MovementSpeed(string apiKey, string serverId, string target, float multiplier)
    {
        try
        {
            await execute(async (rcon) => (await new MovementSpeedCommand(target, multiplier).ExecuteCommand(rcon)).Successful, apiKey, serverId, true, false);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return;
            }

            throw;
        }
    }

    public async Task CleanUp(string apiKey, string serverId, RconObjectType objectType)
    {
        try
        {
            await execute(async (rcon) => (await new CleanUpCommand(objectType).ExecuteCommand(rcon)).Successful, apiKey, serverId, true, false);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return;
            }

            throw;
        }
    }

    public async Task Godmode(string apiKey, string serverId, string target, bool enable)
    {
        try
        {
            await execute(async (rcon) => (await new GodmodeCommand(target, enable).ExecuteCommand(rcon)).Successful, apiKey, serverId, true, false);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return;
            }

            throw;
        }
    }

    public async Task Warp(string apiKey, string serverId, string player, ulong target)
    {
        try
        {
            await execute(async (rcon) => (await new WarpCommand(player, target).ExecuteCommand(rcon)).Successful, apiKey, serverId, true, false);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return;
            }

            throw;
        }
    }

    public async Task AddBot(string apiKey, string serverId, int amount, int? team = null)
    {
        try
        {
            await execute(async (rcon) => (await new AddBotCommand(amount, team).ExecuteCommand(rcon)).Successful, apiKey, serverId, true, false);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return;
            }

            throw;
        }
    }

    public async Task RemoveBot(string apiKey, string serverId, int amount, int? team = null)
    {
        try
        {
            await execute(async (rcon) => (await new RemoveBotCommand(amount, team).ExecuteCommand(rcon)).Successful, apiKey, serverId, true, false);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return;
            }

            throw;
        }
    }

    public async Task Ignite(string apiKey, string serverId, string target)
    {
        try
        {
            await execute(async (rcon) => (await new IgniteCommand(target).ExecuteCommand(rcon)).Successful, apiKey, serverId, true, false);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return;
            }

            throw;
        }
    }

    public async Task DisableItems(string apiKey, string serverId, string target, bool enable)
    {
        try
        {
            await execute(async (rcon) => (await new DisableItemsCommand(target, enable).ExecuteCommand(rcon)).Successful, apiKey, serverId, true, false);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return;
            }

            throw;
        }
    }

    public async Task Detonate(string apiKey, string serverId, string target)
    {
        try
        {
            await execute(async (rcon) => (await new DetonateCommand(target).ExecuteCommand(rcon)).Successful, apiKey, serverId, true, false);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return;
            }

            throw;
        }
    }

    public async Task GameSpeed(string apiKey, string serverId, float multiplier)
    {
        try
        {
            await execute(async (rcon) => (await new GameSpeedCommand(multiplier).ExecuteCommand(rcon)).Successful, apiKey, serverId, true, false);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return;
            }

            throw;
        }
    }

    public async Task SetGravity(string apiKey, string serverId, float multiplier)
    {
        try
        {
            await execute(async (rcon) => (await new SetGravityCommand(multiplier).ExecuteCommand(rcon)).Successful, apiKey, serverId, true, false);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return;
            }

            throw;
        }
    }

    public async Task EnableProne(string apiKey, string serverId, bool enable)
    {
        try
        {
            await execute(async (rcon) => (await new EnableProneCommand(enable).ExecuteCommand(rcon)).Successful, apiKey, serverId, true, false);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return;
            }

            throw;
        }
    }

    public async Task FallDamage(string apiKey, string serverId, bool enable)
    {
        try
        {
            await execute(async (rcon) => (await new FallDamageCommand(enable).ExecuteCommand(rcon)).Successful, apiKey, serverId, true, false);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return;
            }

            throw;
        }
    }

    public async Task EnableBuyMenu(string apiKey, string serverId, string target, bool enable)
    {
        try
        {
            await execute(async (rcon) => (await new EnableBuyMenuCommand(target, enable).ExecuteCommand(rcon)).Successful, apiKey, serverId, true, false);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return;
            }

            throw;
        }
    }

    public async Task NoClip(string apiKey, string serverId, string target, bool enable)
    {
        try
        {
            await execute(async (rcon) => (await new NoClipCommand(target, enable).ExecuteCommand(rcon)).Successful, apiKey, serverId, true, false);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return;
            }

            throw;
        }
    }

    public async Task Supply(string apiKey, string serverId, string target, string? itemId = null)
    {
        try
        {
            await execute(async (rcon) => (await new SupplyCommand(target, itemId).ExecuteCommand(rcon)).Successful, apiKey, serverId, true, false);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return;
            }

            throw;
        }
    }

    public async Task Visibility(string apiKey, string serverId, string target, bool visible)
    {
        try
        {
            await execute(async (rcon) => (await new VisibilityCommand(target, visible).ExecuteCommand(rcon)).Successful, apiKey, serverId, true, false);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return;
            }

            throw;
        }
    }

    public async Task Revive(string apiKey, string serverId, string target)
    {
        try
        {
            await execute(async (rcon) => (await new ReviveCommand(target).ExecuteCommand(rcon)).Successful, apiKey, serverId, true, false);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return;
            }

            throw;
        }
    }

    public async Task DisableVoting(string apiKey, string serverId, bool enable)
    {
        try
        {
            await execute(async (rcon) => (await new DisableVotingCommand(enable).ExecuteCommand(rcon)).Successful, apiKey, serverId, true, false);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return;
            }

            throw;
        }
    }

    public async Task AttachmentMode(string apiKey, string serverId, bool enable)
    {
        try
        {
            await execute(async (rcon) => (await new AttachmentModeCommand(enable).ExecuteCommand(rcon)).Successful, apiKey, serverId, true, false);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return;
            }

            throw;
        }
    }

    public async Task UtilityTrails(string apiKey, string serverId, bool enable)
    {
        try
        {
            await execute(async (rcon) => (await new UtilityTrailsCommand(enable).ExecuteCommand(rcon)).Successful, apiKey, serverId, true, false);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return;
            }

            throw;
        }
    }

    public async Task KillFeedback(string apiKey, string serverId, bool enable)
    {
        try
        {
            await execute(async (rcon) => (await new KillFeedbackCommand(enable).ExecuteCommand(rcon)).Successful, apiKey, serverId, true, false);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return;
            }

            throw;
        }
    }

    public async Task SetTeamSkin(string apiKey, string serverId, int teamId, string? skinId = null)
    {
        try
        {
            await execute(async (rcon) => (await new SetTeamSkinCommand(teamId, skinId).ExecuteCommand(rcon)).Successful, apiKey, serverId, true, false);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return;
            }

            throw;
        }
    }

    public async Task SpawnLootCrate(string apiKey, string serverId, int? crateId = null)
    {
        try
        {
            await execute(async (rcon) => (await new SpawnLootCrateCommand(crateId).ExecuteCommand(rcon)).Successful, apiKey, serverId, true, false);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return;
            }

            throw;
        }
    }

    public async Task SpawnChickens(string apiKey, string serverId, int amount)
    {
        try
        {
            await execute(async (rcon) => (await new SpawnChickensCommand(amount).ExecuteCommand(rcon)).Successful, apiKey, serverId, true, false);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return;
            }

            throw;
        }
    }

    public async Task SpawnZombies(string apiKey, string serverId, int amount)
    {
        try
        {
            await execute(async (rcon) => (await new SpawnZombiesCommand(amount).ExecuteCommand(rcon)).Successful, apiKey, serverId, true, false);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return;
            }

            throw;
        }
    }

    public async Task RemoveZombies(string apiKey, string serverId)
    {
        try
        {
            await execute(async (rcon) => (await new RemoveZombiesCommand().ExecuteCommand(rcon)).Successful, apiKey, serverId, true, false);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return;
            }

            throw;
        }
    }

    public async Task SetVitality(string apiKey, string serverId, string target, int? health = null, int? armor = null, int? helmet = null)
    {
        try
        {
            await execute(async (rcon) => (await new SetVitalityCommand(target, health, armor, helmet).ExecuteCommand(rcon)).Successful, apiKey, serverId, true, false);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return;
            }

            throw;
        }
    }
    #endregion
}
