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

    public double AverageRoundSeconds { get; set; }

    /// <summary>
    ///     Wins over the last three months only. The all-time split is dominated by
    ///     however the map played years ago, which says nothing about the rotation
    ///     as it stands now.
    /// </summary>
    public int RecentTeam0Wins { get; set; }

    public int RecentTeam1Wins { get; set; }

    public DateTime? LastPlayed { get; set; }
}
