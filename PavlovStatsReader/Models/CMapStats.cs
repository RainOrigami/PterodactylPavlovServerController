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

    public double AveragePlayers { get; set; }

    /// <summary>
    ///     Mean gap between the two final scores. Small means the matches on this
    ///     map tend to go down to the wire.
    /// </summary>
    public double AverageScoreMargin { get; set; }

    /// <summary>
    ///     Everything below is counted from the individual match rounds, so short
    ///     rounds and warmups are already gone.
    /// </summary>
    public int RoundCount { get; set; }

    public int Kills { get; set; }

    public int Headshots { get; set; }

    public int RoundsWithPlant { get; set; }

    public int RoundsWithExplosion { get; set; }

    public int RoundsWithDefuse { get; set; }

    /// <summary>
    ///     Round wins over the last three months. Rounds rather than matches give
    ///     enough samples for a side-balance figure inside that window.
    /// </summary>
    public int RecentTeam0RoundWins { get; set; }

    public int RecentTeam1RoundWins { get; set; }
}
