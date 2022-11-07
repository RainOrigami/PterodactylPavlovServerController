using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using PterodactylPavlovServerController.Exceptions;
using PterodactylPavlovServerDomain.Models;
using RestSharp;
using System.Text.Json;
using System.Text.Json.Nodes;

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
            string fileContent = null!;

            RestResponse readFileResponse = executeRestRequest($"client/servers/{serverId}/files/contents?file={path}", false);
            if (readFileResponse.IsSuccessStatusCode)
            {
                fileContent = readFileResponse.Content!;
            }
            else if (readFileResponse.StatusCode == System.Net.HttpStatusCode.BadRequest && readFileResponse.Content!.Contains("too large to view"))
            {
                bool success = false;
                while (!success)
                {
                    try
                    {
                        string downloadUrl = DownloadFileUrl(serverId, path);

                        HttpClient httpClient = new HttpClient();
                        HttpResponseMessage httpResponse = httpClient.Send(new HttpRequestMessage(HttpMethod.Get, downloadUrl));
                        if (!httpResponse.IsSuccessStatusCode)
                        {
                            throw new Exception($"Could not download large file: {httpResponse.StatusCode}: {httpResponse.Content!}");
                        }

                        using (StreamReader reader = new StreamReader(httpResponse.Content.ReadAsStream()))
                        {
                            fileContent = reader.ReadToEnd();
                        }

                        success = true;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
            else
            {
                throw new Exception($"Could not download file: {readFileResponse.StatusCode}: {readFileResponse.Content!}");
            }

            return fileContent;
        }

        public string DownloadFileUrl(string serverId, string path)
        {
            RestResponse readFileResponse = executeRestRequest($"client/servers/{serverId}/files/download?file={path}");

            return JsonDocument.Parse(readFileResponse.Content!).RootElement.GetProperty("attributes").GetProperty("url").ToString();
        }

        public void WriteFile(string serverId, string path, string content)
        {
            RestRequest textBodyRequest = new RestRequest($"client/servers/{serverId}/files/write?file={path}", Method.Post);
            textBodyRequest.AddHeader("Content-Type", "text/plain");

            executeRestRequest(textBodyRequest, content);
        }

        private RestResponse executeRestRequest(string path, bool throwError = true)
        {
            return executeRestRequest(new RestRequest(path), throwError: throwError);
        }

        private RestResponse executeRestRequest(RestRequest restRequest, string? content = null, bool throwError = true)
        {
            string apiKey;
            if (httpContextAccessor.HttpContext?.Request?.Headers?.TryGetValue("x-pterodactyl-api-key", out StringValues apiKeyValues) ?? false && apiKeyValues.Count == 1)
            {
                apiKey = apiKeyValues.First();
            }
            else
            {
                apiKey = configuration["pterodactyl_apikey"];
            }
            restRequest.AddHeader("Authorization", $"Bearer {apiKey}");

            if (content is not null)
            {
                restRequest.AddBody(content);
            }

            RestResponse restResponse = restClient.Execute(restRequest);

            if (!restResponse.IsSuccessStatusCode && throwError)
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
                lock (startupVariableCache)
                {
                    startupVariableCache.Add(serverId, JsonDocument.Parse(executeRestRequest($"client/servers/{serverId}/startup").Content!).RootElement.GetProperty("data").EnumerateArray().ToArray());
                }
            }

            return startupVariableCache[serverId].Select(v => v.GetProperty("attributes")).FirstOrDefault(v => v.GetProperty("env_variable").GetString() == envVariable).GetProperty("server_value").GetString() ?? throw new StartupVariableNotFoundException(envVariable);
        }

        public PterodactylServerModel[] GetServers()
        {
            return JsonDocument.Parse(executeRestRequest("client?include=egg").Content!).RootElement.GetProperty("data").EnumerateArray().Select(e => e.GetProperty("attributes")).Where(a => a.GetProperty("relationships").GetProperty("egg").GetProperty("attributes").GetProperty("name").GetString()?.Equals("Pavlov VR") ?? false).Select(a => new PterodactylServerModel() { Name = a.GetProperty("name").GetString() ?? "-unnamed-", ServerId = a.GetProperty("identifier").GetString() ?? "-invalid-" }).ToArray();
        }

        public PterodactylFile[] GetFileList(string serverId, string directory)
        {
            return JsonConvert.DeserializeObject<PterodactylFile[]>(new JsonArray(JsonDocument.Parse(executeRestRequest($"client/servers/{serverId}/files/list?directory={directory}").Content!).RootElement.GetProperty("data").EnumerateArray().Where(o => o.GetProperty("object").GetString() == "file_object" && o.GetProperty("attributes").GetProperty("is_file").GetBoolean()).ToArray().Select(e => JsonNode.Parse(e.GetProperty("attributes").ToString())!).ToArray()).ToString())!;
        }
    }
}
