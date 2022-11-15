namespace PavlovStatsReader.Models;

public class CPlayerStats : CBaseStats
{
    public ulong UniqueId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public int Assists { get; set; }
    public int TeamKills { get; set; }
    public int Headshots { get; set; }
    public int Suicides { get; set; }
    public int BombsPlanted { get; set; }
    public int BombsDefused { get; set; }
    public int TotalScore { get; set; }
    public double AverageScore { get; set; }
    public string? MostKillsWithGun { get; set; }
    public int MostKillsWithGunAmount { get; set; }
    public string? BestMap { get; set; }
    public string? BestMapGameMode { get; set; }
    public double BestMapAverageScore { get; set; }
    public int RoundsPlayed { get; set; }
}
