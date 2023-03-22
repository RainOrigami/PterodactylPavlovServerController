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

        if (connection.ServerInfo!.CurrentPlayerCount() == 0 && connection.ServerInfo!.GameMode == "SND" && connection.ServerInfo!.Round.HasValue && connection.ServerInfo!.Round.Value > 0)
        {
            await pavlovRconService.ResetSND(apiKey, serverId);
        }
    }
}
