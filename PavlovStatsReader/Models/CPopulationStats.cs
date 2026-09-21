namespace PavlovStatsReader.Models;

/// <summary>
///     Server population for one calendar month, for the trend chart.
/// </summary>
public class CPopulationStats : CBaseStats
{
    public int Year { get; set; }

    public int Month { get; set; }

    public double AveragePlayers { get; set; }

    public int MaxPlayers { get; set; }

    public int Matches { get; set; }
}
