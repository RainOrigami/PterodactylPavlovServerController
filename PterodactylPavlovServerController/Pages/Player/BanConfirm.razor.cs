using Microsoft.AspNetCore.Components;
using PterodactylPavlovServerController.Models;

namespace PterodactylPavlovServerController.Pages.Player;

public partial class BanConfirm
{
    [Parameter]
    [EditorRequired]
    public string Id { get; set; } = string.Empty;

    [Parameter]
    [EditorRequired]
    public string ConfirmationTitle { get; set; } = string.Empty;

    [Parameter]
    [EditorRequired]
    public string ConfirmationMessage { get; set; } = string.Empty;

    [Parameter]
    public EventCallback<BanConfirmEventArgumentsModel> ConfirmationChanged { get; set; }

    private string customReason = string.Empty;
    private int duration = -1;
    private string customDuration = string.Empty;

    protected async Task OnConfirmationChange(bool value, string? reason = null)
    {
        if (duration == -2)
        {
            duration = int.Parse(customDuration) * 60 * 60;
        }

        await ConfirmationChanged.InvokeAsync(new BanConfirmEventArgumentsModel(value, reason, duration));
    }
}
