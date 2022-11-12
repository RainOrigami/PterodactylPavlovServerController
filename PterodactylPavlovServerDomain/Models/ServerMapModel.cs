namespace PterodactylPavlovServerDomain.Models;

public class ServerMapModel
{
    public string MapLabel { get; set; } = string.Empty;
    public string GameMode { get; set; } = string.Empty;

    public bool IsWorkshopMap => this.MapLabel.StartsWith("UGC");
    public long WorkshopId => this.IsWorkshopMap ? long.Parse(this.MapLabel[3..]) : 0;

    public string Url => this.IsWorkshopMap ? $"https://steamcommunity.com/sharedfiles/filedetails/?id={this.WorkshopId}" : $"http://wiki.pavlov-vr.com/index.php?title=Default_Maps#{this.MapLabel}";
}
