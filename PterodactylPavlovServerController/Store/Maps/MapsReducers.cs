using Fluxor;
using PterodactylPavlovServerDomain.Models;

namespace PterodactylPavlovServerController.Store.Maps
{
    public class MapsReducers
    {
        [ReducerMethod]
        public static MapsState OnMapsAdd(MapsState mapsState, MapsAddAction mapsAddAction)
        {
            Dictionary<long, MapDetailModel> maps = (Dictionary<long, MapDetailModel>)mapsState.MapDetails;

            if (maps.ContainsKey(mapsAddAction.MapDetailModel.Id))
            {
                maps.Remove(mapsAddAction.MapDetailModel.Id);
            }

            maps.Add(mapsAddAction.MapDetailModel.Id, mapsAddAction.MapDetailModel);

            return mapsState with
            {
                MapDetails = maps
            };
        }

        [ReducerMethod]
        public static MapsState OnMapsServerAdd(MapsState mapsState, MapsAddServerAction mapsAddServerAction)
        {
            Dictionary<string, MapRowModel[]> maps = (Dictionary<string, MapRowModel[]>)mapsState.ServerMaps;

            if (maps.ContainsKey(mapsAddServerAction.ServerId))
            {
                maps.Remove(mapsAddServerAction.ServerId);
            }

            maps.Add(mapsAddServerAction.ServerId, mapsAddServerAction.Maps);

            return mapsState with
            {
                ServerMaps = maps
            };
        }
    }
}
