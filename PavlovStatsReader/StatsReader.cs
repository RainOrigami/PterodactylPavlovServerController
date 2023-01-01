using Newtonsoft.Json;
using PavlovStatsReader.Models;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PavlovStatsReader;

public class StatsReader
{
    private static readonly Regex logEntryTimestampRegex = new(@"\[(?<date>\d{4}\.\d{2}\.\d{2}-\d{2}\.\d{2}\.\d{2}:\d{3})\]", RegexOptions.Compiled);
    private static readonly Regex statManagerLogRegex = new(@"^\[\d{4}\.\d{2}\.\d{2}-\d{2}\.\d{2}\.\d{2}:\d{3}\]\[\d{3}\]StatManagerLog: {$", RegexOptions.Compiled);

    public StatsReader(string statisticContent)
    {
        string[] statisticLines = statisticContent.Split('\n', '\r');
        StringBuilder statsBlockBuilder = new();
        string statsBlockTimestamp = string.Empty;
        bool insideStatsBlock = false;
        foreach (string line in statisticLines)
        {
            if (insideStatsBlock)
            {
                statsBlockBuilder.AppendLine(line);
                if (line == "}")
                {
                    insideStatsBlock = false;

                    string statBlock = statsBlockBuilder.ToString();

                    parseStatsBlock(statBlock, statsBlockTimestamp);

                    statsBlockBuilder.Clear();
                    statsBlockTimestamp = string.Empty;
                }

                continue;
            }

            if (!statManagerLogRegex.IsMatch(line))
            {
                continue;
            }

            statsBlockTimestamp = line;
            statsBlockBuilder.AppendLine("{");
            insideStatsBlock = true;
        }
    }

    private void parseStatsBlock(string statsBlock, string statsBlockTimestamp)
    {
        JsonElement statRoot = JsonDocument.Parse(statsBlock).RootElement;
        JsonProperty property = statRoot.EnumerateObject().First();
        Type? statsType = null;
        switch (property.Name)
        {
            case "KillData":
                statsType = typeof(KillData);
                break;
            case "RoundState":
                statsType = typeof(RoundState);
                break;
            case "RoundEnd":
                statsType = typeof(RoundEnd);
                break;
            case "allStats":
                statsType = typeof(EndOfMapStats);
                break;
            case "BombData":
                statsType = typeof(BombData);
                break;
            case "SwitchTeam":
                statsType = typeof(SwitchTeam);
                break;
        }

        if (statsType != null)
        {
            BaseStatistic statistic;
            if (statsType == typeof(EndOfMapStats))
            {
                statistic = (BaseStatistic)JsonConvert.DeserializeObject(statRoot.ToString(), statsType)!;
            }
            else
            {
                try
                {
                    statistic = (BaseStatistic)JsonConvert.DeserializeObject(property.Value.ToString(), statsType)!;
                }
                catch
                {
                    return;
                }
            }

            Match logEntryTimestampMatch = StatsReader.logEntryTimestampRegex.Match(statsBlockTimestamp);
            if (!logEntryTimestampMatch.Success)
            {
                Console.WriteLine($"Failed to parse timestamp of log entry: {statsBlockTimestamp}");
                return;
            }

            statistic.LogEntryDate = DateTime.ParseExact(logEntryTimestampMatch.Groups["date"].Value, "yyyy.MM.dd-HH.mm.ss:FFF", null);

            if (statistic is EndOfMapStats endOfMapStats)
            {
                foreach (PlayerStats playerStats in endOfMapStats.PlayerStats)
                {
                    playerStats.LogEntryDate = endOfMapStats.LogEntryDate;
                    foreach (Stats stats in playerStats.Stats)
                    {
                        stats.LogEntryDate = endOfMapStats.LogEntryDate;
                    }
                }
            }

            parsedStats.Add(statistic);
        }
        else
        {
            unparsedStats.AppendLine(statsBlock);
        }
    }

    public string UnparsedStats => this.unparsedStats.ToString();
    private StringBuilder unparsedStats = new();

    public ReadOnlyCollection<BaseStatistic> ParsedStats => this.parsedStats.AsReadOnly();
    private List<BaseStatistic> parsedStats = new();
}
