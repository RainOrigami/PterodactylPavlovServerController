namespace PterodactylPavlovServerDomain.Models;

public class MapWorkshopModel
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string NameId { get; set; } = string.Empty;
    public string URL { get; set; } = string.Empty;
    public string? ImageURL { get; set; }

    /// <summary>
    ///     True when the map could not be resolved from its source (delisted from
    ///     mod.io, or the lookup failed). Such a model carries only the id.
    /// </summary>
    public bool Unavailable { get; set; }

    public bool HasImage => !string.IsNullOrWhiteSpace(this.ImageURL);

    public override string ToString()
    {
        return this.Name;
    }
}
