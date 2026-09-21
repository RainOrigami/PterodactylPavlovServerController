using PavlovVR_Rcon.Models.Pavlov;

namespace PterodactylPavlovServerController.Services.WarmupRoundLoadouts;

public abstract class BaseLoadout
{
    public string Name { get; protected set; } = "Undefined";
    public abstract Task EnableRound(PavlovRconService rconService, string apiKey, string serverId);
    public abstract Task DisableRound(PavlovRconService rconService, string apiKey, string serverId);
    public abstract Task EnablePlayers(PavlovRconService rconService, string apiKey, string serverId);
    public abstract Task DisablePlayers(PavlovRconService rconService, string apiKey, string serverId);

    /// <summary>
    /// Equips one player. Returns false if any item could not be confirmed, so the caller can report it.
    /// </summary>
    public abstract Task<bool> EnablePlayer(PavlovRconService rconService, string apiKey, string serverId, ulong playerId);
    public abstract Task DisablePlayer(PavlovRconService rconService, string apiKey, string serverId, ulong playerId);

    protected static Task<bool> GiveItem(PavlovRconService rconService, string apiKey, string serverId, ulong playerId, Item item)
    {
        return RconRetry.Ensure($"GiveItem {item} to {playerId} on {serverId}", () => rconService.GiveItem(apiKey, serverId, playerId, item));
    }

    protected static Task<bool> GiveItem(PavlovRconService rconService, string apiKey, string serverId, ulong playerId, string item)
    {
        return RconRetry.Ensure($"GiveItem {item} to {playerId} on {serverId}", () => rconService.GiveItem(apiKey, serverId, playerId, item));
    }

    protected static Task<bool> SetCash(PavlovRconService rconService, string apiKey, string serverId, ulong playerId, int amount)
    {
        return RconRetry.Ensure($"SetCash {amount} for {playerId} on {serverId}", () => rconService.SetCash(apiKey, serverId, playerId, amount));
    }

    protected static Task<bool> StripCash(PavlovRconService rconService, string apiKey, string serverId, ulong playerId)
    {
        return BaseLoadout.SetCash(rconService, apiKey, serverId, playerId, 0);
    }
}
