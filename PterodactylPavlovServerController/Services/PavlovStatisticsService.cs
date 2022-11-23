using PavlovStatsReader;
using PavlovStatsReader.Models;
using PterodactylPavlovServerDomain.Models;
using Steam.Models.SteamCommunity;
using Steam.Models.Utilities;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace PterodactylPavlovServerController.Services;

public class PavlovStatisticsService : IDisposable
{
    private const int cardWidth = 300;
    private const int cardsPerRow = 4;

    private static readonly Dictionary<string, string> gunMap = new()
    {
        {
            "Revolver", "Revolver"
        },
        {
            "AutoShotgun", "SPAS-12"
        },
        {
            "LMGA", "M249"
        },
        {
            "cet9", "TEC-9"
        },
        {
            "Shotgun", "M590"
        },
        {
            "flash_ru", "Flashbang (Russian)"
        },
        {
            "MP5", "MP5-N"
        },
        {
            "ak12", "AK-12"
        },
        {
            "1911", "M1911A1"
        },
        {
            "de", "Deagle"
        },
        {
            "m16", "M16A2"
        },
        {
            "sawedoff", "Sawed Off"
        },
        {
            "Grenade", "Explosive grenade (NATO)"
        },
        {
            "uzi", "Mini-UZI"
        },
        {
            "AK47", "AK-47"
        },
        {
            "AR", "M4"
        },
        {
            "57", "Five-Seven"
        },
        {
            "killvolume", "Suicide by map trigger"
        },
        {
            "None", "Suicide"
        },
        {
            "DrumShotgun", "Saiga 12"
        },
        {
            "sndbomb", "Bomb (SND)"
        },
        {
            "grenade_ru", "Explosive grenade (Russian)"
        },
        {
            "Knife", "Knife"
        },
        {
            "m9", "Beretta M9"
        },
        {
            "P90", "P90"
        },
        {
            "aug", "AUG A3"
        },
        {
            "flash", "Flashbang (NATO)"
        },
        {
            "sock", "Glock 18C"
        },
        {
            "awp", "AWM"
        },
        {
            "AntiTank", "Barrett M99"
        },
        {
            "AutoSniper", "G3SG1"
        },
        {
            "akshorty", "Draco AK-47"
        },
        {
            "AK", "PP-19 Bizon-2"
        },
        {
            "SMG", "UMP-45"
        },
        {
            "scur", "FN SCAR-20S"
        },
        {
            "hunting", "Scout"
        },
        {
            "vanas", "FAMAS F1"
        },
        {
            "ar9", "AR9"
        },
        {
            "kross", "Vector"
        },
        {
            "vzz", "VSS"
        },
        {
            "mosin", "Mosin-Nagant"
        },
        {
            "runover", "Vehicular manslaughter"
        },
        {
            "trenchgun", "Winchester M97"
        },
        {
            "galul", "Galil"
        },
        {
            "kar98", "Kar98K"
        },
        {
            "sks", "SKS"
        },
        {
            "snowball", "Snowball"
        },
        {
            "stg44", "Sturmgewehr 44"
        },
        {
            "m1garand", "M1 Garand"
        },
        {
            "ppsh", "PPSh-41"
        },
        {
            "webley", "Webley Mk. VI"
        },
        {
            "mp40", "GSG-MP40"
        },
        {
            "minedeath", "Mine"
        }
    };

    private static readonly Regex fileNameDateTimeRegex = new(@"(?<date>\d{4}\.\d{2}\.\d{2}-\d{2}\.\d{2}\.\d{2})", RegexOptions.Compiled);

    private readonly IConfiguration configuration;
    private readonly PavlovServerService pavlovServerService;
    private readonly PterodactylService pterodactylService;
    private readonly StatsCalculator statsCalculator;

    private readonly CancellationTokenSource statsCancellationTokenSource = new();
    private readonly StatsContext statsContext;
    private readonly SteamService steamService;
    private readonly SteamWorkshopService steamWorkshopService;

    public PavlovStatisticsService(IConfiguration configuration, StatsContext statsContext, PterodactylService pterodactylService, SteamService steamService, StatsCalculator statsCalculator, SteamWorkshopService steamWorkshopService, PavlovServerService pavlovServerService)
    {
        this.configuration = configuration;
        this.statsContext = statsContext;
        this.pterodactylService = pterodactylService;
        this.steamService = steamService;
        this.statsCalculator = statsCalculator;
        this.steamWorkshopService = steamWorkshopService;
        this.pavlovServerService = pavlovServerService;
        statsContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        this.statsCancellationTokenSource.Cancel();
    }

    public void Run()
    {
        Task.Run(this.statsReader);
    }

    private async Task<PterodactylServerModel[]> readStatsToDb()
    {
        Console.WriteLine("Reading stats to DB");
        PterodactylServerModel[] pterodactylServerModels = this.pterodactylService.GetServers(this.configuration["pterodactyl_stats_apikey"]);
        foreach (PterodactylServerModel server in pterodactylServerModels)
        {
            Console.WriteLine($"Reading stats for server {server.Name}");
            Setting? lastStat = this.statsContext.Settings.FirstOrDefault(s => s.Name == "Last Read Statistic" && s.ServerId == server.ServerId);

            DateTime lastReadStatistic = lastStat == null ? DateTimeOffset.FromUnixTimeSeconds(0).LocalDateTime : DateTimeOffset.FromUnixTimeSeconds(long.Parse(lastStat.Value)).LocalDateTime;

            Console.WriteLine($"Last statistic: {lastReadStatistic}");

            PterodactylFile[] files = this.pterodactylService.GetFileList(this.configuration["pterodactyl_stats_apikey"], server.ServerId, "/Pavlov/Saved/Logs");
            List<string> filesToParse = new();

            foreach (PterodactylFile file in files)
            {
                Match match = PavlovStatisticsService.fileNameDateTimeRegex.Match(file.Name);

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
                Console.Write($"Reading file {fileName}...");
                StatsReader statsReader = new(await this.pterodactylService.ReadFile(this.configuration["pterodactyl_stats_apikey"], server.ServerId, $"/Pavlov/Saved/Logs/{fileName}"));
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

                Console.WriteLine(" Done.");
            }

            if (lastStat == null)
            {
                this.statsContext.Settings.Add(new Setting
                {
                    Name = "Last Read Statistic",
                    ServerId = server.ServerId,
                    Value = latestStatistic.ToUnixTimeStamp().ToString(),
                });
            }
            else
            {
                lastStat.Value = latestStatistic.ToUnixTimeStamp().ToString();
            }

            Console.WriteLine("Done with server.");
        }

        Console.Write("Saving...");
        await this.statsContext.SaveChangesAsync();
        Console.WriteLine(" Done!");
        return pterodactylServerModels;
    }

    private async Task generateStatsFiles(PterodactylServerModel[] pterodactylServerModels)
    {
        if (!Directory.Exists("stats"))
        {
            Directory.CreateDirectory("stats");
        }

        Console.WriteLine("Generating stats files");

        foreach (PterodactylServerModel server in pterodactylServerModels)
        {
            Console.WriteLine($"Generating stats for server {server.Name}");

            StringBuilder serverStatsBuilder = new();

            string serverName = await this.pavlovServerService.GetServerName(this.configuration["pterodactyl_stats_apikey"], server.ServerId);


            serverStatsBuilder.AppendLine($@"
    <style>
        .row-alternating {{
            background-color: rgba(0, 0, 0, 0.1);
        }}
        .row-alternating:nth-child(2n) {{
            background-color: rgba(0, 0, 0, 0.2);
        }}
        a:link {{ text-decoration: none; }}
        a:visited {{ text-decoration: none; }}
        a:hover {{ text-decoration: none; }}
        a:active {{ text-decoration: none; }}
    </style>
    <div>
        <h1>{serverName} server statistics</h1>

        <h2>Table of contents</h2>
        <ol>
            <li><a href=""stats/{server.ServerId}#serverstats"" onclick=""scrollToId('serverstats'); return false;"" class=""link-light"">Server statistics</a></li>");

            Setting? serverStatMode = this.statsContext.Settings.FirstOrDefault(s => s.Name == "Stat Type" && s.ServerId == server.ServerId);
            if (serverStatMode?.Value == "SND")
            {
                serverStatsBuilder.AppendLine($@"<li><a href=""stats/{server.ServerId}#mapstats"" onclick=""scrollToId('mapstats'); return false;"" class=""link-light"">Map statistics</a></li>");
            }
            serverStatsBuilder.AppendLine($@"<li><a href=""stats/{server.ServerId}#gunstats"" onclick=""scrollToId('gunstats'); return false;"" class=""link-light"">Gun statistics</a></li>");
            if (serverStatMode?.Value == "SND")
            {
                serverStatsBuilder.AppendLine($@"<li><a href=""stats/{server.ServerId}#teamstats"" onclick=""scrollToId('teamstats'); return false;"" class=""link-light"">Team statistics</a></li>");
            }
            serverStatsBuilder.AppendLine($@"<li><a href=""stats/{server.ServerId}#playerstats"" onclick=""scrollToId('playerstats'); return false;"" class=""link-light"">Player statistics</a></li></ol>");

            CBaseStats[] calculatedStats = this.statsCalculator.CalculateStats(server.ServerId);

            // Server stats
            serverStatsBuilder.AppendLine(@"<h2 id=""serverstats"" class=""mt-3"">Server statistics</h2>
                <div class=""card-group d-flex flex-wrap"">");

            CServerStats serverStats = calculatedStats.OfType<CServerStats>().First();

            string[] serverCards;
            if (serverStatMode?.Value == "SND")
            {
                serverCards = new[]
                {
                        this.createStatsCard(null, null, false, "https://bloodisgood.net/wp-content/uploads/2022/10/image_2022-10-31_113333781.png", "Counts", new Dictionary<string, string>
                    {
                        {
                            "Unique maps/modes", serverStats.TotalUniqueMaps.ToString()
                        },
                        {
                            "Unique players", serverStats.TotalUniquePlayers.ToString()
                        },
                        {
                            "Total matches", serverStats.TotalMatchesPlayed.ToString()
                        },
                    }),
                    this.createStatsCard(null, null, false, "https://bloodisgood.net/wp-content/uploads/2022/10/image_2022-10-31_113751041.png", "Kill stats", new Dictionary<string, string>
                    {
                        {
                            "Total kills", serverStats.TotalKills.ToString()
                        },
                        {
                            "Total headshots", $"{serverStats.TotalHeadshots} ({Math.Round(this.calculateSafePercent(serverStats.TotalHeadshots, serverStats.TotalKills), 1)}%)"
                        },
                        {
                            "Total assists", serverStats.TotalAssists.ToString()
                        },
                        {
                            "Total teamkills", serverStats.TotalTeamkills.ToString()
                        },
                    }),

                    this.createStatsCard(null, null, false, "https://bloodisgood.net/wp-content/uploads/2022/10/image_2022-10-31_114051512.png", "Bomb stats", new Dictionary<string, string>
                    {
                        {
                            "Total plants", serverStats.TotalBombPlants.ToString()
                        },
                        {
                            "Total defuses", $"{serverStats.TotalBombDefuses} ({Math.Round(this.calculateSafePercent(serverStats.TotalBombDefuses, serverStats.TotalBombPlants), 1)}%)"
                        },
                        {
                            "Total explosions", $"{serverStats.TotalBombExplosions} ({Math.Round(this.calculateSafePercent(serverStats.TotalBombExplosions, serverStats.TotalBombPlants), 1)}%)"
                        },
                    }),
                };
            }
            else
            {
                serverCards = new[]
                {
                    this.createStatsCard(null, null, false, "https://bloodisgood.net/wp-content/uploads/2022/10/image_2022-10-31_113333781.png", "Counts", new Dictionary<string, string>
                    {
                        {
                            "Unique players", serverStats.TotalUniquePlayers.ToString()
                        }
                    }),
                    this.createStatsCard(null, null, false, "https://bloodisgood.net/wp-content/uploads/2022/10/image_2022-10-31_113751041.png", "Kill stats", new Dictionary<string, string>
                    {
                        {
                            "Total kills", serverStats.TotalKills.ToString()
                        },
                        {
                            "Total headshots", $"{serverStats.TotalHeadshots} ({Math.Round(this.calculateSafePercent(serverStats.TotalHeadshots, serverStats.TotalKills), 1)}%)"
                        },
                    }),
                };
            }

            serverStatsBuilder.AppendLine(this.splitInRows(serverCards));

            serverStatsBuilder.AppendLine("</div>");

            if (serverStatMode?.Value == "SND")
            {
                // Maps stats
                serverStatsBuilder.AppendLine(@"<h2 id=""mapstats"" class=""mt-3"">Map statistics</h2>
        <div class=""card-group d-flex flex-wrap"">");

                Dictionary<CMapStats, string> mapsStatsContent = new();
                CMapStats[] mapsStats = calculatedStats.OfType<CMapStats>().ToArray();
                foreach (CMapStats mapStats in mapsStats)
                {
                    MapWorkshopModel mapWorkshop;
                    try
                    {
                        mapWorkshop = this.steamWorkshopService.GetMapDetail(long.Parse(mapStats.MapId.Substring(3)));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Failed to retrieve map detail for map {mapStats.MapId}: {e.Message}");
                        return;
                    }

                    string? bestPlayerUsername = null;
                    try
                    {
                        bestPlayerUsername = mapStats.BestPlayer == null ? null : await this.steamService.GetUsername(mapStats.BestPlayer.Value);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Could not get username of player id {(mapStats.BestPlayer.HasValue ? mapStats.BestPlayer.Value.ToString() : "N/A")}: {ex.Message}");
                    }

                    lock (mapsStatsContent)
                    {
                        mapsStatsContent.Add(mapStats, this.createStatsCard($"map-{mapStats.MapId}-{mapStats.GameMode}", mapWorkshop.URL, true, $"{mapWorkshop.ImageURL}/?imw={PavlovStatisticsService.cardWidth}&imh={PavlovStatisticsService.cardWidth}&ima=fit&impolicy=Letterbox&imcolor=%23000000&letterbox=true", $"{mapWorkshop.Name} ({mapStats.GameMode})", new Dictionary<string, string>
                    {
                        {
                            "Played", $"{mapStats.PlayCount} time{(mapStats.PlayCount != 1 ? "s" : "")}"
                        },
                        {
                            "Wins", $"Blue {mapStats.Team0Wins}, Red {mapStats.Team1Wins}"
                        },
                        {
                            "Rounds", $"{Math.Round(mapStats.AverageRounds, 2)} avg, {mapStats.MaxRounds} max, {mapStats.MinRounds} min"
                        },
                        {
                            "Best player", $"{(mapStats.BestPlayer == null ? "Nobody" : $@"<a href=""stats/{server.ServerId}#player-{mapStats.BestPlayer}"" onclick=""scrollToId('player-{mapStats.BestPlayer}'); return false;"">{bestPlayerUsername}</a><br/>{Math.Round(mapStats.MaxAveragePlayerScore, 0)} avg score")}"
                        },
                    }));
                    }
                }

                serverStatsBuilder.AppendLine(this.splitInRows(mapsStatsContent.OrderByDescending(kvp => kvp.Key.PlayCount).Select(kvp => kvp.Value).ToArray()));

                //serverStatsBuilder.AppendLine(String.Join(Environment.NewLine, mapsStatsContent.OrderByDescending(kvp => kvp.Key.PlayCount).Select(kvp => kvp.Value)));
                serverStatsBuilder.AppendLine("</div>");
            }

            // Gun stats
            serverStatsBuilder.AppendLine(@"<h2 id=""gunstats"" class=""mt-3"">Gun statistics</h2>
        <div class=""card-group d-flex flex-wrap"">");

            CGunStats[] gunsStats = calculatedStats.OfType<CGunStats>().ToArray();
            List<string> gunCards = new();

            foreach (CGunStats gunStats in gunsStats.OrderByDescending(g => g.Kills))
            {
                string gunName = getCorrectGunKey(gunStats.Name) is { } gunKey ? PavlovStatisticsService.gunMap[gunKey] : $"{gunStats.Name}(?)";
                string? bestPlayerUsername = null;
                try
                {
                    bestPlayerUsername = gunStats.BestPlayer == null ? null : await this.steamService.GetUsername(gunStats.BestPlayer.Value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not get username of player id {(gunStats.BestPlayer.HasValue ? gunStats.BestPlayer.Value.ToString() : "N/A")}: {ex.Message}");
                }

                gunCards.Add(this.createStatsCard($"gun-{gunStats.Name}", "http://wiki.pavlov-vr.com/index.php?title=Weapons", true, $"https://pavlov.bloodisgood.net/gunimages/{(getCorrectGunKey(gunStats.Name) is { } weaponStatsGunKey ? weaponStatsGunKey : "unknown")}.png", gunName, new Dictionary<string, string>
                {
                    {
                        "Kills", $"{gunStats.Kills} ({Math.Round(this.calculateSafePercent(gunStats.Kills, serverStats.TotalKills), 1)}%)"
                    },
                    {
                        "Headshots", $"{gunStats.Headshots} ({Math.Round(this.calculateSafePercent(gunStats.Headshots, gunStats.Kills))}%<a href=\"stats/{server.ServerId}#asterix-own-kills\" onclick=\"scrollToId('asterix-own-kills'); return false;\">*</a>)"
                    },
                    {
                        "Best player", $"{(gunStats.BestPlayer == null ? "Nobody" : $@"<a href=""stats/{server.ServerId}#player-{gunStats.BestPlayer}"" onclick=""scrollToId('player-{gunStats.BestPlayer}'); return false;"">{bestPlayerUsername}</a> ({gunStats.BestPlayerKills}&nbsp;kills)")}"
                    },
                }));
            }

            serverStatsBuilder.AppendLine(this.splitInRows(gunCards.ToArray()));

            serverStatsBuilder.AppendLine("</div>");

            if (serverStatMode?.Value == "SND")
            {
                // Team stats
                serverStatsBuilder.AppendLine(@"<h2 id=""teamstats"" class=""mt-3"">Team statistics</h2>
        <div class=""card-group d-flex flex-wrap"">");

                CTeamStats[] teamsStats = calculatedStats.OfType<CTeamStats>().ToArray();
                List<string> teamCards = new();
                foreach (CTeamStats teamStats in teamsStats)
                {
                    string? bestPlayerUsername = null;
                    try
                    {
                        bestPlayerUsername = teamStats.BestPlayer == null ? null : await this.steamService.GetUsername(teamStats.BestPlayer.Value);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Could not get username of player id {(teamStats.BestPlayer.HasValue ? teamStats.BestPlayer.Value.ToString() : "N/A")}: {ex.Message}");
                    }

                    teamCards.Add(this.createStatsCard(null, null, false, teamStats.TeamId == 0 ? "https://bloodisgood.net/wp-content/uploads/2022/10/blueteam.png" : "https://bloodisgood.net/wp-content/uploads/2022/10/redteam.png", teamStats.Name, new Dictionary<string, string>
                {
                    {
                        "Kills", $"{teamStats.TotalKills} ({Math.Round(this.calculateSafePercent(teamStats.TotalKills, serverStats.TotalKills), 1)}%)"
                    },
                    {
                        "HS kills", $"{teamStats.TotalHeadshots} ({Math.Round(this.calculateSafePercent(teamStats.TotalHeadshots, teamStats.TotalKills), 1)}%<a href=\"stats/{server.ServerId}#asterix-own-kills\" onclick=\"scrollToId('asterix-own-kills'); return false;\">*</a>)"
                    },
                    {
                        "Assists", $"{teamStats.TotalAssists} ({Math.Round(this.calculateSafePercent(teamStats.TotalAssists, serverStats.TotalAssists), 1)}%)"
                    },
                    {
                        "Teamkills", $"{teamStats.TotalTeamkills} ({Math.Round(this.calculateSafePercent(teamStats.TotalTeamkills, serverStats.TotalTeamkills), 1)}%)"
                    },
                    {
                        "Victories", teamStats.TotalVictories.ToString()
                    },
                    {
                        "Best player", $"{(teamStats.BestPlayer == null ? "Nobody" : $@"<a href=""stats/{server.ServerId}#player-{teamStats.BestPlayer}"" onclick=""scrollToId('player-{teamStats.BestPlayer}'); return false;"">{bestPlayerUsername}</a><br />{Math.Round(teamStats.BestPlayerAverageScore, 0)} avg score")}"
                    },
                    {
                        "Best gun", $"{(teamStats.BestGun == null ? "None" : $@"<a href=""stats/{server.ServerId}#gun-{teamStats.BestGun}"" onclick=""scrollToId('gun-{teamStats.BestGun}'); return false;"">{(getCorrectGunKey(teamStats.BestGun) is { } gunKey ? PavlovStatisticsService.gunMap[gunKey] : teamStats.BestGun)}</a> ({teamStats.BestGunKillCount} kills)")}"
                    },
                }));
                }

                serverStatsBuilder.AppendLine(this.splitInRows(teamCards.ToArray()));

                serverStatsBuilder.AppendLine("</div>");
            }

            // Player stats
            CPlayerStats[] playersStats = calculatedStats.OfType<CPlayerStats>().ToArray();
            serverStatsBuilder.AppendLine(@"<h3 id=""playerstats"" class=""mt-3"">Player statistics</h3>");
            if (playersStats.Any())
            {
                int totalKills = playersStats.Sum(p => p.Kills);
                int totalDeaths = playersStats.Sum(p => p.Deaths);
                int totalAssists = playersStats.Sum(p => p.Assists);
                int totalScore = playersStats.Sum(p => p.TotalScore);

                Dictionary<CPlayerStats, string> playerStatsContent = new();

                int current = 0;
                foreach (CPlayerStats playerStats in playersStats)
                {
                    Console.WriteLine($"Generating stats card for player {playerStats.UniqueId} ({++current} / {playersStats.Length})");
                    using (StatsContext statsContext = new(this.configuration))
                    {
                        PlayerSummaryModel playerSummary;
                        try
                        {
                            playerSummary = await this.steamService.GetPlayerSummary(playerStats.UniqueId);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Could not get summary of player {playerStats.UniqueId}: {e.Message}");
                            continue;
                        }

                        IReadOnlyCollection<PlayerBansModel> playerBans;
                        try
                        {
                            playerBans = await this.steamService.GetBans(playerStats.UniqueId);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Could not get bans of player {playerStats.UniqueId}: {e.Message}");
                            continue;
                        }

                        int vacCount = 0;
                        foreach (PlayerBansModel playerBan in playerBans)
                        {
                            vacCount += (int)playerBan.NumberOfVACBans;
                        }

                        MapWorkshopModel? bestMap = null;
                        if (playerStats.BestMap != null)
                        {
                            bestMap = this.steamWorkshopService.GetMapDetail(long.Parse(playerStats.BestMap[3..]));
                        }

                        if (serverStatMode?.Value == "SND")
                        {
                            playerStatsContent.Add(playerStats, this.createStatsCard($"player-{playerStats.UniqueId}", playerSummary.ProfileUrl, true, playerSummary.AvatarFullUrl, playerSummary.Nickname, new Dictionary<string, string>
                                {
                                    {
                                        "K/D ratio", $"{Math.Round((double) playerStats.Kills / playerStats.Deaths, 1):0.0}"
                                    },
                                    {
                                        "Kills", $"{playerStats.Kills} ({Math.Round(this.calculateSafePercent(playerStats.Kills, totalKills), 1)}%)"
                                    },
                                    {
                                        "Deaths", $"{playerStats.Deaths} ({Math.Round(this.calculateSafePercent(playerStats.Deaths, totalDeaths), 1)}%)"
                                    },
                                    {
                                        "Assists", $"{playerStats.Assists} ({Math.Round(this.calculateSafePercent(playerStats.Assists, totalAssists), 1)}%)"
                                    },
                                    {
                                        "Team kills", $"{playerStats.TeamKills} ({Math.Round(this.calculateSafePercent(playerStats.TeamKills, playerStats.Kills), 1)}%<a href=\"stats/{server.ServerId}#asterix-own-kills\" onclick=\"scrollToId('asterix-own-kills'); return false;\">*</a>)"
                                    },
                                    {
                                        "HS kills", $"{playerStats.Headshots} ({Math.Round(this.calculateSafePercent(playerStats.Headshots, playerStats.Kills), 1)}%<a href=\"stats/{server.ServerId}#asterix-own-kills\" onclick=\"scrollToId('asterix-own-kills'); return false;\">*</a>)"
                                    },
                                    {
                                        "Suicides", playerStats.Suicides.ToString()
                                    },
                                    {
                                        "Avg. points", $"{Math.Round(playerStats.AverageScore, 0)}"
                                    },
                                    {
                                        "Total points", $"{playerStats.TotalScore} ({Math.Round(this.calculateSafePercent(playerStats.TotalScore, totalScore), 1)}%)"
                                    },
                                    {
                                        "Bombs", $"{playerStats.BombsPlanted} planted, {playerStats.BombsDefused} defused"
                                    },
                                    {
                                        "Rounds played", $"{playerStats.RoundsPlayed}"
                                    },
                                    {
                                        "Best gun", $"{(playerStats.MostKillsWithGun == null ? "None" : $@"<a href=""stats/{server.ServerId}#gun-{playerStats.MostKillsWithGun}"" onclick=""scrollToId('gun-{playerStats.MostKillsWithGun}'); return false;"">{(getCorrectGunKey(playerStats.MostKillsWithGun) is {} gunKey ? PavlovStatisticsService.gunMap[gunKey] : playerStats.MostKillsWithGun)}</a><br />{playerStats.MostKillsWithGunAmount} kills ({Math.Round(this.calculateSafePercent(playerStats.MostKillsWithGunAmount, playerStats.Kills), 1)}%<a href=""stats/{server.ServerId}#asterix-own-kills"" onclick=""scrollToId('asterix-own-kills'); return false;"">*</a>)")}"
                                    },
                                    {
                                        "Best map", $"{(playerStats.BestMap == null ? "None" : $@"<a href=""stats/{server.ServerId}#map-{playerStats.BestMap}-{playerStats.BestMapGameMode}"" onclick=""scrollToId('map-{playerStats.BestMap}-{playerStats.BestMapGameMode}'); return false;"">{bestMap!.Name}</a><br />{Math.Round(playerStats.BestMapAverageScore, 0)} avg score")}"
                                    },
                                    {
                                        "VAC", $"{(vacCount > 0 ? $@"<span class=""text-danger"">Yes, {vacCount}" : @"<span class=""text-success"">No")}</span>"
                                    },
                                }));
                        }
                        else if (serverStatMode?.Value == "EFP")
                        {
                            int cash = 0;
                            try
                            {
                                int.TryParse(await pterodactylService.ReadFile(this.configuration["pterodactyl_stats_apikey"], server.ServerId, $"/Pavlov/Saved/Config/ModSave/{playerStats.UniqueId}.txt"), out cash);
                            }
                            catch { }
                            playerStats.TotalScore = cash;

                            playerStatsContent.Add(playerStats, this.createStatsCard($"player-{playerStats.UniqueId}", playerSummary.ProfileUrl, true, playerSummary.AvatarFullUrl, $"{playerSummary.Nickname}{(string.IsNullOrEmpty(playerSummary.CountryCode) ? "" : $"<img src=\"https://countryflagsapi.com/png/{playerSummary.CountryCode}\" alt=\"{playerSummary.CountryCode}\" height=\"16\" class=\"ms-2\"/>")}", new Dictionary<string, string>
                                {
                                    {
                                        "Cash", $"${cash}"
                                    },
                                    {
                                        "K/D ratio", $"{Math.Round((double) playerStats.Kills / playerStats.Deaths, 1):0.0}"
                                    },
                                    {
                                        "Kills", $"{playerStats.Kills} ({Math.Round(this.calculateSafePercent(playerStats.Kills, totalKills), 1)}%)"
                                    },
                                    {
                                        "Deaths", $"{playerStats.Deaths} ({Math.Round(this.calculateSafePercent(playerStats.Deaths, totalDeaths), 1)}%)"
                                    },
                                    {
                                        "HS kills", $"{playerStats.Headshots} ({Math.Round(this.calculateSafePercent(playerStats.Headshots, playerStats.Kills), 1)}%<a href=\"stats/{server.ServerId}#asterix-own-kills\" onclick=\"scrollToId('asterix-own-kills'); return false;\">*</a>)"
                                    },
                                    {
                                        "Suicides", playerStats.Suicides.ToString()
                                    },
                                    {
                                        "Best gun", $"{(playerStats.MostKillsWithGun == null ? "None" : $@"<a href=""stats/{server.ServerId}#gun-{playerStats.MostKillsWithGun}"" onclick=""scrollToId('gun-{playerStats.MostKillsWithGun}'); return false;"">{(getCorrectGunKey(playerStats.MostKillsWithGun) is {} gunKey ? PavlovStatisticsService.gunMap[gunKey] : playerStats.MostKillsWithGun)}</a><br />{playerStats.MostKillsWithGunAmount} kills ({Math.Round(this.calculateSafePercent(playerStats.MostKillsWithGunAmount, playerStats.Kills), 1)}%<a href=""stats/{server.ServerId}#asterix-own-kills"" onclick=""scrollToId('asterix-own-kills'); return false;"">*</a>)")}"
                                    },
                                    {
                                        "VAC", $"{(vacCount > 0 ? $@"<span class=""text-danger"">Yes, {vacCount}" : @"<span class=""text-success"">No")}</span>"
                                    },
                                }));
                        }
                        else
                        {
                            playerStatsContent.Add(playerStats, this.createStatsCard($"player-{playerStats.UniqueId}", playerSummary.ProfileUrl, true, playerSummary.AvatarFullUrl, $"{playerSummary.Nickname}{(string.IsNullOrEmpty(playerSummary.CountryCode) ? "" : $"<img src=\"https://countryflagsapi.com/png/{playerSummary.CountryCode}\" alt=\"{playerSummary.CountryCode}\" height=\"16\" class=\"ms-2\"/>")}", new Dictionary<string, string>
                                {
                                    {
                                        "K/D ratio", $"{Math.Round((double) playerStats.Kills / playerStats.Deaths, 1):0.0}"
                                    },
                                    {
                                        "Kills", $"{playerStats.Kills} ({Math.Round(this.calculateSafePercent(playerStats.Kills, totalKills), 1)}%)"
                                    },
                                    {
                                        "Deaths", $"{playerStats.Deaths} ({Math.Round(this.calculateSafePercent(playerStats.Deaths, totalDeaths), 1)}%)"
                                    },
                                    {
                                        "HS kills", $"{playerStats.Headshots} ({Math.Round(this.calculateSafePercent(playerStats.Headshots, playerStats.Kills), 1)}%<a href=\"stats/{server.ServerId}#asterix-own-kills\" onclick=\"scrollToId('asterix-own-kills'); return false;\">*</a>)"
                                    },
                                    {
                                        "Suicides", playerStats.Suicides.ToString()
                                    },
                                    {
                                        "Best gun", $"{(playerStats.MostKillsWithGun == null ? "None" : $@"<a href=""stats/{server.ServerId}#gun-{playerStats.MostKillsWithGun}"" onclick=""scrollToId('gun-{playerStats.MostKillsWithGun}'); return false;"">{(getCorrectGunKey(playerStats.MostKillsWithGun) is {} gunKey ? PavlovStatisticsService.gunMap[gunKey] : playerStats.MostKillsWithGun)}</a><br />{playerStats.MostKillsWithGunAmount} kills ({Math.Round(this.calculateSafePercent(playerStats.MostKillsWithGunAmount, playerStats.Kills), 1)}%<a href=""stats/{server.ServerId}#asterix-own-kills"" onclick=""scrollToId('asterix-own-kills'); return false;"">*</a>)")}"
                                    },
                                    {
                                        "VAC", $"{(vacCount > 0 ? $@"<span class=""text-danger"">Yes, {vacCount}" : @"<span class=""text-success"">No")}</span>"
                                    },
                                }));
                        }
                    }

                }

                serverStatsBuilder.AppendLine(@"<h4>Honorable mentions</h3>
        <div class=""card-group d-flex flex-wrap"">");

                List<string> honorableMentionsCards = new();

                string? highestKDR = await this.getPlayerCardWith(server, playerStatsContent.Keys.ToList(), p => p.Kills > 10 ? (double)p.Kills / (p.Deaths == 0 ? 1 : p.Deaths) : 0, p => p.Kills, 1, false, "Highest K/D ratio", "K/D ratio");
                if (highestKDR != null)
                {
                    honorableMentionsCards.Add(highestKDR);
                }

                string? mostKills = await this.getPlayerCardWith(server, playerStatsContent.Keys.ToList(), p => p.Kills, p => p.TotalScore, 0, false, "Most kills", "Kills");
                if (mostKills != null)
                {
                    honorableMentionsCards.Add(mostKills);
                }

                if (serverStatMode?.Value == "SND")
                {
                    string? mostAssists = await this.getPlayerCardWith(server, playerStatsContent.Keys.ToList(), p => p.Assists, p => p.TotalScore, 0, false, "Most assists", "Assists");
                    if (mostAssists != null)
                    {
                        honorableMentionsCards.Add(mostAssists);
                    }


                    string? highestTotalScore = await this.getPlayerCardWith(server, playerStatsContent.Keys.ToList(), p => p.TotalScore, p => p.Kills, 0, false, "Highest total score", "Score");
                    if (highestTotalScore != null)
                    {
                        honorableMentionsCards.Add(highestTotalScore);
                    }

                    string? highestAverageScore = await this.getPlayerCardWith(server, playerStatsContent.Keys.ToList(), p => p.AverageScore, p => p.TotalScore, 0, false, "Highest average score", "Score");
                    if (highestAverageScore != null)
                    {
                        honorableMentionsCards.Add(highestAverageScore);
                    }

                    string? mostPlants = await this.getPlayerCardWith(server, playerStatsContent.Keys.ToList(), p => p.BombsPlanted, p => p.TotalScore, 0, false, "Most bomb plants", "Plants");
                    if (mostPlants != null)
                    {
                        honorableMentionsCards.Add(mostPlants);
                    }

                    string? mostDefuses = await this.getPlayerCardWith(server, playerStatsContent.Keys.ToList(), p => p.BombsDefused, p => p.TotalScore, 0, false, "Most bomb defuses", "Defuses");
                    if (mostDefuses != null)
                    {
                        honorableMentionsCards.Add(mostDefuses);
                    }
                }

                string? mostHeadshots = await this.getPlayerCardWith(server, playerStatsContent.Keys.ToList(), p => p.Headshots, p => p.Kills, 0, false, "Most headshot kills", "HS kills");
                if (mostHeadshots != null)
                {
                    honorableMentionsCards.Add(mostHeadshots);
                }

                string? highestHSKR = await this.getPlayerCardWith(server, playerStatsContent.Keys.ToList(), p => p.Kills > 10 ? (double)p.Headshots / p.Kills : 0, p => p.Kills, 1, false, "Highest HS-kill to kill ratio", "HSKR");
                if (highestHSKR != null)
                {
                    honorableMentionsCards.Add(highestHSKR);
                }

                serverStatsBuilder.AppendLine(this.splitInRows(honorableMentionsCards.ToArray()));

                serverStatsBuilder.AppendLine(@"</div><br/>
        <h4>Dishonorable mentions</h3>
        <div class=""card-group d-flex flex-wrap"">");

                List<string> dishonorableMentionsCards = new();

                string? mostDeaths = await this.getPlayerCardWith(server, playerStatsContent.Keys.ToList(), p => p.Deaths, p => p.TotalScore, 0, false, "Most deaths", "Deaths");
                if (mostDeaths != null)
                {
                    dishonorableMentionsCards.Add(mostDeaths);
                }

                if (serverStatMode?.Value == "SND")
                {
                    string? mostTeamKills = await this.getPlayerCardWith(server, playerStatsContent.Keys.ToList(), p => p.TeamKills, p => p.Kills, 0, false, "Most teamkills", "Teamkills");
                    if (mostTeamKills != null)
                    {
                        dishonorableMentionsCards.Add(mostTeamKills);
                    }
                }

                string? lowestHSKR = await this.getPlayerCardWith(server, playerStatsContent.Keys.ToList(), p => p.Kills > 10 ? (double)p.Headshots / p.Kills : double.MaxValue, p => p.Kills, 1, true, "Lowest HS-kill to kill ratio", "HSKR");
                if (lowestHSKR != null)
                {
                    dishonorableMentionsCards.Add(lowestHSKR);
                }

                string? mostSuicides = await this.getPlayerCardWith(server, playerStatsContent.Keys.ToList(), p => p.Kills > 10 ? p.Suicides : 0, p => p.Kills, 0, false, "Most suicides", "Suicides");
                if (mostSuicides != null)
                {
                    dishonorableMentionsCards.Add(mostSuicides);
                }

                serverStatsBuilder.AppendLine(this.splitInRows(dishonorableMentionsCards.ToArray()));

                serverStatsBuilder.AppendLine(@"</div><br/>
        <h4>All players</h3>
        <div class=""card-group d-flex flex-wrap"">");

                serverStatsBuilder.AppendLine(this.splitInRows(playerStatsContent.OrderByDescending(kvp => kvp.Key.TotalScore).ThenByDescending(kvp => kvp.Key.Kills).Select(kvp => kvp.Value).ToArray()));
                serverStatsBuilder.AppendLine("</div>");
            }


            serverStatsBuilder.AppendLine(@$"

    </div>
        <div>
            <h3 id=""asterix-own-kills"" class=""mt-3"">Percentages marked with *</h3>
            <p>Percentages marked with * are calculated using own amounts (eg. own kills), not total kill count.<br />
            Unmarked percentages are calculated using total amounts (eg. total kills).</p>
            <h3 class=""mt-3"">Score</h3>
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
<script>
    function scrollToId(id) {{
        const element = document.getElementById(id);
        if (element instanceof HTMLElement) {{
            element.scrollIntoView({{
                behavior: ""smooth"",
                block: ""start"",
                inline: ""nearest""
            }});
        }}
    }}
</script>
    <footer class=""text-center text-lg-start bg-dark text-muted"">
      <section class=""d-flex justify-content-center justify-content-lg-between p-4 border-bottom"">
        Stats updated on {DateTime.UtcNow.ToLongDateString()} {DateTime.UtcNow.ToLongTimeString()} UTC.<br />
        Stats are updated every eight hours.
      </section>
      <div class=""text-center p-4"">
        Provided by: 
        <a class=""text-reset fw-bold"" href=""https://codefreak.net/"">codefreak.net</a> for <a class=""text-reset fw-bold"" href=""https://bloodisgood.net/"">Blood is Good</a>
      </div>
    </footer>");
            await File.WriteAllTextAsync($"stats/{server.ServerId}.html", serverStatsBuilder.ToString());
        }

        Console.WriteLine("Stats written");
    }

    private string splitInRows(string[] cards)
    {
        if (cards.Length == 0)
        {
            return string.Empty;
        }

        StringBuilder rowCards = new();

        for (int i = 0; i < cards.Length; i++)
        {
            if (i == 0)
            {
                rowCards.AppendLine("<div class=\"row mt-3\">");
            }
            else if (i % PavlovStatisticsService.cardsPerRow == 0)
            {
                rowCards.AppendLine("</div>");
                rowCards.AppendLine("<div class=\"row mt-3\">");
            }

            rowCards.AppendLine(cards[i]);
        }

        rowCards.AppendLine("</div>");

        return rowCards.ToString();
    }

    private async Task<string?> getPlayerCardWith(PterodactylServerModel server, List<CPlayerStats> playerStats, Func<CPlayerStats, double> selector, Func<CPlayerStats, double> secondSort, int round, bool invert, string title, string statName)
    {
        CPlayerStats? highestValuePlayer = playerStats.OrderByDescending(p => selector(p) * (invert ? -1 : 1)).ThenByDescending(secondSort).FirstOrDefault();
        if (highestValuePlayer == null)
        {
            return null;
        }

        PlayerSummaryModel? playerSummary = null;
        try
        {
            playerSummary = await this.steamService.GetPlayerSummary(highestValuePlayer.UniqueId);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Could not get summary of player {highestValuePlayer.UniqueId}: {e.Message}");
            return null;
        }

        double highestValue = selector(highestValuePlayer);

        return this.createStatsCard(null, $"stats/{server.ServerId}#player-{highestValuePlayer.UniqueId}", false, playerSummary.AvatarFullUrl, title, new Dictionary<string, string>
        {
            {
                "Player", $"<a href=\"stats/{server.ServerId}#player-{highestValuePlayer.UniqueId}\" onclick=\"scrollToId('player-{highestValuePlayer.UniqueId}'); return false;\">{playerSummary.Nickname}</a>"
            },
            {
                statName, Math.Round(highestValue == -0 ? 0 : highestValue, round).ToString($"0{(round == 0 ? "" : ".".PadRight(round + 1, '0'))}")
            },
        });
    }

    private double calculateSafePercent(double a, double b)
    {
        return a * 100d / (b == 0 ? 1 : b);
    }

    private string createStatsCard(string? id, string? link, bool openInBlank, string imageURL, string title, Dictionary<string, string> values)
    {
        string onClick = string.Empty;
        if (link?.StartsWith("#") ?? false)
        {
            onClick = $"  onclick=\"scrollToId('{link[1..]}'); return false;\"";
        }
        return @$"<div class=""col""><div class=""card bg-dark h-100"" style=""width: {PavlovStatisticsService.cardWidth}px"" {(id == null ? "" : $@"id=""{id}""")}>
                {(link == null ? "" : $@"<a href=""{link}""{(openInBlank ? " target=\"_blank\"" : "")}{onClick}>")}
                    <img class=""card-img-top"" src=""{imageURL}"" width=""{PavlovStatisticsService.cardWidth}"" height=""{PavlovStatisticsService.cardWidth}"" />
                {(link == null ? "" : "</a>")}
                <div class=""card-body px-0"">
                    <h5 class=""card-title px-3"">{(link == null ? "" : $@"<a href=""{link}""{(openInBlank ? " target=\"blank\"" : "")}{onClick}>")}{title}{(link == null ? "" : @"</a>")}</h5>

                    <p class=""card-text"">
                        <div class=""container px-0"">
                            {string.Join(Environment.NewLine, values.Select(kvp => $"<div class=\"row row-alternating gx-0\"><div class=\"col-auto ps-3 pe-1\"><b>{kvp.Key}:</b></div><div class=\"col text-end ps-1 pe-3\">{kvp.Value}</div></div>"))}
                        </div>
                    </p>
                </div>
            </div></div>";
    }

    private string? getCorrectGunKey(string gunName)
    {
        return PavlovStatisticsService.gunMap.Keys.FirstOrDefault(k => string.Equals(k, gunName, StringComparison.CurrentCultureIgnoreCase));
    }

    private async Task statsReader()
    {
        DateTime lastGenerated = DateTime.MinValue;

        while (!this.statsCancellationTokenSource.Token.IsCancellationRequested)
        {
            if (lastGenerated < DateTime.Now.AddHours(-8))
            {
                Console.WriteLine("Generating stats...");
                Stopwatch sw = Stopwatch.StartNew();
                try
                {
                    await this.generateStatsFiles(await this.readStatsToDb());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                sw.Stop();
                Console.WriteLine($"Stats completed in {sw.ElapsedMilliseconds}ms");
                lastGenerated = DateTime.Now;
            }

            Thread.Sleep(1000);
        }
    }
}
