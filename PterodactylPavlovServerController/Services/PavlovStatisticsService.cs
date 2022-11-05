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
        private const int CARD_WIDTH = 300;
        private const int CARDS_PER_ROW = 4;

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

        private static readonly Dictionary<string, string> gunMap = new()
        {
            { "Revolver", "Revolver" },
            { "AutoShotgun", "SPAS-12" },
            { "LMGA", "M249" },
            { "cet9", "TEC-9" },
            { "Shotgun", "M590" },
            { "flash_ru", "Flashbang (Russian)" },
            { "MP5", "MP5-N" },
            { "ak12", "AK-12" },
            { "1911", "M1911A1" },
            { "de", "Deagle" },
            { "m16", "M16A2" },
            { "sawedoff", "Sawed Off" },
            { "Grenade", "Explosive grenade (US)" },
            { "uzi", "Mini-UZI" },
            { "AK47", "AK-47" },
            { "AR", "M4" },
            { "57", "Five-Seven" },
            { "killvolume", "Suicide by map trigger" },
            { "None", "Suicide" },
            { "DrumShotgun", "Saiga 12" },
            { "sndbomb", "Bomb (SND)" },
            { "grenade_ru", "Explosive grenade (RU)" },
            { "Knife", "Knife" },
            { "m9", "Beretta M9" },
            { "P90", "P90" },
            { "aug", "AUG A3" },
            { "flash", "Flashbang (US)" }
        };

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

                string serverName = (pterodactylService.ReadFile(server.ServerId, "/Pavlov/Saved/Config/LinuxServer/Game.ini").Split('\n').FirstOrDefault(l => l.StartsWith("ServerName=")) ?? $"ServerName={server.Name}").Replace("ServerName=", "");


                serverStatsBuilder.AppendLine($@"<!doctype html>
<html lang=""en"">
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
    <title>{serverName} server statistics</title>
    <link href=""https://cdn.jsdelivr.net/npm/bootstrap@5.2.2/dist/css/bootstrap.min.css"" rel=""stylesheet"" integrity=""sha384-Zenh87qX5JnK2Jl0vWa8Ck2rdkQ2Bzep5IDxbcnCeuOxjzrPF/et3URy9Bv1WTRi"" crossorigin=""anonymous"">
    <style>
        .row-alternating {{
            background: #f0f0f0;
        }}
        .row-alternating:nth-child(2n) {{
            background: #fcfcfc;
        }}
        a:link {{ text-decoration: none; }}
        a:visited {{ text-decoration: none; }}
        a:hover {{ text-decoration: none; }}
        a:active {{ text-decoration: none; }}
    </style>
</head>
<body>
    <div class=""container-xxl"">
        <h1>{serverName} server statistics</h1>

        <h2>Table of contents</h2>
        <ol>
            <li><a href=""#serverstats"">Server statistics</a></li>
            <li><a href=""#mapstats"">Map statistics</a></li>
            <li><a href=""#gunstats"">Gun statistics</a></li>
            <li><a href=""#teamstats"">Team statistics</a></li>
            <li><a href=""#playerstats"">Player statistics</a></li>
        </ol>");

                CBaseStats[] calculatedStats = statsCalculator.CalculateStats(server.ServerId);

                // Server stats
                serverStatsBuilder.AppendLine($@"<h2 id=""serverstats"">Server statistics</h2>
                <div class=""card-group d-flex flex-wrap"">");

                CServerStats serverStats = calculatedStats.OfType<CServerStats>().First();

                string[] serverCards = new string[] {
                    createStatsCard(null, null, false, "https://bloodisgood.net/wp-content/uploads/2022/10/image_2022-10-31_113333781.png", "Counts",
                        new()
                        {
                            { "Unique maps/modes", serverStats.TotalUniqueMaps.ToString() },
                            { "Unique players", serverStats.TotalUniquePlayers.ToString() },
                            { "Total matches", serverStats.TotalMatchesPlayed.ToString() }
                        }),
                    createStatsCard(null, null, false, "https://bloodisgood.net/wp-content/uploads/2022/10/image_2022-10-31_113751041.png", "Kill stats",
                        new()
                        {
                            { "Total kills", serverStats.TotalKills.ToString() },
                            { "Total headshots", $"{serverStats.TotalHeadshots} ({Math.Round(calculateSafePercent(serverStats.TotalHeadshots, serverStats.TotalKills), 1)}%)" },
                            { "Total assists", serverStats.TotalAssists.ToString() },
                            { "Total teamkills", serverStats.TotalTeamkills.ToString() },
                        }),

                    createStatsCard(null, null, false, "https://bloodisgood.net/wp-content/uploads/2022/10/image_2022-10-31_114051512.png", "Bomb stats",
                        new()
                        {
                            { "Total plants", serverStats.TotalBombPlants.ToString() },
                            { "Total defuses", $"{serverStats.TotalBombDefuses} ({Math.Round(calculateSafePercent(serverStats.TotalBombDefuses, serverStats.TotalBombPlants), 1)}%)" },
                            { "Total explosions", $"{serverStats.TotalBombExplosions} ({Math.Round(calculateSafePercent(serverStats.TotalBombExplosions, serverStats.TotalBombPlants), 1)}%)" }
                        })
                };

                serverStatsBuilder.AppendLine(splitInRows(serverCards));

                serverStatsBuilder.AppendLine("</div>");

                // Maps stats
                serverStatsBuilder.AppendLine($@"<h2 id=""mapstats"">Map statistics</h2>
        <div class=""card-group d-flex flex-wrap"">");

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
                        mapsStatsContent.Add(mapStats, createStatsCard($"map-{mapStats.MapId}-{mapStats.GameMode}",
                            mapDetail.URL,
                            true,
                            $"{mapDetail.ImageURL}/?imw={CARD_WIDTH}&imh={CARD_WIDTH}&ima=fit&impolicy=Letterbox&imcolor=%23000000&letterbox=true",

                            $"{mapDetail.Name} ({mapStats.GameMode})", new Dictionary<string, string>()
                            {
                                { "Played",  $"{mapStats.PlayCount} time{(mapStats.PlayCount != 1 ? "s" : "")}" },
                                { "Wins",  $"Blue {mapStats.Team0Wins}, Red {mapStats.Team1Wins}" },
                                { "Rounds", $"{mapStats.AverageRounds} avg, {mapStats.MaxRounds} max, {mapStats.MinRounds} min" },
                                { "Best player", $"{(mapStats.BestPlayer == null ? "Nobody" : $@"<a href=""#player-{mapStats.BestPlayer}"">{bestPlayerUsername}</a><br/>{mapStats.MaxAveragePlayerScore} avg score")}" }
                            }));
                    }
                });

                serverStatsBuilder.AppendLine(splitInRows(mapsStatsContent.OrderByDescending(kvp => kvp.Key.PlayCount).Select(kvp => kvp.Value).ToArray()));

                //serverStatsBuilder.AppendLine(String.Join(Environment.NewLine, mapsStatsContent.OrderByDescending(kvp => kvp.Key.PlayCount).Select(kvp => kvp.Value)));
                serverStatsBuilder.AppendLine("</div>");

                // Gun stats
                serverStatsBuilder.AppendLine($@"<h2 id=""gunstats"">Gun statistics</h2>
        <div class=""card-group d-flex flex-wrap"">");

                CGunStats[] gunsStats = calculatedStats.OfType<CGunStats>().ToArray();
                List<string> gunCards = new List<string>();

                foreach (CGunStats gunStats in gunsStats.OrderByDescending(g => g.Kills))
                {
                    string gunName = gunMap.ContainsKey(gunStats.Name) ? gunMap[gunStats.Name] : $"{gunStats.Name}(?)";
                    string? bestPlayerUsername = gunStats.BestPlayer == null ? null : steamService.GetUsername(gunStats.BestPlayer.Value);

                    gunCards.Add(createStatsCard($"gun-{gunStats.Name}", "http://wiki.pavlov-vr.com/index.php?title=Weapons", true, $"https://pavlov.bloodisgood.net/gunimages/{(gunMap.ContainsKey(gunStats.Name) ? gunStats.Name : "unknown")}.png", gunName,
                        new()
                        {
                            { "Kills", $"{gunStats.Kills} ({Math.Round(calculateSafePercent(gunStats.Kills, serverStats.TotalKills), 1)}%)" },
                            { "Headshots", $"{gunStats.Headshots} ({Math.Round(calculateSafePercent(gunStats.Headshots, gunStats.Kills))}%<a href=\"#asterix-own-kills\">*</a>)" },
                            { "Best player", $"{(gunStats.BestPlayer == null ? "Nobody" : $@"<a href=""#player-{gunStats.BestPlayer}"">{bestPlayerUsername}</a> ({gunStats.BestPlayerKills}&nbsp;kills)")}" }
                        }));
                }
                serverStatsBuilder.AppendLine(splitInRows(gunCards.ToArray()));

                serverStatsBuilder.AppendLine("</div>");


                // Team stats
                serverStatsBuilder.AppendLine($@"<h2 id=""teamstats"">Team statistics</h2>
        <div class=""card-group d-flex flex-wrap"">");

                CTeamStats[] teamsStats = calculatedStats.OfType<CTeamStats>().ToArray();
                List<string> teamCards = new List<string>();
                foreach (CTeamStats teamStats in teamsStats)
                {
                    string? bestPlayerUsername = teamStats.BestPlayer == null ? null : steamService.GetUsername(teamStats.BestPlayer.Value);

                    teamCards.Add(createStatsCard(null, null, false, teamStats.TeamId == 0 ? "https://bloodisgood.net/wp-content/uploads/2022/10/blueteam.png" : "https://bloodisgood.net/wp-content/uploads/2022/10/redteam.png", teamStats.Name,
                        new()
                        {
                            { "Kills", $"{teamStats.TotalKills} ({Math.Round(calculateSafePercent(teamStats.TotalKills, serverStats.TotalKills), 1)}%)" },
                            { "HS kills", $"{teamStats.TotalHeadshots} ({Math.Round(calculateSafePercent(teamStats.TotalHeadshots, teamStats.TotalKills), 1)}%<a href=\"#asterix-own-kills\">*</a>)" },
                            { "Assists", $"{teamStats.TotalAssists} ({Math.Round(calculateSafePercent(teamStats.TotalAssists, serverStats.TotalAssists), 1)}%)" },
                            { "Teamkills", $"{teamStats.TotalTeamkills} ({Math.Round(calculateSafePercent(teamStats.TotalTeamkills, serverStats.TotalTeamkills), 1)}%)" },
                            { "Victories", teamStats.TotalVictories.ToString() },
                            { "Best player", $"{(teamStats.BestPlayer == null ? "Nobody" : $@"<a href=""#player-{teamStats.BestPlayer}"">{bestPlayerUsername}</a><br />{teamStats.BestPlayerAvgScore} avg score")}" },
                            { "Best gun", $"{(teamStats.BestGun == null ? "None" : $@"<a href=""#gun-{teamStats.BestGun}"">{(gunMap.ContainsKey(teamStats.BestGun) ? gunMap[teamStats.BestGun] : teamStats.BestGun)}</a> ({teamStats.BestGunKillCount} kills)")}" }
                        }));
                }

                serverStatsBuilder.AppendLine(splitInRows(teamCards.ToArray()));

                serverStatsBuilder.AppendLine("</div>");

                // Player stats
                CPlayerStats[] playersStats = calculatedStats.OfType<CPlayerStats>().ToArray();
                serverStatsBuilder.AppendLine(@"<h3 id=""playerstats"">Player statistics</h3>");
                if (playersStats.Any())
                {
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

                            int vacCount = 0;
                            foreach (PlayerBansModel playerBan in playerBans)
                            {
                                vacCount += (int)playerBan.NumberOfVACBans;
                            }

                            MapDetailModel? bestMap = null;
                            if (playerStats.BestMap != null)
                            {
                                bestMap = steamWorkshopService.GetMapDetail(long.Parse(playerStats.BestMap[3..]));
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
                                        {"K/D ratio", $"{Math.Round((double)playerStats.Kills / playerStats.Deaths, 1):0.0}"},
                                        {"Kills", $"{playerStats.Kills} ({Math.Round(calculateSafePercent(playerStats.Kills, totalKills), 1)}%)"},
                                        {"Deaths", $"{playerStats.Deaths} ({Math.Round(calculateSafePercent(playerStats.Deaths, totalDeaths), 1)}%)"},
                                        {"Assists", $"{playerStats.Assists} ({Math.Round(calculateSafePercent(playerStats.Assists, totalAssists), 1)}%)"},
                                        {"Team kills", $"{playerStats.TeamKills} ({Math.Round(calculateSafePercent(playerStats.TeamKills, playerStats.Kills), 1)}%<a href=\"#asterix-own-kills\">*</a>)"},
                                        {"HS kills", $"{playerStats.Headshots} ({Math.Round(calculateSafePercent(playerStats.Headshots, playerStats.Kills), 1)}%<a href=\"#asterix-own-kills\">*</a>)"},
                                        {"Suicides", playerStats.Suicides.ToString() },
                                        {"Avg. points", $"{Math.Round(playerStats.AverageScore, 0)}"},
                                        {"Total points", $"{playerStats.TotalScore} ({Math.Round(calculateSafePercent(playerStats.TotalScore, totalScore), 1)}%)"},
                                        {"Bombs", $"{playerStats.BombsPlanted} planted, {playerStats.BombsDefused} defused"},
                                        {"Best gun", $"{(playerStats.MostKillsWithGun == null ? "None" : $@"<a href=""#gun-{playerStats.MostKillsWithGun}"">{(gunMap.ContainsKey(playerStats.MostKillsWithGun) ? gunMap[playerStats.MostKillsWithGun] : playerStats.MostKillsWithGun)}</a><br />{playerStats.MostKillsWithGunAmount} kills ({Math.Round(calculateSafePercent(playerStats.MostKillsWithGunAmount, playerStats.Kills), 1)}%<a href=""#asterix-own-kills"">*</a>)")}"},
                                        {"Best map", $"{(playerStats.BestMap == null ? "None" : $@"<a href=""#map-{playerStats.BestMap}-{playerStats.BestMapGameMode}"">{bestMap!.Name}</a><br />{playerStats.BestMapAverageScore} avg score")}" },
                                        {"VAC", $"{(vacCount > 0 ? $@"<span class=""text-danger"">Yes, {vacCount}" : $@"<span class=""text-success"">No")}</span>"}
                                    }));
                            }
                        }
                    });

                    serverStatsBuilder.AppendLine($@"<h4>Honorable mentions</h3>
        <div class=""card-group d-flex flex-wrap"">");

                    List<string> honorableMentionsCards = new List<string>();

                    string? highestKDR = getPlayerCardWith(playerStatsContent.Keys.ToList(), p => p.Kills > 10 ? (double)p.Kills / (p.Deaths == 0 ? 1 : p.Deaths) : 0, p => p.Kills, 1, false, "Highest K/D ratio", "K/D ratio");
                    if (highestKDR != null)
                    {
                        honorableMentionsCards.Add(highestKDR);
                    }

                    string? mostKills = getPlayerCardWith(playerStatsContent.Keys.ToList(), p => p.Kills, p => p.TotalScore, 0, false, "Most kills", "Kills");
                    if (mostKills != null)
                    {
                        honorableMentionsCards.Add(mostKills);
                    }

                    string? mostAssists = getPlayerCardWith(playerStatsContent.Keys.ToList(), p => p.Assists, p => p.TotalScore, 0, false, "Most assists", "Assists");
                    if (mostAssists != null)
                    {
                        honorableMentionsCards.Add(mostAssists);
                    }

                    string? highestTotalScore = getPlayerCardWith(playerStatsContent.Keys.ToList(), p => p.TotalScore, p => p.Kills, 0, false, "Highest total score", "Score");
                    if (highestTotalScore != null)
                    {
                        honorableMentionsCards.Add(highestTotalScore);
                    }

                    string? highestAverageScore = getPlayerCardWith(playerStatsContent.Keys.ToList(), p => p.AverageScore, p => p.TotalScore, 0, false, "Highest average score", "Score");
                    if (highestAverageScore != null)
                    {
                        honorableMentionsCards.Add(highestAverageScore);
                    }

                    string? mostPlants = getPlayerCardWith(playerStatsContent.Keys.ToList(), p => p.BombsPlanted, p => p.TotalScore, 0, false, "Most bomb plants", "Plants");
                    if (mostPlants != null)
                    {
                        honorableMentionsCards.Add(mostPlants);
                    }

                    string? mostDefuses = getPlayerCardWith(playerStatsContent.Keys.ToList(), p => p.BombsDefused, p => p.TotalScore, 0, false, "Most bomb defuses", "Defuses");
                    if (mostDefuses != null)
                    {
                        honorableMentionsCards.Add(mostDefuses);
                    }

                    string? mostHeadshots = getPlayerCardWith(playerStatsContent.Keys.ToList(), p => p.Headshots, p => p.Kills, 0, false, "Most headshot kills", "HS kills");
                    if (mostHeadshots != null)
                    {
                        honorableMentionsCards.Add(mostHeadshots);
                    }

                    string? highestHSKR = getPlayerCardWith(playerStatsContent.Keys.ToList(), p => p.Kills > 10 ? (double)p.Headshots / p.Kills : 0, p => p.Kills, 1, false, "Highest HS-kill to kill ratio", "HSKR");
                    if (highestHSKR != null)
                    {
                        honorableMentionsCards.Add(highestHSKR);
                    }

                    serverStatsBuilder.AppendLine(splitInRows(honorableMentionsCards.ToArray()));

                    serverStatsBuilder.AppendLine(@"</div><br/>
        <h4>Dishonorable mentions</h3>
        <div class=""card-group d-flex flex-wrap"">");

                    List<string> dishonorableMentionsCards = new();

                    string? mostDeaths = getPlayerCardWith(playerStatsContent.Keys.ToList(), p => p.Deaths, p => p.TotalScore, 0, false, "Most deaths", "Deaths");
                    if (mostDeaths != null)
                    {
                        dishonorableMentionsCards.Add(mostDeaths);
                    }

                    string? mostTeamKills = getPlayerCardWith(playerStatsContent.Keys.ToList(), p => p.TeamKills, p => p.Kills, 0, false, "Most teamkills", "Teamkills");
                    if (mostTeamKills != null)
                    {
                        dishonorableMentionsCards.Add(mostTeamKills);
                    }

                    string? lowestHSKR = getPlayerCardWith(playerStatsContent.Keys.ToList(), p => p.Kills > 10 ? (double)p.Headshots / p.Kills : double.MaxValue, p => p.Kills, 1, true, "Lowest HS-kill to kill ratio", "HSKR");
                    if (lowestHSKR != null)
                    {
                        dishonorableMentionsCards.Add(lowestHSKR);
                    }

                    string? mostSuicides = getPlayerCardWith(playerStatsContent.Keys.ToList(), p => p.Kills > 10 ? p.Suicides : 0, p => p.Kills, 0, false, "Most suicides", "Suicides");
                    if (mostSuicides != null)
                    {
                        dishonorableMentionsCards.Add(mostSuicides);
                    }

                    serverStatsBuilder.AppendLine(splitInRows(dishonorableMentionsCards.ToArray()));

                    serverStatsBuilder.AppendLine(@"</div><br/>
        <h4>All players</h3>
        <div class=""card-group d-flex flex-wrap"">");

                    serverStatsBuilder.AppendLine(splitInRows(playerStatsContent.OrderByDescending(kvp => kvp.Key.TotalScore).Select(kvp => kvp.Value).ToArray()));
                }
                serverStatsBuilder.AppendLine("</div>");

                serverStatsBuilder.AppendLine(@$"
        <h3 id=""asterix-own-kills"">Percentages marked with *</h3>
        <p>Percentages marked with * are calculated using own amounts (eg. own kills), not total kill count. Unmarked percentages are calculated using total amounts (eg. total kills).</p>
        <h3>Score</h3>
        <p>Score is calculated using the following formula:<br />
        Kills * {StatsCalculator.SCORE_WEIGHT_KILL} +<br />
        Deaths * {StatsCalculator.SCORE_WEIGHT_DEATH} +<br />
        Assists * {StatsCalculator.SCORE_WEIGHT_ASSIST} +<br />
        Headshots * {StatsCalculator.SCORE_WEIGHT_HEADSHOT} +<br />
        Teamkills * {StatsCalculator.SCORE_WEIGHT_TEAMKILL} +<br />
        Bomb Plants * {StatsCalculator.SCORE_WEIGHT_PLANT} +<br />
        Bomb Defuses * {StatsCalculator.SCORE_WEIGHT_DEFUSE}</p>
        <h3>Some values don't seem to add up</h3>
        <p>You may notice that some values don't seem to add up, like Team Blue Kills + Team Red Kills is not equal to Total Kills. This is caused because some statistics have filters applied, like only counting matches with at least two players or with a combined Team Blue and Team Red score of at least 10 (draws and skipped maps). Prerounds often are omitted as well. Total kills however counts each single kill.</p>
    </div>
    <footer class=""text-center text-lg-start bg-light text-muted"">
      <section class=""d-flex justify-content-center justify-content-lg-between p-4 border-bottom"">
        Stats updated on {DateTime.UtcNow.ToLongDateString()} {DateTime.UtcNow.ToLongTimeString()} UTC.<br />
        Stats are updated every eight hours.
      </section>
      <div class=""text-center p-4"" style=""background-color: rgba(0, 0, 0, 0.05);"">
        Provided by: 
        <a class=""text-reset fw-bold"" href=""https://codefreak.net/"">codefreak.net</a> for <a class=""text-reset fw-bold"" href=""https://bloodisgood.net/"">Blood is Good</a>
      </div>
    </footer>
    <script src=""https://cdn.jsdelivr.net/npm/bootstrap@5.2.2/dist/js/bootstrap.bundle.min.js"" integrity=""sha384-OERcA2EqjJCMA+/3y+gxIOqMEjwtxJY7qPCqsdltbNJuaOe923+mo//f6V8Qbsw3"" crossorigin=""anonymous""></script>
</body>
</html>");
                File.WriteAllText($"stats/{server.ServerId}.html", serverStatsBuilder.ToString());
            }

            Console.WriteLine("Stats written");
        }

        private string splitInRows(string[] cards)
        {
            if (cards.Length == 0)
            {
                return String.Empty;
            }

            StringBuilder rowCards = new StringBuilder();

            for (int i = 0; i < cards.Length; i++)
            {
                if (i == 0)
                {
                    rowCards.AppendLine("<div class=\"row mt-3\">");
                }
                else if (i % CARDS_PER_ROW == 0)
                {
                    rowCards.AppendLine("</div>");
                    rowCards.AppendLine("<div class=\"row mt-3\">");
                }

                rowCards.AppendLine(cards[i]);
            }

            rowCards.AppendLine("</div>");

            return rowCards.ToString();
        }

        private string? getPlayerCardWith(List<CPlayerStats> playerStats, Func<CPlayerStats, double> selector, Func<CPlayerStats, double> secondSort, int round, bool invert, string title, string statName)
        {
            CPlayerStats? highestValuePlayer = playerStats.OrderByDescending(p => selector(p) * (invert ? -1 : 1)).ThenByDescending(secondSort).FirstOrDefault();
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

            double highestValue = selector(highestValuePlayer);

            return createStatsCard(null,
                $"#player-{highestValuePlayer.UniqueId}",
                false,
                playerSummary.AvatarFullUrl,
                title,
                new()
                {
                    { "Player", $"<a href=\"#player-{highestValuePlayer.UniqueId}\">{playerSummary.Nickname}</a>" },
                    { statName, Math.Round(highestValue == -0 ? 0 : highestValue, round).ToString($"0{(round == 0 ? "" : ".".PadRight(round + 1, '0'))}") }
                });

        }

        private double calculateSafePercent(double a, double b)
        {
            return a * 100d / (b == 0 ? 1 : b);
        }

        private string createStatsCard(string? id, string? link, bool openInBlank, string imageURL, string title, Dictionary<string, string> values)
        {
            return @$"<div class=""col""><div class=""card h-100"" style=""width: {CARD_WIDTH}px"" {(id == null ? "" : $@"id=""{id}""")}>
                {(link == null ? "" : $@"<a href=""{link}""{(openInBlank ? " target=\"_blank\"" : "")}>")}
                    <img class=""card-img-top"" src=""{imageURL}"" width=""{CARD_WIDTH}"" height=""{CARD_WIDTH}"" />
                {(link == null ? "" : "</a>")}
                <div class=""card-body"">
                    <h5 class=""card-title"">{(link == null ? "" : $@"<a href=""{link}""{(openInBlank ? " target=\"blank\"" : "")}>")}{title}{(link == null ? "" : $@"</a>")}</h5>

                    <p class=""card-text"">
                        <div class=""container px-0"">
                            {String.Join(Environment.NewLine, values.Select(kvp => $"<div class=\"row row-alternating gx-0\"><div class=\"col-auto px-1\"><b>{kvp.Key}:</b></div><div class=\"col text-end px-1\">{kvp.Value}</div></div>"))}
                        </div>
                    </p>
                </div>
            </div></div>";
        }

        public void StatsReader()
        {
            DateTime lastGenerated = DateTime.MinValue;

            while (running)
            {
                if (lastGenerated < DateTime.Now.AddHours(-8))
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
    }
}
