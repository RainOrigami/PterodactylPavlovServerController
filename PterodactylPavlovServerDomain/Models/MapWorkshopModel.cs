namespace PterodactylPavlovServerDomain.Models;

public class MapWorkshopModel
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string URL { get; set; } = string.Empty;
    public string? ImageURL { get; set; }

    public override string ToString()
    {
        return this.Name;
    }
}
