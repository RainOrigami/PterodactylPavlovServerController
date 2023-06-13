using Fluxor;
using PterodactylPavlovServerController.Services;
using PterodactylPavlovServerDomain.Models;

namespace PterodactylPavlovServerController.Stores;

[FeatureState]
public record MapsState
{
    public IReadOnlyDictionary<long, MapWorkshopModel> MapDetails { get; init; } = new Dictionary<long, MapWorkshopModel>();
    public IReadOnlyDictionary<string, ServerMapModel[]> ServerMaps { get; init; } = new Dictionary<string, ServerMapModel[]>();
}

public class MapsReducers
{
    [ReducerMethod]
    public static MapsState OnMapsAdd(MapsState mapsState, MapsAddWorkshopAction mapsAddWorkshopAction)
    {
        Dictionary<long, MapWorkshopModel> maps = (Dictionary<long, MapWorkshopModel>)mapsState.MapDetails;
        if (maps.ContainsKey(mapsAddWorkshopAction.MapWorkshopModel.Id))
        {
            maps.Remove(mapsAddWorkshopAction.MapWorkshopModel.Id);
        }

        maps.Add(mapsAddWorkshopAction.MapWorkshopModel.Id, mapsAddWorkshopAction.MapWorkshopModel);

        return mapsState with
        {
            MapDetails = maps,
        };
    }

    [ReducerMethod]
    public static MapsState OnMapsServerAdd(MapsState mapsState, MapsAddServerAction mapsAddServerAction)
    {
        Dictionary<string, ServerMapModel[]> maps = (Dictionary<string, ServerMapModel[]>)mapsState.ServerMaps;

        if (maps.ContainsKey(mapsAddServerAction.ServerId))
        {
            maps.Remove(mapsAddServerAction.ServerId);
        }

        maps.Add(mapsAddServerAction.ServerId, mapsAddServerAction.Maps);

        return mapsState with
        {
            ServerMaps = maps,
        };
    }
}

public class MapsEffects
{
    private readonly IState<MapsState> mapsState;
    private readonly PavlovServerService pavlovServerService;
    private readonly IMapSourceService mapSourceService;

    public MapsEffects(IMapSourceService mapSourceService, PavlovServerService pavlovServerService, IState<MapsState> mapsState)
    {
        this.mapSourceService = mapSourceService;
        this.pavlovServerService = pavlovServerService;
        this.mapsState = mapsState;
    }

    [EffectMethod]
    public async Task LoadMap(MapsLoadWorkshopAction mapsLoadWorkshopAction, IDispatcher dispatcher)
    {
        if (this.mapsState.Value.MapDetails.ContainsKey(mapsLoadWorkshopAction.MapId))
        {
            return;
        }

        try
        {
            dispatcher.Dispatch(new MapsAddWorkshopAction(this.mapSourceService.GetMapDetail(mapsLoadWorkshopAction.MapId)));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        await Task.CompletedTask;
    }

    [EffectMethod]
    public async Task LoadServerMaps(MapsLoadServerAction mapsLoadServerAction, IDispatcher dispatcher)
    {
        ServerMapModel[] mapRows = await this.pavlovServerService.GetCurrentMapRotation(mapsLoadServerAction.ApiKey, mapsLoadServerAction.ServerId);
        dispatcher.Dispatch(new MapsAddServerAction(mapsLoadServerAction.ServerId, mapRows));

        foreach (ServerMapModel map in mapRows)
        {
            if (!map.IsWorkshopMap || this.mapsState.Value.MapDetails.ContainsKey(map.WorkshopId))
            {
                continue;
            }

            dispatcher.Dispatch(new MapsLoadWorkshopAction(map.WorkshopId));
        }

        await Task.CompletedTask;
    }
}

public class MapsLoadWorkshopAction
{
    public MapsLoadWorkshopAction(long mapId)
    {
        this.MapId = mapId;
    }

    public long MapId { get; }
}

public class MapsAddWorkshopAction
{
    public MapsAddWorkshopAction(MapWorkshopModel mapWorkshopModel)
    {
        this.MapWorkshopModel = mapWorkshopModel;
    }

    public MapWorkshopModel MapWorkshopModel { get; }
}

public class MapsLoadServerAction
{
    public MapsLoadServerAction(string apiKey, string serverId)
    {
        this.ApiKey = apiKey;
        this.ServerId = serverId;
    }

    public string ApiKey { get; }
    public string ServerId { get; }
}

public class MapsAddServerAction
{
    public MapsAddServerAction(string serverId, ServerMapModel[] maps)
    {
        this.ServerId = serverId;
        this.Maps = maps;
    }

    public string ServerId { get; }
    public ServerMapModel[] Maps { get; }
}
