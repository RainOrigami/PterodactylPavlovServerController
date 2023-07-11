using Microsoft.EntityFrameworkCore;
using PavlovVR_Rcon.Models.Pavlov;
using PterodactylPavlovServerController.Contexts;
using PterodactylPavlovServerDomain.Models;

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
        if (lastMap == null || lastRoundState == null || this.connection.ServerInfo!.GameMode.ToUpper() != "SND" || warmupEnabled == null || !bool.TryParse(warmupEnabled.SettingValue, out bool isWarmupEnabled) || !isWarmupEnabled)
        {
            lastMap = this.connection.ServerInfo!.MapLabel;
            lastRoundState = this.connection.ServerInfo.RoundState;
            return;
        }

        if (this.connection.ServerInfo!.MapLabel != lastMap)
        {
            mapJustChanged = true;
        }

        if (lastRoundState == "Ended" && this.connection.ServerInfo!.RoundState == "StandBy" && mapJustChanged && !isWarmupRound && /* for safety */ this.connection.ServerInfo.Team1Score == 0 && this.connection.ServerInfo.Team0Score == 0)
        {
            mapJustChanged = false;
            isWarmupRound = true;

            WarmupRoundLoadoutModel[] loadouts = this.pavlovServerContext.WarmupLoadouts.Where(l => l.ServerId == connection.ServerId).ToArray();
            if (loadouts.Length > 0)
            {
                WarmupRoundLoadoutModel loadout = loadouts[Random.Shared.Next(loadouts.Count())];

                await Task.Run(async () =>
                {
                    await Task.Delay(1000);
                    Player[] players = await this.pavlovRconService.GetActivePlayers(apiKey, connection.ServerId);
                    foreach (Player player in players.Where(p => p.UniqueId.HasValue))
                    {
                        await this.pavlovRconService.SetCash(apiKey, connection.ServerId, player.UniqueId!.Value, 0);
                        await Task.Delay(50);
                        try
                        {
                            await this.pavlovRconService.GiveItem(apiKey, connection.ServerId, player.UniqueId!.Value, loadout.Gun!.Value.ToString());
                        }
                        catch { }
                        await Task.Delay(50);
                        try
                        {
                            await this.pavlovRconService.GiveItem(apiKey, connection.ServerId, player.UniqueId!.Value, loadout.Item!.Value.ToString());
                        }
                        catch { }
                        await Task.Delay(50);
                        try
                        {
                            await this.pavlovRconService.GiveItem(apiKey, connection.ServerId, player.UniqueId!.Value, loadout.Attachment!.Value.ToString());
                        }
                        catch { }
                        await Task.Delay(50);
                    }
                });
            }
        }

        if (lastRoundState == "Started" && this.connection.ServerInfo.RoundState == "Ended" && isWarmupRound)
        {
            isWarmupRoundEnding = true;
        }

        if (lastRoundState == "Ended" && this.connection.ServerInfo!.RoundState == "StandBy" && isWarmupRound && isWarmupRoundEnding)
        {
            isWarmupRoundEnding = false;
            isWarmupRound = false;
            await Task.Delay(1000);
            await this.pavlovRconService.ResetSND(apiKey, connection.ServerId);
        }

        lastMap = this.connection.ServerInfo!.MapLabel;
        lastRoundState = this.connection.ServerInfo.RoundState;
    }
}
