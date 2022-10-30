using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PavlovStatsReader.Models
{
    public class BombData : BaseStatistic
    {
        [Key]
        [JsonIgnore]
        public virtual int Id { get; set; }

        [Column("Player")]
        [JsonProperty("Player")]
        public virtual ulong Player { get; set; }

        [Column("BombInteraction")]
        [JsonProperty("BombInteraction")]
        public virtual string BombInteraction { get; set; }
    }
}
