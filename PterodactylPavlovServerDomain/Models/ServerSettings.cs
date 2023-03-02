namespace PterodactylPavlovServerDomain.Models;
public class ServerSettings
{
    public const string SETTING_RESERVED_SLOT_AMOUNT = "reservedSlots.amount";
    public const string SETTING_RESERVED_SLOT_PIN = "reservedSlots.pin";
    public const string SETTING_WARMUP_ENABLED = "warmup.enabled";

    public string ServerId { get; set; } = string.Empty;
    public string SettingName { get; set; } = string.Empty;
    public string SettingValue { get; set; } = string.Empty;
}
