using PterodactylPavlovServerDomain.Models;

namespace PterodactylPavlovServerController.Store.Maps
{
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
}
