using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PavlovStatsReader.Models
{
    public class SwitchTeam : BaseStatistic
    {
        [Key]
        [JsonIgnore]
        public virtual int Id { get; set; }

        [Column("PlayerID")]
        [JsonProperty("PlayerID")]
        public virtual ulong PlayerID { get; set; }

        [Column("NewTeamID")]
        [JsonProperty("NewTeamID")]
        public virtual int NewTeamID { get; set; }
    }
}
