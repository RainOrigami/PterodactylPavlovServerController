namespace PterodactylPavlovServerDomain.Models;
public class ServerSettings
{
    public const string SETTING_RESERVED_SLOT_AMOUNT = "reservedSlots.amount";
    public const string SETTING_RESERVED_SLOT_PIN = "reservedSlots.pin";
    public const string SETTING_WARMUP_ENABLED = "warmup.enabled";
    public const string SETTING_LEAGUE_MODE_ENABLED = "leaguemode.enabled";
    public const string SETTING_PAUSE_SERVER = "server.pause";

    public string ServerId { get; set; } = string.Empty;
    public string SettingName { get; set; } = string.Empty;
    public string SettingValue { get; set; } = string.Empty;
}
