// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PterodactylPavlovServerController.Areas.Identity.Data;

namespace PterodactylPavlovServerController.Areas.Identity.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly NavigationManager navigationManager;
        private readonly SignInManager<PterodactylPavlovServerControllerUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(NavigationManager navigationManager, SignInManager<PterodactylPavlovServerControllerUser> signInManager, ILogger<LogoutModel> logger)
        {
            this.navigationManager = navigationManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        public async Task<IActionResult> OnGet()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return LocalRedirect("/");
        }
    }
}
