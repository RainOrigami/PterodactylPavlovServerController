using PterodactylPavlovServerController.Exceptions;
using System.Text.RegularExpressions;

namespace PterodactylPavlovServerDomain.Models
{
    public class PlayerDetailModel : PlayerListPlayerModel
    {
        public string PlayerName { get; set; } = String.Empty;
        public string KDA { get; set; } = String.Empty;
        public int Cash { get; set; }
        public bool Dead { get; set; }
        public int TeamId { get; set; }
        public int Score { get; set; }

        private int? kills = null;
        private int? deaths = null;
        private int? assists = null;

        private static readonly Regex kdaRegex = new Regex(@"^(?<kills>\d+)/(?<deaths>\d+)/(?<assists>\d+)$");
        private Match? kdaMatch = null;

        private Match KdaMatch
        {
            get
            {
                if (kdaMatch is null)
                {
                    kdaMatch = kdaRegex.Match(KDA);
                    if (!kdaMatch.Success)
                    {
                        throw new RconException();
                    }
                }

                return kdaMatch;
            }
        }

        public int Kills
        {
            get
            {
                if (kills is null)
                {
                    kills = int.Parse(KdaMatch.Groups["kills"].Value);
                }

                return kills.Value;
            }
        }

        public int Deaths
        {
            get
            {
                if (deaths is null)
                {
                    deaths = int.Parse(KdaMatch.Groups["deaths"].Value);
                }

                return deaths.Value;
            }
        }

        public int Assists
        {
            get
            {
                if (assists is null)
                {
                    assists = int.Parse(KdaMatch.Groups["assists"].Value);
                }

                return assists.Value;
            }
        }
    }
}
