using Newtonsoft.Json;
using PterodactylPavlovServerController.Exceptions;
using PterodactylPavlovServerDomain.Models;
using RestSharp;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Web;

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
    }

    public async Task<string> ReadFile(string apiKey, string serverId, string path)
    {
        RestResponse readFileResponse = this.executeRestRequest(apiKey, $"client/servers/{serverId}/files/contents?file={path}", false);
        if (readFileResponse.IsSuccessStatusCode)
        {
            return readFileResponse.Content!;
        }

        if (readFileResponse.StatusCode != HttpStatusCode.BadRequest || !readFileResponse.Content!.Contains("too large to view"))
        {
            throw new Exception($"Could not download file: {readFileResponse.StatusCode}: {readFileResponse.Content!}");
        }

        string downloadUrl;
        HttpClient httpClient;
        HttpResponseMessage httpResponse;

        // Try to download large file
        try
        {
            downloadUrl = this.DownloadFileUrl(apiKey, serverId, path);

            using (httpClient = new HttpClient())
            {
                httpResponse = await httpClient.GetAsync(downloadUrl);
                if (!httpResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"Could not download large file: {httpResponse.StatusCode}: {httpResponse.Content!}");
                }

                return await httpResponse.Content.ReadAsStringAsync();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to download large file ({e.Message}), falling back on copy process.");
        }

        // At this point, the file was so large that it has been written to while being downloaded, causing an error due to a wings bug.
        // So we will instead create a copy of the file, download that, and delete it afterwards.

        RestRequest copyFileRequest = new($"client/servers/{serverId}/files/copy", Method.Post);
        copyFileRequest.AddHeader("Content-Type", "application/json");

        RestResponse copyFileResponse = this.executeRestRequest(apiKey, copyFileRequest, $"{{\"location\": \"{path}\"}}");

        if (!copyFileResponse.IsSuccessStatusCode)
        {
            throw new Exception($"Could not download file: {copyFileResponse.StatusCode}: {copyFileResponse.Content}");
        }

        string newFileName = new Uri($"file://{Path.GetDirectoryName(path)}/{Path.GetFileNameWithoutExtension(path)} copy{Path.GetExtension(path)}").LocalPath;

        downloadUrl = this.DownloadFileUrl(apiKey, serverId, newFileName);

        string content;
        using (httpClient = new HttpClient())
        {

            httpResponse = await httpClient.GetAsync(downloadUrl);
            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new Exception($"Could not download large file: {httpResponse.StatusCode}: {httpResponse.Content!}");
            }

            content = await httpResponse.Content.ReadAsStringAsync();
        }

        RestRequest deleteFileRequest = new($"client/servers/{serverId}/files/delete", Method.Post);
        deleteFileRequest.AddHeader("Content-Type", "application/json");

        RestResponse deleteFileResponse = this.executeRestRequest(apiKey, deleteFileRequest, $"{{ \"root\": \"{new Uri($"file://{Path.GetDirectoryName(newFileName)}").LocalPath}\",\"files\": [ \"{Path.GetFileName(newFileName)}\" ] }}");

        if (!deleteFileResponse.IsSuccessStatusCode)
        {
            await Console.Error.WriteLineAsync($"BAD! Could not delete copied file, polluting the directory! Server {serverId}, path {newFileName}");
            throw new Exception($"Could not delete copied file, polluting the directory! Server {serverId}, path {newFileName}");
        }

        return content;
    }

    public string DownloadFileUrl(string apiKey, string serverId, string path)
    {
        RestResponse readFileResponse = this.executeRestRequest(apiKey, $"client/servers/{serverId}/files/download?file={path}");

        return JsonDocument.Parse(readFileResponse.Content!).RootElement.GetProperty("attributes").GetProperty("url").ToString();
    }

    public List<string> FileList(string apiKey, string serverId, string path)
    {
        RestResponse fileListResponse = this.executeRestRequest(apiKey, $"client/servers/{serverId}/files/list?directory={HttpUtility.UrlEncode(path)}");

        return JsonDocument.Parse(fileListResponse.Content!).RootElement.GetProperty("data").EnumerateArray().Select(x => x.GetProperty("attributes").GetProperty("name").GetString()).ToList();
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
        return JsonDocument.Parse(this.executeRestRequest(apiKey, "client?include=egg").Content!).RootElement.GetProperty("data").EnumerateArray().Select(e => e.GetProperty("attributes")).Where(a => a.GetProperty("relationships").GetProperty("egg").GetProperty("attributes").GetProperty("name").GetString()?.Contains("Pavlov VR") ?? false).Select(a => new PterodactylServerModel
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
