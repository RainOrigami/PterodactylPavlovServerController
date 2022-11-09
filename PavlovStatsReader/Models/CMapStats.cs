namespace PavlovStatsReader.Models;

public class CMapStats : CBaseStats
{
    public string MapId { get; set; } = string.Empty;
    public string GameMode { get; set; } = string.Empty;
    public int PlayCount { get; set; }
    public int Team0Wins { get; set; }
    public int Team1Wins { get; set; }
    public double AverageRounds { get; set; }
    public int MaxRounds { get; set; }
    public int MinRounds { get; set; }

    public ulong? BestPlayer { get; set; }
    public double MaxAveragePlayerScore { get; set; }
}
