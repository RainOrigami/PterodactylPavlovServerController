using Fluxor;
using PterodactylPavlovServerController.Services;
using PterodactylPavlovServerDomain.Models;

namespace PterodactylPavlovServerController.Stores;

[FeatureState]
public record MapsState
{
    public IReadOnlyDictionary<long, MapDetailModel> MapDetails { get; init; } = new Dictionary<long, MapDetailModel>();
    public IReadOnlyDictionary<string, MapRowModel[]> ServerMaps { get; init; } = new Dictionary<string, MapRowModel[]>();
}

public class MapsReducers
{
    [ReducerMethod]
    public static MapsState OnMapsAdd(MapsState mapsState, MapsAddAction mapsAddAction)
    {
        Dictionary<long, MapDetailModel> maps = (Dictionary<long, MapDetailModel>) mapsState.MapDetails;

        if (maps.ContainsKey(mapsAddAction.MapDetailModel.Id))
        {
            maps.Remove(mapsAddAction.MapDetailModel.Id);
        }

        maps.Add(mapsAddAction.MapDetailModel.Id, mapsAddAction.MapDetailModel);

        return mapsState with
        {
            MapDetails = maps,
        };
    }

    [ReducerMethod]
    public static MapsState OnMapsServerAdd(MapsState mapsState, MapsAddServerAction mapsAddServerAction)
    {
        Dictionary<string, MapRowModel[]> maps = (Dictionary<string, MapRowModel[]>) mapsState.ServerMaps;

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
    private readonly SteamWorkshopService steamWorkshopService;

    public MapsEffects(SteamWorkshopService steamWorkshopService, PavlovServerService pavlovServerService, IState<MapsState> mapsState)
    {
        this.steamWorkshopService = steamWorkshopService;
        this.pavlovServerService = pavlovServerService;
        this.mapsState = mapsState;
    }

    [EffectMethod]
    public async Task LoadMap(MapsLoadAction mapsLoadAction, IDispatcher dispatcher)
    {
        if (this.mapsState.Value.MapDetails.ContainsKey(mapsLoadAction.MapId))
        {
            return;
        }

        try
        {
            dispatcher.Dispatch(new MapsAddAction(this.steamWorkshopService.GetMapDetail(mapsLoadAction.MapId)));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        await Task.CompletedTask;
    }

    [EffectMethod]
    public async Task LoadServerMaps(MapsLoadServerAction mapsLoadServerAction, IDispatcher dispatcher)
    {
        MapRowModel[] mapRows = this.pavlovServerService.GetCurrentMapRotation(mapsLoadServerAction.ServerId);
        dispatcher.Dispatch(new MapsAddServerAction(mapsLoadServerAction.ServerId, mapRows));

        foreach (MapRowModel map in mapRows)
        {
            if (this.mapsState.Value.MapDetails.ContainsKey(map.MapId))
            {
                continue;
            }

            dispatcher.Dispatch(new MapsLoadAction(map.MapId));
        }

        await Task.CompletedTask;
    }
}

public class MapsLoadAction
{
    public MapsLoadAction(long mapId)
    {
        this.MapId = mapId;
    }

    public long MapId { get; }
}

public class MapsAddAction
{
    public MapsAddAction(MapDetailModel mapDetailModel)
    {
        this.MapDetailModel = mapDetailModel;
    }

    public MapDetailModel MapDetailModel { get; }
}

public class MapsLoadServerAction
{
    public MapsLoadServerAction(string serverId)
    {
        this.ServerId = serverId;
    }

    public string ServerId { get; }
}

public class MapsAddServerAction
{
    public MapsAddServerAction(string serverId, MapRowModel[] maps)
    {
        this.ServerId = serverId;
        this.Maps = maps;
    }

    public string ServerId { get; }
    public MapRowModel[] Maps { get; }
}
