using Microsoft.AspNetCore.Components;

namespace PterodactylPavlovServerController.Pages;

public partial class Confirm
{
    //protected bool ShowConfirmation { get; set; }

    [Parameter]
    [EditorRequired]
    public string Id { get; set; } = string.Empty;

    [Parameter]
    [EditorRequired]
    public string ConfirmationTitle { get; set; } = string.Empty;

    [Parameter]
    [EditorRequired]
    public string ConfirmationMessage { get; set; } = string.Empty;

    //public void Show()
    //{
    //    ShowConfirmation = true;
    //    StateHasChanged();
    //}

    [Parameter]
    public EventCallback<bool> ConfirmationChanged { get; set; }

    protected async Task OnConfirmationChange(bool value)
    {
        await Console.Out.WriteLineAsync($"Confirmation for {this.ConfirmationTitle} / {this.ConfirmationMessage} result {value}");
        //ShowConfirmation = false;
        await ConfirmationChanged.InvokeAsync(value);
    }
}
