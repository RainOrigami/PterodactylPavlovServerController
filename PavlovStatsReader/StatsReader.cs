using Newtonsoft.Json;
using PavlovStatsReader.Models;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PavlovStatsReader
{
    public class StatsReader
    {
        public string UnparsedStats { get; }
        public ReadOnlyCollection<BaseStatistic> ParsedStats { get; }

        private static readonly Regex logEntryTimestampRegex = new(@"\[(?<date>\d{4}\.\d{2}\.\d{2}-\d{2}\.\d{2}\.\d{2}:\d{3})\]", RegexOptions.Compiled);

        public StatsReader(string statisticContent)
        {
            StringBuilder unparsedStats = new();
            List<BaseStatistic> parsedStats = new();

            Stack<int> bracketStack = new Stack<int>();
            for (int i = 0; i < statisticContent.Length; i++)
            {
                switch (statisticContent[i])
                {
                    case '{':
                        bracketStack.Push(i);
                        break;
                    case '}':
                        int firstBracket = bracketStack.Pop();
                        if (bracketStack.Count == 0)
                        {
                            string statBlock = statisticContent.Substring(firstBracket, i - firstBracket + 1);
                            JsonElement statRoot = JsonDocument.Parse(statBlock).RootElement;
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
                                    statistic = (BaseStatistic)JsonConvert.DeserializeObject(property.Value.ToString(), statsType)!;
                                }

                                int previousNewline = 0;
                                while (true)
                                {
                                    if ((firstBracket - previousNewline) - 1 < 0)
                                    {
                                        break;
                                    }
                                    else if (statisticContent[(firstBracket - previousNewline) - 1] == '\n')
                                    {
                                        break;
                                    }
                                    previousNewline++;
                                }
                                Match logEntryTimestampMatch = logEntryTimestampRegex.Match(statisticContent.Substring(firstBracket - previousNewline, previousNewline));
                                if (!logEntryTimestampMatch.Success)
                                {
                                    Console.WriteLine($"Failed to parse timestamp of log entry: {statisticContent.Substring(firstBracket - previousNewline, i - firstBracket + 1)}");
                                    continue;
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
                                unparsedStats.AppendLine(statBlock);
                            }
                        }
                        break;
                }
            }

            ParsedStats = parsedStats.AsReadOnly();
            UnparsedStats = unparsedStats.ToString();
        }
    }

}