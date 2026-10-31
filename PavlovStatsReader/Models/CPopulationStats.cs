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

    /// <summary>
    ///     Matches divided by the days of the month covered by the data.
    /// </summary>
    public double AverageMatchesPerDay { get; set; }

    /// <summary>
    ///     Most matches played on a single day of the month.
    /// </summary>
    public int PeakMatchesPerDay { get; set; }
}
