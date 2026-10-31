namespace PterodactylPavlovServerController.Models;

/// <summary>
///     Plain-language explanation of how each stat on the stats page is worked
///     out, shown as a tooltip on the label. Keyed by the label text itself, so
///     adding a stat without a description here simply leaves it without a
///     tooltip rather than breaking anything.
/// </summary>
public static class StatsDescriptions
{
    /// <summary>
    ///     Most numbers on the page come from match rounds only: warmup rounds,
    ///     pre-match noise and rounds shorter than 10 seconds are excluded, as are
    ///     push and deathmatch games and matches with fewer than two players or
    ///     fewer than ten rounds.
    /// </summary>
    private static readonly Dictionary<string, string> descriptions = new(StringComparer.OrdinalIgnoreCase)
    {
        // ── server totals ──
        ["Unique maps/modes"] = "Distinct map and game mode combinations that have been played.",
        ["Total matches"] = "Completed matches with at least 2 players and at least 10 rounds.",
        ["Unique players"] = "Everyone who has ever appeared in an end-of-match report on this server.",
        ["Total rounds"] = "Every match round on record. Warmups, rounds shorter than 10 seconds, push and deathmatch are left out.",
        ["Total chickens"] = "Chickens killed by everyone, from the per-match chicken counter.",
        ["Total points"] = "Experience points earned by all players across all counted matches.",
        ["Total kills"] = "Every kill logged during match rounds, teamkills and suicides included.",
        ["Total headshots"] = "Kills flagged as a headshot in the kill log.",
        ["Total assists"] = "Assists credited in the end-of-match reports.",
        ["Total teamkills"] = "Kills where killer and victim were on the same team.",
        ["Sharpest weapon"] = "Weapon with the highest share of its kills being headshots, counting only weapons with at least 100 kills.",

        // ── bombs ──
        ["Total plants"] = "BombPlanted events logged during match rounds.",
        ["Total defuses"] = "BombDefused events logged during match rounds.",
        ["Total explosions"] = "Planted bombs that ran all the way down and detonated. Far fewer than there were plants, because a round ends the moment one team is wiped out - a bomb still ticking at that point never goes off.",
        ["Plants defused"] = "Defuse events as a share of plant events.",
        ["Plants exploded"] = "Your plants that detonated, and what share of your plants that is. The rest were either defused or still ticking when the round ended because one team had been wiped out.",
        ["Bombs"] = "Plants and defuses credited to you in the end-of-match reports.",

        // ── rounds ──
        ["Average round"] = "Mean time from round start to round end across all match rounds.",
        ["Fastest round"] = "Shortest match round on record.",
        ["Longest round"] = "Longest match round on record.",
        ["First blood wins"] = "How often the team that landed the round's first kill went on to win that round.",

        // ── population chart ──
        ["Population"] = "Per calendar month: average and peak players in a counted match, and matches per day. The matches average only counts days with at least one match, so a server that plays four matches every Friday shows 4. Peak is the most matches on a single day. Dashed lines are least-squares trends.",

        // ── matches ──
        ["Biggest blowout"] = "The match with the largest gap between the two final scores.",
        ["Nailbiters"] = "Matches decided by a single round.",
        ["Most one-sided map"] = "Of maps with at least 5 decided matches in the last 3 months, the one where one team won the largest share.",
        ["Most balanced map"] = "Of maps with at least 5 decided matches in the last 3 months, the one with the most even win split.",

        // ── teams ──
        ["Victories"] = "Matches this team won outright.",
        ["Best player"] = "Highest average experience per match on this team or map.",
        ["Best gun"] = "Weapon this team got the most kills with.",

        // ── maps ──
        ["Played"] = "Counted matches on this map and mode.",
        ["Wins"] = "Match wins by each team on this map, all time.",
        ["Balance"] = "Share of decided matches won by the stronger side on this map, all time.",
        ["Round length"] = "Mean time from round start to round end on this map.",
        ["Rounds"] = "Rounds per match on this map: the average, the longest and the shortest.",
        ["Avg. players"] = "Mean number of players in a counted match on this map.",
        ["Kills/round"] = "Kills of an opponent per match round on this map.",

        // ── map highlights ──
        ["Map"] = "The map this card is about.",
        ["Matches"] = "Counted matches on this map and mode.",
        ["Red rounds"] = "Share of rounds red won on this map over the last 3 months, of maps with at least 40 rounds in that time.",
        ["Blue rounds"] = "Share of rounds blue won on this map over the last 3 months, of maps with at least 40 rounds in that time.",
        ["Split"] = "Round wins by each side over the last 3 months, of maps with at least 40 rounds in that time. Closest to 50/50 wins.",
        ["Avg. margin"] = "Mean gap between the two final scores, of maps with at least 3 counted matches.",
        ["HS rate"] = "Share of kills that were headshots, of maps with at least 200 kills.",
        ["Explosions"] = "Share of rounds where a planted bomb detonated, of maps with at least 40 rounds.",
        ["Defused"] = "Share of rounds with a plant where the bomb was defused, of maps with at least 20 such rounds.",

        // ── guns ──
        ["Kills"] = "Kills of an opponent. Teamkills and suicides are counted separately.",
        ["Headshots"] = "Kills flagged as a headshot.",
        ["Teamkills"] = "Kills where killer and victim were on the same team.",

        // ── players ──
        ["Rank"] = "Position on this page, ordered by total experience.",
        ["K/D ratio"] = "Kills divided by deaths. A player with no deaths counts as one death.",
        ["Deaths"] = "Times an opponent killed you. Suicides are counted separately.",
        ["Assists"] = "Assists credited to you in the end-of-match reports.",
        ["Team kills"] = "Kills where you and the victim were on the same team.",
        ["HS kills"] = "Your kills that were headshots, and what share of your kills that is.",
        ["Suicides"] = "Deaths where you were your own killer.",
        ["Chickens"] = "Chickens you killed, from the per-match chicken counter.",
        ["Avg. points"] = "Mean experience per counted match.",
        ["Total points"] = "Experience across all counted matches.",
        ["Rounds played"] = "Matches you appear in, not individual rounds.",
        ["First bloods"] = "Rounds where you landed the first kill of an opponent.",
        ["Best round"] = "Most opponents killed in a single round.",
        ["Kill streak"] = "Longest run of kills inside one round without being killed yourself.",
        ["Multikills"] = "Rounds with 3 or more kills. Aces are rounds with 5 or more.",
        ["Plays"] = "Share of matches started on the blue team.",
        ["Team switches"] = "Times you changed team mid-match.",
        ["Nemesis"] = "The opponent who killed you most often.",
        ["Favourite victim"] = "The opponent you killed most often.",
        ["Best map"] = "Map where your average experience per match is highest.",
        ["Best gun"] = "Weapon you got the most kills with, and what share of your kills that is.",
        ["Last seen"] = "When PPSC last saw you connected, in UTC.",
        ["Total time"] = "Time connected to this server, tracked by PPSC rather than by the match logs.",
        ["VAC"] = "Valve Anti-Cheat bans on the Steam account.",
        ["Country"] = "Country on the Steam profile, where it is public.",
        ["Cash"] = "Current cash balance.",
        ["Total cash"] = "Cash held by all players together.",

        // ── mentions and rivalries ──
        ["Player"] = "The player this card is about.",
        ["Score"] = "Experience points.",
        ["Streak"] = "Longest run of kills inside one round without being killed.",
        ["Aces"] = "Rounds with 5 or more kills.",
        ["Dead birds"] = "Chickens killed.",
        ["HSKR"] = "Headshot kills divided by total kills, for players with more than 10 kills.",
        ["Plants"] = "Bombs planted, from the end-of-match reports.",
        ["Defuses"] = "Bombs defused, from the end-of-match reports.",
        ["Exploded"] = "Share of this player's plants that detonated, for players with at least 10 plants. A plant that was still ticking when the round ended by elimination does not count.",
        ["Switches"] = "Times this player changed team mid-match.",
        ["Time"] = "Time connected to this server, tracked by PPSC.",
        ["Kills/hour"] = "Kills divided by time connected. Playtime is PPSC's presence tracking, so it includes warmups and idling.",
        ["Leads"] = "The player ahead in this pairing, and how often they killed the other.",
        ["Trails"] = "The player behind in this pairing, and how often they killed the other.",
        ["Total"] = "Kills between these two players in either direction.",
    };

    public static string? For(string label)
    {
        return StatsDescriptions.descriptions.GetValueOrDefault(label);
    }
}
