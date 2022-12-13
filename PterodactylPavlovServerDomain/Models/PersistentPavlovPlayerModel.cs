using Newtonsoft.Json;

namespace PterodactylPavlovServerDomain.Models;

public class PersistentPavlovPlayerModel
{
    public ulong UniqueId { get; set; }

    [JsonIgnore]
    public string ServerId { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public DateTime LastSeen { get; set; } = DateTime.MinValue;

    public TimeSpan TotalTime { get; set; } = TimeSpan.Zero;

    public string? BanReason { get; set; } = null;

    public string Comments { get; set; } = string.Empty;
}
