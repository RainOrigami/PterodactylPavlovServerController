using System.ComponentModel.DataAnnotations;

namespace PterodactylPavlovServerDomain.Models;
public class MapRotationModel
{
    [MaxLength(64)]
    public string ServerId { get; set; }

    [MaxLength(64)]
    public string Name { get; set; }
    public ICollection<ServerMapModel> Maps { get; set; }
    public List<MapInMapRotationModel>? MapsInRotation { get; set; }
}
