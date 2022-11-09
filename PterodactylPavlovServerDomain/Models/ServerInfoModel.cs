using PterodactylPavlovServerController.Exceptions;
using System.Text.RegularExpressions;

namespace PterodactylPavlovServerDomain.Models;

public class ServerInfoModel
{
    private static readonly Regex playerCountRegex = new(@"^(?<current>\d+)/(?<max>\d+)$");

    private int? currentPlayerCount;
    private int? maximumPlayerCount;
    private Match? playerCountMatch;
    public string ServerId { get; set; } = string.Empty;
    public string MapLabel { get; set; } = string.Empty;
    public string GameMode { get; set; } = string.Empty;
    public string ServerName { get; set; } = string.Empty;
    public bool Teams { get; set; }
    public int Team0Score { get; set; }
    public int Team1Score { get; set; }
    public int Round { get; set; }
    public string RoundState { get; set; } = string.Empty;
    public string PlayerCount { get; set; } = string.Empty;

    private Match PlayerCountMatch
    {
        get
        {
            if (this.playerCountMatch is null)
            {
                this.playerCountMatch = ServerInfoModel.playerCountRegex.Match(this.PlayerCount);

                if (!this.playerCountMatch.Success)
                {
                    throw new RconException();
                }
            }

            return this.playerCountMatch;
        }
    }

    public int CurrentPlayerCount
    {
        get
        {
            this.currentPlayerCount ??= int.Parse(this.PlayerCountMatch.Groups["current"].Value);

            return this.currentPlayerCount.Value;
        }
    }

    public int MaximumPlayerCount
    {
        get
        {
            this.maximumPlayerCount ??= int.Parse(this.PlayerCountMatch.Groups["max"].Value);

            return this.maximumPlayerCount.Value;
        }
    }
}
