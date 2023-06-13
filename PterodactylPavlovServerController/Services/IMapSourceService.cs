using PterodactylPavlovServerDomain.Models;

namespace PterodactylPavlovServerController.Services;

public interface IMapSourceService
{
    public MapWorkshopModel GetMapDetail(long mapId);
}
