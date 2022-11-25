using BlazorTemplater;
using PavlovStatsReader;
using PavlovStatsReader.Models;
using PterodactylPavlovServerController.Models;
using PterodactylPavlovServerController.Pages.Stats;
using PterodactylPavlovServerDomain.Models;
using Steam.Models.SteamCommunity;
using Steam.Models.Utilities;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace PterodactylPavlovServerController.Services;

public class PavlovStatisticsService : IDisposable
{
    private const int cardWidth = 300;
    private const int cardsPerRow = 4;

    public static readonly IReadOnlyDictionary<string, string> GunMap = new Dictionary<string, string>()
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

            ComponentRenderer<StatsTemplate> templateRenderer = new ComponentRenderer<StatsTemplate>();

            templateRenderer.Set(m => m.ServerId, server.ServerId);

            string serverStatsType = "UNSET";
            Setting? serverStatMode = this.statsContext.Settings.FirstOrDefault(s => s.Name == "Stat Type" && s.ServerId == server.ServerId);
            if (serverStatMode != null)
            {
                serverStatsType = serverStatMode.Value;
            }

            templateRenderer.Set(m => m.ServerStatsType, serverStatsType);

            Console.WriteLine("Calculating stats...");
            CBaseStats[] allStats = this.statsCalculator.CalculateStats(server.ServerId);

            templateRenderer.Set(m => m.ServerCountStats, getServerCountStats(allStats, serverStatsType));
            templateRenderer.Set(m => m.ServerKillStats, getServerKillStats(allStats, serverStatsType));
            templateRenderer.Set(m => m.ServerBombStats, getServerBombStats(allStats, serverStatsType));

            // EFP Player Cash
            if (serverStatsType == "EFP")
            {
                Console.WriteLine("Getting EFP cash...");
                List<CBaseStats> baseStats = allStats.ToList();
                baseStats.AddRange(allStats.OfType<CPlayerStats>().AsParallel().Select(async p =>
                {
                    int cash = 0;
                    try
                    {
                        Console.WriteLine($"Cash of {p.UniqueId}...");
                        int.TryParse(await this.pterodactylService.ReadFile(this.configuration["pterodactyl_stats_apikey"], server.ServerId, $"/Pavlov/Saved/Config/ModSave/{p.UniqueId}.txt"), out cash);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                    return new { Player = p, Cash = cash };
                }).Select(c => new CEFPPlayerCashModel(c.Result.Player.UniqueId, c.Result.Cash)));
                allStats = baseStats.ToArray();
            }

            // Map stats
            if (serverStatsType == "SND")
            {
                Console.WriteLine("Generating map stats...");
                List<(MapWorkshopModel? workshop, CMapStats mapStats, Dictionary<string, object> items)> mapStatistics = new();
                foreach (CMapStats mapStats in getMaps(allStats, serverStatsType))
                {
                    MapWorkshopModel? mapWorkshop = null;
                    if (mapStats.MapId.StartsWith("UGC"))
                    {
                        try
                        {
                            mapWorkshop = this.steamWorkshopService.GetMapDetail(long.Parse(mapStats.MapId.Substring(3)));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Failed to retrieve map detail for map {mapStats.MapId}: {e.Message}");
                        }
                    }

                    Dictionary<string, object> items = await getMapStats(mapStats);
                    mapStatistics.Add((mapWorkshop, mapStats, items));
                }
                templateRenderer.Set(m => m.MapStatistics, mapStatistics);

                templateRenderer.Set(m => m.Team0Stats, await getTeamStats(0, allStats, serverStatsType));
                templateRenderer.Set(m => m.Team1Stats, await getTeamStats(1, allStats, serverStatsType));
            }

            // Gun stats
            Console.WriteLine("Generating gun stats...");
            List<(CGunStats gunStats, Dictionary<string, object> items)> gunStatistics = new List<(CGunStats gunStats, Dictionary<string, object> items)>();
            foreach (CGunStats gunStats in getGuns(allStats, serverStatsType))
            {
                gunStatistics.Add((gunStats, await getGunStats(gunStats, allStats, serverStatsType)));
            }
            templateRenderer.Set(m => m.GunStatistics, gunStatistics);

            // Player stats
            Console.WriteLine("Generating player stats...");
            List<(PlayerSummaryModel summary, CPlayerStats playerStats, Dictionary<string, object> items)> playerStatistics = new List<(PlayerSummaryModel summary, CPlayerStats playerStats, Dictionary<string, object> items)>();
            foreach (CPlayerStats playerStats in getPlayers(allStats, serverStatsType))
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

                try
                {
                    playerStatistics.Add((playerSummary, playerStats, await getPlayerStats(playerStats, allStats, serverStatsType)));
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Uuuuh {e.Message}");
                }
            }
            templateRenderer.Set(m => m.PlayerStatistics, playerStatistics);

            Console.WriteLine("Rendering...");
            await File.WriteAllTextAsync($"stats/{server.ServerId}.html", templateRenderer.Render());
            Console.WriteLine("Done!");
            continue;

            //            // Player stats
            //            CPlayerStats[] playersStats = calculatedStats.OfType<CPlayerStats>().ToArray();
            //            serverStatsBuilder.AppendLine(@"<h3 id=""playerstats"" class=""mt-3"">Player statistics</h3>");
            //            if (playersStats.Any())
            //            {
            //                int totalKills = playersStats.Sum(p => p.Kills);
            //                int totalDeaths = playersStats.Sum(p => p.Deaths);
            //                int totalAssists = playersStats.Sum(p => p.Assists);
            //                int totalScore = playersStats.Sum(p => p.TotalScore);

            //                Dictionary<CPlayerStats, string> playerStatsContent = new();

            //                int current = 0;
            //                foreach (CPlayerStats playerStats in playersStats)
            //                {
            //                    Console.WriteLine($"Generating stats card for player {playerStats.UniqueId} ({++current} / {playersStats.Length})");
            //                    using (StatsContext statsContext = new(this.configuration))
            //                    {
            //                        PlayerSummaryModel playerSummary;
            //                        try
            //                        {
            //                            playerSummary = await this.steamService.GetPlayerSummary(playerStats.UniqueId);
            //                        }
            //                        catch (Exception e)
            //                        {
            //                            Console.WriteLine($"Could not get summary of player {playerStats.UniqueId}: {e.Message}");
            //                            continue;
            //                        }

            //                        IReadOnlyCollection<PlayerBansModel> playerBans;
            //                        try
            //                        {
            //                            playerBans = await this.steamService.GetBans(playerStats.UniqueId);
            //                        }
            //                        catch (Exception e)
            //                        {
            //                            Console.WriteLine($"Could not get bans of player {playerStats.UniqueId}: {e.Message}");
            //                            continue;
            //                        }

            //                        int vacCount = 0;
            //                        foreach (PlayerBansModel playerBan in playerBans)
            //                        {
            //                            vacCount += (int)playerBan.NumberOfVACBans;
            //                        }

            //                        MapWorkshopModel? bestMap = null;
            //                        if (playerStats.BestMap != null)
            //                        {
            //                            bestMap = this.steamWorkshopService.GetMapDetail(long.Parse(playerStats.BestMap[3..]));
            //                        }

            //                        if (serverStatMode?.Value == "SND")
            //                        {
            //                            playerStatsContent.Add(playerStats, this.createStatsCard($"player-{playerStats.UniqueId}", playerSummary.ProfileUrl, true, playerSummary.AvatarFullUrl, playerSummary.Nickname, new Dictionary<string, string>
            //                                {
            //                                    {
            //                                        "K/D ratio", $"{Math.Round((double) playerStats.Kills / playerStats.Deaths, 1):0.0}"
            //                                    },
            //                                    {
            //                                        "Kills", $"{playerStats.Kills} ({Math.Round(this.calculateSafePercent(playerStats.Kills, totalKills), 1)}%)"
            //                                    },
            //                                    {
            //                                        "Deaths", $"{playerStats.Deaths} ({Math.Round(this.calculateSafePercent(playerStats.Deaths, totalDeaths), 1)}%)"
            //                                    },
            //                                    {
            //                                        "Assists", $"{playerStats.Assists} ({Math.Round(this.calculateSafePercent(playerStats.Assists, totalAssists), 1)}%)"
            //                                    },
            //                                    {
            //                                        "Team kills", $"{playerStats.TeamKills} ({Math.Round(this.calculateSafePercent(playerStats.TeamKills, playerStats.Kills), 1)}%<a href=\"stats/{server.ServerId}#asterix-own-kills\" onclick=\"scrollToId('asterix-own-kills'); return false;\">*</a>)"
            //                                    },
            //                                    {
            //                                        "HS kills", $"{playerStats.Headshots} ({Math.Round(this.calculateSafePercent(playerStats.Headshots, playerStats.Kills), 1)}%<a href=\"stats/{server.ServerId}#asterix-own-kills\" onclick=\"scrollToId('asterix-own-kills'); return false;\">*</a>)"
            //                                    },
            //                                    {
            //                                        "Suicides", playerStats.Suicides.ToString()
            //                                    },
            //                                    {
            //                                        "Avg. points", $"{Math.Round(playerStats.AverageScore, 0)}"
            //                                    },
            //                                    {
            //                                        "Total points", $"{playerStats.TotalScore} ({Math.Round(this.calculateSafePercent(playerStats.TotalScore, totalScore), 1)}%)"
            //                                    },
            //                                    {
            //                                        "Bombs", $"{playerStats.BombsPlanted} planted, {playerStats.BombsDefused} defused"
            //                                    },
            //                                    {
            //                                        "Rounds played", $"{playerStats.RoundsPlayed}"
            //                                    },
            //                                    {
            //                                        "Best gun", $"{(playerStats.MostKillsWithGun == null ? "None" : $@"<a href=""stats/{server.ServerId}#gun-{playerStats.MostKillsWithGun}"" onclick=""scrollToId('gun-{playerStats.MostKillsWithGun}'); return false;"">{(getCorrectGunKey(playerStats.MostKillsWithGun) is {} gunKey ? PavlovStatisticsService.GunMap[gunKey] : playerStats.MostKillsWithGun)}</a><br />{playerStats.MostKillsWithGunAmount} kills ({Math.Round(this.calculateSafePercent(playerStats.MostKillsWithGunAmount, playerStats.Kills), 1)}%<a href=""stats/{server.ServerId}#asterix-own-kills"" onclick=""scrollToId('asterix-own-kills'); return false;"">*</a>)")}"
            //                                    },
            //                                    {
            //                                        "Best map", $"{(playerStats.BestMap == null ? "None" : $@"<a href=""stats/{server.ServerId}#map-{playerStats.BestMap}-{playerStats.BestMapGameMode}"" onclick=""scrollToId('map-{playerStats.BestMap}-{playerStats.BestMapGameMode}'); return false;"">{bestMap!.Name}</a><br />{Math.Round(playerStats.BestMapAverageScore, 0)} avg score")}"
            //                                    },
            //                                    {
            //                                        "VAC", $"{(vacCount > 0 ? $@"<span class=""text-danger"">Yes, {vacCount}" : @"<span class=""text-success"">No")}</span>"
            //                                    },
            //                                }));
            //                        }
            //                        else if (serverStatMode?.Value == "EFP")
            //                        {
            //                            int cash = 0;
            //                            try
            //                            {
            //                                int.TryParse(await pterodactylService.ReadFile(this.configuration["pterodactyl_stats_apikey"], server.ServerId, $"/Pavlov/Saved/Config/ModSave/{playerStats.UniqueId}.txt"), out cash);
            //                            }
            //                            catch { }
            //                            playerStats.TotalScore = cash;

            //                            playerStatsContent.Add(playerStats, this.createStatsCard($"player-{playerStats.UniqueId}", playerSummary.ProfileUrl, true, playerSummary.AvatarFullUrl, $"{playerSummary.Nickname}{(string.IsNullOrEmpty(playerSummary.CountryCode) ? "" : $"<img src=\"https://countryflagsapi.com/png/{playerSummary.CountryCode}\" alt=\"{playerSummary.CountryCode}\" height=\"16\" class=\"ms-2\"/>")}", new Dictionary<string, string>
            //                                {
            //                                    {
            //                                        "Cash", $"${cash}"
            //                                    },
            //                                    {
            //                                        "K/D ratio", $"{Math.Round((double) playerStats.Kills / playerStats.Deaths, 1):0.0}"
            //                                    },
            //                                    {
            //                                        "Kills", $"{playerStats.Kills} ({Math.Round(this.calculateSafePercent(playerStats.Kills, totalKills), 1)}%)"
            //                                    },
            //                                    {
            //                                        "Deaths", $"{playerStats.Deaths} ({Math.Round(this.calculateSafePercent(playerStats.Deaths, totalDeaths), 1)}%)"
            //                                    },
            //                                    {
            //                                        "HS kills", $"{playerStats.Headshots} ({Math.Round(this.calculateSafePercent(playerStats.Headshots, playerStats.Kills), 1)}%<a href=\"stats/{server.ServerId}#asterix-own-kills\" onclick=\"scrollToId('asterix-own-kills'); return false;\">*</a>)"
            //                                    },
            //                                    {
            //                                        "Suicides", playerStats.Suicides.ToString()
            //                                    },
            //                                    {
            //                                        "Best gun", $"{(playerStats.MostKillsWithGun == null ? "None" : $@"<a href=""stats/{server.ServerId}#gun-{playerStats.MostKillsWithGun}"" onclick=""scrollToId('gun-{playerStats.MostKillsWithGun}'); return false;"">{(getCorrectGunKey(playerStats.MostKillsWithGun) is {} gunKey ? PavlovStatisticsService.GunMap[gunKey] : playerStats.MostKillsWithGun)}</a><br />{playerStats.MostKillsWithGunAmount} kills ({Math.Round(this.calculateSafePercent(playerStats.MostKillsWithGunAmount, playerStats.Kills), 1)}%<a href=""stats/{server.ServerId}#asterix-own-kills"" onclick=""scrollToId('asterix-own-kills'); return false;"">*</a>)")}"
            //                                    },
            //                                    {
            //                                        "VAC", $"{(vacCount > 0 ? $@"<span class=""text-danger"">Yes, {vacCount}" : @"<span class=""text-success"">No")}</span>"
            //                                    },
            //                                }));
            //                        }
            //                        else
            //                        {
            //                            playerStatsContent.Add(playerStats, this.createStatsCard($"player-{playerStats.UniqueId}", playerSummary.ProfileUrl, true, playerSummary.AvatarFullUrl, $"{playerSummary.Nickname}{(string.IsNullOrEmpty(playerSummary.CountryCode) ? "" : $"<img src=\"https://countryflagsapi.com/png/{playerSummary.CountryCode}\" alt=\"{playerSummary.CountryCode}\" height=\"16\" class=\"ms-2\"/>")}", new Dictionary<string, string>
            //                                {
            //                                    {
            //                                        "K/D ratio", $"{Math.Round((double) playerStats.Kills / playerStats.Deaths, 1):0.0}"
            //                                    },
            //                                    {
            //                                        "Kills", $"{playerStats.Kills} ({Math.Round(this.calculateSafePercent(playerStats.Kills, totalKills), 1)}%)"
            //                                    },
            //                                    {
            //                                        "Deaths", $"{playerStats.Deaths} ({Math.Round(this.calculateSafePercent(playerStats.Deaths, totalDeaths), 1)}%)"
            //                                    },
            //                                    {
            //                                        "HS kills", $"{playerStats.Headshots} ({Math.Round(this.calculateSafePercent(playerStats.Headshots, playerStats.Kills), 1)}%<a href=\"stats/{server.ServerId}#asterix-own-kills\" onclick=\"scrollToId('asterix-own-kills'); return false;\">*</a>)"
            //                                    },
            //                                    {
            //                                        "Suicides", playerStats.Suicides.ToString()
            //                                    },
            //                                    {
            //                                        "Best gun", $"{(playerStats.MostKillsWithGun == null ? "None" : $@"<a href=""stats/{server.ServerId}#gun-{playerStats.MostKillsWithGun}"" onclick=""scrollToId('gun-{playerStats.MostKillsWithGun}'); return false;"">{(getCorrectGunKey(playerStats.MostKillsWithGun) is {} gunKey ? PavlovStatisticsService.GunMap[gunKey] : playerStats.MostKillsWithGun)}</a><br />{playerStats.MostKillsWithGunAmount} kills ({Math.Round(this.calculateSafePercent(playerStats.MostKillsWithGunAmount, playerStats.Kills), 1)}%<a href=""stats/{server.ServerId}#asterix-own-kills"" onclick=""scrollToId('asterix-own-kills'); return false;"">*</a>)")}"
            //                                    },
            //                                    {
            //                                        "VAC", $"{(vacCount > 0 ? $@"<span class=""text-danger"">Yes, {vacCount}" : @"<span class=""text-success"">No")}</span>"
            //                                    },
            //                                }));
            //                        }
            //                    }

            //                }

            //                serverStatsBuilder.AppendLine(@"<h4>Honorable mentions</h3>
            //        <div class=""card-group d-flex flex-wrap"">");

            //                List<string> honorableMentionsCards = new();

            //                string? highestKDR = await this.getPlayerCardWith(server, playerStatsContent.Keys.ToList(), p => p.Kills > 10 ? (double)p.Kills / (p.Deaths == 0 ? 1 : p.Deaths) : 0, p => p.Kills, 1, false, "Highest K/D ratio", "K/D ratio");
            //                if (highestKDR != null)
            //                {
            //                    honorableMentionsCards.Add(highestKDR);
            //                }

            //                string? mostKills = await this.getPlayerCardWith(server, playerStatsContent.Keys.ToList(), p => p.Kills, p => p.TotalScore, 0, false, "Most kills", "Kills");
            //                if (mostKills != null)
            //                {
            //                    honorableMentionsCards.Add(mostKills);
            //                }

            //                if (serverStatMode?.Value == "SND")
            //                {
            //                    string? mostAssists = await this.getPlayerCardWith(server, playerStatsContent.Keys.ToList(), p => p.Assists, p => p.TotalScore, 0, false, "Most assists", "Assists");
            //                    if (mostAssists != null)
            //                    {
            //                        honorableMentionsCards.Add(mostAssists);
            //                    }


            //                    string? highestTotalScore = await this.getPlayerCardWith(server, playerStatsContent.Keys.ToList(), p => p.TotalScore, p => p.Kills, 0, false, "Highest total score", "Score");
            //                    if (highestTotalScore != null)
            //                    {
            //                        honorableMentionsCards.Add(highestTotalScore);
            //                    }

            //                    string? highestAverageScore = await this.getPlayerCardWith(server, playerStatsContent.Keys.ToList(), p => p.AverageScore, p => p.TotalScore, 0, false, "Highest average score", "Score");
            //                    if (highestAverageScore != null)
            //                    {
            //                        honorableMentionsCards.Add(highestAverageScore);
            //                    }

            //                    string? mostPlants = await this.getPlayerCardWith(server, playerStatsContent.Keys.ToList(), p => p.BombsPlanted, p => p.TotalScore, 0, false, "Most bomb plants", "Plants");
            //                    if (mostPlants != null)
            //                    {
            //                        honorableMentionsCards.Add(mostPlants);
            //                    }

            //                    string? mostDefuses = await this.getPlayerCardWith(server, playerStatsContent.Keys.ToList(), p => p.BombsDefused, p => p.TotalScore, 0, false, "Most bomb defuses", "Defuses");
            //                    if (mostDefuses != null)
            //                    {
            //                        honorableMentionsCards.Add(mostDefuses);
            //                    }
            //                }

            //                string? mostHeadshots = await this.getPlayerCardWith(server, playerStatsContent.Keys.ToList(), p => p.Headshots, p => p.Kills, 0, false, "Most headshot kills", "HS kills");
            //                if (mostHeadshots != null)
            //                {
            //                    honorableMentionsCards.Add(mostHeadshots);
            //                }

            //                string? highestHSKR = await this.getPlayerCardWith(server, playerStatsContent.Keys.ToList(), p => p.Kills > 10 ? (double)p.Headshots / p.Kills : 0, p => p.Kills, 1, false, "Highest HS-kill to kill ratio", "HSKR");
            //                if (highestHSKR != null)
            //                {
            //                    honorableMentionsCards.Add(highestHSKR);
            //                }

            //                serverStatsBuilder.AppendLine(this.splitInRows(honorableMentionsCards.ToArray()));

            //                serverStatsBuilder.AppendLine(@"</div><br/>
            //        <h4>Dishonorable mentions</h3>
            //        <div class=""card-group d-flex flex-wrap"">");

            //                List<string> dishonorableMentionsCards = new();

            //                string? mostDeaths = await this.getPlayerCardWith(server, playerStatsContent.Keys.ToList(), p => p.Deaths, p => p.TotalScore, 0, false, "Most deaths", "Deaths");
            //                if (mostDeaths != null)
            //                {
            //                    dishonorableMentionsCards.Add(mostDeaths);
            //                }

            //                if (serverStatMode?.Value == "SND")
            //                {
            //                    string? mostTeamKills = await this.getPlayerCardWith(server, playerStatsContent.Keys.ToList(), p => p.TeamKills, p => p.Kills, 0, false, "Most teamkills", "Teamkills");
            //                    if (mostTeamKills != null)
            //                    {
            //                        dishonorableMentionsCards.Add(mostTeamKills);
            //                    }
            //                }

            //                string? lowestHSKR = await this.getPlayerCardWith(server, playerStatsContent.Keys.ToList(), p => p.Kills > 10 ? (double)p.Headshots / p.Kills : double.MaxValue, p => p.Kills, 1, true, "Lowest HS-kill to kill ratio", "HSKR");
            //                if (lowestHSKR != null)
            //                {
            //                    dishonorableMentionsCards.Add(lowestHSKR);
            //                }

            //                string? mostSuicides = await this.getPlayerCardWith(server, playerStatsContent.Keys.ToList(), p => p.Kills > 10 ? p.Suicides : 0, p => p.Kills, 0, false, "Most suicides", "Suicides");
            //                if (mostSuicides != null)
            //                {
            //                    dishonorableMentionsCards.Add(mostSuicides);
            //                }

            //                serverStatsBuilder.AppendLine(this.splitInRows(dishonorableMentionsCards.ToArray()));

            //                serverStatsBuilder.AppendLine(@"</div><br/>
            //        <h4>All players</h3>
            //        <div class=""card-group d-flex flex-wrap"">");

            //                serverStatsBuilder.AppendLine(this.splitInRows(playerStatsContent.OrderByDescending(kvp => kvp.Key.TotalScore).ThenByDescending(kvp => kvp.Key.Kills).Select(kvp => kvp.Value).ToArray()));
            //                serverStatsBuilder.AppendLine("</div>");
            //            }


            //            serverStatsBuilder.AppendLine(@$"

            //    </div>
            //        <div>
            //            <h3 id=""asterix-own-kills"" class=""mt-3"">Percentages marked with *</h3>
            //            <p>Percentages marked with * are calculated using own amounts (eg. own kills), not total kill count.<br />
            //            Unmarked percentages are calculated using total amounts (eg. total kills).</p>
            //            <h3 class=""mt-3"">Score</h3>
            //            <p>Score is calculated using the following formula:<br />
            //            Kills * {StatsCalculator.SCORE_WEIGHT_KILL} +<br />
            //            Deaths * {StatsCalculator.SCORE_WEIGHT_DEATH} +<br />
            //            Assists * {StatsCalculator.SCORE_WEIGHT_ASSIST} +<br />
            //            Headshots * {StatsCalculator.SCORE_WEIGHT_HEADSHOT} +<br />
            //            Teamkills * {StatsCalculator.SCORE_WEIGHT_TEAMKILL} +<br />
            //            Bomb Plants * {StatsCalculator.SCORE_WEIGHT_PLANT} +<br />
            //            Bomb Defuses * {StatsCalculator.SCORE_WEIGHT_DEFUSE}</p>
            //            <h3>Some values don't seem to add up</h3>
            //            <p>You may notice that some values don't seem to add up, like Team Blue Kills + Team Red Kills is not equal to Total Kills. This is caused because some statistics have filters applied, like only counting matches with at least two players or with a combined Team Blue and Team Red score of at least 10 (draws and skipped maps). Prerounds often are omitted as well. Total kills however counts each single kill.</p>
            //        </div>
            //<script>
            //    function scrollToId(id) {{
            //        const element = document.getElementById(id);
            //        if (element instanceof HTMLElement) {{
            //            element.scrollIntoView({{
            //                behavior: ""smooth"",
            //                block: ""start"",
            //                inline: ""nearest""
            //            }});
            //        }}
            //    }}
            //</script>
            //    <footer class=""text-center text-lg-start bg-dark text-muted"">
            //      <section class=""d-flex justify-content-center justify-content-lg-between p-4 border-bottom"">
            //        Stats updated on {DateTime.UtcNow.ToLongDateString()} {DateTime.UtcNow.ToLongTimeString()} UTC.<br />
            //        Stats are updated every eight hours.
            //      </section>
            //      <div class=""text-center p-4"">
            //        Provided by: 
            //        <a class=""text-reset fw-bold"" href=""https://codefreak.net/"">codefreak.net</a> for <a class=""text-reset fw-bold"" href=""https://bloodisgood.net/"">Blood is Good</a>
            //      </div>
            //    </footer>");
            //            await File.WriteAllTextAsync($"stats/{server.ServerId}.html", serverStatsBuilder.ToString());
        }

        Console.WriteLine("Stats written");
    }

    //private string splitInRows(string[] cards)
    //{
    //    if (cards.Length == 0)
    //    {
    //        return string.Empty;
    //    }

    //    StringBuilder rowCards = new();

    //    for (int i = 0; i < cards.Length; i++)
    //    {
    //        if (i == 0)
    //        {
    //            rowCards.AppendLine("<div class=\"row mt-3\">");
    //        }
    //        else if (i % PavlovStatisticsService.cardsPerRow == 0)
    //        {
    //            rowCards.AppendLine("</div>");
    //            rowCards.AppendLine("<div class=\"row mt-3\">");
    //        }

    //        rowCards.AppendLine(cards[i]);
    //    }

    //    rowCards.AppendLine("</div>");

    //    return rowCards.ToString();
    //}

    //private async Task<string?> getPlayerCardWith(PterodactylServerModel server, List<CPlayerStats> playerStats, Func<CPlayerStats, double> selector, Func<CPlayerStats, double> secondSort, int round, bool invert, string title, string statName)
    //{
    //    CPlayerStats? highestValuePlayer = playerStats.OrderByDescending(p => selector(p) * (invert ? -1 : 1)).ThenByDescending(secondSort).FirstOrDefault();
    //    if (highestValuePlayer == null)
    //    {
    //        return null;
    //    }

    //    PlayerSummaryModel? playerSummary = null;
    //    try
    //    {
    //        playerSummary = await this.steamService.GetPlayerSummary(highestValuePlayer.UniqueId);
    //    }
    //    catch (Exception e)
    //    {
    //        Console.WriteLine($"Could not get summary of player {highestValuePlayer.UniqueId}: {e.Message}");
    //        return null;
    //    }

    //    double highestValue = selector(highestValuePlayer);

    //    return this.createStatsCard(null, $"stats/{server.ServerId}#player-{highestValuePlayer.UniqueId}", false, playerSummary.AvatarFullUrl, title, new Dictionary<string, string>
    //    {
    //        {
    //            "Player", $"<a href=\"stats/{server.ServerId}#player-{highestValuePlayer.UniqueId}\" onclick=\"scrollToId('player-{highestValuePlayer.UniqueId}'); return false;\">{playerSummary.Nickname}</a>"
    //        },
    //        {
    //            statName, Math.Round(highestValue == -0 ? 0 : highestValue, round).ToString($"0{(round == 0 ? "" : ".".PadRight(round + 1, '0'))}")
    //        },
    //    });
    //}

    //private double calculateSafePercent(double a, double b)
    //{
    //    return a * 100d / (b == 0 ? 1 : b);
    //}

    //private string createStatsCard(string? id, string? link, bool openInBlank, string imageURL, string title, Dictionary<string, string> values)
    //{
    //    string onClick = string.Empty;
    //    if (link?.StartsWith("#") ?? false)
    //    {
    //        onClick = $"  onclick=\"scrollToId('{link[1..]}'); return false;\"";
    //    }
    //    return @$"<div class=""col""><div class=""card bg-dark h-100"" style=""width: {PavlovStatisticsService.cardWidth}px"" {(id == null ? "" : $@"id=""{id}""")}>
    //            {(link == null ? "" : $@"<a href=""{link}""{(openInBlank ? " target=\"_blank\"" : "")}{onClick}>")}
    //                <img class=""card-img-top"" src=""{imageURL}"" width=""{PavlovStatisticsService.cardWidth}"" height=""{PavlovStatisticsService.cardWidth}"" />
    //            {(link == null ? "" : "</a>")}
    //            <div class=""card-body px-0"">
    //                <h5 class=""card-title px-3"">{(link == null ? "" : $@"<a href=""{link}""{(openInBlank ? " target=\"blank\"" : "")}{onClick}>")}{title}{(link == null ? "" : @"</a>")}</h5>

    //                <p class=""card-text"">
    //                    <div class=""container px-0"">
    //                        {string.Join(Environment.NewLine, values.Select(kvp => $"<div class=\"row row-alternating gx-0\"><div class=\"col-auto ps-3 pe-1\"><b>{kvp.Key}:</b></div><div class=\"col text-end ps-1 pe-3\">{kvp.Value}</div></div>"))}
    //                    </div>
    //                </p>
    //            </div>
    //        </div></div>";
    //}


    private Dictionary<string, object> getServerCountStats(CBaseStats[] allStats, string serverStatsType)
    {
        Console.WriteLine("Generating server count stats");

        Dictionary<string, object> serverCountStats = new();

        CServerStats serverStats = allStats.OfType<CServerStats>().First();

        if (serverStatsType == "SND")
        {
            serverCountStats.Add("Unique maps/modes", serverStats.TotalUniqueMaps.ToString());
            serverCountStats.Add("Total matches", serverStats.TotalMatchesPlayed.ToString());
        }
        else if (serverStatsType == "EFP")
        {
            serverCountStats.Add("Total cash", allStats.OfType<CEFPPlayerCashModel>().Sum(s => s.Cash).ToString());
        }
        serverCountStats.Add("Unique players", serverStats.TotalUniquePlayers.ToString());

        return serverCountStats;
    }

    private Dictionary<string, object> getServerKillStats(CBaseStats[] allStats, string serverStatsType)
    {
        Console.WriteLine("Generating server kill stats");

        Dictionary<string, object> serverKillStats = new();

        CServerStats serverStats = allStats.OfType<CServerStats>().First();

        serverKillStats.Add("Total kills", serverStats.TotalKills.ToString());
        serverKillStats.Add("Total headshots", serverStats.TotalHeadshots.ToString());

        if (serverStatsType == "SND")
        {
            serverKillStats.Add("Total assists", serverStats.TotalAssists.ToString());
            serverKillStats.Add("Total teamkills", serverStats.TotalTeamkills.ToString());
        }

        return serverKillStats;
    }

    private Dictionary<string, object> getServerBombStats(CBaseStats[] allStats, string serverStatsType)
    {
        Console.WriteLine("Generating server bomb stats");

        Dictionary<string, object> serverBombStats = new();

        CServerStats serverStats = allStats.OfType<CServerStats>().First();

        if (serverStatsType == "SND")
        {
            serverBombStats.Add("Total plants", serverStats.TotalBombPlants.ToString());
            serverBombStats.Add("Total defuses", serverStats.TotalBombDefuses.ToString());
            serverBombStats.Add("Total explosions", serverStats.TotalBombExplosions.ToString());
        }

        return serverBombStats;
    }

    private IEnumerable<CMapStats> getMaps(CBaseStats[] allStats, string serverStatsType)
    {
        return allStats.OfType<CMapStats>().OrderByDescending(m => m.PlayCount);
    }

    private async Task<Dictionary<string, object>> getMapStats(CMapStats mapStats)
    {
        Console.WriteLine($"Generating server map stats for {mapStats.MapId}");

        string? bestPlayerUsername = null;
        if (mapStats.BestPlayer != null)
        {
            try
            {
                bestPlayerUsername = await this.steamService.GetUsername(mapStats.BestPlayer.Value);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not get username of player id {mapStats.BestPlayer.Value}: {ex.Message}");
            }
        }

        Dictionary<string, object> mapStatValues = new();
        mapStatValues.Add("Played", $"{mapStats.PlayCount} time{(mapStats.PlayCount != 1 ? "s" : "")}");
        mapStatValues.Add("Wins", $"Blue {mapStats.Team0Wins}, Red {mapStats.Team1Wins}");
        mapStatValues.Add("Rounds", $"{Math.Round(mapStats.AverageRounds, 2)} avg, {mapStats.MaxRounds} max, {mapStats.MinRounds} min");
        if (mapStats.BestPlayer != null)
        {
            mapStatValues.Add("Best player", new StatsLinkModel($"player-{mapStats.BestPlayer}", bestPlayerUsername ?? mapStats.BestPlayer.Value.ToString(), $"{Math.Round(mapStats.MaxAveragePlayerScore, 0)} avg score"));
        }

        return mapStatValues;
    }

    private IEnumerable<CGunStats> getGuns(CBaseStats[] allStats, string serverStatsType)
    {
        return allStats.OfType<CGunStats>().OrderByDescending(g => g.Kills);
    }

    private async Task<Dictionary<string, object>> getGunStats(CGunStats gunStats, CBaseStats[] allStats, string serverStatsType)
    {
        Console.WriteLine($"Generating server gun stats for {gunStats.Name}");

        CServerStats serverStats = allStats.OfType<CServerStats>().First();

        string gunName = $"{gunStats.Name}(?)";
        string? gunKey = GetCorrectGunKey(gunStats.Name);
        if (gunKey != null)
        {
            gunName = PavlovStatisticsService.GunMap[gunKey];
        }

        string? bestPlayerUsername = null;
        if (gunStats.BestPlayer != null)
        {
            try
            {
                bestPlayerUsername = await this.steamService.GetUsername(gunStats.BestPlayer.Value);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not get username of player id {gunStats.BestPlayer.Value}: {ex.Message}");
            }
        }

        Dictionary<string, object> gunStatValues = new();
        gunStatValues.Add("Kills", $"{gunStats.Kills} ({Math.Round(this.calculateSafePercent(gunStats.Kills, serverStats.TotalKills), 1)}%)");
        gunStatValues.Add("Headshots", new StatsOwnPercentageModel(gunStats.Headshots.ToString(), Math.Round(this.calculateSafePercent(gunStats.Headshots, gunStats.Kills), 1)));
        if (gunStats.BestPlayer != null)
        {
            gunStatValues.Add("Best player", new StatsLinkModel($"player-{gunStats.BestPlayer}", bestPlayerUsername ?? gunStats.BestPlayer.Value.ToString(), new StatsOwnPercentageModel($"{gunStats.BestPlayerKills} kills", Math.Round(this.calculateSafePercent(gunStats.BestPlayerKills, gunStats.Kills), 1))));
        }

        return gunStatValues;
    }

    private async Task<Dictionary<string, object>> getTeamStats(int teamId, CBaseStats[] allStats, string serverStatsType)
    {
        Console.WriteLine($"Generating server team stats for {teamId}");

        CServerStats serverStats = allStats.OfType<CServerStats>().First();
        CTeamStats teamStats = allStats.OfType<CTeamStats>().First(t => t.TeamId == teamId);

        string? bestPlayerUsername = null;
        if (teamStats.BestPlayer != null)
        {
            try
            {
                bestPlayerUsername = await this.steamService.GetUsername(teamStats.BestPlayer.Value);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not get username of player id {(teamStats.BestPlayer.HasValue ? teamStats.BestPlayer.Value.ToString() : "N/A")}: {ex.Message}");
            }
        }

        Dictionary<string, object> teamStatValues = new();
        teamStatValues.Add("Kills", $"{teamStats.TotalKills} ({Math.Round(this.calculateSafePercent(teamStats.TotalKills, serverStats.TotalKills), 1)}%)");
        teamStatValues.Add("HS kills", new StatsOwnPercentageModel(teamStats.TotalHeadshots.ToString(), Math.Round(this.calculateSafePercent(teamStats.TotalHeadshots, teamStats.TotalKills), 1)));
        teamStatValues.Add("Assists", $"{teamStats.TotalAssists} ({Math.Round(this.calculateSafePercent(teamStats.TotalAssists, serverStats.TotalAssists), 1)}%)");
        teamStatValues.Add("Teamkills", $"{teamStats.TotalTeamkills} ({Math.Round(this.calculateSafePercent(teamStats.TotalTeamkills, serverStats.TotalTeamkills), 1)}%)");
        teamStatValues.Add("Victories", teamStats.TotalVictories.ToString());

        if (teamStats.BestPlayer != null)
        {
            teamStatValues.Add("Best player", new StatsLinkModel($"player-{teamStats.BestPlayer}", bestPlayerUsername ?? teamStats.BestPlayer.Value.ToString(), $"{Math.Round(teamStats.BestPlayerAverageScore, 0)} avg score"));
        }

        if (teamStats.BestGun != null)
        {
            string? gunKey = GetCorrectGunKey(teamStats.BestGun);
            string gunName = teamStats.BestGun;
            if (gunKey != null)
            {
                gunName = PavlovStatisticsService.GunMap[gunKey];
            }

            teamStatValues.Add("Best gun", new StatsLinkModel($"gun-{teamStats.BestGun}", gunName, $"{teamStats.BestGunKillCount} kills {Math.Round(this.calculateSafePercent(teamStats.BestGunKillCount, serverStats.TotalKills), 1)}%)"));
        }

        return teamStatValues;
    }

    private IEnumerable<CPlayerStats> getPlayers(CBaseStats[] allStats, string serverStatsType)
    {
        if (serverStatsType == "SND")
        {
            return allStats.OfType<CPlayerStats>().OrderByDescending(p => p.TotalScore).ThenByDescending(p => p.Kills);
        }
        else if (serverStatsType == "EFP")
        {
            List<CEFPPlayerCashModel> efpPlayerCash = allStats.OfType<CEFPPlayerCashModel>().ToList();
            return allStats.OfType<CPlayerStats>().OrderByDescending(p => efpPlayerCash.FirstOrDefault(c => c.UniqueId == p.UniqueId)?.Cash ?? 0).ThenByDescending(p => p.Kills);
        }
        else
        {
            return allStats.OfType<CPlayerStats>().OrderByDescending(p => p.Kills);
        }
    }

    private async Task<Dictionary<string, object>> getPlayerStats(CPlayerStats playerStats, CBaseStats[] allStats, string serverStatsType)
    {
        Console.WriteLine($"Generating server player stats for {playerStats.UniqueId}");

        CServerStats serverStats = allStats.OfType<CServerStats>().First();
        CPlayerStats[] allPlayerStats = allStats.OfType<CPlayerStats>().ToArray();

        PlayerSummaryModel? playerSummary = null;
        try
        {
            playerSummary = await this.steamService.GetPlayerSummary(playerStats.UniqueId);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Could not get summary of player {playerStats.UniqueId}: {e.Message}");
        }

        IReadOnlyCollection<PlayerBansModel>? playerBans = null;
        try
        {
            playerBans = await this.steamService.GetBans(playerStats.UniqueId);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Could not get bans of player {playerStats.UniqueId}: {e.Message}");
        }

        int vacCount = 0;
        if (playerBans != null)
        {
            foreach (PlayerBansModel playerBan in playerBans)
            {
                vacCount += (int)playerBan.NumberOfVACBans;
            }
        }

        string? bestGunName = playerStats.MostKillsWithGun;
        string? bestGunKey = null;

        if (bestGunName != null)
        {
            bestGunKey = GetCorrectGunKey(bestGunName);
        }

        if (bestGunKey != null)
        {
            bestGunName = PavlovStatisticsService.GunMap[bestGunKey];
        }

        int totalKills = allPlayerStats.Sum(p => p.Kills);
        int totalDeaths = allPlayerStats.Sum(p => p.Deaths);
        int totalAssists = allPlayerStats.Sum(p => p.Assists);
        int totalScore = allPlayerStats.Sum(p => p.TotalScore);

        Dictionary<string, object> playerStatValues = new();

        if (serverStatsType == "EFP")
        {
            int cash = 0;
            if (allStats.OfType<CEFPPlayerCashModel>().FirstOrDefault(c => c.UniqueId == playerStats.UniqueId) is CEFPPlayerCashModel playerCash)
            {
                cash = playerCash.Cash;
            }
            playerStatValues.Add("Cash", $"${cash}");
        }

        playerStatValues.Add("K/D ratio", $"{Math.Round((double)playerStats.Kills / playerStats.Deaths, 1):0.0}");
        playerStatValues.Add("Kills", $"{playerStats.Kills} ({Math.Round(this.calculateSafePercent(playerStats.Kills, totalKills), 1)}%)");
        playerStatValues.Add("Deaths", $"{playerStats.Deaths} ({Math.Round(this.calculateSafePercent(playerStats.Deaths, totalKills), 1)}%)");
        if (serverStatsType == "SND")
        {
            playerStatValues.Add("Assists", $"{playerStats.Assists} ({Math.Round(this.calculateSafePercent(playerStats.Assists, totalAssists), 1)}%)");
            playerStatValues.Add("Team kills", new StatsOwnPercentageModel(playerStats.TeamKills.ToString(), Math.Round(this.calculateSafePercent(playerStats.TeamKills, playerStats.Kills), 1)));
        }
        playerStatValues.Add("HS kills", new StatsOwnPercentageModel(playerStats.Headshots.ToString(), Math.Round(this.calculateSafePercent(playerStats.Headshots, playerStats.Kills), 1)));
        playerStatValues.Add("Suicides", playerStats.Suicides.ToString());

        if (serverStatsType == "SND")
        {
            playerStatValues.Add("Avg. points", $"{Math.Round(playerStats.AverageScore, 0)}");
            playerStatValues.Add("Total points", $"{playerStats.TotalScore} ({Math.Round(this.calculateSafePercent(playerStats.TotalScore, totalScore), 1)}%)");
            playerStatValues.Add("Bombs", $"{playerStats.BombsPlanted} planted, {playerStats.BombsDefused} defused");
            playerStatValues.Add("Rounds played", $"{playerStats.RoundsPlayed}");
        }

        if (bestGunName != null)
        {
            playerStatValues.Add("Best gun", new StatsLinkModel($"gun-{playerStats.MostKillsWithGun}", bestGunName, new StatsOwnPercentageModel($"{playerStats.MostKillsWithGunAmount} kills", Math.Round(this.calculateSafePercent(playerStats.MostKillsWithGunAmount, playerStats.Kills), 1))));
        }

        if (serverStatsType == "SND")
        {
            MapWorkshopModel? bestMap = null;
            if (playerStats.BestMap != null)
            {
                try
                {
                    bestMap = this.steamWorkshopService.GetMapDetail(long.Parse(playerStats.BestMap[3..]));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not get map detail for {playerStats.BestMap}: {ex.Message}");
                }
            }

            if (bestMap != null)
            {
                playerStatValues.Add("Best map", new StatsLinkModel($"map-{playerStats.BestMap}-{playerStats.BestMapGameMode}", bestMap.Name, $"{Math.Round(playerStats.BestMapAverageScore, 0)} avg score"));
            }
        }

        if (playerSummary != null && playerSummary.CountryCode != null)
        {
            playerStatValues.Add("Country", new StatsImageModel($"https://countryflagsapi.com/png/{playerSummary.CountryCode}", playerSummary.CountryCode));
        }

        if (playerBans != null)
        {
            if (vacCount > 0)
            {
                playerStatValues.Add("VAC", new StatsColoredTextModel($"Yes, {vacCount}", "text-danger"));
            }
            else
            {
                playerStatValues.Add("VAC", new StatsColoredTextModel("No", "text-success"));
            }
        }

        return playerStatValues;
    }

    public static string? GetCorrectGunKey(string gunName)
    {
        return PavlovStatisticsService.GunMap.Keys.FirstOrDefault(k => string.Equals(k, gunName, StringComparison.CurrentCultureIgnoreCase));
    }

    private double calculateSafePercent(double a, double b)
    {
        return a * 100d / (b == 0 ? 1 : b);
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
                    Console.WriteLine(ex);
                }

                sw.Stop();
                Console.WriteLine($"Stats completed in {sw.ElapsedMilliseconds}ms");
                lastGenerated = DateTime.Now;
            }

            Thread.Sleep(1000);
        }
    }
}
