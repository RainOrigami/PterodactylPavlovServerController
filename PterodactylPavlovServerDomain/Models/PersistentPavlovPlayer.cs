using Newtonsoft.Json;

namespace PterodactylPavlovServerDomain.Models;

public class PersistentPavlovPlayer
{
    public ulong UniqueId { get; set; }

    [JsonIgnore]
    public string ServerId { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public DateTime LastSeen { get; set; } = DateTime.MinValue;
}
