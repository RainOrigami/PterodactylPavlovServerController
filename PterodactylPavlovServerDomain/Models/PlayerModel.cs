namespace PterodactylPavlovRconClient.Models
{
    public class PlayerModel
    {
        public string UniqueId { get; set; } = String.Empty;
        public string PlayerName { get; set; } = String.Empty;
        public int TeamId { get; set; }
        public int Score { get; set; }
        public bool Dead { get; set; }
        public int Cash { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Assists { get; set; }
    }
}
