using PterodactylPavlovServerController.Exceptions;
using System.Text.RegularExpressions;

namespace PterodactylPavlovServerController.Models
{
    public class ServerInfoModel
    {
        public string ServerId { get; set; } = String.Empty;
        public string MapLabel { get; set; } = String.Empty;
        public string GameMode { get; set; } = String.Empty;
        public string ServerName { get; set; } = String.Empty;
        public bool Teams { get; set; }
        public int Team0Score { get; set; }
        public int Team1Score { get; set; }
        public int Round { get; set; }
        public string RoundState { get; set; } = String.Empty;
        public string PlayerCount { get; set; } = String.Empty;

        private static readonly Regex playerCountRegex = new(@"^(?<current>\d+)/(?<max>\d+)$");
        private Match? playerCountMatch = null;
        private Match PlayerCountMatch
        {
            get
            {
                if (playerCountMatch is null)
                {
                    playerCountMatch = playerCountRegex.Match(PlayerCount);

                    if (!playerCountMatch.Success)
                    {
                        throw new RconException();
                    }
                }

                return playerCountMatch;
            }
        }

        private int? currentPlayerCount = null;
        private int? maximumPlayerCount = null;

        public int CurrentPlayerCount
        {
            get
            {
                currentPlayerCount ??= int.Parse(PlayerCountMatch.Groups["current"].Value);

                return currentPlayerCount.Value;
            }
        }

        public int MaximumPlayerCount
        {
            get
            {
                maximumPlayerCount ??= int.Parse(PlayerCountMatch.Groups["max"].Value);

                return maximumPlayerCount.Value;
            }
        }
    }
}
