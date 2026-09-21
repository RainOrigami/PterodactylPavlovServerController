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
    public int ChickensKilled { get; set; }

    /// <summary>Player who killed this one the most, and how often.</summary>
    public ulong? Nemesis { get; set; }

    public int NemesisKills { get; set; }

    /// <summary>Player this one killed the most, and how often.</summary>
    public ulong? FavouriteVictim { get; set; }

    public int FavouriteVictimKills { get; set; }

    /// <summary>Rounds where this player landed the round's first enemy kill.</summary>
    public int FirstBloods { get; set; }

    /// <summary>Most enemy kills in a single round.</summary>
    public int BestRoundKills { get; set; }

    /// <summary>Rounds with three or more kills, and with five or more.</summary>
    public int Multikills { get; set; }

    public int Aces { get; set; }

    /// <summary>Most kills in a row within one round without being killed.</summary>
    public int LongestKillStreak { get; set; }

    public int TeamSwitches { get; set; }

    public int BlueMatches { get; set; }

    public int RedMatches { get; set; }

    /// <summary>Bombs planted during match rounds, counted from the bomb log.</summary>
    public int PlantsMade { get; set; }

    /// <summary>Bombs planted in rounds that went on to explode.</summary>
    public int PlantsConverted { get; set; }
}
