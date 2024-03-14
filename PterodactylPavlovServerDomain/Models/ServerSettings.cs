namespace PterodactylPavlovServerDomain.Models;
public class ServerSettings
{
    public const string SETTING_RESERVED_SLOT_AMOUNT = "reservedSlots.amount";
    public const string SETTING_RESERVED_SLOT_PIN = "reservedSlots.pin";
    public const string SETTING_WARMUP_ENABLED = "warmup.enabled";
    public const string SETTING_LEAGUE_MODE_ENABLED = "leaguemode.enabled";
    public const string SETTING_PAUSE_SERVER = "server.pause";
    public const string SETTING_SKIN_ENABLED = "killskin.enabled";
    public const string SETTING_SKIN_THRESHOLD = "killskin.threshold";
    public const string SETTING_SKIN_SKIN = "killskin.skin";
    public const string SETTING_PING_KICK_ENABLED = "pingkick.enabled";
    public const string SETTING_PING_KICK_THRESHOLD = "pingkick.threshold";
    public const string SETTING_PING_KICK_MEASURETIME = "pingkick.measuretime";
    public const string SETTING_PING_EXEMPTEES = "pingkick.exemptees";

    public string ServerId { get; set; } = string.Empty;
    public string SettingName { get; set; } = string.Empty;
    public string SettingValue { get; set; } = string.Empty;
}
