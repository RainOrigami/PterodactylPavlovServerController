using Newtonsoft.Json;

namespace PterodactylPavlovServerDomain.Models
{
    public class PterodactylFile
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
