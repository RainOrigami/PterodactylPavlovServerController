namespace PavlovStatsReader.Models;

public class CTeamStats : CBaseStats
{
    public int TeamId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TotalKills { get; set; }
    public int TotalHeadshots { get; set; }
    public int TotalDeaths { get; set; }
    public int TotalAssists { get; set; }
    public int TotalTeamkills { get; set; }
    public int TotalVictories { get; set; }
    public ulong? BestPlayer { get; set; }
    public double BestPlayerAverageScore { get; set; }
    public string? BestGun { get; set; }
    public int BestGunKillCount { get; set; }
}
