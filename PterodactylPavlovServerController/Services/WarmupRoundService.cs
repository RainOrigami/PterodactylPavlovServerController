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
    private readonly IConfiguration configuration;
    private readonly PavlovServerContext pavlovServerContext;

    private string? lastMap = null;
    private string? lastRoundState = null;
    private bool mapJustChanged = false;
    private bool isWarmupRound = false;

    public WarmupRoundService(string apiKey, PavlovRconConnection connection, PavlovRconService pavlovRconService, IConfiguration configuration)
    {
        this.apiKey = apiKey;
        this.connection = connection;
        this.pavlovRconService = pavlovRconService;
        this.configuration = configuration;
        this.pavlovServerContext = new(configuration);

        this.connection.OnServerInfoUpdated += this.Connection_OnServerInfoUpdated;
    }

    private async void Connection_OnServerInfoUpdated(string serverId)
    {
        if (connection.ServerId == "69b945c4")
            Console.WriteLine($"{connection.ServerId} Connection_OnServerInfoUpdated");
        ServerSettings? warmupEnabled = await this.pavlovServerContext.Settings.FirstOrDefaultAsync(s => s.ServerId == connection.ServerId && s.SettingName == ServerSettings.SETTING_WARMUP_ENABLED);
        if (lastMap == null || lastRoundState == null || this.connection.ServerInfo!.GameMode != "SND" || warmupEnabled == null || !bool.TryParse(warmupEnabled.SettingValue, out bool isWarmupEnabled) || !isWarmupEnabled)
        {
            if (connection.ServerId == "69b945c4")
                Console.WriteLine($"{connection.ServerId} Not applicable");
            lastMap = this.connection.ServerInfo!.MapLabel;
            lastRoundState = this.connection.ServerInfo.RoundState;
            return;
        }

        if (connection.ServerId == "69b945c4")
            Console.WriteLine($"{connection.ServerId} Applicable");

        if (this.connection.ServerInfo!.MapLabel != lastMap)
        {
            if (connection.ServerId == "69b945c4")
                Console.WriteLine($"{connection.ServerId} Map just changed!");
            mapJustChanged = true;
        }

        if (lastRoundState == "Ended" && this.connection.ServerInfo!.RoundState == "StandBy" && mapJustChanged && !isWarmupRound && /* for safety */ this.connection.ServerInfo.Team1Score == 0 && this.connection.ServerInfo.Team0Score == 0)
        {
            if (connection.ServerId == "69b945c4")
                Console.WriteLine($"{connection.ServerId} Warmup round detected!");
            mapJustChanged = false;
            isWarmupRound = true;

            List<Item> warmupItems = this.pavlovServerContext.WarmupItems.Where(w => w.ServerId == connection.ServerId).Select(w => w.Item).ToList();
            if (warmupItems.Count > 0)
            {
                Player[] players = await this.pavlovRconService.GetActivePlayers(apiKey, connection.ServerId);
                if (connection.ServerId == "69b945c4")
                    Console.WriteLine($"{connection.ServerId} Randomly giving {warmupItems.Count} different items to {players.Length} players");
                foreach (Player player in players.Where(p => p.UniqueId.HasValue))
                {
                    await this.pavlovRconService.GiveItem(apiKey, connection.ServerId, player.UniqueId!.Value, warmupItems[Random.Shared.Next(0, warmupItems.Count)].ToString());
                }
            }
        }

        if (lastRoundState == "Started" && this.connection.ServerInfo.RoundState == "Ended" && isWarmupRound)
        {
            if (connection.ServerId == "69b945c4")
                Console.WriteLine($"{connection.ServerId} Warmup round ending detected, resetting SND");
            isWarmupRound = false;
            await this.pavlovRconService.ResetSND(apiKey, connection.ServerId);
        }

        if (connection.ServerId == "69b945c4")
            Console.WriteLine($"{connection.ServerId} End.");

        lastMap = this.connection.ServerInfo!.MapLabel;
        lastRoundState = this.connection.ServerInfo.RoundState;
    }
}
