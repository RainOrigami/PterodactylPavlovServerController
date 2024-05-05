using Newtonsoft.Json;

namespace PterodactylPavlovServerDomain.Models;

public class PterodactylFile
{
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;
    [JsonProperty("modified_at")]
    public DateTime ModifiedAt { get; set; }
    [JsonProperty("size")]
    public long Size { get; set; }
}
