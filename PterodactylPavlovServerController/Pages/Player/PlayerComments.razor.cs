using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using PterodactylPavlovServerController.Contexts;
using PterodactylPavlovServerDomain.Models;

namespace PterodactylPavlovServerController.Pages.Player;

public partial class PlayerComments
{
    [Parameter]
    [EditorRequired]
    public string Id { get; set; } = string.Empty;

    [Parameter]
    [EditorRequired]
    public string PlayerName { get; set; } = string.Empty;

    [Parameter]
    [EditorRequired]
    public ulong PlayerId { get; set; }

    [Parameter]
    [EditorRequired]
    public string ServerId { get; set; } = string.Empty;

    private string comments = string.Empty;

    protected override async Task OnParametersSetAsync()
    {
        using (PavlovServerContext pavlovServerContext = new(this.Configuration))
        {
            this.comments = (await pavlovServerContext.Players.FirstOrDefaultAsync(p => p.UniqueId == PlayerId && p.ServerId == ServerId))?.Comments ?? string.Empty;
        }
        await base.OnParametersSetAsync();
    }

    protected async Task OnCommentChange(string comments)
    {
        using (PavlovServerContext pavlovServerContext = new(this.Configuration))
        {
            PersistentPavlovPlayerModel? dbPlayer = await pavlovServerContext.Players.FirstOrDefaultAsync(p => p.UniqueId == PlayerId && p.ServerId == ServerId);
            if (dbPlayer == null)
            {
                throw new Exception($"Could not store player comment as no database player is associated. PlayerId: {PlayerId}");
            }

            dbPlayer.Comments = comments;
            await pavlovServerContext.SaveChangesAsync();
            await this.AuditService.Add(ServerId, $"Set player {PlayerId} comment \"{comments}\"");
        }
    }
}
