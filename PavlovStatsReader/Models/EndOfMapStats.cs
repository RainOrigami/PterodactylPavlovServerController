using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PavlovStatsReader.Models;

public class EndOfMapStats : BaseStatistic
{
    [Key]
    [JsonIgnore]
    public virtual int Id { get; set; }

    [Column("PlayerStats")]
    [JsonProperty("allStats")]
    public virtual HashSet<PlayerStats> PlayerStats { get; set; } = new();

    [Column("MapLabel")]
    [JsonProperty("MapLabel")]
    public virtual string MapLabel { get; set; } = string.Empty;

    [Column("GameMode")]
    [JsonProperty("GameMode")]
    public virtual string GameMode { get; set; } = string.Empty;

    [Column("PlayerCount")]
    [JsonProperty("PlayerCount")]
    public virtual int PlayerCount { get; set; }

    [Column("Teams")]
    [JsonProperty("bTeams")]
    public virtual bool Teams { get; set; }

    [Column("Team0Score")]
    [JsonProperty("Team0Score")]
    public virtual int Team0Score { get; set; }

    [Column("Team1Score")]
    [JsonProperty("Team1Score")]
    public virtual int Team1Score { get; set; }
}
