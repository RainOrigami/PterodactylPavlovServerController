using BlazorTemplater;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;
using PavlovStatsReader;
using PavlovStatsReader.Models;
using PterodactylPavlovServerController.Contexts;
using PterodactylPavlovServerController.Models;
using PterodactylPavlovServerController.Pages.Stats;
using PterodactylPavlovServerDomain.Models;
using Steam.Models.SteamCommunity;
using Steam.Models.Utilities;
using System.Diagnostics;
using System.Globalization;
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
        },
        {
            "fall", "Suicide by gravity"
        },
        {
            "grenade_us", "Explosive grenade (US)"
        },
        {
            "sten", "STEN"
        },
        {
            "tripalarm", "Trip mine"
        }
    };

    private static readonly Regex fileNameDateTimeRegex = new(@"(?<date>\d{4}\.\d{2}\.\d{2}-\d{2}\.\d{2}\.\d{2})", RegexOptions.Compiled);

    private readonly IConfiguration configuration;
    private readonly PavlovServerService pavlovServerService;
    private readonly CountryService countryService;
    private readonly PterodactylService pterodactylService;
    private readonly StatsCalculator statsCalculator;

    private readonly CancellationTokenSource statsCancellationTokenSource = new();
    private readonly StatsContext statsContext;
    private readonly SteamService steamService;
    private readonly SteamWorkshopService steamWorkshopService;

    public PavlovStatisticsService(IConfiguration configuration, StatsContext statsContext, PterodactylService pterodactylService, SteamService steamService, StatsCalculator statsCalculator, SteamWorkshopService steamWorkshopService, PavlovServerService pavlovServerService, CountryService countryService)
    {
        this.configuration = configuration;
        this.statsContext = statsContext;
        this.pterodactylService = pterodactylService;
        this.steamService = steamService;
        this.statsCalculator = statsCalculator;
        this.steamWorkshopService = steamWorkshopService;
        this.pavlovServerService = pavlovServerService;
        this.countryService = countryService;
        statsContext.Database.Migrate();
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
            templateRenderer.Set(m => m.ServerName, await pavlovServerService.GetServerName(this.configuration["pterodactyl_stats_apikey"], server.ServerId));

            string serverStatsType = "UNSET";
            Setting? serverStatMode = this.statsContext.Settings.FirstOrDefault(s => s.Name == "Stat Type" && s.ServerId == server.ServerId);
            if (serverStatMode != null)
            {
                serverStatsType = serverStatMode.Value;
            }

            templateRenderer.Set(m => m.ServerStatsType, serverStatsType);

            Console.WriteLine("Calculating stats...");
            CBaseStats[] allStats = this.statsCalculator.CalculateStats(server.ServerId);

            // EFP Player Cash
            if (serverStatsType == "EFP")
            {
                Console.WriteLine("Getting EFP cash...");
                List<CBaseStats> baseStats = allStats.ToList();
                Regex cashFileRegex = new(@"^(?<UniqueId>\d+)\.txt$", RegexOptions.Compiled);
                baseStats.AddRange(pterodactylService.FileList(this.configuration["pterodactyl_stats_apikey"], server.ServerId, "/Pavlov/Saved/Config/ModSave/").Select(p => cashFileRegex.Match(p)).Where(r => r.Success).Select(r => r.Groups["UniqueId"].Value).AsParallel().Select(async p =>
                {
                    int cash = 0;
                    try
                    {
                        Console.WriteLine($"Cash of {p}...");
                        int.TryParse(await this.pterodactylService.ReadFile(this.configuration["pterodactyl_stats_apikey"], server.ServerId, $"/Pavlov/Saved/Config/ModSave/{p}.txt"), out cash);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                    return new CEFPPlayerCashModel(ulong.Parse(p), cash);
                }).Select(p => p.Result).ToArray());

                List<ulong> playerStats = baseStats.OfType<CPlayerStats>().Select(p => p.UniqueId).ToList();
                List<CEFPPlayerCashModel> cashStats = baseStats.OfType<CEFPPlayerCashModel>().ToList();
                foreach (CEFPPlayerCashModel cashModel in cashStats)
                {
                    if (playerStats.Contains(cashModel.UniqueId))
                    {
                        continue;
                    }

                    baseStats.Add(new CPlayerStats()
                    {
                        UniqueId = cashModel.UniqueId
                    });
                }

                allStats = baseStats.ToArray();
            }

            templateRenderer.Set(m => m.ServerCountStats, getServerCountStats(allStats, serverStatsType));
            templateRenderer.Set(m => m.ServerKillStats, getServerKillStats(allStats, serverStatsType));
            templateRenderer.Set(m => m.ServerBombStats, getServerBombStats(allStats, serverStatsType));

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
            int playerRank = 0;
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
                    Dictionary<string, object> statItems = new()
                    {
                        { "Rank", $"#{++playerRank}" }
                    };
                    statItems.AddRange(await getPlayerStats(playerStats, allStats, serverStatsType, server.ServerId));

                    playerStatistics.Add((playerSummary, playerStats, statItems));
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Uuuuh {e.Message}");
                }
            }
            templateRenderer.Set(m => m.PlayerStatistics, playerStatistics);

            templateRenderer.Set(m => m.HonorableMentions, await getHonorableMentions(allStats, serverStatsType, server.ServerId));
            templateRenderer.Set(m => m.DishonorableMentions, await getDishonorableMentions(allStats, serverStatsType));

            Console.WriteLine("Rendering...");
            await File.WriteAllTextAsync($"stats/{server.ServerId}.html", templateRenderer.Render());
            Console.WriteLine("Done!");
        }

        Console.WriteLine("Stats written");
    }

    private async Task<List<(string title, PlayerSummaryModel summary, Dictionary<string, object> items)>> getHonorableMentions(CBaseStats[] allStats, string serverStatsType, string serverId)
    {
        Console.WriteLine("Generating honorable player stats...");

        CPlayerStats[] playerStats = allStats.OfType<CPlayerStats>().ToArray();
        List<(string title, PlayerSummaryModel summary, Dictionary<string, object> items)?> honorableMentions = new()
        {
            await createPlayerMentionStat(playerStats, "Highest K/D ratio", "K/D ratio", p => p.Kills > 10 ? (double)p.Kills / (p.Deaths == 0 ? 1 : p.Deaths) : 0, p => p.Kills, false, (v,p) => $"{Math.Round(v, 1):0.0}"),
            await createPlayerMentionStat(playerStats, "Most kills", "Kills", p => p.Kills, p => p.TotalScore, false, (v,p) => $"{Math.Round(v, 0)}"),
            await createPlayerMentionStat(playerStats, "Most headshot kills", "HS kills", p => p.Headshots, p => p.Kills, false, (v,p) => $"{Math.Round(v, 0)}"),
            await createPlayerMentionStat(playerStats, "Highest HS-kill to kill ratio", "HSKR",  p => p.Kills > 10 ? (double)p.Headshots / p.Kills : 0, p => p.Kills, false, (v,p) => $"{Math.Round(v, 1):0.0}"),
        };

        if (serverStatsType == "SND")
        {
            honorableMentions.Add(await createPlayerMentionStat(playerStats, "Most assists", "Assists", p => p.Assists, p => p.TotalScore, false, (v, p) => $"{Math.Round(v, 0)}"));
            honorableMentions.Add(await createPlayerMentionStat(playerStats, "Highest total score", "Score", p => p.TotalScore, p => p.Kills, false, (v, p) => $"{Math.Round(v, 0)}"));
            honorableMentions.Add(await createPlayerMentionStat(playerStats, "Highest average score", "Score", p => p.AverageScore, p => p.TotalScore, false, (v, p) => $"{Math.Round(v, 0)}"));
            honorableMentions.Add(await createPlayerMentionStat(playerStats, "Most bomb plants", "Plants", p => p.BombsPlanted, p => p.TotalScore, false, (v, p) => $"{Math.Round(v, 0)}"));
            honorableMentions.Add(await createPlayerMentionStat(playerStats, "Most bomb defuses", "Defuses", p => p.BombsDefused, p => p.TotalScore, false, (v, p) => $"{Math.Round(v, 1)}"));
        }

        using PavlovServerContext pavlovServerContext = new(this.configuration);
        PersistentPavlovPlayerModel? mostTimeDbPlayer = await pavlovServerContext.Players.Where(p => p.ServerId == serverId).OrderByDescending(p => p.TotalTime).ThenByDescending(p => p.LastSeen).FirstOrDefaultAsync();
        if (mostTimeDbPlayer != null)
        {
            PlayerSummaryModel? playerSummary = await this.steamService.GetPlayerSummary(mostTimeDbPlayer.UniqueId);
            if (playerSummary != null)
            {
                honorableMentions.Add(("Most time on this server", playerSummary, new Dictionary<string, object>()
                {
                    { "Player", new StatsLinkModel($"player-{playerSummary.SteamId}", playerSummary.Nickname, null) },
                    { "Time", new StatsSinceDateModel(mostTimeDbPlayer.TotalTime.ToString("d\\.hh\\:mm\\:ss"), new DateTime(2022, 12, 12)) }
                }));
            }
        }

        return honorableMentions.Where(s => s.HasValue).Select(s => s!.Value).ToList();
    }

    private async Task<List<(string title, PlayerSummaryModel summary, Dictionary<string, object> items)>> getDishonorableMentions(CBaseStats[] allStats, string serverStatsType)
    {
        Console.WriteLine("Generating dishonorable player stats...");

        CPlayerStats[] playerStats = allStats.OfType<CPlayerStats>().ToArray();
        List<(string title, PlayerSummaryModel summary, Dictionary<string, object> items)?> dishonorableMentions = new()
        {
            await createPlayerMentionStat(playerStats, "Most deaths", "Deaths",p => p.Deaths, p => p.TotalScore, false, (v,p) => $"{Math.Round(v, 0)}"),
            await createPlayerMentionStat(playerStats, "Lowest HS-kill to kill ratio", "HSKR",p => p.Kills > 10 ? (double)p.Headshots / p.Kills : double.MaxValue, p => p.Kills, true, (v,p) => $"{Math.Round(v, 1):0.0} in {p.Kills} kills"),
            await createPlayerMentionStat(playerStats, "Most suicides", "Suicides",p => p.Kills > 10 ? p.Suicides : 0, p => p.Kills, false, (v,p) => $"{Math.Round(v, 0)}"),
        };

        if (serverStatsType == "SND")
        {
            dishonorableMentions.Add(await createPlayerMentionStat(playerStats, "Most teamkills", "Teamkills", p => p.TeamKills, p => p.Kills, false, (v, p) => $"{Math.Round(v, 0)}"));
        }

        return dishonorableMentions.Where(s => s.HasValue).Select(s => s!.Value).ToList();
    }

    private async Task<(string title, PlayerSummaryModel summary, Dictionary<string, object> items)?> createPlayerMentionStat(CPlayerStats[] playerStats, string title, string metric, Func<CPlayerStats, double> selector, Func<CPlayerStats, double> secondSort, bool invert, Func<double, CPlayerStats, string> valueFormatter)
    {
        if (getPlayerWith(playerStats, selector, secondSort, invert) is (CPlayerStats player, double value))
        {
            PlayerSummaryModel? playerSummary = await this.steamService.GetPlayerSummary(player.UniqueId);
            if (playerSummary != null)
            {
                return (title, playerSummary, new()
                {
                    { "Player", new StatsLinkModel($"player-{playerSummary.SteamId}", playerSummary.Nickname, null) },
                    { metric, valueFormatter(value, player) }
                });
            }
        }

        return null;
    }

    private (CPlayerStats player, double value)? getPlayerWith(CPlayerStats[] playerStats, Func<CPlayerStats, double> selector, Func<CPlayerStats, double> secondSort, bool invert)
    {
        CPlayerStats? highestValuePlayer = playerStats.OrderByDescending(p => selector(p) * (invert ? -1 : 1)).ThenByDescending(secondSort).FirstOrDefault();
        if (highestValuePlayer == null)
        {
            return null;
        }

        double highestValue = selector(highestValuePlayer);

        return (highestValuePlayer, highestValue == -0 ? 0 : highestValue);
    }

    private Dictionary<string, object> getServerCountStats(CBaseStats[] allStats, string serverStatsType)
    {
        Console.WriteLine("Generating server count stats");

        Dictionary<string, object> serverCountStats = new();

        CServerStats serverStats = allStats.OfType<CServerStats>().First();

        if (serverStatsType == "SND")
        {
            serverCountStats.Add("Unique maps/modes", serverStats.TotalUniqueMaps.ToString());
            serverCountStats.Add("Total matches", serverStats.TotalMatchesPlayed.ToString());
            serverCountStats.Add("Unique players", serverStats.TotalUniquePlayers.ToString());
        }
        else if (serverStatsType == "EFP")
        {
            serverCountStats.Add("Total cash", $"${ToKMB(allStats.OfType<CEFPPlayerCashModel>().Sum(s => s.Cash))}");
            serverCountStats.Add("Unique players", allStats.OfType<CEFPPlayerCashModel>().Count().ToString());
        }

        return serverCountStats;
    }

    public static string ToKMB(decimal num)
    {
        if (num > 999999999 || num < -999999999)
        {
            return num.ToString("0,,,.###B", CultureInfo.InvariantCulture);
        }
        else
        if (num > 999999 || num < -999999)
        {
            return num.ToString("0,,.##M", CultureInfo.InvariantCulture);
        }
        else
        if (num > 999 || num < -999)
        {
            return num.ToString("0,.#K", CultureInfo.InvariantCulture);
        }
        else
        {
            return num.ToString(CultureInfo.InvariantCulture);
        }
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

            teamStatValues.Add("Best gun", new StatsLinkModel($"gun-{teamStats.BestGun}", gunName, $"{teamStats.BestGunKillCount} kills ({Math.Round(this.calculateSafePercent(teamStats.BestGunKillCount, serverStats.TotalKills), 1)}%)"));
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

    private async Task<Dictionary<string, object>> getPlayerStats(CPlayerStats playerStats, CBaseStats[] allStats, string serverStatsType, string serverId)
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
            playerStatValues.Add("Cash", $"${ToKMB(cash)}");
        }

        if (playerStats.Deaths != 0 || playerStats.Kills != 0)
        {
            playerStatValues.Add("K/D ratio", $"{Math.Round((double)playerStats.Kills / playerStats.Deaths, 1):0.0}");
        }
        else
        {
            playerStatValues.Add("K/D ratio", $"N/A");
        }

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
            try
            {
                CountryModel countryModel = await this.countryService.GetCountry(playerSummary.CountryCode);
                playerStatValues.Add("Country", new StatsImageModel(countryModel.FlagUrl, countryModel.Name));
            }
            catch { }
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

        using PavlovServerContext pavlovServerContext = new(this.configuration);
        PersistentPavlovPlayerModel? dbPlayer = await pavlovServerContext.Players.SingleOrDefaultAsync(p => p.ServerId == serverId && p.UniqueId == playerStats.UniqueId);
        if (dbPlayer != null)
        {
            playerStatValues.Add("Last seen", dbPlayer.LastSeen.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"));
            playerStatValues.Add("Total time", new StatsSinceDateModel(dbPlayer.TotalTime.ToString("d\\.hh\\:mm\\:ss"), new DateTime(2022, 12, 12)));
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
                    Console.WriteLine(ex.ToString());
                }

                sw.Stop();
                Console.WriteLine($"Stats completed in {sw.ElapsedMilliseconds}ms");
                lastGenerated = DateTime.Now;
            }

            Thread.Sleep(1000);
        }
    }
}
