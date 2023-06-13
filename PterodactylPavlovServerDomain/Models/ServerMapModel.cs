using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace PterodactylPavlovServerDomain.Models;

public class ServerMapModel : IEqualityComparer<ServerMapModel>
{
    [MaxLength(64)]
    public string MapLabel { get; set; } = string.Empty;
    [MaxLength(64)]
    public string GameMode { get; set; } = string.Empty;

    public string NameId { get; set; } = string.Empty;

    [NotMapped]
    public bool IsWorkshopMap => this.MapLabel.StartsWith("UGC");
    [NotMapped]
    public long WorkshopId => this.IsWorkshopMap ? long.Parse(this.MapLabel[3..]) : 0;
    [NotMapped]
    public string Url => this.IsWorkshopMap ? $"https://mod.io/g/pavlov/m/{this.NameId}" : $"http://wiki.pavlov-vr.com/index.php?title=Default_Maps#{this.MapLabel}";

    public ICollection<MapRotationModel> Rotations { get; set; }
    public List<MapInMapRotationModel> MapsInRotation { get; set; }

    public bool Equals(ServerMapModel? x, ServerMapModel? y) => x?.MapLabel == y?.MapLabel && x?.GameMode == y?.GameMode;
    public int GetHashCode([DisallowNull] ServerMapModel obj) => $"{MapLabel}-{GameMode}".GetHashCode();
}
