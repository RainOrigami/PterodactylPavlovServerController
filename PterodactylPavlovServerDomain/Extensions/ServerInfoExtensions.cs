using PavlovVR_Rcon.Models.Pavlov;
using System.Text.RegularExpressions;

namespace PterodactylPavlovServerDomain.Extensions;
public static class ServerInfoExtensions
{
    private static readonly Regex playerCountRegex = new(@"^(?<current>\d+)/(?<max>\d+)$", RegexOptions.Compiled);

    public static int CurrentPlayerCount(this ServerInfo serverInfo) => int.Parse(ServerInfoExtensions.playerCountRegex.Match(serverInfo.PlayerCount).Groups["current"].Value);
    public static int MaximumPlayerCount(this ServerInfo serverInfo) => int.Parse(ServerInfoExtensions.playerCountRegex.Match(serverInfo.PlayerCount).Groups["max"].Value);
}
