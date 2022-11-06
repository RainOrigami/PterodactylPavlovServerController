namespace PterodactylPavlovServerDomain.Models
{
    public class MapDetailModel
    {
        public long Id { get; set; }
        public string Name { get; set; } = String.Empty;
        public string URL { get; set; } = String.Empty;
        public string? ImageURL { get; set; }

        // Any size square image, append: ?imw=256&imh=256&impolicy=Letterbox

        public override string ToString() => Name;
    }
}
