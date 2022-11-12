namespace PavlovStatsReader.Models;

public class CServerStats : CBaseStats
{
    public int TotalUniquePlayers { get; set; }
    public int TotalUniqueMaps { get; set; }
    public int TotalMatchesPlayed { get; set; }
    public int TotalKills { get; set; }
    public int TotalHeadshots { get; set; }
    public int TotalAssists { get; set; }
    public int TotalTeamkills { get; set; }
    public int TotalBombPlants { get; set; }
    public int TotalBombDefuses { get; set; }
    public int TotalBombExplosions { get; set; }
    public string? MostPopularGun { get; set; }
    public int MostPopularGunKillCount { get; set; }
}
