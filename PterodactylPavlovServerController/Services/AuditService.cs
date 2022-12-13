using Microsoft.AspNetCore.Identity;
using PterodactylPavlovServerController.Areas.Identity.Data;
using PterodactylPavlovServerController.Contexts;
using PterodactylPavlovServerDomain.Models;

namespace PterodactylPavlovServerController.Services;

public class AuditService
{
    private readonly PavlovServerContext pavlovServerContext;
    private readonly UserManager<PterodactylPavlovServerControllerUser> userManager;
    private readonly IHttpContextAccessor httpContextAccessor;

    public AuditService(PavlovServerContext pavlovServerContext, UserManager<PterodactylPavlovServerControllerUser> userManager, IHttpContextAccessor httpContextAccessor)
    {
        this.pavlovServerContext = pavlovServerContext;
        this.userManager = userManager;
        this.httpContextAccessor = httpContextAccessor;
    }

    public async Task Add(string server, string action)
    {
        if (httpContextAccessor.HttpContext == null)
        {
            throw new Exception("Unable to audit action, HttpContext is not available");
        }

        this.pavlovServerContext.AuditActions.Add(new AuditActionModel()
        {
            Server = server,
            User = userManager.GetUserName(httpContextAccessor.HttpContext.User),
            Time = DateTime.Now,
            Action = action
        });
        await this.pavlovServerContext.SaveChangesAsync();
    }
}
