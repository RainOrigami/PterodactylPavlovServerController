using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PavlovStatsReader.Models;

public class PlayerStats : BaseStatistic
{
    [Key]
    [JsonIgnore]
    public virtual int Id { get; set; }

    [Column("UniqueId")]
    [JsonProperty("uniqueId")]
    public virtual ulong UniqueId { get; set; }

    [Column("PlayerName")]
    [JsonProperty("playerName")]
    public virtual string PlayerName { get; set; } = string.Empty;

    [Column("TeamId")]
    [JsonProperty("teamId")]
    public virtual int TeamId { get; set; }

    [Column("Stats")]
    [JsonProperty("stats")]
    public virtual HashSet<Stats> Stats { get; set; } = new();

    [JsonIgnore]
    public virtual EndOfMapStats EndOfMapStats { get; set; } = new();
}
