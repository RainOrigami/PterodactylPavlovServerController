using Microsoft.EntityFrameworkCore;
using PavlovVR_Rcon.Models.Pavlov;
using PterodactylPavlovServerController.Contexts;
using PterodactylPavlovServerDomain.Models;

namespace PterodactylPavlovServerController.Services;

public class PavlovPingLimiterService
{
    private readonly string apiKey;
    private readonly PavlovRconConnection connection;
    private readonly PavlovRconService pavlovRconService;
    private readonly PavlovServerContext pavlovServerContext;
    private readonly AuditService auditService;

    private readonly Dictionary<ulong, Queue<(DateTime Timestamp, int Ping)>> playerPingHistory = new();

    public PavlovPingLimiterService(string apiKey, PavlovRconConnection connection, PavlovRconService pavlovRconService, AuditService auditService, IConfiguration configuration)
    {
        this.apiKey = apiKey;
        this.connection = connection;
        this.pavlovRconService = pavlovRconService;
        this.auditService = auditService;

        this.connection.OnServerInfoUpdated += Connection_OnServerInfoUpdated;
        this.pavlovServerContext = new(configuration);
        this.pavlovServerContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    private async void Connection_OnServerInfoUpdated(string serverId)
    {
        if (this.connection.PlayerDetails == null || this.connection.PlayerDetails.Count == 0 || this.connection.PlayerListPlayers == null || this.connection.PlayerListPlayers.Count == 0)
        {
            return;
        }

        ServerSettings? pingKickEnabledSetting = await this.pavlovServerContext.Settings.FirstOrDefaultAsync(s => s.ServerId == this.connection.ServerId && s.SettingName == ServerSettings.SETTING_PING_KICK_ENABLED);
        if (pingKickEnabledSetting == null || !bool.TryParse(pingKickEnabledSetting.SettingValue, out bool setKillSkinEnabled) || !setKillSkinEnabled)
        {
            return;
        }

        ServerSettings? pingKickThresholdSetting = await this.pavlovServerContext.Settings.FirstOrDefaultAsync(s => s.ServerId == this.connection.ServerId && s.SettingName == ServerSettings.SETTING_PING_KICK_THRESHOLD);
        ServerSettings? pingKickMeasureTimeSetting = await this.pavlovServerContext.Settings.FirstOrDefaultAsync(s => s.ServerId == this.connection.ServerId && s.SettingName == ServerSettings.SETTING_PING_KICK_MEASURETIME);

        if (pingKickThresholdSetting == null || !int.TryParse(pingKickThresholdSetting.SettingValue, out int pingKickThreshold) ||
            pingKickMeasureTimeSetting == null || !int.TryParse(pingKickMeasureTimeSetting.SettingValue, out int pingKickMeasureTime))
        {
            return;
        }

        foreach (PlayerDetail playerDetail in this.connection.PlayerDetails.Values)
        {
            if (!this.playerPingHistory.ContainsKey(playerDetail.UniqueId))
            {
                this.playerPingHistory.Add(playerDetail.UniqueId, new Queue<(DateTime, int)>());
            }

            this.playerPingHistory[playerDetail.UniqueId].Enqueue((DateTime.Now, (int)playerDetail.Ping));
            bool overflow = false;
            while (this.playerPingHistory[playerDetail.UniqueId].First().Timestamp < DateTime.Now.AddSeconds(-pingKickMeasureTime))
            {
                this.playerPingHistory[playerDetail.UniqueId].Dequeue();
                overflow = true;
            }

            if (overflow)
            {
                double averagePing = this.playerPingHistory[playerDetail.UniqueId].Average(p => p.Ping);
                if (averagePing > pingKickThreshold)
                {
                    try
                    {
                        await auditService.Add(this.connection.ServerId, $"Kicked player {playerDetail.PlayerName} ({playerDetail.UniqueId}) for high ping ({averagePing} > {pingKickThreshold}) over {pingKickMeasureTime} seconds.");
                        await this.pavlovRconService.KickPlayer(this.apiKey, this.connection.ServerId, playerDetail.UniqueId);
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine(ex.ToString());
                    }
                }
            }
        }

        foreach (ulong uniqueId in this.playerPingHistory.Keys.ToList())
        {
            if (!this.connection.PlayerDetails.ContainsKey(uniqueId))
            {
                this.playerPingHistory.Remove(uniqueId);
            }
        }
    }
}
