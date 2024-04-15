using Microsoft.EntityFrameworkCore;
using PavlovVR_Rcon.Models.Pavlov;
using PterodactylPavlovServerController.Contexts;
using PterodactylPavlovServerDomain.Models;
using System.Numerics;

namespace PterodactylPavlovServerController.Services;

public class WarmupRoundService
{
    private readonly string apiKey;
    private readonly PavlovRconConnection connection;
    private readonly PavlovRconService pavlovRconService;
    private readonly PavlovServerContext pavlovServerContext;

    private string? lastMap = null;
    private string? lastRoundState = null;
    private bool mapJustChanged = false;
    private bool isWarmupRound = false;
    private bool isWarmupRoundEnding = false;
    private bool isWarmupRoundStarted = false;

    private List<string> usedLoadouts = new();

    public WarmupRoundService(string apiKey, PavlovRconConnection connection, PavlovRconService pavlovRconService, IConfiguration configuration)
    {
        this.apiKey = apiKey;
        this.connection = connection;
        this.pavlovRconService = pavlovRconService;
        this.pavlovServerContext = new(configuration);

        this.connection.OnServerInfoUpdated += this.Connection_OnServerInfoUpdated;
    }

    private async void Connection_OnServerInfoUpdated(string serverId)
    {
        ServerSettings? warmupEnabled = await this.pavlovServerContext.Settings.FirstOrDefaultAsync(s => s.ServerId == connection.ServerId && s.SettingName == ServerSettings.SETTING_WARMUP_ENABLED);
        
        //await Console.Out.WriteLineAsync($"Pre-states: GameMode: {this.connection.ServerInfo!.GameMode}, RoundState: {this.connection.ServerInfo.RoundState}, MapLabel: {this.connection.ServerInfo.MapLabel}, WarmupEnabled: {warmupEnabled?.SettingValue}, lastMap: {lastMap}, lastRoundState: {lastRoundState}, isWarmupRound: {isWarmupRound}, isWarmupRoundEnding: {isWarmupRoundEnding}, isWarmupRoundStarted: {isWarmupRoundStarted}, mapJustChanged: {mapJustChanged}");

        if (lastMap == null || lastRoundState == null || this.connection.ServerInfo!.GameMode.ToUpper() != "SND" || warmupEnabled == null || !bool.TryParse(warmupEnabled.SettingValue, out bool isWarmupEnabled) || !isWarmupEnabled)
        {
            //await Console.Out.WriteLineAsync("Abort because ignored state");

            lastMap = this.connection.ServerInfo!.MapLabel;
            lastRoundState = this.connection.ServerInfo.RoundState;
            return;
        }

        //await Console.Out.WriteLineAsync("Open state");

        if (this.connection.ServerInfo!.MapLabel != lastMap)
        {
            mapJustChanged = true;
            await Console.Out.WriteLineAsync($"Map just changed! mapJustChanged: {mapJustChanged}");
        }

        //await Console.Out.WriteLineAsync($"For safety: Team1Score: {this.connection.ServerInfo.Team1Score}, Team0Score: {this.connection.ServerInfo.Team0Score}");

        if (lastRoundState == "Ended" && this.connection.ServerInfo!.RoundState == "StandBy" && mapJustChanged && !isWarmupRound && /* for safety */ this.connection.ServerInfo.Team1Score == 0 && this.connection.ServerInfo.Team0Score == 0)
        {
            await Console.Out.WriteLineAsync("Warmup round start");

            mapJustChanged = false;
            isWarmupRound = true;

            //await this.pavlovRconService.SetBalanceTableURL(apiKey, connection.ServerId, "vankruptgames/BalancingTable/Beta_5.1");
            //await this.pavlovRconService.SetBalanceTableURL(apiKey, connection.ServerId, "RainOrigami/PPSCBalancingTable/Warmup_NoTK");
            //await Console.Out.WriteLineAsync("Warmup_NoTK");

            WarmupRoundLoadoutModel[] loadouts = this.pavlovServerContext.WarmupLoadouts.Where(l => l.ServerId == connection.ServerId && !usedLoadouts.Contains(l.Name)).ToArray();
            if (loadouts.Length == 0)
            {
                usedLoadouts.Clear();
                loadouts = this.pavlovServerContext.WarmupLoadouts.Where(l => l.ServerId == connection.ServerId).ToArray();
            }
            if (loadouts.Length > 0)
            {
                WarmupRoundLoadoutModel loadout = loadouts[Random.Shared.Next(loadouts.Count())];
                usedLoadouts.Add(loadout.Name);

                await Task.Run(async () =>
                {
                    List<ulong> playersToEquip = new();

                    int waitForPlayersTimeout = 0;
                    while (playersToEquip.Count == 0 && waitForPlayersTimeout < 16)
                    {
                        playersToEquip = (await this.pavlovRconService.GetActivePlayerDetails(apiKey, connection.ServerId))
                    .Where(p => !p.Dead)
                        .Select(p => p.UniqueId)
                        .ToList();

                        if (playersToEquip.Count == 0)
                        {
                            await Task.Delay(250);
                            waitForPlayersTimeout++;
                        }
                    }

                    List<ulong> equippedPlayers = new();

                    while (playersToEquip.Count > 0)
                    {
                        foreach (ulong uniqueId in playersToEquip)
                        {
                            await this.pavlovRconService.SetCash(apiKey, connection.ServerId, uniqueId, 0);
                            await Task.Delay(25);
                        }

                        foreach (ulong uniqueId in playersToEquip)
                        {
                            try
                            {
                                await this.pavlovRconService.GiveItem(apiKey, connection.ServerId, uniqueId, loadout.Gun!.Value.ToString());
                            }
                            catch { }
                            await Task.Delay(50);
                            try
                            {
                                await this.pavlovRconService.GiveItem(apiKey, connection.ServerId, uniqueId, loadout.Item!.Value.ToString());
                            }
                            catch { }
                            await Task.Delay(50);
                            try
                            {
                                await this.pavlovRconService.GiveItem(apiKey, connection.ServerId, uniqueId, loadout.Attachment!.Value.ToString());
                            }
                            catch { }
                            await Task.Delay(50);
                            equippedPlayers.Add(uniqueId);
                        }
                        playersToEquip.Clear();

                        waitForPlayersTimeout = 0;
                        while (playersToEquip.Count == 0 && waitForPlayersTimeout < 16)
                        {
                            playersToEquip = (await this.pavlovRconService.GetActivePlayerDetails(apiKey, connection.ServerId))
                            .Where(p => !p.Dead && !equippedPlayers.Contains(p.UniqueId))
                            .Select(p => p.UniqueId)
                            .ToList();

                            if (playersToEquip.Count == 0)
                            {
                                await Task.Delay(250);
                                waitForPlayersTimeout++;
                            }
                        }
                    }
                });
            }

            await Console.Out.WriteLineAsync("Warmup round loadout set");
        }

        if (lastRoundState == "StandBy" && this.connection.ServerInfo.RoundState == "Started" && isWarmupRound && !isWarmupRoundStarted)
        {
            await Console.Out.WriteLineAsync("Warmup round started");

            isWarmupRoundStarted = true;

#pragma warning disable CS4014
            Task.Run(async () =>
#pragma warning restore CS4014
            {
                await Task.Delay(4000);
                if (this.isWarmupRound && this.connection.ServerInfo.RoundState == "Started" && isWarmupRoundStarted)
                {
                    await Console.Out.WriteLineAsync("4-second delay NOP");
                    //await Console.Out.WriteLineAsync("Warmup_NoStarterPistol");
                    //await this.pavlovRconService.SetBalanceTableURL(apiKey, connection.ServerId, "RainOrigami/PPSCBalancingTable/Warmup_NoStarterPistol");
                }
            });
        }

        if (lastRoundState == "Started" && this.connection.ServerInfo.RoundState == "Ended" && isWarmupRound)
        {
            await Console.Out.WriteLineAsync("Warmup round ending");
            isWarmupRoundEnding = true;
            isWarmupRoundStarted = false;
            //await Console.Out.WriteLineAsync("Vankrupt default");
            //await this.pavlovRconService.SetBalanceTableURL(apiKey, connection.ServerId, "vankruptgames/BalancingTable/Beta_5.1");
        }

        if (lastRoundState == "Ended" && this.connection.ServerInfo!.RoundState == "StandBy" && isWarmupRound && isWarmupRoundEnding)
        {
            await Console.Out.WriteLineAsync("Warmup round ended, SND Reset");

            isWarmupRoundEnding = false;
            isWarmupRound = false;
            await Task.Delay(1000);
            await this.pavlovRconService.ResetSND(apiKey, connection.ServerId);
        }

        lastMap = this.connection.ServerInfo!.MapLabel;
        lastRoundState = this.connection.ServerInfo.RoundState;
    }
}
