using Newtonsoft.Json;
using PterodactylPavlovServerController.Exceptions;
using PterodactylPavlovServerDomain.Models;
using RestSharp;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace PterodactylPavlovServerController.Services;

public class PterodactylService
{
    private readonly IConfiguration configuration;
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly RestClient restClient;
    private readonly Dictionary<string, JsonElement[]> startupVariableCache = new();

    public PterodactylService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        this.configuration = configuration;
        this.httpContextAccessor = httpContextAccessor;
        this.restClient = new RestClient($"{configuration["pterodactyl_baseurl"]}/api/");
        //restClient.AddDefaultHeader("Authorization", $"Bearer {configuration["pterodactyl_bearertoken"]}");
    }

    public string ReadFile(string apiKey, string serverId, string path)
    {
        string fileContent = null!;

        RestResponse readFileResponse = this.executeRestRequest(apiKey, $"client/servers/{serverId}/files/contents?file={path}", false);
        if (readFileResponse.IsSuccessStatusCode)
        {
            fileContent = readFileResponse.Content!;
        }
        else if (readFileResponse.StatusCode == HttpStatusCode.BadRequest && readFileResponse.Content!.Contains("too large to view"))
        {
            bool success = false;
            while (!success)
            {
                try
                {
                    string downloadUrl = this.DownloadFileUrl(apiKey, serverId, path);

                    HttpClient httpClient = new();
                    HttpResponseMessage httpResponse = httpClient.Send(new HttpRequestMessage(HttpMethod.Get, downloadUrl));
                    if (!httpResponse.IsSuccessStatusCode)
                    {
                        throw new Exception($"Could not download large file: {httpResponse.StatusCode}: {httpResponse.Content!}");
                    }

                    using (StreamReader reader = new(httpResponse.Content.ReadAsStream()))
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

    public string DownloadFileUrl(string apiKey, string serverId, string path)
    {
        RestResponse readFileResponse = this.executeRestRequest(apiKey, $"client/servers/{serverId}/files/download?file={path}");

        return JsonDocument.Parse(readFileResponse.Content!).RootElement.GetProperty("attributes").GetProperty("url").ToString();
    }

    public void WriteFile(string apiKey, string serverId, string path, string content)
    {
        RestRequest textBodyRequest = new($"client/servers/{serverId}/files/write?file={path}", Method.Post);
        _ = textBodyRequest.AddHeader("Content-Type", "text/plain");

        _ = this.executeRestRequest(apiKey, textBodyRequest, content);
    }

    private RestResponse executeRestRequest(string apiKey, string path, bool throwError = true)
    {
        return this.executeRestRequest(apiKey, new RestRequest(path), throwError: throwError);
    }

    private RestResponse executeRestRequest(string apiKey, RestRequest restRequest, string? content = null, bool throwError = true)
    {
        _ = restRequest.AddHeader("Authorization", $"Bearer {apiKey}");

        if (content is not null)
        {
            _ = restRequest.AddBody(content);
        }

        RestResponse restResponse = this.restClient.Execute(restRequest);

        if (!restResponse.IsSuccessStatusCode && throwError)
        {
            throw new Exception($"Received error code {restResponse.StatusCode} from Pterodactyl.");
        }

        return restResponse;
    }

    public string GetHost(string apiKey, string serverId)
    {
        return JsonDocument.Parse(this.executeRestRequest(apiKey, $"client/servers/{serverId}").Content!).RootElement.GetProperty("attributes").GetProperty("relationships").GetProperty("allocations").GetProperty("data").EnumerateArray().First(o => o.GetProperty("object").GetString() == "allocation").GetProperty("attributes").GetProperty("ip").GetString()!;
    }

    public string GetStartupVariable(string apiKey, string serverId, string envVariable)
    {
        if (!this.startupVariableCache.ContainsKey(serverId))
        {
            lock (this.startupVariableCache)
            {
                this.startupVariableCache.Add(serverId, JsonDocument.Parse(this.executeRestRequest(apiKey, $"client/servers/{serverId}/startup").Content!).RootElement.GetProperty("data").EnumerateArray().ToArray());
            }
        }

        return this.startupVariableCache[serverId].Select(v => v.GetProperty("attributes")).FirstOrDefault(v => v.GetProperty("env_variable").GetString() == envVariable).GetProperty("server_value").GetString() ?? throw new StartupVariableNotFoundException(envVariable);
    }

    public PterodactylServerModel[] GetServers(string apiKey)
    {
        return JsonDocument.Parse(this.executeRestRequest(apiKey, "client?include=egg").Content!).RootElement.GetProperty("data").EnumerateArray().Select(e => e.GetProperty("attributes")).Where(a => a.GetProperty("relationships").GetProperty("egg").GetProperty("attributes").GetProperty("name").GetString()?.Equals("Pavlov VR") ?? false).Select(a => new PterodactylServerModel
        {
            Name = a.GetProperty("name").GetString() ?? "-unnamed-",
            ServerId = a.GetProperty("identifier").GetString() ?? "-invalid-",
        }).ToArray();
    }

    public PterodactylFile[] GetFileList(string apiKey, string serverId, string directory)
    {
        return JsonConvert.DeserializeObject<PterodactylFile[]>(new JsonArray(JsonDocument.Parse(this.executeRestRequest(apiKey, $"client/servers/{serverId}/files/list?directory={directory}").Content!).RootElement.GetProperty("data").EnumerateArray().Where(o => o.GetProperty("object").GetString() == "file_object" && o.GetProperty("attributes").GetProperty("is_file").GetBoolean()).ToArray().Select(e => JsonNode.Parse(e.GetProperty("attributes").ToString())!).ToArray()).ToString())!;
    }
}
