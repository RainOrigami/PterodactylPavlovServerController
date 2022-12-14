namespace PterodactylPavlovServerDomain.Models;
public class MapInMapRotationModel
{
    public ServerMapModel Map { get; set; }
    public MapRotationModel Rotation { get; set; }

    public string MapLabel { get; set; }
    public string GameMode { get; set; }
    public string ServerId { get; set; }
    public string RotationName { get; set; }
    public int Order { get; set; }
}
