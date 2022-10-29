using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PterodactylPavlovRconClient.Models;
using PterodactylPavlovServerController.Models;
using Steam.Models.SteamCommunity;
using System.Text.Json;

namespace PterodactylPavlovRconClient.Services
{
    public class PterodactylAPIService
    {
        HttpClient httpClient;
        private readonly ILogger logger;

        public PterodactylAPIService(ILogger logger)
        {
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(Properties.Settings.Default.ppsc_basepath);
            httpClient.DefaultRequestHeaders.Add("x-api-key", Properties.Settings.Default.ppsc_apikey);
            httpClient.DefaultRequestHeaders.Add("x-pterodactyl-api-key", Properties.Settings.Default.ppsc_pterodactyl_key);
            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            this.logger = logger;
        }

        private ApiResponse send(string url)
        {
            return send(url, HttpMethod.Get);
        }

        private ApiResponse send(string url, HttpMethod method)
        {
            ApiResponse response;
            using (HttpResponseMessage responseMessage = sendMessage(url, method))
            {
                (response, string messageContent) = readMessageContent(responseMessage);
                responseMessage.RequestMessage?.Dispose();
            }

            return response;
        }

        private ApiResponse<T> send<T>(string url)
        {
            return send<T>(url, HttpMethod.Get);
        }

        private ApiResponse<T> send<T>(string url, HttpMethod method)
        {
            T? data;
            using (HttpResponseMessage responseMessage = sendMessage(url, method))
            {
                (ApiResponse response, string messageContent) = readMessageContent(responseMessage);
                if (!response.Success)
                {
                    responseMessage.RequestMessage?.Dispose();
                    return new(false, response.ErrorMessage, default(T));
                }

                data = JsonConvert.DeserializeObject<T>(messageContent);
                if (data == null)
                {
                    responseMessage.RequestMessage?.Dispose();
                    return new(false, $"Unable to deserialize message to type {typeof(T).Name}.", default(T));
                }

                responseMessage.RequestMessage?.Dispose();
            }

            return new(true, null, data);
        }

        private HttpResponseMessage sendMessage(string url, HttpMethod method)
        {
            return httpClient.Send(new HttpRequestMessage(method, url));
        }

        private (ApiResponse response, string content) readMessageContent(HttpResponseMessage message)
        {
            string messageContent = readMessage(message);

            if (message.IsSuccessStatusCode)
            {
                logger.LogDebug($"Request to {message.RequestMessage?.RequestUri} successful. Response message content:{Environment.NewLine}{messageContent}");
                return (new(true, null), messageContent);
            }

            if (String.IsNullOrWhiteSpace(messageContent))
            {
                if (message.StatusCode != 0)
                {
                    logger.LogError($"Request to {message.RequestMessage?.RequestUri} failed with status code {message.StatusCode}.");
                    return (new(false, message.StatusCode.ToString()), messageContent);
                }

                logger.LogError($"Request to {message.RequestMessage?.RequestUri} could not be executed.");
            }

            string? errorDetail = JsonDocument.Parse(messageContent).RootElement.GetProperty("detail").GetString();
            if (errorDetail == null)
            {
                logger.LogError($"Request to {message.RequestMessage?.RequestUri} failed, but no error detail is given. Response message content:{Environment.NewLine}{messageContent}");
                return (new(false, messageContent), messageContent);
            }

            logger.LogError($"Request to {message.RequestMessage?.RequestUri} failed. Error detail: {errorDetail}");
            return (new(false, errorDetail), messageContent);
        }

        private string readMessage(HttpResponseMessage message)
        {
            using (StreamReader streamReader = new StreamReader(message.Content.ReadAsStream()))
            {
                return streamReader.ReadToEnd();
            }
        }

        public ApiResponse<PterodactylServerModel[]> GetServers() => send<PterodactylServerModel[]>("server/list");
        public ApiResponse<ServerInfoModel> GetServerInfo(string serverId) => send<ServerInfoModel>($"rcon/serverinfo?serverId={serverId}");
        public ApiResponse<PlayerListPlayerModel[]> GetPlayerList(string serverId) => send<PlayerListPlayerModel[]>($"rcon/playerlist?serverId={serverId}");
        public ApiResponse<string[]> GetBanlist(string serverId) => send<string[]>($"rcon/banlist?serverId={serverId}");
        public ApiResponse<string> GetSteamUsername(string steamId) => send<string>($"steam/username?steamid={steamId}");
        public ApiResponse<MapRowModel[]> GetMapRotation(string serverId) => send<MapRowModel[]>($"maps/current?serverId={serverId}");
        public ApiResponse<MapDetailModel> GetMapDetails(long mapId) => send<MapDetailModel>($"maps/details?mapId={mapId}");
        public ApiResponse DoSwitchMap(string serverId, string mapId, string gameMode) => send($"rcon/switchMap?serverId={serverId}&mapId={mapId}&gamemode={gameMode}", HttpMethod.Post);
        public ApiResponse DoRotateMap(string serverId) => send($"rcon/rotateMap?serverId={serverId}", HttpMethod.Post);
        public ApiResponse<PlayerBansModel[]> GetPlayerBans(string steamId) => send<PlayerBansModel[]>($"steam/bans?steamId={steamId}");
        public ApiResponse<PlayerModel> GetPlayerInfo(string serverId, string steamId) => send<PlayerModel>($"rcon/player?serverId={serverId}&uniqueId={steamId}");
        public ApiResponse DoKickPlayer(string serverId, string steamId) => send($"rcon/kick?serverId={serverId}&uniqueId={steamId}", HttpMethod.Post);
        public ApiResponse DoUnbanPlayer(string serverId, string steamId) => send($"rcon/unban?serverId={serverId}&uniqueId={steamId}", HttpMethod.Post);
        public ApiResponse DoBanPlayer(string serverId, string steamId) => send($"rcon/ban?serverId={serverId}&uniqueId={steamId}", HttpMethod.Post);
        internal ApiResponse DoGiveItem(string serverId, string uniqueId, string itemName) => send($"rcon/giveItem?serverId={serverId}&uniqueId={uniqueId}&item={itemName}");
        internal ApiResponse DoGiveCash(string serverId, string uniqueId, int amount) => send($"rcon/giveCash?serverId={serverId}&uniqueId={uniqueId}&amount={amount}");
        internal ApiResponse DoGiveVehicle(string serverId, string uniqueId, string vehicleName) => send($"rcon/giveVehicle?serverId={serverId}&uniqueId={uniqueId}&vehicle={vehicleName}");
        internal ApiResponse DoSetSkin(string serverId, string uniqueId, string skinName) => send($"rcon/setSkin?serverId={serverId}&uniqueId={uniqueId}&skin={skinName}");
        internal ApiResponse DoSlap(string serverId, string uniqueId, int amount) => send($"rcon/slap?serverId={serverId}&uniqueId={uniqueId}&amount={amount}");
        internal ApiResponse DoSwitchTeam(string serverId, string uniqueId, int targetTeam) => send($"rcon/giveItem?serverId={serverId}&uniqueId={uniqueId}&team={targetTeam}");
    }

    public class ApiResponse
    {
        public bool Success { get; }
        public string? ErrorMessage { get; }

        public ApiResponse(bool success, string? errorMessage)
        {
            this.Success = success;
            this.ErrorMessage = errorMessage;
        }
    }

    public class ApiResponse<T> : ApiResponse
    {
        public T? Data { get; }

        public ApiResponse(bool success, string? errorMessage, T? data) : base(success, errorMessage)
        {
            this.Data = data;
        }
    }
}
