using Fluxor;
using PterodactylPavlovServerController.Services;

namespace PterodactylPavlovServerController.Stores;

[FeatureState]
public record PavlovServersState
{
    public IReadOnlyDictionary<string, string> ServerNamesFromGameIni { get; init; } = new Dictionary<string, string>();
}

public class PavlovServersReducers
{
    [ReducerMethod]
    public static PavlovServersState OnServerSetNameFromGameIni(PavlovServersState pavlovServersState, PavlovServerAddNameFromGameIniAction pavlovServerAddNameFromGameIniAction)
    {
        Dictionary<string, string> serverNames = (Dictionary<string, string>)pavlovServersState.ServerNamesFromGameIni;

        if (serverNames.ContainsKey(pavlovServerAddNameFromGameIniAction.ServerId))
        {
            serverNames.Remove(pavlovServerAddNameFromGameIniAction.ServerId);
        }

        serverNames.Add(pavlovServerAddNameFromGameIniAction.ServerId, pavlovServerAddNameFromGameIniAction.ServerName);

        return pavlovServersState with
        {
            ServerNamesFromGameIni = serverNames,
        };
    }
}

public class PavlovServersEffects
{
    private readonly PavlovRconConnectionService pavlovRconConnectionService;
    private readonly PavlovServerService pavlovServerService;
    private readonly IState<PavlovServersState> pavlovServersState;

    public PavlovServersEffects(PavlovServerService pavlovServerService, PavlovRconConnectionService pavlovRconConnectionService, IState<PavlovServersState> pavlovServersState)
    {
        this.pavlovServerService = pavlovServerService;
        this.pavlovRconConnectionService = pavlovRconConnectionService;
        this.pavlovServersState = pavlovServersState;
    }

    [EffectMethod]
    public async Task LoadServerNameFromGameIni(PavlovServerLoadNameFromGameIniAction pavlovServerLoadNameFromGameIniAction, IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new PavlovServerAddNameFromGameIniAction(pavlovServerLoadNameFromGameIniAction.ServerId, this.pavlovServerService.GetServerName(pavlovServerLoadNameFromGameIniAction.ApiKey, pavlovServerLoadNameFromGameIniAction.ServerId)));

        await Task.CompletedTask;
    }
}

public class PavlovServerLoadNameFromGameIniAction
{
    public PavlovServerLoadNameFromGameIniAction(string apiKey, string serverId)
    {
        this.ApiKey = apiKey;
        this.ServerId = serverId;
    }

    public string ApiKey { get; }
    public string ServerId { get; }
}

public class PavlovServerAddNameFromGameIniAction
{
    public PavlovServerAddNameFromGameIniAction(string serverId, string serverName)
    {
        this.ServerId = serverId;
        this.ServerName = serverName;
    }

    public string ServerId { get; }
    public string ServerName { get; }
}

public class PavlovServersSetErrorAction
{
    public PavlovServersSetErrorAction(string serverId, string? error)
    {
        this.ServerId = serverId;
        this.Error = error;
    }

    public string ServerId { get; }
    public string? Error { get; }
}
