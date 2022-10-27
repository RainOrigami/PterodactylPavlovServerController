using PavlovVR_Rcon;

namespace PterodactylPavlovRconClient.Models
{
    public class MapModel
    {
        public long Id { get; set; }
        public string Name { get; set; } = String.Empty;
        public string URL { get; set; } = String.Empty;
        public string? ImageURL { get; set; }
        public PavlovGameMode GameMode { get; set; }

        public override string ToString() => Name;
    }
}
