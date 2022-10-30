using Newtonsoft.Json;
using PterodactylPavlovServerController.Exceptions;
using Steam.Models.SteamCommunity;
using SteamWebAPI2.Interfaces;
using SteamWebAPI2.Utilities;

namespace PterodactylPavlovServerController.Services
{
    public class SteamService
    {
        private readonly IConfiguration configuration;
        private readonly SteamWebInterfaceFactory factory;

        private Dictionary<ulong, (DateTime lastUpdated, PlayerSummaryModel playerSummary)> playerSummariesCache;
        private Dictionary<ulong, (DateTime lastUpdated, IReadOnlyCollection<PlayerBansModel> playerBans)> playerBansCache;

        private static String lastRequestLock = String.Empty;
        private static DateTime lastRequest = DateTime.MinValue;

        public SteamService(IConfiguration configuration)
        {
            this.configuration = configuration;

            factory = new(configuration["steam_apikey"]);

            try
            {
                playerSummariesCache = JsonConvert.DeserializeObject<Dictionary<ulong, (DateTime lastUpdated, PlayerSummaryModel playerSummary)>>(File.ReadAllText(configuration["steam_summarycache"])) ?? new Dictionary<ulong, (DateTime lastUpdated, PlayerSummaryModel playerSummary)>();
            }
            catch (Exception)
            {
                playerSummariesCache = new Dictionary<ulong, (DateTime lastUpdated, PlayerSummaryModel playerSummary)>();
            }

            try
            {
                playerBansCache = JsonConvert.DeserializeObject<Dictionary<ulong, (DateTime lastUpdated, IReadOnlyCollection<PlayerBansModel> playerBans)>>(File.ReadAllText(configuration["steam_bancache"])) ?? new Dictionary<ulong, (DateTime lastUpdated, IReadOnlyCollection<PlayerBansModel> playerBans)>();
            }
            catch (Exception)
            {
                playerBansCache = new Dictionary<ulong, (DateTime lastUpdated, IReadOnlyCollection<PlayerBansModel> playerBans)>();
            }
        }

        public PlayerSummaryModel GetPlayerSummary(ulong steamId)
        {
            DateTime? lastUpdated = null;
            PlayerSummaryModel? playerSummary = null;
            lock (playerSummariesCache)
            {
                if (playerSummariesCache.ContainsKey(steamId))
                {
                    (lastUpdated, playerSummary) = playerSummariesCache[steamId];
                }
            }

            if (!lastUpdated.HasValue || lastUpdated.Value < DateTime.Now.AddDays(-7))
            {
                lock (lastRequestLock)
                {
                    if (lastRequest < DateTime.Now.AddSeconds(-2))
                    {
                        Thread.Sleep(2000);
                    }
                    lastRequest = DateTime.Now;
                }

                SteamUser user = factory.CreateSteamWebInterface<SteamUser>();
                ISteamWebResponse<PlayerSummaryModel> response = user.GetPlayerSummaryAsync(steamId).GetAwaiter().GetResult();

                if (response.Data is null)
                {
                    throw new SteamException();
                }

                playerSummary = response.Data;
                lastUpdated = DateTime.Now;

                lock (playerSummariesCache)
                {
                    if (playerSummariesCache.ContainsKey(steamId))
                    {
                        playerSummariesCache.Remove(steamId);
                    }

                    playerSummariesCache.Add(steamId, (lastUpdated.Value, playerSummary));
                    File.WriteAllText(configuration["steam_summarycache"], JsonConvert.SerializeObject(playerSummariesCache));
                }
            }

            lock (playerSummariesCache)
            {
                return playerSummariesCache[steamId].playerSummary;
            }
        }

        private IReadOnlyCollection<PlayerBansModel> getPlayerBans(ulong steamId)
        {
            DateTime? lastUpdated = null;
            IReadOnlyCollection<PlayerBansModel>? playerBans = null;

            lock (playerBansCache)
            {
                if (playerBansCache.ContainsKey(steamId))
                {
                    (lastUpdated, playerBans) = playerBansCache[steamId];
                }
            }

            if (!lastUpdated.HasValue || lastUpdated.Value < DateTime.Now.AddDays(-7))
            {
                lock (lastRequestLock)
                {
                    if (lastRequest < DateTime.Now.AddSeconds(-2))
                    {
                        Thread.Sleep(2000);
                    }
                    lastRequest = DateTime.Now;
                }

                SteamUser user = factory.CreateSteamWebInterface<SteamUser>();
                ISteamWebResponse<IReadOnlyCollection<PlayerBansModel>> response = user.GetPlayerBansAsync(steamId).GetAwaiter().GetResult();

                if (response.Data is null)
                {
                    throw new SteamException();
                }

                playerBans = response.Data;
                lastUpdated = DateTime.Now;

                lock (playerBansCache)
                {
                    if (playerBansCache.ContainsKey(steamId))
                    {
                        playerBansCache.Remove(steamId);
                    }

                    playerBansCache.Add(steamId, (lastUpdated.Value, playerBans));
                    File.WriteAllText(configuration["steam_bancache"], JsonConvert.SerializeObject(playerBansCache));
                }
            }

            lock (playerBansCache)
            {
                return playerBansCache[steamId].playerBans;
            }
        }

        public string GetUsername(ulong steamId)
        {
            return GetPlayerSummary(steamId).Nickname;
        }

        public IReadOnlyCollection<PlayerBansModel> GetBans(ulong steamId)
        {
            return getPlayerBans(steamId);
        }
    }
}
