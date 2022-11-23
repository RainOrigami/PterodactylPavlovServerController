using Fluxor;
using PterodactylPavlovServerController.Services;
using Steam.Models.SteamCommunity;

namespace PterodactylPavlovServerController.Stores;

[FeatureState]
public record PlayersState
{
    public IReadOnlyDictionary<ulong, PlayerSummaryModel> PlayerSummaries { get; init; } = new Dictionary<ulong, PlayerSummaryModel>();
    public IReadOnlyDictionary<ulong, IReadOnlyCollection<PlayerBansModel>> PlayerBans { get; init; } = new Dictionary<ulong, IReadOnlyCollection<PlayerBansModel>>();
}

public class PlayersReducer
{
    [ReducerMethod]
    public static PlayersState OnPlayersSummaryAdd(PlayersState playersState, PlayersAddSummaryAction playersAddSummaryAction)
    {
        Dictionary<ulong, PlayerSummaryModel> playerSummaries = (Dictionary<ulong, PlayerSummaryModel>)playersState.PlayerSummaries;
        if (playerSummaries.ContainsKey(playersAddSummaryAction.PlayerSummary.SteamId))
        {
            _ = playerSummaries.Remove(playersAddSummaryAction.PlayerSummary.SteamId);
        }

        playerSummaries.Add(playersAddSummaryAction.PlayerSummary.SteamId, playersAddSummaryAction.PlayerSummary);

        return playersState with
        {
            PlayerSummaries = playerSummaries,
        };
    }

    [ReducerMethod]
    public static PlayersState OnPlayersBansAdd(PlayersState playersState, PlayersAddBansAction playersAddBansAction)
    {
        Dictionary<ulong, IReadOnlyCollection<PlayerBansModel>> playerBans = (Dictionary<ulong, IReadOnlyCollection<PlayerBansModel>>)playersState.PlayerBans;
        if (playerBans.ContainsKey(playersAddBansAction.PlayerId))
        {
            _ = playerBans.Remove(playersAddBansAction.PlayerId);
        }

        playerBans.Add(playersAddBansAction.PlayerId, playersAddBansAction.PlayerBans);

        return playersState with
        {
            PlayerBans = playerBans,
        };
    }
}

public class PlayersEffects
{
    private readonly SteamService steamService;

    public PlayersEffects(SteamService steamService)
    {
        this.steamService = steamService;
    }

    [EffectMethod]
    public async Task PlayersLoadSummary(PlayersLoadSummaryAction playersLoadSummaryAction, IDispatcher dispatcher)
    {
        try
        {
            dispatcher.Dispatch(new PlayersAddSummaryAction(await this.steamService.GetPlayerSummary(playersLoadSummaryAction.PlayerId)));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not load player summary of {playersLoadSummaryAction.PlayerId}: {ex.Message}");
        }

        await Task.CompletedTask;
    }

    [EffectMethod]
    public async Task PlayersLoadBans(PlayersLoadBansAction playersLoadBansAction, IDispatcher dispatcher)
    {
        try
        {
            dispatcher.Dispatch(new PlayersAddBansAction(playersLoadBansAction.PlayerId, await this.steamService.GetBans(playersLoadBansAction.PlayerId)));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        await Task.CompletedTask;
    }
}

public class PlayersLoadSummaryAction
{
    public PlayersLoadSummaryAction(ulong playerId)
    {
        this.PlayerId = playerId;
    }

    public ulong PlayerId { get; }
}

public class PlayersAddSummaryAction
{
    public PlayersAddSummaryAction(PlayerSummaryModel playerSummary)
    {
        this.PlayerSummary = playerSummary;
    }

    public PlayerSummaryModel PlayerSummary { get; }
}

public class PlayersLoadBansAction
{
    public PlayersLoadBansAction(ulong playerId)
    {
        this.PlayerId = playerId;
    }

    public ulong PlayerId { get; }
}

public class PlayersAddBansAction
{
    public PlayersAddBansAction(ulong playerId, IReadOnlyCollection<PlayerBansModel> playerBans)
    {
        this.PlayerId = playerId;
        this.PlayerBans = playerBans;
    }

    public ulong PlayerId { get; }
    public IReadOnlyCollection<PlayerBansModel> PlayerBans { get; }
}
