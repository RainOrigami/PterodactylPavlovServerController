namespace PterodactylPavlovServerController.Models;

public class BanConfirmEventArgumentsModel
{
    public BanConfirmEventArgumentsModel(bool confirmed, string? reason, int duration)
    {
        this.Confirmed = confirmed;
        this.Reason = reason;
        this.Duration = duration;
    }

    public bool Confirmed { get; }
    public string? Reason { get; }
    public int Duration { get; }
}
