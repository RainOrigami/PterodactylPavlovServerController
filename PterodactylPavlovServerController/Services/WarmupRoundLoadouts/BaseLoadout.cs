namespace PterodactylPavlovServerController.Services.WarmupRoundLoadouts;

public abstract class BaseLoadout
{
    public abstract Task EnableRound(PavlovRconService rconService, string apiKey, string serverId);
    public abstract Task DisableRound(PavlovRconService rconService, string apiKey, string serverId);
    public abstract Task EnablePlayers(PavlovRconService rconService, string apiKey, string serverId);
    public abstract Task DisablePlayers(PavlovRconService rconService, string apiKey, string serverId);
    public abstract Task EnablePlayer(PavlovRconService rconService, string apiKey, string serverId, ulong playerId);
    public abstract Task DisablePlayer(PavlovRconService rconService, string apiKey, string serverId, ulong playerId);

    protected async Task StripCash(PavlovRconService rconService, string apiKey, string serverId, ulong playerId)
    {
        await rconService.SetCash(apiKey, serverId, playerId, 0);
        await Task.Delay(50);
    }
}
