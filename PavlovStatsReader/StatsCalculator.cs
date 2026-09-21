using Microsoft.Extensions.Configuration;
using PavlovStatsReader.Models;
using System.Diagnostics;
using System.Linq;

namespace PavlovStatsReader;

public class StatsCalculator
{
    private readonly IConfiguration configuration;
    private readonly StatsContext statsContext;

    public StatsCalculator(StatsContext statsContext, IConfiguration configuration)
    {
        this.statsContext = statsContext;
        this.configuration = configuration;
    }

    public CBaseStats[] CalculateStats(string serverId, ulong[] relevantPlayers)
    {
        Console.WriteLine("Prefetching relevant data for stats calculation");

        Stopwatch sw = Stopwatch.StartNew();

        Console.WriteLine("Reading round states...");
        RoundState[] roundStates = this.statsContext.RoundStates.Where(r => r.ServerId == serverId).OrderBy(r => r.LogEntryDate).ToArray();
        Console.WriteLine("Reading kills...");
        KillData[] baseKillData = this.statsContext.KillData.Where(k => k.ServerId == serverId).ToArray();
        Console.WriteLine("Reading bombs...");
        BombData[] baseBombData = this.statsContext.BombData.Where(b => b.ServerId == serverId).ToArray();
        Console.WriteLine("Reading round ends...");
        RoundEnd[] baseRoundEnds = this.statsContext.RoundEnds.Where(r => r.ServerId == serverId).ToArray();
        Console.WriteLine("Reading end-of-map stats...");
        EndOfMapStats[] endOfMapStats = this.statsContext.EndOfMapStats.Where(m => m.ServerId == serverId).OrderBy(m => m.LogEntryDate).ToArray();

        // Pass 1: collect all valid round windows (Started→Ended pairs with a matching RoundEnd log entry).
        Console.WriteLine($"Building round windows from {roundStates.Length} round states...");
        var allRoundWindows = new List<(DateTime Start, DateTime End, int WinningTeam, KillData[] Kills, BombData[] Bombs)>();
        DateTime? roundStartTime = null;
        foreach (RoundState state in roundStates)
        {
            if (state.State == "Started")
            {
                // Clean or dirty start — always reset the window to the latest "Started" event.
                roundStartTime = state.LogEntryDate;
            }
            else if (roundStartTime.HasValue && state.State == "Ended")
            {
                DateTime roundEndTime = state.LogEntryDate;
                RoundEnd? roundEnd = baseRoundEnds.FirstOrDefault(m => m.LogEntryDate >= roundEndTime.AddSeconds(-2) && m.LogEntryDate <= roundEndTime.AddSeconds(2));

                if (roundEnd == null)
                {
                    Console.WriteLine($"Round end not found for round {roundStartTime}→{roundEndTime}, skipping");
                    roundStartTime = null;
                    continue;
                }

                allRoundWindows.Add((
                    roundStartTime.Value,
                    roundEndTime,
                    roundEnd.WinningTeam,
                    baseKillData.Where(k => k.LogEntryDate >= roundStartTime && k.LogEntryDate <= roundEndTime).ToArray(),
                    baseBombData.Where(b => b.LogEntryDate >= roundStartTime && b.LogEntryDate <= roundEndTime).ToArray()
                ));
                roundStartTime = null;
            }
        }

        // Pass 2: for each map session (delimited by EndOfMapStats events), keep only the last
        // (Team0Score + Team1Score) round windows — those are the real match rounds. Warmup
        // rounds, SND resets and any other pre-match noise that occurred earlier in the session
        // are discarded. Sessions with fewer than 4 real rounds are skipped entirely.
        Console.WriteLine($"Filtering to real match rounds across {endOfMapStats.Length} map sessions...");
        List<KillData> killData = new();
        List<BombData> bombData = new();
        List<RoundWindow> matchRounds = new();
        DateTime sessionStart = DateTime.MinValue;
        foreach (EndOfMapStats eoms in endOfMapStats)
        {
            int realRoundCount = eoms.Team0Score + eoms.Team1Score;
            if (realRoundCount < 4)
            {
                sessionStart = eoms.LogEntryDate;
                continue;
            }

            var sessionRounds = allRoundWindows
                .Where(r => r.Start > sessionStart && r.End <= eoms.LogEntryDate)
                .OrderBy(r => r.Start)
                .ToList();

            foreach (var round in sessionRounds.TakeLast(realRoundCount))
            {
                killData.AddRange(round.Kills);
                bombData.AddRange(round.Bombs);
                matchRounds.Add(new RoundWindow(round.Start, round.End, round.WinningTeam, eoms.MapLabel, eoms.GameMode, round.Kills, round.Bombs));
            }

            sessionStart = eoms.LogEntryDate;
        }

        List<Stats> playerStats = this.statsContext.PlayerStats.Where(m => m.ServerId == serverId).SelectMany(m => m.Stats).ToList();

        Console.WriteLine($"Prefetching took {sw.ElapsedMilliseconds}ms");
        Console.WriteLine("Calculating stats");
        sw.Restart();

        List<CBaseStats> allStats = new()
        {
            this.CalculateServerStats(serverId, killData, bombData, matchRounds),
        };

        allStats.AddRange(this.CalculateMapStats(serverId, relevantPlayers, matchRounds));
        allStats.AddRange(this.CalculateGunStats(serverId, relevantPlayers, killData));
        allStats.AddRange(this.CalculateTeamStatistics(serverId, relevantPlayers, killData));
        allStats.AddRange(this.CalculatePlayerStats(serverId, relevantPlayers, killData, matchRounds));
        allStats.AddRange(this.CalculateRivalries(relevantPlayers, killData));
        allStats.AddRange(this.CalculatePopulationStats(serverId));

        Console.WriteLine($"Calculation took {sw.ElapsedMilliseconds}ms");

        return allStats.ToArray();
    }


    public CServerStats CalculateServerStats(string serverId, List<KillData> killData, List<BombData> bombData, List<RoundWindow> matchRounds)
    {
        int uniqueMapCount = this.statsContext.EndOfMapStats.Where(m => m.ServerId == serverId).GroupBy(m => new
        {
            m.MapLabel,
            m.GameMode,
        }).Count();
        int uniquePlayerCount = this.statsContext.PlayerStats.Where(m => m.ServerId == serverId).GroupBy(m => m.UniqueId).Count();
        int totalMatches = this.statsContext.EndOfMapStats.Count(m => m.ServerId == serverId && m.PlayerCount >= 2 && m.Team0Score + m.Team1Score >= 10);
        int totalKills = killData.Count();
        int totalHeadshots = killData.Count(m => m.Headshot);
        int totalAssists = this.statsContext.EndOfMapStats.Where(m => m.ServerId == serverId).SelectMany(m => m.PlayerStats).SelectMany(m => m.Stats).Where(m => m.StatType == "Assist").Sum(m => m.Amount);
        int totalTeamkills = killData.Count(m => m.KilledTeamID == m.KillerTeamID);
        int totalBombPlants = bombData.Count(m => m.BombInteraction == "BombPlanted");
        int totalBombDefuses = bombData.Count(m => m.BombInteraction == "BombDefused");
        int totalBombExplosions = bombData.Count(m => m.BombInteraction == "BombExploded");
        int totalChickensKilled = this.statsContext.EndOfMapStats.Where(m => m.ServerId == serverId).SelectMany(m => m.PlayerStats).SelectMany(m => m.Stats).Where(m => m.StatType == "ChickenKilled").Sum(m => m.Amount);
        int totalPoints = this.statsContext.EndOfMapStats.Where(m => m.ServerId == serverId).SelectMany(m => m.PlayerStats).SelectMany(m => m.Stats).Where(s => s.StatType == "Experience").Sum(m => m.Amount);
        int totalRoundsPlayed = this.statsContext.EndOfMapStats.Where(m => m.ServerId == serverId).Sum(m => m.Team0Score + m.Team1Score);

        var mostKillsByGun = killData.GroupBy(k => k.KilledBy).Select(k => new
        {
            Gun = k.Key,
            Count = k.Count(),
        }).OrderByDescending(k => k.Count).FirstOrDefault();

        // A round's first enemy kill, and whether that player's team went on to win it.
        int firstBloodRounds = 0;
        int firstBloodWins = 0;
        foreach (RoundWindow round in matchRounds)
        {
            KillData? firstBlood = round.EnemyKills().FirstOrDefault();
            if (firstBlood == null)
            {
                continue;
            }

            firstBloodRounds++;
            if (firstBlood.KillerTeamID == round.WinningTeam)
            {
                firstBloodWins++;
            }
        }

        double[] roundSeconds = matchRounds.Select(r => (r.End - r.Start).TotalSeconds).Where(seconds => seconds is > 0 and < 3600).ToArray();

        EndOfMapStats[] realMatches = this.statsContext.EndOfMapStats.Where(m => m.ServerId == serverId && m.PlayerCount >= 2 && m.Team0Score + m.Team1Score >= 10).ToArray();
        EndOfMapStats? blowout = realMatches.MaxBy(m => Math.Abs(m.Team0Score - m.Team1Score));
        int nailbiters = realMatches.Count(m => Math.Abs(m.Team0Score - m.Team1Score) == 1);

        return new CServerStats
        {
            FirstBloodRounds = firstBloodRounds,
            FirstBloodWins = firstBloodWins,
            AverageRoundSeconds = roundSeconds.Length == 0 ? 0 : roundSeconds.Average(),
            FastestRoundSeconds = roundSeconds.Length == 0 ? 0 : roundSeconds.Min(),
            LongestRoundSeconds = roundSeconds.Length == 0 ? 0 : roundSeconds.Max(),
            BiggestBlowoutMap = blowout?.MapLabel,
            BiggestBlowoutGameMode = blowout?.GameMode,
            BiggestBlowoutScore = blowout == null ? null : $"{Math.Max(blowout.Team0Score, blowout.Team1Score)}-{Math.Min(blowout.Team0Score, blowout.Team1Score)}",
            NailbiterMatches = nailbiters,
            MostPopularGun = mostKillsByGun?.Gun,
            MostPopularGunKillCount = mostKillsByGun?.Count ?? 0,
            TotalAssists = totalAssists,
            TotalBombDefuses = totalBombDefuses,
            TotalBombExplosions = totalBombExplosions,
            TotalBombPlants = totalBombPlants,
            TotalHeadshots = totalHeadshots,
            TotalKills = totalKills,
            TotalMatchesPlayed = totalMatches,
            TotalTeamkills = totalTeamkills,
            TotalUniqueMaps = uniqueMapCount,
            TotalUniquePlayers = uniquePlayerCount,
            TotalChickensKilled = totalChickensKilled,
            TotalPoints = totalPoints,
            TotalRoundsPlayed = totalRoundsPlayed,
        };
    }

    public CTeamStats[] CalculateTeamStatistics(string serverId, ulong[] relevantPlayers, List<KillData> killData)
    {
        List<CTeamStats> teamStats = new();
        for (int i = 0; i < 2; i++)
        {
            IQueryable<PlayerStats> teamPlayerStats = this.statsContext.EndOfMapStats.Where(m => m.ServerId == serverId && m.PlayerCount >= 2 && m.Team0Score + m.Team1Score >= 10).SelectMany(m => m.PlayerStats).Where(m => m.TeamId == i);
            IQueryable<Stats> teamPlayerStatsStats = teamPlayerStats.SelectMany(p => p.Stats);

            int winCount;
            if (i == 0)
            {
                winCount = this.statsContext.EndOfMapStats.Where(m => m.ServerId == serverId && m.PlayerCount >= 2 && m.Team0Score + m.Team1Score >= 10).Count(m => m.Team0Score > m.Team1Score);
            }
            else
            {
                winCount = this.statsContext.EndOfMapStats.Where(m => m.ServerId == serverId && m.PlayerCount >= 2 && m.Team0Score + m.Team1Score >= 10).Count(m => m.Team1Score > m.Team0Score);
            }

            var bestPlayer = teamPlayerStats.AsEnumerable().Where(t => relevantPlayers.Contains(t.UniqueId)).GroupBy(t => t.UniqueId).Select(g => new
            {
                Player = g.Key,
                AvgScore = g.Average(this.calculatePlayerScore),
            }).MaxBy(g => g.AvgScore);
            var bestGun = killData.Where(m => m.KillerTeamID == i).GroupBy(k => k.KilledBy).Select(g => new
            {
                Gun = g.Key,
                Count = g.Count(),
            }).OrderByDescending(g => g.Count).FirstOrDefault();

            teamStats.Add(new CTeamStats
            {
                TeamId = i,
                Name = i == 0 ? "Blue Team" : "Red Team",
                TotalKills = killData.Where(k => k.KillerTeamID == i).Count(),
                TotalAssists = teamPlayerStatsStats.Where(p => p.StatType == "Assist").Sum(p => p.Amount),
                TotalDeaths = killData.Where(k => k.KilledTeamID == i).Count(),
                TotalHeadshots = killData.Where(k => k.KillerTeamID == i).Count(k => k.Headshot),
                TotalTeamkills = killData.Count(k => k.KillerTeamID == i && k.KilledTeamID == i),
                TotalVictories = winCount,
                BestPlayer = bestPlayer?.Player,
                BestPlayerAverageScore = bestPlayer?.AvgScore ?? 0d,
                BestGun = bestGun?.Gun,
                BestGunKillCount = bestGun?.Count ?? 0,
            });
        }

        return teamStats.ToArray();
    }

    public CGunStats[] CalculateGunStats(string serverId, ulong[] relevantPlayers, List<KillData> killData)
    {
        List<CGunStats> gunsStats = new();

        string[] guns = killData.GroupBy(m => m.KilledBy).Select(m => m.Key).ToArray();
        guns.AsParallel().ForAll(gunName =>
        {
            using (StatsContext statsContext = new(this.configuration))
            {
                List<KillData> gunStats = killData.Where(m => m.KilledBy == gunName).ToList();

                int kills = gunStats.Count();
                int headshots = gunStats.Count(g => g.Headshot);
                int teamKills = gunStats.Count(g => g.KillerTeamID == g.KilledTeamID);

                var mostKills = gunStats.GroupBy(g => g.Killer).Where(g => relevantPlayers.Contains(g.Key)).Select(g => new
                {
                    Player = g.Key,
                    Count = g.Count(),
                }).OrderByDescending(g => g.Count).FirstOrDefault();

                lock (gunsStats)
                {
                    gunsStats.Add(new CGunStats
                    {
                        Name = gunName,
                        Kills = kills,
                        Headshots = headshots,
                        BestPlayer = mostKills?.Player,
                        BestPlayerKills = mostKills?.Count ?? 0,
                        TeamKills = teamKills,
                    });
                }
            }
        });

        return gunsStats.ToArray();
    }

    public CPlayerStats[] CalculatePlayerStats(string serverId, ulong[] relevantPlayers, List<KillData> killData, List<RoundWindow> matchRounds)
    {
        Setting? serverStatMode = this.statsContext.Settings.FirstOrDefault(s => s.Name == "Stat Type" && s.ServerId == serverId);
        List<CPlayerStats> playerStats = new();

        // Worked out once for everyone rather than per player: the per-player loop
        // below runs in parallel and these all need a pass over every round.
        Dictionary<ulong, PlayerRoundStats> roundStats = this.calculateRoundDerivedStats(matchRounds);
        Dictionary<ulong, PlayerDuelStats> duels = this.calculateDuels(killData);
        Dictionary<ulong, int> teamSwitches = this.statsContext.SwitchTeams
            .Where(s => s.ServerId == serverId)
            .GroupBy(s => s.PlayerID)
            .Select(g => new { Player = g.Key, Count = g.Count() })
            .ToDictionary(g => g.Player, g => g.Count);

        if (serverStatMode?.Value == "SND")
        {
            PlayerStats[] pstats = this.statsContext.EndOfMapStats.Where(m => m.ServerId == serverId && m.PlayerCount >= 2 && m.Team0Score + m.Team1Score >= 10).SelectMany(m => m.PlayerStats).Where(p => relevantPlayers.Contains(p.UniqueId)).ToArray();

            pstats.GroupBy(p => p.UniqueId).AsParallel().ForAll(pgrp =>
            {
                using StatsContext statsContext = new(this.configuration);
                IGrouping<ulong, PlayerStats> playerGrouping = statsContext.EndOfMapStats.Where(m => m.ServerId == serverId && m.PlayerCount >= 2 && m.Team0Score + m.Team1Score >= 10).SelectMany(m => m.PlayerStats).Where(p => p.UniqueId == pgrp.Key).AsEnumerable().GroupBy(p => p.UniqueId).First();
                int teamKills = killData.Count(k => k.Killer == playerGrouping.Key && k.KilledTeamID == k.KillerTeamID && k.Killer != k.Killed);
                int suicides = killData.Count(k => k.Killer == playerGrouping.Key && k.Killed == playerGrouping.Key);
                int kills = killData.Count(k => k.Killer == playerGrouping.Key && k.KilledTeamID != k.KillerTeamID && k.Killer != k.Killed);
                int deaths = killData.Count(k => k.Killed == playerGrouping.Key && k.Killer != k.Killed);
                int assists = playerGrouping.Sum(p => p.Stats.FirstOrDefault(s => s.StatType == "Assist")?.Amount ?? 0);
                int headshots = killData.Count(k => k.Killer == playerGrouping.Key && k.Headshot && k.KilledTeamID != k.KillerTeamID && k.Killer != k.Killed);
                int bombsPlanted = playerGrouping.Sum(p => p.Stats.FirstOrDefault(s => s.StatType == "BombPlanted")?.Amount ?? 0);
                int bombsDefused = playerGrouping.Sum(p => p.Stats.FirstOrDefault(s => s.StatType == "BombDefused")?.Amount ?? 0);
                int totalScore = playerGrouping.Sum(this.calculatePlayerScore);
                double averageScore = playerGrouping.Average(this.calculatePlayerScore);
                int roundsPlayed = playerGrouping.Count();
                int chickensKilled = playerGrouping.Sum(p => p.Stats.FirstOrDefault(s => s.StatType == "ChickenKilled")?.Amount ?? 0);
                int blueMatches = playerGrouping.Count(p => p.TeamId == 0);
                int redMatches = playerGrouping.Count(p => p.TeamId == 1);

                var playerKillsGroup = killData.Where(d => d.Killer == playerGrouping.Key).GroupBy(k => k.KilledBy).Select(g => new
                {
                    Gun = g.Key,
                    Count = g.Count(),
                }).OrderByDescending(g => g.Count).FirstOrDefault();
                string? mostKillsWith = playerKillsGroup?.Gun;
                int mostKillsWithAmount = playerKillsGroup?.Count ?? 0;

                var playerMapAverageScoreGroup = playerGrouping.Select(g => g.EndOfMapStats).GroupBy(m => new
                {
                    Map = m.MapLabel,
                    m.GameMode,
                }).Select(g => new
                {
                    g.Key.Map,
                    g.Key.GameMode,
                    AverageScore = g.Average(m => this.calculatePlayerScore(m.PlayerStats.First(p => p.UniqueId == playerGrouping.Key))),
                }).MaxBy(g => g.AverageScore);
                string? bestMap = playerMapAverageScoreGroup?.Map;
                string? bestMapGameMode = playerMapAverageScoreGroup?.GameMode;
                double bestMapAverageScore = playerMapAverageScoreGroup?.AverageScore ?? 0;

                lock (playerStats)
                {
                    playerStats.Add(this.applyExtras(new CPlayerStats
                    {
                        UniqueId = playerGrouping.Key,
                        PlayerName = playerGrouping.First().PlayerName,
                        Kills = kills,
                        Deaths = deaths,
                        Assists = assists,
                        TeamKills = teamKills,
                        Headshots = headshots,
                        Suicides = suicides,
                        BombsPlanted = bombsPlanted,
                        BombsDefused = bombsDefused,
                        TotalScore = totalScore,
                        AverageScore = averageScore,
                        MostKillsWithGun = mostKillsWith,
                        MostKillsWithGunAmount = mostKillsWithAmount,
                        BestMap = bestMap,
                        BestMapGameMode = bestMapGameMode,
                        BestMapAverageScore = bestMapAverageScore,
                        RoundsPlayed = roundsPlayed,
                        ChickensKilled = chickensKilled,
                        BlueMatches = blueMatches,
                        RedMatches = redMatches,
                        TeamSwitches = teamSwitches.GetValueOrDefault(playerGrouping.Key),
                    }, roundStats.GetValueOrDefault(playerGrouping.Key), duels.GetValueOrDefault(playerGrouping.Key)));
                }
            });
        }
        else
        {
            ulong[] players = this.statsContext.KillData.Where(k => k.ServerId == serverId).Select(k => k.Killer).Distinct().ToArray().Union(this.statsContext.KillData.Where(k => k.ServerId == serverId).Select(k => k.Killed).Distinct().ToArray()).Distinct().Where(p => relevantPlayers.Contains(p)).ToArray();

            //int current = 0;
            foreach (ulong player in players)
            {
                //Console.WriteLine($"Generating stats for player {player} ({++current} / {players.Length})");

                int kills = killData.Count(k => k.Killer == player);
                int deaths = killData.Count(k => k.Killed == player);
                int headshots = killData.Count(k => k.Killer == player && k.Headshot);
                int suicides = killData.Count(k => k.Killer == player && k.Killed == player);
                var playerKillsGroup = killData.Where(k => k.Killer == player).GroupBy(k => k.KilledBy).Select(g => new { Gun = g.Key, Count = g.Count() }).OrderByDescending(g => g.Count).FirstOrDefault();
                string? mostKillsWith = playerKillsGroup?.Gun;
                int mostKillsWithAmount = playerKillsGroup?.Count ?? 0;

                playerStats.Add(this.applyExtras(new CPlayerStats
                {
                    UniqueId = player,
                    Kills = kills,
                    Deaths = deaths,
                    Headshots = headshots,
                    Suicides = suicides,
                    MostKillsWithGun = mostKillsWith,
                    MostKillsWithGunAmount = mostKillsWithAmount,
                    TeamSwitches = teamSwitches.GetValueOrDefault(player),
                }, roundStats.GetValueOrDefault(player), duels.GetValueOrDefault(player)));
            }
        }

        return playerStats.ToArray();
    }

    public CMapStats[] CalculateMapStats(string serverId, ulong[] relevantPlayers, List<RoundWindow> matchRounds)
    {
        EndOfMapStats[] eoms = this.statsContext.EndOfMapStats.Where(m => m.ServerId == serverId).ToArray();

        Dictionary<(string map, string mode), double> averageRoundSeconds = matchRounds
            .Select(r => new { Key = (map: r.MapLabel, mode: r.GameMode.ToLower()), Seconds = (r.End - r.Start).TotalSeconds })
            .Where(r => r.Seconds is > 0 and < 3600)
            .GroupBy(r => r.Key)
            .ToDictionary(g => g.Key, g => g.Average(r => r.Seconds));

        DateTime recentCutoff = DateTime.Now.AddMonths(-3);

        List<CMapStats> mapsStats = new();

        eoms.Where(m => m.PlayerCount >= 2 && m.Team0Score + m.Team1Score >= 10).GroupBy(m => (mapLabel: m.MapLabel, gameMode: m.GameMode)).AsParallel().ForAll(mgrp =>
        {
            using StatsContext statsContext = new(this.configuration);
            IGrouping<(string mapLabel, string gameMode), EndOfMapStats> mapGrouping = statsContext.EndOfMapStats.Where(m => m.ServerId == serverId && m.PlayerCount >= 2 && m.Team0Score + m.Team1Score >= 10 && m.MapLabel == mgrp.Key.mapLabel && m.GameMode.ToLower() == mgrp.Key.gameMode.ToLower()).ToArray().GroupBy(m => (mapLabel: m.MapLabel, gameMode: m.GameMode)).First();
            int[] totalScore = mapGrouping.Select(e => e.Team0Score + e.Team1Score).ToArray();
            IGrouping<ulong, (PlayerStats player, int score)>[] playerStats = mapGrouping.SelectMany(e => e.PlayerStats).Where(p => relevantPlayers.Contains(p.UniqueId)).Select(e => (player: e, score: this.calculatePlayerScore(e))).GroupBy(p => p.player.UniqueId).ToArray();

            IGrouping<ulong, (PlayerStats player, int score)>? bestPlayerGrouping = playerStats.MaxBy(g => g.Average(p => p.score));
            ulong? bestPlayer = bestPlayerGrouping?.Key;
            double maxAveragePlayerScore = bestPlayerGrouping?.Average(p => p.score) ?? 0;

            lock (mapsStats)
            {
                mapsStats.Add(new CMapStats
                {
                    MapId = mapGrouping.Key.mapLabel,
                    GameMode = mapGrouping.Key.gameMode,
                    PlayCount = mapGrouping.Count(),
                    Team0Wins = mapGrouping.Count(e => e.Team0Score > e.Team1Score),
                    Team1Wins = mapGrouping.Count(e => e.Team1Score > e.Team0Score),
                    AverageRounds = totalScore.Average(),
                    MaxRounds = totalScore.Max(),
                    MinRounds = totalScore.Min(),
                    BestPlayer = bestPlayer,
                    MaxAveragePlayerScore = maxAveragePlayerScore,
                    AverageRoundSeconds = averageRoundSeconds.GetValueOrDefault((mapGrouping.Key.mapLabel, mapGrouping.Key.gameMode.ToLower())),
                    RecentTeam0Wins = mapGrouping.Count(e => e.LogEntryDate >= recentCutoff && e.Team0Score > e.Team1Score),
                    RecentTeam1Wins = mapGrouping.Count(e => e.LogEntryDate >= recentCutoff && e.Team1Score > e.Team0Score),
                    LastPlayed = mapGrouping.Max(e => e.LogEntryDate),
                });
            }
        });

        return mapsStats.ToArray();
    }

    /// <summary>
    ///     Pairs every player with the opponent who killed them most and the one
    ///     they killed most. One pass over the kills, since this table is by far
    ///     the biggest one here.
    /// </summary>
    /// <summary>
    ///     Folds the two precomputed lookups onto a player's stats. Both are
    ///     optional: a player with no match rounds or no duels simply keeps the
    ///     defaults.
    /// </summary>
    private CPlayerStats applyExtras(CPlayerStats playerStats, PlayerRoundStats? roundStats, PlayerDuelStats? duels)
    {
        if (roundStats != null)
        {
            playerStats.FirstBloods = roundStats.FirstBloods;
            playerStats.BestRoundKills = roundStats.BestRoundKills;
            playerStats.Multikills = roundStats.Multikills;
            playerStats.Aces = roundStats.Aces;
            playerStats.LongestKillStreak = roundStats.LongestKillStreak;
            playerStats.PlantsMade = roundStats.PlantsMade;
            playerStats.PlantsConverted = roundStats.PlantsConverted;
        }

        if (duels != null)
        {
            playerStats.Nemesis = duels.Nemesis;
            playerStats.NemesisKills = duels.NemesisKills;
            playerStats.FavouriteVictim = duels.FavouriteVictim;
            playerStats.FavouriteVictimKills = duels.FavouriteVictimKills;
        }

        return playerStats;
    }

    private Dictionary<ulong, PlayerDuelStats> calculateDuels(List<KillData> killData)
    {
        Dictionary<(ulong killer, ulong killed), int> pairCounts = new();
        foreach (KillData kill in killData)
        {
            if (kill.Killer == kill.Killed || kill.KillerTeamID == kill.KilledTeamID)
            {
                continue;
            }

            pairCounts.TryGetValue((kill.Killer, kill.Killed), out int count);
            pairCounts[(kill.Killer, kill.Killed)] = count + 1;
        }

        Dictionary<ulong, PlayerDuelStats> duels = new();

        PlayerDuelStats duelsFor(ulong player)
        {
            if (!duels.TryGetValue(player, out PlayerDuelStats? stats))
            {
                stats = new PlayerDuelStats();
                duels[player] = stats;
            }

            return stats;
        }

        foreach (((ulong killer, ulong killed), int count) in pairCounts)
        {
            PlayerDuelStats killerStats = duelsFor(killer);
            if (count > killerStats.FavouriteVictimKills)
            {
                killerStats.FavouriteVictim = killed;
                killerStats.FavouriteVictimKills = count;
            }

            PlayerDuelStats killedStats = duelsFor(killed);
            if (count > killedStats.NemesisKills)
            {
                killedStats.Nemesis = killer;
                killedStats.NemesisKills = count;
            }
        }

        return duels;
    }

    /// <summary>
    ///     Everything that only means something inside a single round: first
    ///     bloods, best round, multikills, kill streaks and whether a plant went on
    ///     to explode.
    /// </summary>
    private Dictionary<ulong, PlayerRoundStats> calculateRoundDerivedStats(List<RoundWindow> matchRounds)
    {
        Dictionary<ulong, PlayerRoundStats> roundStats = new();

        PlayerRoundStats statsFor(ulong player)
        {
            if (!roundStats.TryGetValue(player, out PlayerRoundStats? stats))
            {
                stats = new PlayerRoundStats();
                roundStats[player] = stats;
            }

            return stats;
        }

        foreach (RoundWindow round in matchRounds)
        {
            KillData[] enemyKills = round.EnemyKills().ToArray();
            if (enemyKills.Length > 0)
            {
                statsFor(enemyKills[0].Killer).FirstBloods++;
            }

            // Kills in this round, and the longest run of them a player managed
            // before being killed themselves.
            Dictionary<ulong, int> killsThisRound = new();
            Dictionary<ulong, int> currentStreak = new();
            foreach (KillData kill in enemyKills)
            {
                killsThisRound.TryGetValue(kill.Killer, out int kills);
                killsThisRound[kill.Killer] = kills + 1;

                currentStreak.TryGetValue(kill.Killer, out int streak);
                streak++;
                currentStreak[kill.Killer] = streak;

                PlayerRoundStats killerStats = statsFor(kill.Killer);
                if (streak > killerStats.LongestKillStreak)
                {
                    killerStats.LongestKillStreak = streak;
                }

                currentStreak[kill.Killed] = 0;
            }

            foreach ((ulong player, int kills) in killsThisRound)
            {
                PlayerRoundStats stats = statsFor(player);
                if (kills > stats.BestRoundKills)
                {
                    stats.BestRoundKills = kills;
                }

                if (kills >= 3)
                {
                    stats.Multikills++;
                }

                if (kills >= 5)
                {
                    stats.Aces++;
                }
            }

            // Counted from the bomb log rather than the end-of-match reports, so
            // that plants and explosions come from the same place and can be put
            // over one another.
            bool exploded = round.Bombs.Any(b => b.BombInteraction == "BombExploded");
            foreach (ulong planter in round.Bombs.Where(b => b.BombInteraction == "BombPlanted").Select(b => b.Player).Distinct())
            {
                PlayerRoundStats planterStats = statsFor(planter);
                planterStats.PlantsMade++;
                if (exploded)
                {
                    planterStats.PlantsConverted++;
                }
            }
        }

        return roundStats;
    }

    /// <summary>
    ///     The pair that fought the most, and the pair where it was the most
    ///     one-sided. Both need enough kills between them to not be a fluke.
    /// </summary>
    public CRivalryStats[] CalculateRivalries(ulong[] relevantPlayers, List<KillData> killData)
    {
        const int minimumKills = 15;

        // Set lookup, not the array: this runs once per kill and there are tens of
        // thousands of them.
        HashSet<ulong> relevant = relevantPlayers.ToHashSet();

        Dictionary<(ulong low, ulong high), (int lowKills, int highKills)> pairs = new();
        foreach (KillData kill in killData)
        {
            if (kill.Killer == kill.Killed || kill.KillerTeamID == kill.KilledTeamID)
            {
                continue;
            }

            if (!relevant.Contains(kill.Killer) || !relevant.Contains(kill.Killed))
            {
                continue;
            }

            ulong low = Math.Min(kill.Killer, kill.Killed);
            ulong high = Math.Max(kill.Killer, kill.Killed);
            pairs.TryGetValue((low, high), out (int lowKills, int highKills) counts);
            pairs[(low, high)] = kill.Killer == low
                ? (counts.lowKills + 1, counts.highKills)
                : (counts.lowKills, counts.highKills + 1);
        }

        var candidates = pairs
            .Select(p => new
            {
                Low = p.Key.low,
                High = p.Key.high,
                LowKills = p.Value.lowKills,
                HighKills = p.Value.highKills,
                Total = p.Value.lowKills + p.Value.highKills,
            })
            .Where(p => p.Total >= minimumKills)
            .ToArray();

        if (candidates.Length == 0)
        {
            return Array.Empty<CRivalryStats>();
        }

        CRivalryStats build(ulong low, ulong high, int lowKills, int highKills, string kind)
        {
            // A is always whoever is ahead, so the card reads the right way round.
            bool lowAhead = lowKills >= highKills;

            return new CRivalryStats
            {
                Kind = kind,
                PlayerA = lowAhead ? low : high,
                PlayerB = lowAhead ? high : low,
                AKills = lowAhead ? lowKills : highKills,
                BKills = lowAhead ? highKills : lowKills,
            };
        }

        var feud = candidates.MaxBy(p => p.Total)!;
        var lopsided = candidates
            .OrderByDescending(p => (double)Math.Max(p.LowKills, p.HighKills) / (Math.Min(p.LowKills, p.HighKills) + 1))
            .ThenByDescending(p => p.Total)
            .First();

        List<CRivalryStats> rivalries = new()
        {
            build(feud.Low, feud.High, feud.LowKills, feud.HighKills, "feud"),
        };

        // Only worth a second card if it is a different pair.
        if (lopsided.Low != feud.Low || lopsided.High != feud.High)
        {
            rivalries.Add(build(lopsided.Low, lopsided.High, lopsided.LowKills, lopsided.HighKills, "lopsided"));
        }

        return rivalries.ToArray();
    }

    /// <summary>
    ///     Average and peak player count per calendar month, for the trend chart.
    /// </summary>
    public CPopulationStats[] CalculatePopulationStats(string serverId)
    {
        return this.statsContext.EndOfMapStats
            .Where(m => m.ServerId == serverId && m.PlayerCount >= 2 && m.Team0Score + m.Team1Score >= 10)
            .AsEnumerable()
            .GroupBy(m => new { m.LogEntryDate.Year, m.LogEntryDate.Month })
            .OrderBy(g => g.Key.Year)
            .ThenBy(g => g.Key.Month)
            .Select(g => new CPopulationStats
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                AveragePlayers = g.Average(m => m.PlayerCount),
                MaxPlayers = g.Max(m => m.PlayerCount),
                Matches = g.Count(),
            })
            .ToArray();
    }

    private sealed class PlayerDuelStats
    {
        public ulong? Nemesis { get; set; }

        public int NemesisKills { get; set; }

        public ulong? FavouriteVictim { get; set; }

        public int FavouriteVictimKills { get; set; }
    }

    private sealed class PlayerRoundStats
    {
        public int FirstBloods { get; set; }

        public int BestRoundKills { get; set; }

        public int Multikills { get; set; }

        public int Aces { get; set; }

        public int LongestKillStreak { get; set; }

        public int PlantsMade { get; set; }

        public int PlantsConverted { get; set; }
    }

    private int calculatePlayerScore(PlayerStats playerStats)
    {
        return playerStats.Stats.FirstOrDefault(s => s.StatType == "Experience")?.Amount ?? 0;
    }
}
