using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace PavlovStatsReader.Models;

public class BaseStatistic
{
    [Column("ServerId")]
    [JsonIgnore]
    public string ServerId { get; set; } = string.Empty;

    [JsonIgnore]
    public DateTime LogEntryDate { get; set; }
}
