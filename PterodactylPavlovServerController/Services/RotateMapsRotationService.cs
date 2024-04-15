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

        await Console.Out.WriteLineAsync($"Map changed from {this.lastMap} to {this.connection.ServerInfo!.MapLabel}");

        //if ((this.connection.PlayerListPlayers?.Count ?? 0) < 2 || this.connection.ServerInfo!.RoundState != "Started")
        //{
        //    return;
        //}

        this.lastMap = this.connection.ServerInfo!.MapLabel;

        List<ServerMapModel> currentRotation = new(await pavlovServerService.GetCurrentMapRotation(apiKey, serverId));

        await Console.Out.WriteLineAsync($"Current rotation: {string.Join(", ", currentRotation.Select(r => r.MapLabel))}");

        if (currentRotation.Count < 1)
        {
            return;
        }

        if (currentRotation.Last().MapLabel.ToLower() == connection.ServerInfo!.MapLabel.ToLower() && (connection.ServerInfo!.GameMode.ToLower() == "custom" ? "custom" : currentRotation.Last().GameMode.ToLower()) == connection.ServerInfo!.GameMode.ToLower())
        {
            await Console.Out.WriteLineAsync("Map is already at the end of the rotation");
            return;
        }

        if (!currentRotation.Any(r => r.MapLabel.ToLower() == connection.ServerInfo!.MapLabel.ToLower() && (connection.ServerInfo!.GameMode.ToLower() == "custom" ? "custom" : r.GameMode.ToLower()) == connection.ServerInfo!.GameMode.ToLower()))
        {
            await Console.Out.WriteLineAsync("Map is not in the rotation");
            return;
        }

        int iterations = 0;

        while (currentRotation.Last().MapLabel.ToLower() != connection.ServerInfo!.MapLabel.ToLower() || (connection.ServerInfo!.GameMode.ToLower() == "custom" ? "custom" : currentRotation.Last().GameMode.ToLower()) != connection.ServerInfo!.GameMode.ToLower())
        {
            ServerMapModel old = currentRotation[0];
            currentRotation.RemoveAt(0);
            currentRotation.Add(old);
            await Console.Out.WriteLineAsync($"Moving {old.MapLabel} to the end of the rotation");
            if (++iterations > currentRotation.Count + 2)
            {
                return;
            }
        }

        await Console.Out.WriteLineAsync($"New rotation: {string.Join(", ", currentRotation.Select(r => r.MapLabel))}");

        await pavlovServerService.ApplyMapList(apiKey, serverId, currentRotation.ToArray());
    }
}
