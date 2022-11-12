using Microsoft.AspNetCore.Identity;

namespace PterodactylPavlovServerController.Areas.Identity.Data;

// Add profile data for application users by adding properties to the PterodactylPavlovServerControllerUser class
public class PterodactylPavlovServerControllerUser : IdentityUser
{
    public string PterodactylApiKey { get; set; } = string.Empty;
}

