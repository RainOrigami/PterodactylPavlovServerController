using Microsoft.EntityFrameworkCore;
using PterodactylPavlovServerController.Contexts;
using PterodactylPavlovServerDomain.Extensions;
using PterodactylPavlovServerDomain.Models;

namespace PterodactylPavlovServerController.Services;

public class RotateMapsRotationService
{
    private readonly string apiKey;
    private readonly PavlovRconConnection connection;
    private readonly PavlovServerService pavlovServerService;
    private string lastMap = string.Empty;

    public RotateMapsRotationService(string apiKey, PavlovRconConnection connection, PavlovServerService pavlovServerService)
    {
        this.apiKey = apiKey;
        this.connection = connection;
        this.connection.OnServerInfoUpdated += this.Connection_OnServerInfoUpdated;
        this.pavlovServerService = pavlovServerService;
    }

    private async void Connection_OnServerInfoUpdated(string serverId)
    {
        if (this.lastMap == this.connection.ServerInfo!.MapLabel)
        {
            return;
        }

        if ((this.connection.PlayerListPlayers?.Count ?? 0) <= 2 || this.connection.ServerInfo!.RoundState != "Started")
        {
            return;
        }

        this.lastMap = this.connection.ServerInfo!.MapLabel;

        List<ServerMapModel> currentRotation = new(await pavlovServerService.GetCurrentMapRotation(apiKey, serverId));

        if (currentRotation.Count < 1)
        {
            return;
        }

        if (currentRotation[0].MapLabel.ToLower() == connection.ServerInfo!.MapLabel.ToLower() && currentRotation[0].GameMode.ToLower() == connection.ServerInfo!.GameMode.ToLower())
        {
            return;
        }

        if (!currentRotation.Any(r => r.MapLabel.ToLower() == connection.ServerInfo!.MapLabel.ToLower() && r.GameMode.ToLower() == connection.ServerInfo!.GameMode.ToLower()))
        {
            return;
        }

        int iterations = 0;

        while (currentRotation[0].MapLabel.ToLower() != connection.ServerInfo!.MapLabel.ToLower() || currentRotation[0].GameMode.ToLower() != connection.ServerInfo!.GameMode.ToLower())
        {
            ServerMapModel old = currentRotation[0];
            currentRotation.RemoveAt(0);
            currentRotation.Add(old);
            if (++iterations > currentRotation.Count + 2)
            {
                return;
            }
        }

        await pavlovServerService.ApplyMapList(apiKey, serverId, currentRotation.ToArray());
    }
}
