namespace PterodactylPavlovServerController.Models;

public class BanConfirmEventArgumentsModel
{
    public BanConfirmEventArgumentsModel(bool confirmed, string? reason)
    {
        this.Confirmed = confirmed;
        this.Reason = reason;
    }

    public bool Confirmed { get; }
    public string? Reason { get; }
}
