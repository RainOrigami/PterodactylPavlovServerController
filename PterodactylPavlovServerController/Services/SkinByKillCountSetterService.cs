using Microsoft.EntityFrameworkCore;
using PavlovVR_Rcon.Models.Pavlov;
using PterodactylPavlovServerController.Contexts;
using PterodactylPavlovServerController.Models;
using PterodactylPavlovServerDomain.Extensions;
using PterodactylPavlovServerDomain.Models;

namespace PterodactylPavlovServerController.Services;

public class SkinByKillCountSetterService
{
    private readonly string apiKey;
    private readonly PavlovRconConnection connection;
    private readonly PavlovRconService pavlovRconService;
    private readonly PavlovServerContext pavlovServerContext;

    private string currentMap = string.Empty;
    private bool setSkin = false;

    private ulong skinTarget = 0;
    private Skin skin = Skin.clown;

    public SkinByKillCountSetterService(string apiKey, PavlovRconConnection connection, PavlovRconService pavlovRconService, IConfiguration configuration)
    {
        this.apiKey = apiKey;
        this.connection = connection;
        this.pavlovRconService = pavlovRconService;

        this.connection.OnServerInfoUpdated += Connection_OnServerInfoUpdated;
        this.pavlovServerContext = new(configuration);
        this.pavlovServerContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    private async void Connection_OnServerInfoUpdated(string serverId)
    {
        if (this.connection.ServerInfo!.MapLabel != currentMap)
        {
            setSkin = false;
            skinTarget = 0;
            currentMap = this.connection.ServerInfo!.MapLabel;
            return;
        }

        if (this.connection.PlayerDetails == null)
        {
            return;
        }

        if (this.connection.ServerInfo!.RoundState != "Started")
        {
            return;
        }

        if (setSkin)
        {
            if (skinTarget > 0)
            {
                PlayerDetail? target = this.connection.PlayerDetails.Values.FirstOrDefault(d => d.UniqueId == skinTarget);
                if (target == null)
                {
                    setSkin = false;
                    skinTarget = 0;
                    return;
                }
                if (!target.Dead)
                {
                    await this.pavlovRconService.SetSkin(this.apiKey, this.connection.ServerId, skinTarget, skin.ToString());
                }
            }

            return;
        }

        ServerSettings? setKillSkinEnabledSetting = await this.pavlovServerContext.Settings.FirstOrDefaultAsync(s => s.ServerId == this.connection.ServerId && s.SettingName == ServerSettings.SETTING_SKIN_ENABLED);
        if (setKillSkinEnabledSetting == null || !bool.TryParse(setKillSkinEnabledSetting.SettingValue, out bool setKillSkinEnabled) || !setKillSkinEnabled)
        {
            setSkin = true;
            return;
        }

        ServerSettings? setKillSkinThresholdSetting = await this.pavlovServerContext.Settings.FirstOrDefaultAsync(s => s.ServerId == this.connection.ServerId && s.SettingName == ServerSettings.SETTING_SKIN_THRESHOLD);
        ServerSettings? setKillSkinSkinSetting = await this.pavlovServerContext.Settings.FirstOrDefaultAsync(s => s.ServerId == this.connection.ServerId && s.SettingName == ServerSettings.SETTING_SKIN_SKIN);

        if (setKillSkinThresholdSetting == null || !int.TryParse(setKillSkinThresholdSetting.SettingValue, out int setKillSkinThreshold) ||
            setKillSkinSkinSetting == null || !Enum.TryParse<Skin>(setKillSkinSkinSetting.SettingValue, out skin))
        {
            setSkin = true;
            return;
        }

        PlayerDetail? mostKills = this.connection.PlayerDetails.Values.OrderByDescending(d => d.Kills()).FirstOrDefault();
        if (mostKills == null)
        {
            return;
        }

        if (mostKills.Kills() >= setKillSkinThreshold)
        {
            skinTarget = mostKills.UniqueId;

            try
            {
                setSkin = true;
                await this.pavlovRconService.SetSkin(this.apiKey, this.connection.ServerId, mostKills.UniqueId, skin.ToString());
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }
        }
    }
}
