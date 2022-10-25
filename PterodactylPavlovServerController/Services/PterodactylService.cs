using RestSharp;

namespace PterodactylPavlovServerController.Services
{
    public class PterodactylService
    {
        private readonly IConfiguration configuration;

        public PterodactylService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public string ReadFile(string serverId, string path)
        {
            RestResponse readFileResponse = executeRestRequest($"client/servers/{serverId}/files/contents?file={path}");

            return readFileResponse.Content!;
        }

        public void WriteFile(string serverId, string path, string content)
        {
            RestRequest textBodyRequest = new RestRequest();
            textBodyRequest.Method = Method.Post;
            textBodyRequest.AddHeader("Content-Type", "text/plain");

            executeRestRequest($"client/servers/{serverId}/files/write?file={path}", content, textBodyRequest);
        }

        private RestResponse executeRestRequest(string path, string? content = null, RestRequest? baseRequest = null)
        {
            RestClient restClient = new RestClient($"{configuration["pterodactyl_baseurl"]}/api/{path}");
            RestRequest restRequest = baseRequest ?? new RestRequest();
            restRequest.AddHeader("Authorization", $"Bearer {configuration["pterodactyl_bearertoken"]}");

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
    }
}
