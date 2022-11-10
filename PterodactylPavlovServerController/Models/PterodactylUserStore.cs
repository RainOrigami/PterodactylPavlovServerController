using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PterodactylPavlovServerController.Contexts;

namespace PterodactylPavlovServerController.Models;

public class PterodactylUserStore : IUserStore<PterodactylUserModel>
{
    private readonly PterodactylContext pterodactylContext;

    public PterodactylUserStore(PterodactylContext pterodactylContext)
    {
        this.pterodactylContext = pterodactylContext;
    }
    public void Dispose()
    {

    }

    public Task<string> GetUserIdAsync(PterodactylUserModel user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Username);
    }

    public Task<string> GetUserNameAsync(PterodactylUserModel user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Username);
    }

    public Task SetUserNameAsync(PterodactylUserModel user, string userName, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<string> GetNormalizedUserNameAsync(PterodactylUserModel user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Username);
    }

    public Task SetNormalizedUserNameAsync(PterodactylUserModel user, string normalizedName, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IdentityResult> CreateAsync(PterodactylUserModel user, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IdentityResult> UpdateAsync(PterodactylUserModel user, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IdentityResult> DeleteAsync(PterodactylUserModel user, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<PterodactylUserModel?> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        return await this.pterodactylContext.Users.FirstOrDefaultAsync(u => u.Username == userId, cancellationToken);
    }

    public async Task<PterodactylUserModel?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        return await this.FindByIdAsync(normalizedUserName, cancellationToken);
    }
}
