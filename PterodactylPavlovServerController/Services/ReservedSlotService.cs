using Microsoft.EntityFrameworkCore;
using PterodactylPavlovServerController.Contexts;
using PterodactylPavlovServerDomain.Extensions;
using PterodactylPavlovServerDomain.Models;

namespace PterodactylPavlovServerController.Services;

public class ReservedSlotService
{
    private readonly string apiKey;
    private readonly PavlovRconConnection connection;
    private readonly PavlovRconService pavlovRconService;
    private readonly IConfiguration configuration;
    private readonly PavlovServerContext pavlovServerContext;
    private int? currentPin = 0;

    public bool IsPinLocked => currentPin.HasValue;

    public ReservedSlotService(string apiKey, PavlovRconConnection connection, PavlovRconService pavlovRconService, IConfiguration configuration)
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
        ServerSettings? pin = await pavlovServerContext.Settings.FirstOrDefaultAsync(s => s.ServerId == serverId && s.SettingName == ServerSettings.SETTING_RESERVED_SLOT_PIN);
        ServerSettings? amount = await pavlovServerContext.Settings.FirstOrDefaultAsync(s => s.ServerId == serverId && s.SettingName == ServerSettings.SETTING_RESERVED_SLOT_AMOUNT);

        if (amount == null || pin == null)
        {
            return;
        }

        if (!int.TryParse(pin.SettingValue, out int reservedSlotPin) || !int.TryParse(amount.SettingValue, out int reservedSlotAmount))
        {
            return;
        }

        if (reservedSlotPin < 1000 || reservedSlotPin > 9999 || reservedSlotAmount <= 0 || reservedSlotAmount >= this.connection.ServerInfo!.MaximumPlayerCount())
        {
            return;
        }

        if (currentPin.HasValue && currentPin.Value == 0)
        {
            await pavlovRconService.SetPin(apiKey, serverId, null);
            currentPin = null;
        }

        if (connection.ServerInfo!.MaximumPlayerCount() - connection.ServerInfo!.CurrentPlayerCount() <= reservedSlotAmount)
        {
            if (!currentPin.HasValue || currentPin.Value != reservedSlotPin)
            {
                await pavlovRconService.SetPin(apiKey, serverId, reservedSlotPin);
                currentPin = reservedSlotPin;
            }
        }
        else
        {
            if (currentPin.HasValue)
            {
                await pavlovRconService.SetPin(apiKey, serverId, null);
                currentPin = null;
            }
        }
    }
}
