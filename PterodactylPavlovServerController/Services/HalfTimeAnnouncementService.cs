using Microsoft.EntityFrameworkCore;
using PterodactylPavlovServerController.Contexts;
using PterodactylPavlovServerDomain.Models;

namespace PterodactylPavlovServerController.Services;

public class HalfTimeAnnouncementService
{
    private readonly string apiKey;
    private readonly PavlovRconConnection connection;
    private readonly PavlovRconService pavlovRconService;
    private readonly PavlovServerContext pavlovServerContext;

    private DateTime lastAnnouncement = DateTime.MinValue;

    public HalfTimeAnnouncementService(string apiKey, PavlovRconConnection connection, PavlovRconService pavlovRconService, IConfiguration configuration)
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
        // Only works in SND mode, with a score of 9, and the round has ended and it's been at least a minute since the last announcement
        if (this.connection.ServerInfo == null || this.connection.ServerInfo.GameMode.ToUpper() != "SND" || this.connection.ServerInfo.Team0Score + this.connection.ServerInfo.Team1Score != 9 || this.connection.ServerInfo.RoundState != "Ended" || DateTime.Now - this.lastAnnouncement < TimeSpan.FromMinutes(1) || this.connection.PlayerListPlayers == null)
        {
            return;
        }

        ServerSettings? halfTimeAnnouncementEnabledSetting = this.pavlovServerContext.Settings.FirstOrDefault(s => s.ServerId == serverId && s.SettingName == ServerSettings.SETTING_HALF_TIME_ANNOUNCEMENT_ENABLED);
        ServerSettings? halfTimeAnnouncementMessageSetting = this.pavlovServerContext.Settings.FirstOrDefault(s => s.ServerId == serverId && s.SettingName == ServerSettings.SETTING_HALF_TIME_ANNOUNCEMENT_MESSAGE);

        if (halfTimeAnnouncementEnabledSetting == null || halfTimeAnnouncementMessageSetting == null || !bool.TryParse(halfTimeAnnouncementEnabledSetting.SettingValue, out bool halfTimeAnnouncementEnabled) || !halfTimeAnnouncementEnabled || string.IsNullOrEmpty(halfTimeAnnouncementMessageSetting.SettingValue))
        {
            return;
        }

        this.lastAnnouncement = DateTime.Now;

        _ = this.pavlovRconService.Notify(this.apiKey, serverId, "All", halfTimeAnnouncementMessageSetting.SettingValue, 10);
    }
}
