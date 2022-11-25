namespace PterodactylPavlovServerController.Models;

public class StatsLinkModel
{
    public StatsLinkModel(string targetId, string linkText, object additionalText)
    {
        this.TargetId = targetId;
        this.LinkText = linkText;
        this.AdditionalText = additionalText;
    }

    public string TargetId { get; }
    public string LinkText { get; }
    public object AdditionalText { get; }
}
