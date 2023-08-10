using Microsoft.Extensions.Configuration;
using PavlovStatsReader.Models;

namespace PavlovStatsReader;

public class StatsCalculator
{
    public const int SCORE_WEIGHT_KILL = 10;
    public const int SCORE_WEIGHT_DEATH = -1;
    public const int SCORE_WEIGHT_ASSIST = 2;
    public const int SCORE_WEIGHT_HEADSHOT = 5;
    public const int SCORE_WEIGHT_TEAMKILL = -40;
    public const int SCORE_WEIGHT_PLANT = 50;
    public const int SCORE_WEIGHT_DEFUSE = 50;
    private readonly IConfiguration configuration;
    private readonly StatsContext statsContext;

    public StatsCalculator(StatsContext statsContext, IConfiguration configuration)
    {
        this.statsContext = statsContext;
        this.configuration = configuration;
    }

    public CBaseStats[] CalculateStats(string serverId)
    {
        List<CBaseStats> allStats = new()
        {
            this.CalculateServerStats(serverId),
        };

        allStats.AddRange(this.CalculateMapStats(serverId));
        allStats.AddRange(this.CalculateGunStats(serverId));
        allStats.AddRange(this.CalculateTeamStatistics(serverId));
        allStats.AddRange(this.CalculatePlayerStats(serverId));
        return allStats.ToArray();
    }


    public CServerStats CalculateServerStats(string serverId)
    {
        int uniqueMapCount = this.statsContext.EndOfMapStats.Where(m => m.ServerId == serverId).GroupBy(m => new
        {
            m.MapLabel,
            m.GameMode,
        }).Count();
        int uniquePlayerCount = this.statsContext.PlayerStats.Where(m => m.ServerId == serverId).GroupBy(m => m.UniqueId).Count();
        int totalMatches = this.statsContext.EndOfMapStats.Count(m => m.ServerId == serverId && m.PlayerCount >= 2 && m.Team0Score + m.Team1Score >= 10);
        int totalKills = this.statsContext.KillData.Count(d => d.ServerId == serverId);
        int totalHeadshots = this.statsContext.KillData.Count(d => d.ServerId == serverId && d.Headshot);
        int totalAssists = this.statsContext.EndOfMapStats.Where(m => m.ServerId == serverId).SelectMany(m => m.PlayerStats).SelectMany(m => m.Stats).Where(m => m.StatType == "Assist").Sum(m => m.Amount);
        int totalTeamkills = this.statsContext.KillData.Where(m => m.ServerId == serverId).Count(m => m.KilledTeamID == m.KillerTeamID);
        int totalBombPlants = this.statsContext.BombData.Count(m => m.ServerId == serverId && m.BombInteraction == "BombPlanted");
        int totalBombDefuses = this.statsContext.BombData.Count(m => m.ServerId == serverId && m.BombInteraction == "BombDefused");
        int totalBombExplosions = this.statsContext.BombData.Count(m => m.ServerId == serverId && m.BombInteraction == "BombExploded");
        int totalChickensKilled = this.statsContext.EndOfMapStats.Where(m => m.ServerId == serverId).SelectMany(m => m.PlayerStats).SelectMany(m => m.Stats).Where(m => m.StatType == "ChickenKilled").Sum(m => m.Amount);
        int totalPoints = this.statsContext.EndOfMapStats.Where(m => m.ServerId == serverId).SelectMany(m => m.PlayerStats).SelectMany(m => m.Stats).Where(s => s.StatType == "Experience").Sum(m => m.Amount);
        int totalRoundsPlayed = this.statsContext.EndOfMapStats.Where(m => m.ServerId == serverId).Sum(m => m.Team0Score + m.Team1Score);

        var mostKillsByGun = this.statsContext.KillData.Where(k => k.ServerId == serverId).GroupBy(k => k.KilledBy).Select(k => new
        {
            Gun = k.Key,
            Count = k.Count(),
        }).OrderByDescending(k => k.Count).FirstOrDefault();

        return new CServerStats
        {
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

    public CTeamStats[] CalculateTeamStatistics(string serverId)
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

            var bestPlayer = teamPlayerStats.AsEnumerable().GroupBy(t => t.UniqueId).Select(g => new
            {
                Player = g.Key,
                AvgScore = g.Average(this.calculatePlayerScore),
            }).MaxBy(g => g.AvgScore);
            var bestGun = this.statsContext.KillData.Where(m => m.ServerId == serverId && m.KillerTeamID == i).GroupBy(k => k.KilledBy).Select(g => new
            {
                Gun = g.Key,
                Count = g.Count(),
            }).OrderByDescending(g => g.Count).FirstOrDefault();

            teamStats.Add(new CTeamStats
            {
                TeamId = i,
                Name = i == 0 ? "Blue Team" : "Red Team",
                TotalKills = teamPlayerStatsStats.Where(p => p.StatType == "Kill").Sum(p => p.Amount),
                TotalAssists = teamPlayerStatsStats.Where(p => p.StatType == "Assist").Sum(p => p.Amount),
                TotalDeaths = teamPlayerStatsStats.Where(p => p.StatType == "Death").Sum(p => p.Amount),
                TotalHeadshots = teamPlayerStatsStats.Where(p => p.StatType == "Headshot").Sum(p => p.Amount),
                TotalTeamkills = statsContext.KillData.Where(p => p.ServerId == serverId && p.KilledTeamID == p.KillerTeamID).Count(p => p.KillerTeamID == i),
                TotalVictories = winCount,
                BestPlayer = bestPlayer?.Player,
                BestPlayerAverageScore = bestPlayer?.AvgScore ?? 0d,
                BestGun = bestGun?.Gun,
                BestGunKillCount = bestGun?.Count ?? 0,
            });
        }

        return teamStats.ToArray();
    }

    public CGunStats[] CalculateGunStats(string serverId)
    {
        List<CGunStats> gunsStats = new();

        string[] guns = this.statsContext.KillData.Where(m => m.ServerId == serverId).GroupBy(m => m.KilledBy).Select(m => m.Key).ToArray();
        guns.AsParallel().ForAll(gunName =>
        {
            using (StatsContext statsContext = new(this.configuration))
            {
                IQueryable<KillData> gunStats = statsContext.KillData.Where(m => m.ServerId == serverId && m.KilledBy == gunName);

                int kills = gunStats.Count();
                int headshots = gunStats.Count(g => g.Headshot);
                int teamKills = gunStats.Count(g => g.KillerTeamID == g.KilledTeamID);

                var mostKills = gunStats.GroupBy(g => g.Killer).Select(g => new
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

    public CPlayerStats[] CalculatePlayerStats(string serverId)
    {
        Setting? serverStatMode = this.statsContext.Settings.FirstOrDefault(s => s.Name == "Stat Type" && s.ServerId == serverId);
        List<CPlayerStats> playerStats = new();

        if (serverStatMode?.Value == "SND")
        {
            PlayerStats[] pstats = this.statsContext.EndOfMapStats.Where(m => m.ServerId == serverId && m.PlayerCount >= 2 && m.Team0Score + m.Team1Score >= 10).SelectMany(m => m.PlayerStats).ToArray();

            pstats.GroupBy(p => p.UniqueId).AsParallel().ForAll(pgrp =>
            {
                using StatsContext statsContext = new(this.configuration);
                IGrouping<ulong, PlayerStats> playerGrouping = statsContext.EndOfMapStats.Where(m => m.ServerId == serverId && m.PlayerCount >= 2 && m.Team0Score + m.Team1Score >= 10).SelectMany(m => m.PlayerStats).Where(p => p.UniqueId == pgrp.Key).AsEnumerable().GroupBy(p => p.UniqueId).First();
                int kills = playerGrouping.Sum(p => p.Stats.FirstOrDefault(s => s.StatType == "Kill")?.Amount ?? 0);
                int deaths = playerGrouping.Sum(p => p.Stats.FirstOrDefault(s => s.StatType == "Death")?.Amount ?? 0);
                int assists = playerGrouping.Sum(p => p.Stats.FirstOrDefault(s => s.StatType == "Assist")?.Amount ?? 0);
                int teamKills = statsContext.KillData.Count(d => d.ServerId == serverId && d.Killer == playerGrouping.Key && d.KillerTeamID == d.KilledTeamID);
                int headshots = playerGrouping.Sum(p => p.Stats.FirstOrDefault(s => s.StatType == "Headshot")?.Amount ?? 0);
                int bombsPlanted = playerGrouping.Sum(p => p.Stats.FirstOrDefault(s => s.StatType == "BombPlanted")?.Amount ?? 0);
                int bombsDefused = playerGrouping.Sum(p => p.Stats.FirstOrDefault(s => s.StatType == "BombDefused")?.Amount ?? 0);
                int totalScore = playerGrouping.Sum(this.calculatePlayerScore);
                double averageScore = playerGrouping.Average(this.calculatePlayerScore);
                int roundsPlayed = playerGrouping.Count();
                int chickensKilled = playerGrouping.Sum(p => p.Stats.FirstOrDefault(s => s.StatType == "ChickenKilled")?.Amount ?? 0);

                int suicides = statsContext.KillData.Count(k => k.Killer == playerGrouping.Key && (k.KilledBy == "None" || k.KilledBy == "killvolume"));

                var playerKillsGroup = statsContext.KillData.Where(d => d.Killer == playerGrouping.Key).GroupBy(k => k.KilledBy).Select(g => new
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
                    playerStats.Add(new CPlayerStats
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
                    });
                }
            });
        }
        else
        {
            ulong[] players = this.statsContext.KillData.Where(k => k.ServerId == serverId).Select(k => k.Killer).Distinct().ToArray().Union(this.statsContext.KillData.Where(k => k.ServerId == serverId).Select(k => k.Killed).Distinct().ToArray()).Distinct().ToArray();

            //int current = 0;
            foreach (ulong player in players)
            {
                //Console.WriteLine($"Generating stats for player {player} ({++current} / {players.Length})");

                int kills = this.statsContext.KillData.Count(k => k.ServerId == serverId && k.Killer == player && k.Killed != player);
                int deaths = this.statsContext.KillData.Count(k => k.ServerId == serverId && k.Killed == player);
                int headshots = this.statsContext.KillData.Count(k => k.ServerId == serverId && k.Killer == player && k.Headshot && k.Killed != player);
                int suicides = this.statsContext.KillData.Count(k => k.ServerId == serverId && k.Killer == player && k.Killed == player);
                var playerKillsGroup = this.statsContext.KillData.Where(k => k.ServerId == serverId && k.Killer == player).GroupBy(k => k.KilledBy).Select(g => new { Gun = g.Key, Count = g.Count() }).OrderByDescending(g => g.Count).FirstOrDefault();
                string? mostKillsWith = playerKillsGroup?.Gun;
                int mostKillsWithAmount = playerKillsGroup?.Count ?? 0;

                playerStats.Add(new CPlayerStats
                {
                    UniqueId = player,
                    Kills = kills,
                    Deaths = deaths,
                    Headshots = headshots,
                    Suicides = suicides,
                    MostKillsWithGun = mostKillsWith,
                    MostKillsWithGunAmount = mostKillsWithAmount,
                });
            }
        }

        return playerStats.ToArray();
    }

    public CMapStats[] CalculateMapStats(string serverId)
    {
        EndOfMapStats[] eoms = this.statsContext.EndOfMapStats.Where(m => m.ServerId == serverId).ToArray();

        List<CMapStats> mapsStats = new();

        eoms.Where(m => m.PlayerCount >= 2 && m.Team0Score + m.Team1Score >= 10).GroupBy(m => (mapLabel: m.MapLabel, gameMode: m.GameMode)).AsParallel().ForAll(mgrp =>
        {
            using StatsContext statsContext = new(this.configuration);
            IGrouping<(string mapLabel, string gameMode), EndOfMapStats> mapGrouping = statsContext.EndOfMapStats.Where(m => m.ServerId == serverId && m.PlayerCount >= 2 && m.Team0Score + m.Team1Score >= 10 && m.MapLabel == mgrp.Key.mapLabel && m.GameMode.ToLower() == mgrp.Key.gameMode.ToLower()).ToArray().GroupBy(m => (mapLabel: m.MapLabel, gameMode: m.GameMode)).First();
            int[] totalScore = mapGrouping.Select(e => e.Team0Score + e.Team1Score).ToArray();
            IGrouping<ulong, (PlayerStats player, int score)>[] playerStats = mapGrouping.SelectMany(e => e.PlayerStats).Select(e => (player: e, score: this.calculatePlayerScore(e))).GroupBy(p => p.player.UniqueId).ToArray();

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
                });
            }
        });

        return mapsStats.ToArray();
    }

    private int calculatePlayerScore(PlayerStats playerStats)
    {
        return playerStats.Stats.FirstOrDefault(s => s.StatType == "Experience")?.Amount ?? 0;
    }
}
