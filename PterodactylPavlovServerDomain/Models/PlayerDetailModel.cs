using PterodactylPavlovServerController.Exceptions;
using System.Text.RegularExpressions;

namespace PterodactylPavlovServerDomain.Models;

public class PlayerDetailModel : PlayerListPlayerModel
{
    private static readonly Regex kdaRegex = new(@"^(?<kills>\d+)/(?<deaths>\d+)/(?<assists>\d+)$");
    private int? assists;
    private int? deaths;
    private Match? kdaMatch;

    private int? kills;
    public string PlayerName { get; set; } = string.Empty;
    public string KDA { get; set; } = string.Empty;
    public int Cash { get; set; }
    public bool Dead { get; set; }
    public int TeamId { get; set; }
    public int Score { get; set; }

    private Match KdaMatch
    {
        get
        {
            if (this.kdaMatch is null)
            {
                this.kdaMatch = PlayerDetailModel.kdaRegex.Match(this.KDA);
                if (!this.kdaMatch.Success)
                {
                    throw new RconException();
                }
            }

            return this.kdaMatch;
        }
    }

    public int Kills
    {
        get
        {
            if (this.kills is null)
            {
                this.kills = int.Parse(this.KdaMatch.Groups["kills"].Value);
            }

            return this.kills.Value;
        }
    }

    public int Deaths
    {
        get
        {
            if (this.deaths is null)
            {
                this.deaths = int.Parse(this.KdaMatch.Groups["deaths"].Value);
            }

            return this.deaths.Value;
        }
    }

    public int Assists
    {
        get
        {
            if (this.assists is null)
            {
                this.assists = int.Parse(this.KdaMatch.Groups["assists"].Value);
            }

            return this.assists.Value;
        }
    }
}
