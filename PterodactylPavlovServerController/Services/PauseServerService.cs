using Microsoft.EntityFrameworkCore;
using PterodactylPavlovServerController.Contexts;
using PterodactylPavlovServerDomain.Extensions;
using PterodactylPavlovServerDomain.Models;

namespace PterodactylPavlovServerController.Services;

public class PauseServerService
{
    private readonly string apiKey;
    private readonly PavlovRconConnection connection;
    private readonly PavlovRconService pavlovRconService;
    private readonly IConfiguration configuration;
    private readonly PavlovServerContext pavlovServerContext;

    private bool isPaused = false;

    public PauseServerService(string apiKey, PavlovRconConnection connection, PavlovRconService pavlovRconService, IConfiguration configuration)
    {
        this.apiKey = apiKey;
        this.connection = connection;
        this.pavlovRconService = pavlovRconService;
        this.configuration = configuration;
        this.connection.OnServerInfoUpdated += this.Connection_OnServerInfoUpdated;
        this.pavlovServerContext = new(configuration);
        this.pavlovServerContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    private async void Connection_OnServerInfoUpdated(string serverId)
    {
        ServerSettings? pauseServerSetting = await pavlovServerContext.Settings.FirstOrDefaultAsync(s => s.ServerId == serverId && s.SettingName == ServerSettings.SETTING_PAUSE_SERVER);

        if (pauseServerSetting == null || !bool.TryParse(pauseServerSetting.SettingValue, out bool pauseServer) || !pauseServer)
        {
            return;
        }

        if (connection.ServerInfo!.CurrentPlayerCount() < 1 && connection.ServerInfo!.GameMode.ToUpper() == "SND")
        {
            await Task.Delay(50);
            isPaused = true;
            await pavlovRconService.PauseMatch(apiKey, serverId, 3600);
        }
        else if (isPaused)
        {
            await Task.Delay(50);
            await pavlovRconService.PauseMatch(apiKey, serverId, 0);
            isPaused = false;
        }
    }
}
