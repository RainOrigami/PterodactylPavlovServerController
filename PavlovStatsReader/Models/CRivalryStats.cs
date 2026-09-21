namespace PavlovStatsReader.Models;

/// <summary>
///     A pair of players and how often each killed the other. Kind separates the
///     pair that fought the most from the pair that was the most one-sided.
/// </summary>
public class CRivalryStats : CBaseStats
{
    public string Kind { get; set; } = string.Empty;

    public ulong PlayerA { get; set; }

    public ulong PlayerB { get; set; }

    /// <summary>Times A killed B. A is always the player ahead in the pair.</summary>
    public int AKills { get; set; }

    public int BKills { get; set; }

    public int TotalKills => this.AKills + this.BKills;
}
