// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PterodactylPavlovServerController.Areas.Identity.Data;
using PterodactylPavlovServerController.Contexts;
using PterodactylPavlovServerController.Models;
using System.ComponentModel.DataAnnotations;

namespace PterodactylPavlovServerController.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly PterodactylContext pterodactylContext;
        private readonly UserManager<PterodactylPavlovServerControllerUser> userManager;
        private readonly SignInManager<PterodactylPavlovServerControllerUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(PterodactylContext pterodactylContext, UserManager<PterodactylPavlovServerControllerUser> userManager, SignInManager<PterodactylPavlovServerControllerUser> signInManager, ILogger<LoginModel> logger)
        {
            this.pterodactylContext = pterodactylContext;
            this.userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        ///// <summary>
        /////     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        /////     directly from your code. This API may change or be removed in future releases.
        ///// </summary>
        //public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            ///// <summary>
            /////     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            /////     directly from your code. This API may change or be removed in future releases.
            ///// </summary>
            //[Required]
            //[EmailAddress]
            //public string Email { get; set; }

            [Required]
            public string Username { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            //ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            //ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var user = await this.userManager.FindByNameAsync(Input.Username);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }

                PterodactylUserModel? targetUser = await this.pterodactylContext.Users.FirstOrDefaultAsync(u => u.Username == this.Input.Username!);

                if (targetUser == null || !BCrypt.Net.BCrypt.Verify(this.Input.Password!, targetUser.Password))
                {
                    this.ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return this.Page();
                }

                await this._signInManager.SignInAsync(user, Input.RememberMe);
                //var result = await _signInManager.PasswordSignInAsync(Input.Username, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                //if (result.Succeeded)
                //{
                _logger.LogInformation("User logged in.");
                return LocalRedirect(returnUrl);
                //}
                //if (result.RequiresTwoFactor)
                //{
                //    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                //}
                //if (result.IsLockedOut)
                //{
                //    _logger.LogWarning("User account locked out.");
                //    return RedirectToPage("./Lockout");
                //}
                //else
                //{
                //ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                //return Page();
                //}
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
