namespace PavlovStatsReader.Models
{
    public class CGunStats : CBaseStats
    {
        public string Name { get; set; }
        public int Kills { get; set; }
        public int Headshots { get; set; }
        public ulong? BestPlayer { get; set; }
        public int BestPlayerKills { get; set; }
    }
}
