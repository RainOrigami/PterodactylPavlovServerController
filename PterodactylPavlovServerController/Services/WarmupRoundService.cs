using Microsoft.EntityFrameworkCore;
using PavlovVR_Rcon.Models.Pavlov;
using PterodactylPavlovServerController.Contexts;
using PterodactylPavlovServerController.Services.WarmupRoundLoadouts;
using PterodactylPavlovServerDomain.Models;
using PterodactylPavlovServerDomain.Rcon.Commands;
using System.Diagnostics;

namespace PterodactylPavlovServerController.Services;

public class WarmupRoundService
{
    private readonly string apiKey;
    private readonly PavlovRconConnection connection;
    private readonly PavlovRconService pavlovRconService;
    private readonly PavlovServerContext pavlovServerContext;

    private static readonly BaseLoadout[] loadouts = new BaseLoadout[]
    {
        new GrenadeOnlyLoadout(),
        new NewtonlauncherLoadout(),
        new Speedy50CalLoadout(),
        new KnifeOnlyLoadout(),
        new JohnWickLoadout(),
        new BallisticShieldLoadout(),
        new FlaregunLoadout(),
        new GoldenGunLoadout(),
        new M1GarandLoadout(),
        new RPGLoadout(),
        new TranquillizerLoadout(),
    };

    private string? lastMap = null;
    private string? lastRoundState = null;
    private bool mapJustChanged = false;
    private bool isWarmupRound = false;
    private bool isWarmupRoundEnding = false;
    private bool isWarmupRoundStarted = false;

    private DateTime? ungodmodeAt = null;
    private readonly List<ulong> alivePlayers = new();

    private List<BaseLoadout> usedLoadouts = new();
    private BaseLoadout? currentLoadout = null;

    // Server info arrives every second, but a warmup start or teardown takes several seconds. Overlapping
    // runs all compared against a lastRoundState that had not been written yet, so one Ended -> StandBy
    // transition could fire both the teardown and a fresh warmup start.
    private readonly SemaphoreSlim updateLock = new(1, 1);
    private volatile bool resetRequested = false;

    public WarmupRoundService(string apiKey, PavlovRconConnection connection, PavlovRconService pavlovRconService, IConfiguration configuration)
    {
        this.apiKey = apiKey;
        this.connection = connection;
        this.pavlovRconService = pavlovRconService;
        this.pavlovServerContext = new(configuration);

        this.connection.OnServerInfoUpdated += this.Connection_OnServerInfoUpdated;
        this.connection.OnServerOnlineStateChanged += this.Connection_OnServerOnlineStateChanged;
    }

    private void Connection_OnServerOnlineStateChanged(string serverId)
    {
        // A server restart can come back on the same map, which the map change check would not notice
        if (this.connection.Online != true)
        {
            this.resetRequested = true;
        }
    }

    private async void Connection_OnServerInfoUpdated(string serverId)
    {
        // Skip the tick while a previous run is still busy. The next run compares against the last state
        // that was actually processed, so a transition that completes in the meantime is still seen.
        if (!this.updateLock.Wait(0))
        {
            return;
        }

        try
        {
            await this.processServerInfo();
        }
        catch (Exception ex)
        {
            await Console.Out.WriteLineAsync($"Warmup round update failed on {this.connection.ServerId}: {ex.Message}");
        }
        finally
        {
            this.updateLock.Release();
        }
    }

    private async Task processServerInfo()
    {
        // ServerInfo is replaced on every poll; work from one snapshot so the state stored at the end
        // is the state the decisions were made on, not whatever arrived while this run was busy
        ServerInfo serverInfo = this.connection.ServerInfo!;

        ServerSettings? warmupEnabled = await this.pavlovServerContext.Settings.FirstOrDefaultAsync(s => s.ServerId == connection.ServerId && s.SettingName == ServerSettings.SETTING_WARMUP_ENABLED);

        //await Console.Out.WriteLineAsync($"Pre-states: GameMode: {serverInfo.GameMode}, RoundState: {serverInfo.RoundState}, MapLabel: {serverInfo.MapLabel}, WarmupEnabled: {warmupEnabled?.SettingValue}, lastMap: {lastMap}, lastRoundState: {lastRoundState}, isWarmupRound: {isWarmupRound}, isWarmupRoundEnding: {isWarmupRoundEnding}, isWarmupRoundStarted: {isWarmupRoundStarted}, mapJustChanged: {mapJustChanged}");

        if (this.resetRequested)
        {
            this.resetRequested = false;
            await Console.Out.WriteLineAsync($"Server {this.connection.ServerId} was offline, discarding warmup state");
            await this.resetWarmupState();

            // A restart begins a new match; the 0-0 score guard keeps this from firing after a mere blip
            mapJustChanged = true;
        }

        // Checked before the game mode filter: a warmup never carries over to another map, and a DM map in
        // between must not leave a half-finished warmup waiting to tear down the next SND map's first round
        if (lastMap != null && serverInfo.MapLabel != lastMap)
        {
            await this.resetWarmupState();
            mapJustChanged = true;
            await Console.Out.WriteLineAsync($"Map just changed! mapJustChanged: {mapJustChanged}");
        }

        if (lastMap == null || lastRoundState == null || serverInfo.GameMode.ToUpper() != "SND" || warmupEnabled == null || !bool.TryParse(warmupEnabled.SettingValue, out bool isWarmupEnabled) || !isWarmupEnabled)
        {
            //await Console.Out.WriteLineAsync("Abort because ignored state");

            lastMap = serverInfo.MapLabel;
            lastRoundState = serverInfo.RoundState;
            return;
        }

        //await Console.Out.WriteLineAsync("Open state");

        if (this.connection.PlayerListPlayers?.Count == 0)
        {
            // Keep tracking while empty, otherwise the first transition after someone joins is compared
            // against a round state from before everyone left
            lastMap = serverInfo.MapLabel;
            lastRoundState = serverInfo.RoundState;
            return;
        }

        if (this.ungodmodeAt != null)
        {
            if (this.connection.PlayerDetails != null)
            {
                // Suicide detection
                ulong[] currentAlivePlayers = this.connection.PlayerDetails.Values.Where(p => !p.Dead).Select(p => p.UniqueId).ToArray();
                ulong[] playersNotAliveAnymore = this.alivePlayers.Except(currentAlivePlayers).ToArray();
                foreach (ulong playerId in playersNotAliveAnymore)
                {
                    try
                    {
                        await this.pavlovRconService.Godmode(this.apiKey, this.connection.ServerId, playerId.ToString(), false);
                    }
                    catch { }
                }

                this.alivePlayers.Clear();
                this.alivePlayers.AddRange(currentAlivePlayers);
            }

            if (DateTime.Now > this.ungodmodeAt)
            {
                await this.pavlovRconService.Godmode(this.apiKey, this.connection.ServerId, "All", false);
                this.ungodmodeAt = null;
            }
        }

        //await Console.Out.WriteLineAsync($"For safety: Team1Score: {serverInfo.Team1Score}, Team0Score: {serverInfo.Team0Score}");

        if (lastRoundState == "Ended" && serverInfo.RoundState == "StandBy" && mapJustChanged && !isWarmupRound && /* for safety */ serverInfo.Team1Score == 0 && serverInfo.Team0Score == 0)
        {
            await Console.Out.WriteLineAsync("Warmup round start");

            mapJustChanged = false;
            isWarmupRound = true;

            BaseLoadout[] loadouts = WarmupRoundService.loadouts.Where(l => !usedLoadouts.Contains(l)).ToArray();
            if (loadouts.Length == 0)
            {
                usedLoadouts.Clear();
                loadouts = WarmupRoundService.loadouts;
            }
            if (loadouts.Length > 0)
            {
                currentLoadout = loadouts[Random.Shared.Next(loadouts.Count())];
                usedLoadouts.Add(currentLoadout);

                try
                {
                    await Task.Run(async () =>
                    {
                        await this.pavlovRconService.Notify(apiKey, connection.ServerId, "All", $"- WARMUP ROUND - CURRENT LOADOUT: {currentLoadout.Name}", 10);
                        await Task.Delay(15);

                        await currentLoadout.EnableRound(this.pavlovRconService, apiKey, connection.ServerId);

                        List<ulong> playersToEquip = new();

                        // Wait for players to spawn or at least 2 seconds
                        Stopwatch stopwatch = Stopwatch.StartNew();
                        int waitForPlayersTimeout = 0;
                        while (playersToEquip.Count == 0 && waitForPlayersTimeout < 16)
                        {
                            try
                            {
                                PlayerDetail[] players = await this.pavlovRconService.GetActivePlayerDetails(apiKey, connection.ServerId);
                                playersToEquip = players
                                    .Where(p => !p.Dead)
                                    .Select(p => p.UniqueId)
                                    .ToList();

                                if (players.Any(p => p.Dead))
                                {
                                    await Task.Delay(250);
                                    waitForPlayersTimeout++;
                                }
                            }
                            catch (Exception ex)
                            {
                                await Console.Out.WriteLineAsync($"Error while waiting for players to spawn: {ex.Message}");
                                await Task.Delay(250);
                                waitForPlayersTimeout++;
                            }
                        }
                        await Task.Delay(Math.Max(2000 - (int)stopwatch.ElapsedMilliseconds, 0));

                        // Clean round start
                        await this.pavlovRconService.EnableBuyMenu(apiKey, connection.ServerId, "All", false);
                        for (int i = 0; i < 2; i++)
                        {
                            foreach (ulong uniqueId in playersToEquip)
                            {
                                await RconRetry.Ensure($"SetCash 0 for {uniqueId} on {connection.ServerId}", () => this.pavlovRconService.SetCash(apiKey, connection.ServerId, uniqueId, 0));
                            }
                            await this.pavlovRconService.DropItems(apiKey, connection.ServerId, "All");
                            await this.pavlovRconService.CleanUp(apiKey, connection.ServerId, RconObjectType.All);
                        }

                        await currentLoadout.EnablePlayers(this.pavlovRconService, apiKey, connection.ServerId);
                        await this.pavlovRconService.Godmode(apiKey, connection.ServerId, "All", true);

                        // Only equip players who are still on the server, so a disconnect during
                        // setup does not burn retries on someone who will never receive anything.
                        ulong[] stillConnected = await this.getConnectedPlayers();
                        List<ulong> notFullyEquipped = new();
                        foreach (ulong uniqueId in playersToEquip)
                        {
                            if (stillConnected.Length > 0 && !stillConnected.Contains(uniqueId))
                            {
                                await Console.Out.WriteLineAsync($"Skipping loadout for {uniqueId}: no longer on the server");
                                continue;
                            }

                            if (!await currentLoadout.EnablePlayer(this.pavlovRconService, apiKey, connection.ServerId, uniqueId))
                            {
                                notFullyEquipped.Add(uniqueId);
                            }
                        }

                        if (notFullyEquipped.Count > 0)
                        {
                            await Console.Out.WriteLineAsync($"Warmup loadout incomplete for {notFullyEquipped.Count} player(s): {string.Join(", ", notFullyEquipped)}");
                        }

                        playersToEquip.Clear();
                    });
                }
                catch (Exception ex)
                {
                    await Console.Out.WriteLineAsync($"Warmup round setup failed: {ex.Message}");

                    // Roll back so this loadout can be drawn again and the warmup can retry
                    if (currentLoadout != null)
                    {
                        usedLoadouts.Remove(currentLoadout);
                        currentLoadout = null;
                    }

                    isWarmupRound = false;
                    isWarmupRoundStarted = false;
                    isWarmupRoundEnding = false;

                    // Re-arm: the 0-0 score guard keeps this from firing mid-match
                    mapJustChanged = true;
                }
            }

            if (isWarmupRound)
            {
                await Console.Out.WriteLineAsync("Warmup round loadout set");
            }
        }

        if (lastRoundState == "StandBy" && serverInfo.RoundState == "Started" && isWarmupRound && !isWarmupRoundStarted)
        {
            await Console.Out.WriteLineAsync("Warmup round started");

            isWarmupRoundStarted = true;
            this.ungodmodeAt = DateTime.Now.AddSeconds(4);

            //_ = Task.Run(async () =>
            //{
            //    await Task.Delay(4000);
            //    if (this.isWarmupRound && this.connection.ServerInfo.RoundState == "Started" && isWarmupRoundStarted)
            //    {
            //        await this.pavlovRconService.Godmode(apiKey, connection.ServerId, "All", false);
            //    }
            //});
        }

        if (lastRoundState == "Started" && serverInfo.RoundState == "Ended" && isWarmupRound)
        {
            await Console.Out.WriteLineAsync("Warmup round ending");
            isWarmupRoundEnding = true;
            isWarmupRoundStarted = false;
            //await Console.Out.WriteLineAsync("Vankrupt default");
            //await this.pavlovRconService.SetBalanceTableURL(apiKey, connection.ServerId, "vankruptgames/BalancingTable/Beta_5.1");
        }

        if (lastRoundState == "Ended" && serverInfo.RoundState == "StandBy" && isWarmupRound && isWarmupRoundEnding)
        {
            await Console.Out.WriteLineAsync("Warmup round ended, SND Reset");

            isWarmupRoundEnding = false;
            isWarmupRound = false;
            Stopwatch stopwatch = Stopwatch.StartNew();
            if (currentLoadout != null)
            {
                await currentLoadout.DisablePlayers(this.pavlovRconService, apiKey, connection.ServerId);
                Stopwatch playerStopwatch = new();
                foreach (ulong playerId in (await this.pavlovRconService.GetActivePlayerDetails(apiKey, connection.ServerId)).Select(p => p.UniqueId))
                {
                    playerStopwatch.Restart();
                    await currentLoadout.DisablePlayer(this.pavlovRconService, apiKey, connection.ServerId, playerId);
                    await Task.Delay(Math.Max(50 - (int)playerStopwatch.ElapsedMilliseconds, 0));
                }
                await currentLoadout.DisableRound(this.pavlovRconService, apiKey, connection.ServerId);
                await this.pavlovRconService.Notify(apiKey, connection.ServerId, "All", "MATCH BEGINS NOW", 10);
                currentLoadout = null;
            }
            await this.pavlovRconService.Godmode(apiKey, connection.ServerId, "All", false);
            await this.pavlovRconService.EnableBuyMenu(apiKey, connection.ServerId, "All", true);
            stopwatch.Stop();
            await Task.Delay(Math.Max(5000 - (int)stopwatch.ElapsedMilliseconds, 50));
            await this.pavlovRconService.ResetSND(apiKey, connection.ServerId);
        }

        lastMap = serverInfo.MapLabel;
        lastRoundState = serverInfo.RoundState;
    }

    /// <summary>
    /// Drops any warmup in progress. If a loadout was still active its server-wide settings (gravity,
    /// game speed, ...) are put back, since those may outlive the round that set them.
    /// </summary>
    private async Task resetWarmupState()
    {
        if (this.currentLoadout != null)
        {
            await Console.Out.WriteLineAsync($"Discarding unfinished warmup round ({this.currentLoadout.Name}) on {this.connection.ServerId}");

            try
            {
                await this.currentLoadout.DisableRound(this.pavlovRconService, this.apiKey, this.connection.ServerId);
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync($"Could not restore round settings after {this.currentLoadout.Name}: {ex.Message}");
            }
        }

        this.currentLoadout = null;
        this.isWarmupRound = false;
        this.isWarmupRoundStarted = false;
        this.isWarmupRoundEnding = false;
        this.mapJustChanged = false;
        this.ungodmodeAt = null;
        this.alivePlayers.Clear();
    }

    /// <summary>
    /// Best-effort list of players currently on the server. An empty array means the query failed,
    /// in which case callers should carry on rather than skip everyone.
    /// </summary>
    private async Task<ulong[]> getConnectedPlayers()
    {
        try
        {
            return (await this.pavlovRconService.GetActivePlayerDetails(this.apiKey, this.connection.ServerId)).Select(p => p.UniqueId).ToArray();
        }
        catch (Exception ex)
        {
            await Console.Out.WriteLineAsync($"Could not refresh the player list before equipping: {ex.Message}");
            return Array.Empty<ulong>();
        }
    }
}
