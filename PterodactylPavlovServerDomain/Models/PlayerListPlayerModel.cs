using Newtonsoft.Json;

namespace PterodactylPavlovServerDomain.Models;

public class PlayerListPlayerModel
{
    public string UniqueId { get; set; } = string.Empty;

    [JsonIgnore]
    public string ServerId { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;
}
