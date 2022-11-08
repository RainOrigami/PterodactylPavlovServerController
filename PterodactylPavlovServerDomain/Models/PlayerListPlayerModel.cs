using Newtonsoft.Json;

namespace PterodactylPavlovServerDomain.Models
{
    public class PlayerListPlayerModel
    {
        public string UniqueId { get; set; } = String.Empty;

        [JsonIgnore]
        public string ServerId { get; set; } = String.Empty;

        public string Username { get; set; } = String.Empty;

    }
}
