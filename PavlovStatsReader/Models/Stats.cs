using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PavlovStatsReader.Models;

public class Stats : BaseStatistic
{
    [Key]
    [JsonIgnore]
    public virtual int Id { get; set; }

    [Column("StatType")]
    [JsonProperty("statType")]
    public virtual string StatType { get; set; } = string.Empty;

    [Column("Amount")]
    [JsonProperty("amount")]
    public virtual int Amount { get; set; }

    [JsonIgnore]
    public virtual PlayerStats PlayerStats { get; set; } = new();
}
