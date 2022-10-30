using PavlovStatsReader;
using PavlovStatsReader.Models;
using PterodactylPavlovServerController.Models;
using PterodactylPavlovServerDomain.Models;
using Steam.Models.SteamCommunity;
using Steam.Models.Utilities;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace PterodactylPavlovServerController.Services
{
    public class PavlovStatisticsService
    {
        private readonly IConfiguration configuration;
        private readonly StatsContext statsContext;
        private readonly PterodactylService pterodactylService;
        private readonly SteamService steamService;
        private readonly StatsCalculator statsCalculator;
        private readonly SteamWorkshopService steamWorkshopService;

        public PavlovStatisticsService(IConfiguration configuration, StatsContext statsContext, PterodactylService pterodactylService, SteamService steamService, StatsCalculator statsCalculator, SteamWorkshopService steamWorkshopService)
        {
            this.configuration = configuration;
            this.statsContext = statsContext;
            this.pterodactylService = pterodactylService;
            this.steamService = steamService;
            this.statsCalculator = statsCalculator;
            this.steamWorkshopService = steamWorkshopService;
            statsContext.Database.EnsureCreated();
        }

        public void RunStatsReader()
        {
            new Thread(new ThreadStart(() => StatsReader())).Start();
        }

        private static readonly Regex pavlovLineHeaderRegex = new(@"^\[\d{4}\.\d{2}\.\d{2}-\d{2}\.\d{2}\.\d{2}:\d+\]\[\d+\]", RegexOptions.Compiled);
        private static readonly Regex fileNameDateTimeRegex = new(@"(?<date>\d{4}\.\d{2}\.\d{2}-\d{2}\.\d{2}\.\d{2})", RegexOptions.Compiled);

        private PterodactylServerModel[] readStatsToDb()
        {
            PterodactylServerModel[] pterodactylServerModels = pterodactylService.GetServers();
            foreach (PterodactylServerModel server in pterodactylServerModels)
            {
                Setting? lastStat = statsContext.Settings.FirstOrDefault(s => s.Name == "Last Read Statistic" && s.ServerId == server.ServerId);

                DateTime lastReadStatistic = lastStat == null ? DateTime.MinValue : DateTimeOffset.FromUnixTimeSeconds(long.Parse(lastStat.Value)).LocalDateTime;

                PterodactylFile[] files = pterodactylService.GetFileList(server.ServerId, "/Pavlov/Saved/Logs");
                List<string> filesToParse = new List<string>();

                foreach (PterodactylFile file in files)
                {
                    Match match = fileNameDateTimeRegex.Match(file.Name);

                    if (match.Success)
                    {
                        DateTime fileDateTime = DateTime.ParseExact(match.Groups["date"].Value, "yyyy.MM.dd-HH.mm.ss", null);
                        if (fileDateTime > lastReadStatistic)
                        {
                            filesToParse.Add(file.Name);
                        }
                    }
                    else
                    {
                        if (file.Name == "Pavlov.log")
                        {
                            filesToParse.Add(file.Name);
                        }
                    }
                }

                DateTime latestStatistic = lastReadStatistic;

                foreach (string fileName in filesToParse)
                {
                    StatsReader statsReader = new StatsReader(pterodactylService.ReadFile(server.ServerId, $"/Pavlov/Saved/Logs/{fileName}"));
                    bool startReading = false;

                    foreach (BaseStatistic baseStatistic in statsReader.ParsedStats)
                    {
                        if (!startReading && baseStatistic.LogEntryDate > lastReadStatistic)
                        {
                            startReading = true;
                        }

                        if (!startReading)
                        {
                            continue;
                        }

                        if (baseStatistic.LogEntryDate > latestStatistic)
                        {
                            latestStatistic = baseStatistic.LogEntryDate;
                        }

                        baseStatistic.ServerId = server.ServerId;

                        switch (baseStatistic)
                        {
                            case BombData bombData:
                                this.statsContext.BombData.Add(bombData);
                                break;
                            case EndOfMapStats endOfMapStats:
                                foreach (PlayerStats playerStats in endOfMapStats.PlayerStats)
                                {
                                    playerStats.ServerId = server.ServerId;
                                    foreach (Stats stats in playerStats.Stats)
                                    {
                                        stats.ServerId = server.ServerId;
                                    }
                                }
                                this.statsContext.EndOfMapStats.Add(endOfMapStats);
                                break;
                            case KillData killData:
                                this.statsContext.KillData.Add(killData);
                                break;
                            case RoundEnd roundEnd:
                                this.statsContext.RoundEnds.Add(roundEnd);
                                break;
                            case RoundState roundState:
                                this.statsContext.RoundStates.Add(roundState);
                                break;
                            case SwitchTeam switchTeam:
                                this.statsContext.SwitchTeams.Add(switchTeam);
                                break;
                            default:
                                Console.WriteLine($"Unhandled statistic type {baseStatistic.GetType().Name}");
                                break;
                        }
                    }
                }

                if (lastStat == null)
                {
                    this.statsContext.Settings.Add(new Setting()
                    {
                        Name = "Last Read Statistic",
                        ServerId = server.ServerId,
                        Value = latestStatistic.ToUnixTimeStamp().ToString()
                    });
                }
                else
                {
                    lastStat.Value = latestStatistic.ToUnixTimeStamp().ToString();
                }

            }
            this.statsContext.SaveChanges();
            return pterodactylServerModels;
        }

        private void generateStatsFiles(PterodactylServerModel[] pterodactylServerModels)
        {
            if (!Directory.Exists("stats"))
            {
                Directory.CreateDirectory("stats");
            }

            foreach (PterodactylServerModel server in pterodactylServerModels)
            {
                StringBuilder serverStatsBuilder = new StringBuilder();

                serverStatsBuilder.AppendLine($@"<!doctype html>
<html lang=""en"">
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
    <title>{server.Name} server statistics</title>
    <link href=""https://cdn.jsdelivr.net/npm/bootstrap@5.2.2/dist/css/bootstrap.min.css"" rel=""stylesheet"" integrity=""sha384-Zenh87qX5JnK2Jl0vWa8Ck2rdkQ2Bzep5IDxbcnCeuOxjzrPF/et3URy9Bv1WTRi"" crossorigin=""anonymous"">
</head>
<body>
    <div class=""container-lg"">
        <h1>{server.Name} server statistics</h1>

        <h2>Table of contents</h2>
        <ol>
            <li><a href=""#serverstats"">Server statistics</a></li>
            <li><a href=""#mapstats"">Map statistics</a></li>
            <li><a href=""#gunstats"">Gun statistics</a></li>
            <li><a href=""#bombstats"">Bomb statistics</a></li>
            <li><a href=""#teamstats"">Team statistics</a></li>
            <li><a href=""#playerstats"">Player statistics</a></li>
        </ol>");

                CBaseStats[] calculatedStats = statsCalculator.CalculateStats(server.ServerId);

                // Server stats
                // TODO

                // Maps stats
                serverStatsBuilder.AppendLine($@"<h2 id=""mapstats"">Map statistics</h2>
        <div>");

                Dictionary<CMapStats, string> mapsStatsContent = new();
                CMapStats[] mapsStats = calculatedStats.OfType<CMapStats>().ToArray();
                mapsStats.AsParallel().ForAll(mapStats =>
                {
                    MapDetailModel mapDetail;
                    try
                    {
                        mapDetail = steamWorkshopService.GetMapDetail(long.Parse(mapStats.MapId.Substring(3)));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Failed to retrieve map detail for map {mapStats.MapId}: {e.Message} {e}");
                        return;
                    }

                    string? bestPlayerUsername = mapStats.BestPlayer == null ? null : steamService.GetUsername(mapStats.BestPlayer.Value);

                    lock (mapsStatsContent)
                    {
                        mapsStatsContent.Add(mapStats, createStatsCard($"{mapStats.MapId}-{mapStats.GameMode}",
                            mapDetail.URL,
                            true,
                            $"{mapDetail.ImageURL}/?imw=256&imh=256&ima=fit&impolicy=Letterbox&imcolor=%23000000&letterbox=true",

                            $"{mapDetail.Name} ({mapStats.GameMode})", new Dictionary<string, string>()
                            {
                                {"Played",  $"{mapStats.PlayCount} time{(mapStats.PlayCount != 1 ? "s" : "")}"},
                                { "Wins",  $"Blue {mapStats.Team0Wins}, Red {mapStats.Team1Wins}" },
                                { "Rounds", $"{mapStats.AverageRounds} avg, {mapStats.MaxRounds} max, {mapStats.MinRounds} min" },
                                { "Best player", $"{(mapStats.BestPlayer == null ? "Nobody" : $@"<a href=""#player-{mapStats.BestPlayer}"">{bestPlayerUsername}</a> ({mapStats.MaxAveragePlayerScore} average score)")}" }
                            }));

                        //            mapsStatsContent.Add(mapStats, @$"<div class=""card"" style=""max-width: 256px; display: inline-block;"" id=""{mapStats.MapId}-{mapStats.GameMode}"">
                        //    <a href=""{mapDetail.URL}"">
                        //        <img class=""card-img-top"" src=""{mapDetail.ImageURL}/?imw=256&imh=256&ima=fit&impolicy=Letterbox&imcolor=%23000000&letterbox=true"" width=""256"" height=""256"" />
                        //    </a>
                        //    <div class=""card-body"">
                        //        <h5 class=""card-title""><a href=""{mapDetail.URL}"">{mapDetail.Name}</a> ({mapStats.GameMode})</h5>

                        //        <p class=""card-text"">
                        //            <b>Played:</b> {mapStats.PlayCount} time{(mapStats.PlayCount != 1 ? "s" : "")}<br />
                        //            <b>Wins:</b> Blue {mapStats.Team0Wins}, Red {mapStats.Team1Wins}<br />
                        //            <b>Rounds:</b> {mapStats.AverageRounds} avg, {mapStats.MaxRounds} max, {mapStats.MinRounds} min<br />
                        //            <b>Best player:</b> {(mapStats.BestPlayer == null ? "Nobody" : $@"<a href=""#player-{mapStats.BestPlayer}"">{bestPlayerUsername}</a> ({mapStats.MaxAveragePlayerScore} average score)")}<br />
                        //        </p>
                        //    </div>
                        //</div>");
                    }
                });
                serverStatsBuilder.AppendLine(String.Join(Environment.NewLine, mapsStatsContent.OrderByDescending(kvp => kvp.Key.PlayCount).Select(kvp => kvp.Value)));
                serverStatsBuilder.AppendLine("</div>");

                // Gun stats
                // TODO

                // Bomb stats
                // TODO

                // Team stats
                // TODO

                // Player stats
                CPlayerStats[] playersStats = calculatedStats.OfType<CPlayerStats>().ToArray();
                if (playersStats.Any())
                {
                    serverStatsBuilder.AppendLine(@"<h3>All players</h3>
        <div id=""playerstats"">");
                    int totalKills = playersStats.Sum(p => p.Kills);
                    int totalDeaths = playersStats.Sum(p => p.Deaths);
                    int totalAssists = playersStats.Sum(p => p.Assists);
                    int totalScore = playersStats.Sum(p => p.TotalScore);

                    Dictionary<CPlayerStats, string> playerStatsContent = new();

                    playersStats.AsParallel().ForAll(playerStats =>
                    {
                        using (StatsContext statsContext = new StatsContext(configuration))
                        {
                            PlayerSummaryModel playerSummary;
                            try
                            {
                                playerSummary = steamService.GetPlayerSummary(playerStats.UniqueId);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine($"Could not get summary of player {playerStats.UniqueId}: {e.Message} {e}");
                                return;
                            }

                            IReadOnlyCollection<PlayerBansModel> playerBans;
                            try
                            {
                                playerBans = steamService.GetBans(playerStats.UniqueId);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine($"Could not get bans of player {playerStats.UniqueId}: {e.Message} {e}");
                                return;
                            }

                            string? mostKillsWith = null;
                            IGrouping<string, KillData>[] playerKillsGroups = statsContext.KillData.Where(d => d.Killer == playerStats.UniqueId).AsEnumerable().GroupBy(g => g.KilledBy).ToArray();
                            if (playerKillsGroups.Any())
                            {
                                int mostKills = playerKillsGroups.Max(g => g.Count());
                                mostKillsWith = playerKillsGroups.FirstOrDefault(g => g.Count() == mostKills)?.Key;
                            }

                            int vacCount = 0;
                            foreach (PlayerBansModel playerBan in playerBans)
                            {
                                vacCount += (int)playerBan.NumberOfVACBans;
                            }

                            lock (playerStatsContent)
                            {
                                playerStatsContent.Add(playerStats, createStatsCard($"player-{playerStats.UniqueId}",
                                    playerSummary.ProfileUrl,
                                    true,
                                    playerSummary.AvatarFullUrl,
                                    playerSummary.Nickname,
                                    new Dictionary<string, string>()
                                    {
                                    {"K/D ratio", $"{Math.Round((double)playerStats.Kills / (playerStats.Deaths == 0 ? 1 : playerStats.Deaths), 1):0.0}"},
                                    {"Kills", $"{playerStats.Kills} ({Math.Round(playerStats.Kills * 100d / totalKills, 1)}%)"},
                                    {"Deaths", $"{playerStats.Deaths} ({Math.Round(playerStats.Deaths * 100d / totalDeaths, 1)}%)"},
                                    {"Assists", $"{playerStats.Assists} ({Math.Round(playerStats.Assists * 100d / totalAssists, 1)}%"},
                                    {"Team kills", $"{playerStats.TeamKills} ({Math.Round(playerStats.TeamKills * 100d / playerStats.Kills, 1)}% of own kills)"},
                                    {"HS kills", $"{playerStats.Headshots} ({Math.Round(playerStats.Headshots * 100d / playerStats.Kills, 1)}% of own kills)"},
                                    {"Avg. points", $"{Math.Round(playerStats.AverageScore, 0)}"},
                                    {"Total points", $"{playerStats.TotalScore} ({Math.Round(playerStats.TotalScore * 100d / totalScore, 1)}%)"},
                                    {"Bombs", $"{playerStats.BombsPlanted} planted, {playerStats.BombsDefused} defused"},
                                    {"Best gun", $"{(mostKillsWith == null ? "none" : mostKillsWith)}"},
                                    {"VAC", $"{(vacCount > 0 ? $@"<span class=""text-danger"">Yes, {vacCount}" : $@"<span class=""text-success"">No")}</span>"}
                                    }));
                            }
                        }
                    });

                    serverStatsBuilder.AppendLine($@"<h3>Honorable mentions</h3>
        <div>");

                    string? highestKDR = getPlayerCardWith(playerStatsContent.Keys.ToList(), p => p.Kills > 10 ? (double)p.Kills / (p.Deaths == 0 ? 1 : p.Deaths) : 0, p => p.Kills, 1, false, "Highest K/D ratio", "K/D ratio");
                    if (highestKDR != null)
                    {
                        serverStatsBuilder.AppendLine(highestKDR);
                    }

                    string? mostKills = getPlayerCardWith(playerStatsContent.Keys.ToList(), p => p.Kills, p => p.TotalScore, 0, false, "Most kills", "Kills");
                    if (mostKills != null)
                    {
                        serverStatsBuilder.AppendLine(mostKills);
                    }

                    string? mostAssists = getPlayerCardWith(playerStatsContent.Keys.ToList(), p => p.Assists, p => p.TotalScore, 0, false, "Most assists", "Assists");
                    if (mostAssists != null)
                    {
                        serverStatsBuilder.AppendLine(mostAssists);
                    }

                    string? highestTotalScore = getPlayerCardWith(playerStatsContent.Keys.ToList(), p => p.TotalScore, p => p.Kills, 0, false, "Highest total score", "Score");
                    if (highestTotalScore != null)
                    {
                        serverStatsBuilder.AppendLine(highestTotalScore);
                    }

                    string? highestAverageScore = getPlayerCardWith(playerStatsContent.Keys.ToList(), p => p.AverageScore, p => p.TotalScore, 0, false, "Highest average score", "Score");
                    if (highestAverageScore != null)
                    {
                        serverStatsBuilder.AppendLine(highestAverageScore);
                    }

                    string? mostPlants = getPlayerCardWith(playerStatsContent.Keys.ToList(), p => p.BombsPlanted, p => p.TotalScore, 0, false, "Most bomb plants", "Plants");
                    if (mostPlants != null)
                    {
                        serverStatsBuilder.AppendLine(mostPlants);
                    }

                    string? mostDefuses = getPlayerCardWith(playerStatsContent.Keys.ToList(), p => p.BombsDefused, p => p.TotalScore, 0, false, "Most bomb defuses", "Defuses");
                    if (mostDefuses != null)
                    {
                        serverStatsBuilder.AppendLine(mostDefuses);
                    }

                    string? mostHeadshots = getPlayerCardWith(playerStatsContent.Keys.ToList(), p => p.Headshots, p => p.Kills, 0, false, "Most headshot kills", "HS kills");
                    if (mostHeadshots != null)
                    {
                        serverStatsBuilder.AppendLine(mostHeadshots);
                    }

                    string? highestHSKR = getPlayerCardWith(playerStatsContent.Keys.ToList(), p => p.Kills > 10 ? (double)p.Headshots / p.Kills : 0, p => p.Kills, 1, false, "Highest headshot kill to kill ratio", "HSKR");
                    if (highestHSKR != null)
                    {
                        serverStatsBuilder.AppendLine(highestHSKR);
                    }

                    serverStatsBuilder.AppendLine(@"</div><br/>
        <h3>Dishonorable mentions</h3>
        <div>");

                    string? mostDeaths = getPlayerCardWith(playerStatsContent.Keys.ToList(), p => p.Deaths, p => p.TotalScore, 0, false, "Most deaths", "Deaths");
                    if (mostDeaths != null)
                    {
                        serverStatsBuilder.AppendLine(mostDeaths);
                    }

                    string? mostTeamKills = getPlayerCardWith(playerStatsContent.Keys.ToList(), p => p.TeamKills, p => p.Kills, 0, false, "Most teamkills", "Teamkills");
                    if (mostTeamKills != null)
                    {
                        serverStatsBuilder.AppendLine(mostTeamKills);
                    }

                    string? lowestHSKR = getPlayerCardWith(playerStatsContent.Keys.ToList(), p => p.Kills > 10 ? (double)p.Headshots / p.Kills : double.MaxValue, p => p.Kills, 1, true, "Lowest headshot kill to kill ratio", "HSKR");
                    if (lowestHSKR != null)
                    {
                        serverStatsBuilder.AppendLine(lowestHSKR);
                    }

                    serverStatsBuilder.AppendLine(@"</div><br/>
        <h3>All players</h3>
        <div>");

                    serverStatsBuilder.AppendLine(String.Join(Environment.NewLine, playerStatsContent.OrderByDescending(kvp => kvp.Key.TotalScore).Select(kvp => kvp.Value)));

                    serverStatsBuilder.AppendLine("</div>");
                }

                serverStatsBuilder.AppendLine(@"
    </div>
    <script src=""https://cdn.jsdelivr.net/npm/bootstrap@5.2.2/dist/js/bootstrap.bundle.min.js"" integrity=""sha384-OERcA2EqjJCMA+/3y+gxIOqMEjwtxJY7qPCqsdltbNJuaOe923+mo//f6V8Qbsw3"" crossorigin=""anonymous""></script>
</body>
</html>");
                File.WriteAllText($"stats/{server.ServerId}.html", serverStatsBuilder.ToString());
            }

            Console.WriteLine("Stats written");
        }

        private string? getPlayerCardWith(List<CPlayerStats> playerStats, Func<CPlayerStats, double> selector, Func<CPlayerStats, double> secondSort, int round, bool invert, string title, string statName)
        {
            double highestValue = playerStats.Max(p => selector(p) * (invert ? -1 : 1));
            CPlayerStats? highestValuePlayer = playerStats.OrderBy(secondSort).FirstOrDefault(p => selector(p) * (invert ? -1 : 1) == highestValue);
            if (highestValuePlayer == null)
            {
                return null;
            }

            PlayerSummaryModel? playerSummary = null;
            try
            {
                playerSummary = steamService.GetPlayerSummary(highestValuePlayer.UniqueId);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not get summary of player {highestValuePlayer.UniqueId}: {e.Message} {e}");
                return null;
            }

            return createStatsCard(null,
                $"#player-{highestValuePlayer.UniqueId}",
                true,
                playerSummary.AvatarFullUrl,
                title,
                new()
                {
                    { "Player", $"<a href=\"#player-{highestValuePlayer.UniqueId}\">{playerSummary.Nickname}</a>" },
                    { statName, Math.Round(highestValue == -0 ? 0 : highestValue, round).ToString() }
                });

        }

        private string createStatsCard(string? id, string? link, bool openInBlank, string imageURL, string title, Dictionary<string, string> values)
        {
            return @$"<div class=""card"" style=""max-width: 256px; display: inline-block;"" {(id == null ? "" : $@"id=""{id}""")}>
                {(link == null ? "" : $@"<a href=""{link}""{(openInBlank ? " target=\"_blank\"" : "")}>")}
                    <img class=""card-img-top"" src=""{imageURL}"" width=""256"" height=""256"" />
                {(link == null ? "" : "</a>")}
                <div class=""card-body"">
                    <h5 class=""card-title"">{(link == null ? "" : $@"<a href=""{link}""{(openInBlank ? " target=\"blank\"" : "")}>")}{title}{(link == null ? "" : $@"</a>")}</h5>

                    <p class=""card-text"">
                        {String.Join(Environment.NewLine, values.Select(kvp => $"<b>{kvp.Key}:</b> {kvp.Value}<br />"))}
                    </p>
                </div>
            </div>";
        }

        public void StatsReader()
        {
            DateTime lastGenerated = DateTime.MinValue;

            while (running)
            {
                if (lastGenerated < DateTime.Now.AddDays(-8))
                {
                    Console.WriteLine("Generating stats...");
                    Stopwatch sw = Stopwatch.StartNew();
                    generateStatsFiles(readStatsToDb());
                    sw.Stop();
                    Console.WriteLine($"Stats completed in {sw.ElapsedMilliseconds}ms");
                    lastGenerated = DateTime.Now;
                }

                Thread.Sleep(1000);
            }
        }

        private bool running = true;
        internal void StopStatsReader() => running = false;

        //private static void processPavlovLogLine(string line)
        //{
        //    line = pavlovLineHeaderRegex.Replace(line, "");
        //    Console.WriteLine(line);
        //}
    }
}
