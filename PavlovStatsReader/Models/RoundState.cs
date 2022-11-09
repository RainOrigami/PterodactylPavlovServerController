using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PavlovStatsReader.Models;

public class RoundState : BaseStatistic
{
    [Key]
    [JsonIgnore]
    public virtual int Id { get; set; }

    [Column("State")]
    [JsonProperty("State")]
    public virtual string State { get; set; } = string.Empty;

    [JsonProperty("Timestamp")]
    [NotMapped]
    public virtual string JsonTimestamp
    {
        get => this.Timestamp.ToString("yyyy.MM.dd-HH.mm.ss");
        set => this.Timestamp = DateTime.ParseExact(value, "yyyy.MM.dd-HH.mm.ss", null);
    }

    [Column("Timestamp")]
    [JsonIgnore]
    public virtual DateTime Timestamp { get; set; }
}
