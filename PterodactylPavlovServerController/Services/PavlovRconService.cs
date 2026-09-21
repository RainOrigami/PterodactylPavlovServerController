using PavlovVR_Rcon;
using PavlovVR_Rcon.Exceptions;
using PavlovVR_Rcon.Models.Commands;
using PavlovVR_Rcon.Models.Pavlov;
using PavlovVR_Rcon.Models.Replies;
using PterodactylPavlovServerDomain.Rcon.Commands;
using System.Collections.Concurrent;

namespace PterodactylPavlovServerController.Services;

public class PavlovRconService
{
    private static readonly Dictionary<string, PavlovRcon> pavlovRconConnections = new();
    private readonly PterodactylService pterodactylService;
    private readonly IConfiguration configuration;

    /// <summary>
    /// Pavlov drops commands that arrive back to back, so every command for a server waits
    /// until this many milliseconds have passed since the previous one was sent.
    /// </summary>
    private readonly int commandIntervalMilliseconds;

    public PavlovRconService(PterodactylService pterodactylService, IConfiguration configuration)
    {
        this.pterodactylService = pterodactylService;
        this.configuration = configuration;
        this.commandIntervalMilliseconds = Math.Max(0, configuration.GetValue<int?>("rcon_command_interval_ms") ?? 60);
    }

    private readonly ConcurrentDictionary<string, SemaphoreSlim> serverCommandLocks = new();
    private readonly ConcurrentDictionary<string, SemaphoreSlim> serverPacingLocks = new();
    private readonly ConcurrentDictionary<string, DateTime> serverLastCommand = new();

    private SemaphoreSlim commandLock(string serverId) => this.serverCommandLocks.GetOrAdd(serverId, _ => new SemaphoreSlim(1, 1));
    private SemaphoreSlim pacingLock(string serverId) => this.serverPacingLocks.GetOrAdd(serverId, _ => new SemaphoreSlim(1, 1));

    private async Task delay(string serverId)
    {
        SemaphoreSlim pacing = this.pacingLock(serverId);
        await pacing.WaitAsync();
        try
        {
            DateTime last = this.serverLastCommand.TryGetValue(serverId, out DateTime previous) ? previous : DateTime.MinValue;
            int wait = this.commandIntervalMilliseconds - (int)(DateTime.Now - last).TotalMilliseconds;
            if (wait > 0)
            {
                await Task.Delay(wait);
            }

            this.serverLastCommand[serverId] = DateTime.Now;
        }
        finally
        {
            pacing.Release();
        }
    }

    private async Task<T> execute<T>(Func<PavlovRcon, Task<T>> action, string apiKey, string serverId, bool separateConnection, bool awaitResponse = true)
    {
        SemaphoreSlim? held = null;
        if (!separateConnection)
        {
            // One command at a time on the shared connection: a check-then-set flag let two
            // callers through at once, which is how replies ended up matched to the wrong command.
            held = this.commandLock(serverId);
            await held.WaitAsync();
        }

        try
        {
            await this.delay(serverId);

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
                        await Console.Out.WriteLineAsync("Disconnecting separate connection");
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

                throw;
            }

            return result;
        }
        finally
        {
            held?.Release();
        }
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

    public async Task<bool> GiveItem(string apiKey, string serverId, ulong uniqueId, Item item, bool separateConnection = false)
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

    public async Task<(bool success, bool gagged)> GagPlayer(string apiKey, string serverId, ulong uniqueId, bool? gag = null, bool separateConnection = false)
    {
        try
        {
            return await execute(async (rcon) =>
            {
                GagReply gagReply = await new GagCommand(uniqueId, gag).ExecuteCommand(rcon);
                return (true, gagReply.Gag);
            }, apiKey, serverId, separateConnection);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                // A refused gag returns the same false as a successful ungag,
                // so the caller needs the success flag to tell them apart.
                return (false, false);
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

    #region Base RCON commands added from the Pavlov RCON command reference

    public async Task<bool> Kill(string apiKey, string serverId, ulong uniqueId, bool separateConnection = false)
    {
        try
        {
            return await execute(async (rcon) => (await new KillCommand(uniqueId).ExecuteCommand(rcon)).Successful, apiKey, serverId, separateConnection);
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

    public async Task<bool> Teleport(string apiKey, string serverId, ulong sourceUniqueId, ulong targetUniqueId, bool separateConnection = false)
    {
        try
        {
            return await execute(async (rcon) => (await new TeleportCommand(sourceUniqueId, targetUniqueId).ExecuteCommand(rcon)).Successful, apiKey, serverId, separateConnection);
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

    public async Task<bool> SetTimeLimit(string apiKey, string serverId, int seconds, bool separateConnection = false)
    {
        try
        {
            return await execute(async (rcon) => (await new SetTimeLimitCommand(seconds).ExecuteCommand(rcon)).Successful, apiKey, serverId, separateConnection);
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

    public async Task<bool> SetMaxPlayers(string apiKey, string serverId, int maxPlayers, bool separateConnection = false)
    {
        try
        {
            return await execute(async (rcon) => (await new SetMaxPlayersCommand(maxPlayers).ExecuteCommand(rcon)).Successful, apiKey, serverId, separateConnection);
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

    public async Task<bool> UpdateServerName(string apiKey, string serverId, string serverName, bool separateConnection = false)
    {
        try
        {
            return await execute(async (rcon) => (await new UpdateServerNameCommand(serverName).ExecuteCommand(rcon)).Successful, apiKey, serverId, separateConnection);
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

    public async Task<bool> EnableCompMode(string apiKey, string serverId, bool enable, bool separateConnection = false)
    {
        try
        {
            return await execute(async (rcon) => (await new EnableCompModeCommand(enable).ExecuteCommand(rcon)).Successful, apiKey, serverId, separateConnection);
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

    public async Task<bool> EnableVerboseLogging(string apiKey, string serverId, bool enable, bool separateConnection = false)
    {
        try
        {
            return await execute(async (rcon) => (await new EnableVerboseLoggingCommand(enable).ExecuteCommand(rcon)).Successful, apiKey, serverId, separateConnection);
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

    public async Task<bool> EnableWhitelist(string apiKey, string serverId, bool enable, bool separateConnection = false)
    {
        try
        {
            return await execute(async (rcon) => (await new EnableWhitelistCommand(enable).ExecuteCommand(rcon)).Successful, apiKey, serverId, separateConnection);
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

    public async Task<bool> SetBotsEnabled(string apiKey, string serverId, bool enable, bool separateConnection = false)
    {
        try
        {
            return await execute(async (rcon) => (await new SetBotsEnabledCommand(enable).ExecuteCommand(rcon)).Successful, apiKey, serverId, separateConnection);
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

    public async Task<bool> ShutdownServer(string apiKey, string serverId, bool separateConnection = false)
    {
        try
        {
            return await execute(async (rcon) => (await new ShutdownServerCommand().ExecuteCommand(rcon)).Successful, apiKey, serverId, separateConnection);
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

    public async Task<bool> AddMapRotation(string apiKey, string serverId, string mapLabel, GameMode gameMode, bool separateConnection = false)
    {
        try
        {
            return await execute(async (rcon) => (await new AddMapRotationCommand(mapLabel, gameMode).ExecuteCommand(rcon)).Successful, apiKey, serverId, separateConnection);
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

    public async Task<bool> RemoveMapRotation(string apiKey, string serverId, string mapLabel, GameMode gameMode, bool separateConnection = false)
    {
        try
        {
            return await execute(async (rcon) => (await new RemoveMapRotationCommand(mapLabel, gameMode).ExecuteCommand(rcon)).Successful, apiKey, serverId, separateConnection);
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

    public async Task<bool> GiveAll(string apiKey, string serverId, int teamId, string item, bool separateConnection = false)
    {
        try
        {
            return await execute(async (rcon) => (await new GiveAllCommand(teamId, item).ExecuteCommand(rcon)).Successful, apiKey, serverId, separateConnection);
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

    public async Task<bool> TTTSetRole(string apiKey, string serverId, ulong uniqueId, string roleId, bool separateConnection = false)
    {
        try
        {
            return await execute(async (rcon) => (await new TTTSetRoleCommand(uniqueId, roleId).ExecuteCommand(rcon)).Successful, apiKey, serverId, separateConnection);
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

    public async Task<bool> TTTGiveCredits(string apiKey, string serverId, ulong uniqueId, int amount, bool separateConnection = false)
    {
        try
        {
            return await execute(async (rcon) => (await new TTTGiveCreditsCommand(uniqueId, amount).ExecuteCommand(rcon)).Successful, apiKey, serverId, separateConnection);
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

    public async Task<bool> UGCAddMod(string apiKey, string serverId, long modId, bool separateConnection = false)
    {
        try
        {
            return await execute(async (rcon) => (await new UGCAddModCommand(modId).ExecuteCommand(rcon)).Successful, apiKey, serverId, separateConnection);
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

    public async Task<bool> UGCRemoveMod(string apiKey, string serverId, long modId, bool separateConnection = false)
    {
        try
        {
            return await execute(async (rcon) => (await new UGCRemoveModCommand(modId).ExecuteCommand(rcon)).Successful, apiKey, serverId, separateConnection);
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

    public async Task<bool> UGCClearModList(string apiKey, string serverId, bool separateConnection = false)
    {
        try
        {
            return await execute(async (rcon) => (await new UGCClearModListCommand().ExecuteCommand(rcon)).Successful, apiKey, serverId, separateConnection);
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

    public async Task<string[]> UGCModList(string apiKey, string serverId, bool separateConnection = false)
    {
        try
        {
            return await execute(async (rcon) => (await new UGCModListCommand().ExecuteCommand(rcon)).ModList ?? Array.Empty<string>(), apiKey, serverId, separateConnection);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return Array.Empty<string>();
            }

            throw;
        }
    }

    public async Task<string[]> ItemList(string apiKey, string serverId, bool separateConnection = false)
    {
        try
        {
            return await execute(async (rcon) => (await new ItemListCommand().ExecuteCommand(rcon)).ItemList ?? Array.Empty<string>(), apiKey, serverId, separateConnection);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return Array.Empty<string>();
            }

            throw;
        }
    }

    public async Task<MapListEntry[]> MapList(string apiKey, string serverId, bool separateConnection = false)
    {
        try
        {
            return await execute(async (rcon) => (await new MapListCommand().ExecuteCommand(rcon)).MapList ?? Array.Empty<MapListEntry>(), apiKey, serverId, separateConnection);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return Array.Empty<MapListEntry>();
            }

            throw;
        }
    }

    public async Task<ulong[]> ModeratorList(string apiKey, string serverId, bool separateConnection = false)
    {
        try
        {
            return await execute(async (rcon) => (await new ModeratorListCommand().ExecuteCommand(rcon)).ModeratorList ?? Array.Empty<ulong>(), apiKey, serverId, separateConnection);
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return Array.Empty<ulong>();
            }

            throw;
        }
    }

    public async Task<PlayerDetail[]> InspectTeam(string apiKey, string serverId, int teamId, bool separateConnection = false)
    {
        return await execute(async (rcon) => (await new InspectTeamCommand(teamId).ExecuteCommand(rcon)).InspectList ?? Array.Empty<PlayerDetail>(), apiKey, serverId, separateConnection);
    }

    #endregion

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

    public Task Notify(string apiKey, string serverId, string target, string message, int? duration = null)
    {
        try
        {
            _ = execute(async (rcon) => (await new NotifyCommand(target, message, duration).ExecuteCommand(rcon)).Successful, apiKey, serverId, true, false);
            return Task.CompletedTask;
        }
        catch (CommandFailedException ex)
        {
            if (ex.InnerException == null)
            {
                return Task.CompletedTask;
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
