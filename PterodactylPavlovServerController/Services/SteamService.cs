using Newtonsoft.Json;
using PterodactylPavlovServerController.Exceptions;
using Steam.Models.SteamCommunity;
using SteamWebAPI2.Interfaces;
using SteamWebAPI2.Utilities;

namespace PterodactylPavlovServerController.Services;

public class SteamService
{
    private static readonly string lastRequestLock = string.Empty;
    private static DateTime lastRequest = DateTime.MinValue;
    private readonly IConfiguration configuration;
    private readonly SteamWebInterfaceFactory factory;
    private readonly Dictionary<ulong, (DateTime lastUpdated, IReadOnlyCollection<PlayerBansModel> playerBans)> playerBansCache;

    private readonly Dictionary<ulong, (DateTime lastUpdated, PlayerSummaryModel playerSummary)> playerSummariesCache;

    public SteamService(IConfiguration configuration)
    {
        this.configuration = configuration;

        this.factory = new SteamWebInterfaceFactory(configuration["steam_apikey"]);

        try
        {
            this.playerSummariesCache = JsonConvert.DeserializeObject<Dictionary<ulong, (DateTime lastUpdated, PlayerSummaryModel playerSummary)>>(File.ReadAllText(configuration["steam_summarycache"]!)) ?? new Dictionary<ulong, (DateTime lastUpdated, PlayerSummaryModel playerSummary)>();
        }
        catch (Exception)
        {
            this.playerSummariesCache = new Dictionary<ulong, (DateTime lastUpdated, PlayerSummaryModel playerSummary)>();
        }

        try
        {
            this.playerBansCache = JsonConvert.DeserializeObject<Dictionary<ulong, (DateTime lastUpdated, IReadOnlyCollection<PlayerBansModel> playerBans)>>(File.ReadAllText(configuration["steam_bancache"]!)) ?? new Dictionary<ulong, (DateTime lastUpdated, IReadOnlyCollection<PlayerBansModel> playerBans)>();
        }
        catch (Exception)
        {
            this.playerBansCache = new Dictionary<ulong, (DateTime lastUpdated, IReadOnlyCollection<PlayerBansModel> playerBans)>();
        }
    }

    public async Task<PlayerSummaryModel> GetPlayerSummary(ulong steamId)
    {
        DateTime? lastUpdated = null;
        PlayerSummaryModel? playerSummary = null;
        lock (this.playerSummariesCache)
        {
            if (this.playerSummariesCache.ContainsKey(steamId))
            {
                (lastUpdated, playerSummary) = this.playerSummariesCache[steamId];
            }
        }

        if (!lastUpdated.HasValue || lastUpdated.Value < DateTime.Now.AddDays(-7))
        {
            lock (SteamService.lastRequestLock)
            {
                if (SteamService.lastRequest < DateTime.Now.AddSeconds(-2))
                {
                    Thread.Sleep(2000);
                }

                SteamService.lastRequest = DateTime.Now;
            }

            SteamUser user = this.factory.CreateSteamWebInterface<SteamUser>();
            ISteamWebResponse<PlayerSummaryModel> response = await user.GetPlayerSummaryAsync(steamId);

            if (response.Data is null)
            {
                throw new SteamException();
            }

            playerSummary = response.Data;
            lastUpdated = DateTime.Now;

            lock (this.playerSummariesCache)
            {
                if (this.playerSummariesCache.ContainsKey(steamId))
                {
                    this.playerSummariesCache.Remove(steamId);
                }

                this.playerSummariesCache.Add(steamId, (lastUpdated.Value, playerSummary));
                File.WriteAllText(this.configuration["steam_summarycache"]!, JsonConvert.SerializeObject(this.playerSummariesCache));
            }
        }

        lock (this.playerSummariesCache)
        {
            return this.playerSummariesCache[steamId].playerSummary;
        }
    }

    private async Task<IReadOnlyCollection<PlayerBansModel>> getPlayerBans(ulong steamId)
    {
        DateTime? lastUpdated = null;
        IReadOnlyCollection<PlayerBansModel>? playerBans = null;

        lock (this.playerBansCache)
        {
            if (this.playerBansCache.ContainsKey(steamId))
            {
                (lastUpdated, playerBans) = this.playerBansCache[steamId];
            }
        }

        if (!lastUpdated.HasValue || lastUpdated.Value < DateTime.Now.AddDays(-7))
        {
            lock (SteamService.lastRequestLock)
            {
                if (SteamService.lastRequest < DateTime.Now.AddSeconds(-2))
                {
                    Thread.Sleep(2000);
                }

                SteamService.lastRequest = DateTime.Now;
            }

            SteamUser user = this.factory.CreateSteamWebInterface<SteamUser>();
            ISteamWebResponse<IReadOnlyCollection<PlayerBansModel>> response = await user.GetPlayerBansAsync(steamId);

            if (response.Data is null)
            {
                throw new SteamException();
            }

            playerBans = response.Data;
            lastUpdated = DateTime.Now;

            lock (this.playerBansCache)
            {
                if (this.playerBansCache.ContainsKey(steamId))
                {
                    this.playerBansCache.Remove(steamId);
                }

                this.playerBansCache.Add(steamId, (lastUpdated.Value, playerBans));
                File.WriteAllText(this.configuration["steam_bancache"]!, JsonConvert.SerializeObject(this.playerBansCache));
            }
        }

        lock (this.playerBansCache)
        {
            return this.playerBansCache[steamId].playerBans;
        }
    }

    public async Task<string> GetUsername(ulong steamId)
    {
        return (await this.GetPlayerSummary(steamId)).Nickname;
    }

    public async Task<IReadOnlyCollection<PlayerBansModel>> GetBans(ulong steamId)
    {
        return await this.getPlayerBans(steamId);
    }
}
