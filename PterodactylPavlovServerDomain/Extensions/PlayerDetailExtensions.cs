using PavlovVR_Rcon.Models.Pavlov;
using System.Text.RegularExpressions;

namespace PterodactylPavlovServerDomain.Extensions;
public static class PlayerDetailExtensions
{
    private static readonly Regex kdaRegex = new Regex(@"^(?<kills>\d+)/(?<deaths>\d+)/(?<assists>\d+)$", RegexOptions.Compiled);

    public static int Kills(this PlayerDetail playerDetail) => int.Parse(kdaRegex.Match(playerDetail.KDA).Groups["kills"].Value);
    public static int Deaths(this PlayerDetail playerDetail) => int.Parse(kdaRegex.Match(playerDetail.KDA).Groups["deaths"].Value);
    public static int Assists(this PlayerDetail playerDetail) => int.Parse(kdaRegex.Match(playerDetail.KDA).Groups["assists"].Value);
}
