using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PavlovStatsReader.Models;

public class KillData : BaseStatistic
{
    [Key]
    [JsonIgnore]
    public virtual int Id { get; set; }

    [Column("Killer")]
    [JsonProperty("Killer")]
    public virtual ulong Killer { get; set; }

    [Column("KillerTeamID")]
    [JsonProperty("KillerTeamID")]
    public virtual int KillerTeamID { get; set; }

    [Column("Killed")]
    [JsonProperty("Killed")]
    public virtual ulong Killed { get; set; }

    [Column("KilledTeamID")]
    [JsonProperty("KilledTeamID")]
    public virtual int KilledTeamID { get; set; }

    [Column("KilledBy")]
    [JsonProperty("KilledBy")]
    public virtual string KilledBy { get; set; } = string.Empty;

    [Column("Headshot")]
    [JsonProperty("Headshot")]
    public virtual bool Headshot { get; set; }
}
