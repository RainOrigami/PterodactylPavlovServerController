
using Microsoft.Extensions.Configuration;
using PavlovStatsReader.Models;

namespace PavlovStatsReader
{
    public class StatsCalculator
    {
        private readonly StatsContext statsContext;
        private readonly IConfiguration configuration;

        public StatsCalculator(StatsContext statsContext, IConfiguration configuration)
        {
            this.statsContext = statsContext;
            this.configuration = configuration;
        }

        public CBaseStats[] CalculateStats(string serverId)
        {
            List<CBaseStats> allStats = new List<CBaseStats>();
            allStats.AddRange(CalculateMapStats(serverId));
            allStats.AddRange(CalculatePlayerStats(serverId));
            return allStats.ToArray();
        }

        public CPlayerStats[] CalculatePlayerStats(string serverId)
        {
            PlayerStats[] pstats = statsContext.EndOfMapStats.Where(m => m.ServerId == serverId && m.PlayerCount >= 2 && m.Team0Score + m.Team1Score >= 10).SelectMany(m => m.PlayerStats).ToArray();

            List<CPlayerStats> playerStats = new List<CPlayerStats>();

            pstats.GroupBy(p => p.UniqueId).AsParallel().ForAll(pgrp =>
            {
                using (StatsContext statsContext = new StatsContext(configuration))
                {
                    IGrouping<ulong, PlayerStats> playerGrouping = statsContext.EndOfMapStats.Where(m => m.ServerId == serverId && m.PlayerCount >= 2 && m.Team0Score + m.Team1Score >= 10).SelectMany(m => m.PlayerStats).Where(p => p.UniqueId == pgrp.Key).AsEnumerable().GroupBy(p => p.UniqueId).First();
                    int kills = playerGrouping.Sum(p => p.Stats.FirstOrDefault(s => s.StatType == "Kill")?.Amount ?? 0);
                    int deaths = playerGrouping.Sum(p => p.Stats.FirstOrDefault(s => s.StatType == "Death")?.Amount ?? 0);
                    int assists = playerGrouping.Sum(p => p.Stats.FirstOrDefault(s => s.StatType == "Assist")?.Amount ?? 0);
                    int teamKills = playerGrouping.Sum(p => p.Stats.FirstOrDefault(s => s.StatType == "TeamKill")?.Amount ?? 0);
                    int headshots = playerGrouping.Sum(p => p.Stats.FirstOrDefault(s => s.StatType == "Headshot")?.Amount ?? 0);
                    int bombsPlanted = playerGrouping.Sum(p => p.Stats.FirstOrDefault(s => s.StatType == "BombPlanted")?.Amount ?? 0);
                    int bombsDefused = playerGrouping.Sum(p => p.Stats.FirstOrDefault(s => s.StatType == "BombDefused")?.Amount ?? 0);
                    int totalScore = playerGrouping.Sum(p => calculatePlayerScore(p));
                    double averageScore = playerGrouping.Average(p => calculatePlayerScore(p));

                    lock (playerStats)
                    {
                        playerStats.Add(new CPlayerStats()
                        {
                            UniqueId = playerGrouping.Key,
                            PlayerName = playerGrouping.First().PlayerName,
                            Kills = kills,
                            Deaths = deaths,
                            Assists = assists,
                            TeamKills = teamKills,
                            Headshots = headshots,
                            BombsPlanted = bombsPlanted,
                            BombsDefused = bombsDefused,
                            TotalScore = totalScore,
                            AverageScore = averageScore
                        });
                    }
                }
            });

            return playerStats.ToArray();
        }

        public CMapStats[] CalculateMapStats(string serverId)
        {
            EndOfMapStats[] eoms = statsContext.EndOfMapStats.Where(m => m.ServerId == serverId).ToArray();

            List<CMapStats> mapsStats = new List<CMapStats>();

            eoms.Where(m => m.PlayerCount >= 2 && m.Team0Score + m.Team1Score >= 10).GroupBy(m => (mapLabel: m.MapLabel, gameMode: m.GameMode)).AsParallel().ForAll(mgrp =>
            {
                using (StatsContext statsContext = new StatsContext(configuration))
                {
                    IGrouping<(string mapLabel, string gameMode), EndOfMapStats> mapGrouping = statsContext.EndOfMapStats.Where(m => m.ServerId == serverId && m.PlayerCount >= 2 && m.Team0Score + m.Team1Score >= 10 && m.MapLabel == mgrp.Key.mapLabel && m.GameMode == mgrp.Key.gameMode).ToArray().GroupBy(m => (mapLabel: m.MapLabel, gameMode: m.GameMode)).First();
                    int[] totalScore = mapGrouping.Select(e => e.Team0Score + e.Team1Score).ToArray();
                    IGrouping<ulong, (PlayerStats player, int score)>[] playerStats = mapGrouping.SelectMany(e => e.PlayerStats).Select(e => (player: e, score: calculatePlayerScore(e))).GroupBy(p => p.player.UniqueId).ToArray();

                    double maxAveragePlayerScore = playerStats.Max(g => g.Average(p => p.score));
                    ulong? bestPlayer = playerStats.FirstOrDefault(g => g.Average(p => p.score) == maxAveragePlayerScore)?.Key;

                    lock (mapsStats)
                    {
                        mapsStats.Add(new CMapStats()
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
                            MaxAveragePlayerScore = maxAveragePlayerScore
                        });
                    }
                }
            });

            return mapsStats.ToArray();
        }

        private const int SCORE_WEIGHT_KILL = 10;
        private const int SCORE_WEIGHT_DEATH = -1;
        private const int SCORE_WEIGHT_ASSIST = 2;
        private const int SCORE_WEIGHT_HEADSHOT = 5;
        private const int SCORE_WEIGHT_TEAMKILL = -40;
        private const int SCORE_WEIGHT_PLANT = 50;
        private const int SCORE_WEIGHT_DEFUSE = 50;

        private int calculatePlayerScore(PlayerStats playerStats)
        {
            Stats? kills = playerStats.Stats.FirstOrDefault(s => s.StatType == "Kill");
            Stats? deaths = playerStats.Stats.FirstOrDefault(s => s.StatType == "Death");
            Stats? assists = playerStats.Stats.FirstOrDefault(s => s.StatType == "Assist");
            Stats? headshots = playerStats.Stats.FirstOrDefault(s => s.StatType == "Headshot");
            Stats? teamkills = playerStats.Stats.FirstOrDefault(s => s.StatType == "TeamKill");
            Stats? plants = playerStats.Stats.FirstOrDefault(s => s.StatType == "BombPlanted");
            Stats? defuses = playerStats.Stats.FirstOrDefault(s => s.StatType == "BombDefused");

            return (kills?.Amount ?? 0) * SCORE_WEIGHT_KILL +
                (deaths?.Amount ?? 0) * SCORE_WEIGHT_DEATH +
                (assists?.Amount ?? 0) * SCORE_WEIGHT_ASSIST +
                (headshots?.Amount ?? 0) * SCORE_WEIGHT_HEADSHOT +
                (teamkills?.Amount ?? 0) * SCORE_WEIGHT_TEAMKILL +
                (plants?.Amount ?? 0) * SCORE_WEIGHT_PLANT +
                (defuses?.Amount ?? 0) * SCORE_WEIGHT_DEFUSE;
        }
    }
}
