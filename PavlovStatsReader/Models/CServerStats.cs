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
    public int TotalChickensKilled { get; set; }
    public int TotalRoundsPlayed { get; set; }
    public int TotalPoints { get; set; }

    /// <summary>Rounds where an enemy kill happened at all, and how many of those
    /// were won by the team that landed it.</summary>
    public int FirstBloodRounds { get; set; }

    public int FirstBloodWins { get; set; }

    public double AverageRoundSeconds { get; set; }

    public double FastestRoundSeconds { get; set; }

    public double LongestRoundSeconds { get; set; }

    public string? BiggestBlowoutMap { get; set; }

    public string? BiggestBlowoutGameMode { get; set; }

    public string? BiggestBlowoutScore { get; set; }

    /// <summary>Matches decided by a single round.</summary>
    public int NailbiterMatches { get; set; }
}
