using PavlovStatsReader.Models;

namespace PavlovStatsReader;

/// <summary>
///     One real match round: the window between a "Started" and its matching
///     "Ended" round state, with the events that fell inside it. Warmup rounds and
///     pre-match noise are already filtered out by the time these are built, so
///     anything derived from them is match play only.
/// </summary>
public record RoundWindow(DateTime Start, DateTime End, int WinningTeam, string MapLabel, string GameMode, KillData[] Kills, BombData[] Bombs)
{
    /// <summary>
    ///     Kills of an actual opponent, in the order they happened: suicides and
    ///     teamkills are not kills for the purposes of first bloods, streaks or
    ///     multikills.
    /// </summary>
    public IEnumerable<KillData> EnemyKills()
    {
        return this.Kills
            .Where(k => k.Killer != k.Killed && k.KillerTeamID != k.KilledTeamID)
            .OrderBy(k => k.LogEntryDate);
    }
}
