using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace PavlovStatsReader.Models
{
    public class RoundEnd : BaseStatistic
    {
        [Key]
        [JsonIgnore]
        public virtual int Id { get; set; }

        [Column("Round")]
        [JsonProperty("Round")]
        public virtual int Round { get; set; }

        [Column("WinningTeam")]
        [JsonProperty("WinningTeam")]
        public virtual int WinningTeam { get; set; }
    }
}
