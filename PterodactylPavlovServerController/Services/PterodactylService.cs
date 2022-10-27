using PterodactylPavlovServerController.Exceptions;
using PterodactylPavlovServerController.Models;
using RestSharp;
using System.Text.Json;

namespace PterodactylPavlovServerController.Services
{
    public class PterodactylService
    {
        private readonly IConfiguration configuration;
        private readonly IHttpContextAccessor httpContextAccessor;
        private Dictionary<string, JsonElement[]> startupVariableCache = new Dictionary<string, JsonElement[]>();
        private readonly RestClient restClient;

        public PterodactylService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            this.configuration = configuration;
            this.httpContextAccessor = httpContextAccessor;
            restClient = new RestClient($"{configuration["pterodactyl_baseurl"]}/api/");
            //restClient.AddDefaultHeader("Authorization", $"Bearer {configuration["pterodactyl_bearertoken"]}");
        }

        public string ReadFile(string serverId, string path)
        {
            RestResponse readFileResponse = executeRestRequest($"client/servers/{serverId}/files/contents?file={path}");

            return readFileResponse.Content!;
        }

        public void WriteFile(string serverId, string path, string content)
        {
            RestRequest textBodyRequest = new RestRequest($"client/servers/{serverId}/files/write?file={path}");
            textBodyRequest.Method = Method.Post;
            textBodyRequest.AddHeader("Content-Type", "text/plain");

            executeRestRequest(textBodyRequest, content);
        }

        private RestResponse executeRestRequest(string path)
        {
            return executeRestRequest(new RestRequest(path));
        }

        private RestResponse executeRestRequest(RestRequest restRequest, string? content = null)
        {
            restRequest.AddHeader("Authorization", $"Bearer {httpContextAccessor.HttpContext!.Request.Headers["x-pterodactyl-api-key"]}");

            if (content is not null)
            {
                restRequest.AddBody(content);
            }

            RestResponse restResponse = restClient.Execute(restRequest);

            if (restResponse.StatusCode is not System.Net.HttpStatusCode.OK and not System.Net.HttpStatusCode.NoContent)
            {
                throw new Exception($"Received error code {restResponse.StatusCode} from Pterodactyl.");
            }

            return restResponse;
        }

        public string GetHost(string serverId)
        {
            return JsonDocument.Parse(executeRestRequest($"client/servers/{serverId}").Content!).RootElement.GetProperty("attributes").GetProperty("relationships").GetProperty("allocations").GetProperty("data").EnumerateArray().First(o => o.GetProperty("object").GetString() == "allocation").GetProperty("attributes").GetProperty("ip").GetString()!;
        }

        public string GetStartupVariable(string serverId, string envVariable)
        {
            if (!startupVariableCache.ContainsKey(serverId))
            {
                startupVariableCache.Add(serverId, JsonDocument.Parse(executeRestRequest($"client/servers/{serverId}/startup").Content!).RootElement.GetProperty("data").EnumerateArray().ToArray());
            }

            return startupVariableCache[serverId].Select(v => v.GetProperty("attributes")).FirstOrDefault(v => v.GetProperty("env_variable").GetString() == envVariable).GetProperty("server_value").GetString() ?? throw new StartupVariableNotFoundException(envVariable);
        }

        public PterodactylServerModel[] GetServers()
        {
            return JsonDocument.Parse(executeRestRequest("client?include=egg").Content!).RootElement.GetProperty("data").EnumerateArray().Select(e => e.GetProperty("attributes")).Where(a => a.GetProperty("relationships").GetProperty("egg").GetProperty("attributes").GetProperty("name").GetString()?.Equals("Pavlov VR") ?? false).Select(a => new PterodactylServerModel() { Name = a.GetProperty("name").GetString() ?? "-unnamed-", ServerId = a.GetProperty("identifier").GetString() ?? "-invalid-" }).ToArray();
        }
    }
}
