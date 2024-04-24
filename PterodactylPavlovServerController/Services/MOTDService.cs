using Microsoft.EntityFrameworkCore;
using PterodactylPavlovServerController.Contexts;
using PterodactylPavlovServerDomain.Models;

namespace PterodactylPavlovServerController.Services;

public class MOTDService
{
    private readonly string apiKey;
    private readonly PavlovRconConnection connection;
    private readonly PavlovRconService pavlovRconService;
    private readonly PavlovServerContext pavlovServerContext;

    private List<ulong> onlinePlayers = new();

    public MOTDService(string apiKey, PavlovRconConnection connection, PavlovRconService pavlovRconService, IConfiguration configuration)
    {
        this.apiKey = apiKey;
        this.connection = connection;
        this.pavlovRconService = pavlovRconService;
        this.connection.OnServerInfoUpdated += this.Connection_OnServerInfoUpdated;
        this.pavlovServerContext = new(configuration);
        this.pavlovServerContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    private void Connection_OnServerInfoUpdated(string serverId)
    {
        if (this.onlinePlayers.Count == 0)
        {
            return;
        }

        ServerSettings? motdEnabledSetting = this.pavlovServerContext.Settings.FirstOrDefault(s => s.ServerId == serverId && s.SettingName == ServerSettings.SETTING_MOTD_ENABLED);
        ServerSettings? motdMessageSetting = this.pavlovServerContext.Settings.FirstOrDefault(s => s.ServerId == serverId && s.SettingName == ServerSettings.SETTING_MOTD_MESSAGE);

        if (motdEnabledSetting == null || motdMessageSetting == null || !bool.TryParse(motdEnabledSetting.SettingValue, out bool motdEnabled) || !motdEnabled)
        {
            return;
        }

        List<ulong> players = this.connection.PlayerListPlayers?.Keys.ToList() ?? new List<ulong>();
        foreach (ulong newPlayer in players.Except(this.onlinePlayers))
        {
            _ = this.pavlovRconService.Notify(this.apiKey, serverId, newPlayer.ToString(), motdMessageSetting.SettingValue, 20);
        }

        this.onlinePlayers = players;
    }
}
