// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PterodactylPavlovServerController.Areas.Identity.Data;
using PterodactylPavlovServerController.Contexts;
using PterodactylPavlovServerController.Models;
using PterodactylPavlovServerController.Services;
using System.ComponentModel.DataAnnotations;

namespace PterodactylPavlovServerController.Areas.Identity.Pages.Account;

public class RegisterModel : PageModel
{
    //private readonly IUserEmailStore<PterodactylPavlovServerControllerUser> _emailStore;
    private readonly ILogger<RegisterModel> logger;
    private readonly SignInManager<PterodactylPavlovServerControllerUser> signInManager;
    private readonly UserManager<PterodactylPavlovServerControllerUser> userManager;
    private readonly IUserStore<PterodactylPavlovServerControllerUser> userStore;

    private readonly PterodactylContext pterodactylContext;

    private readonly PterodactylService pterodactylService;
    //private readonly IEmailSender _emailSender;

    public RegisterModel(UserManager<PterodactylPavlovServerControllerUser> userManager, IUserStore<PterodactylPavlovServerControllerUser> userStore, SignInManager<PterodactylPavlovServerControllerUser> signInManager, ILogger<RegisterModel> logger,
                         //IEmailSender emailSender
                         PterodactylContext pterodactylContext, PterodactylService pterodactylService)
    {
        this.userManager = userManager;
        this.userStore = userStore;
        //_emailStore = GetEmailStore();
        this.signInManager = signInManager;
        this.logger = logger;
        this.pterodactylContext = pterodactylContext;
        this.pterodactylService = pterodactylService;
        //_emailSender = emailSender;
    }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = new();

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public string? ReturnUrl { get; set; }

    public async Task OnGetAsync(string? returnUrl = null)
    {
        this.ReturnUrl = returnUrl;
        await Task.CompletedTask;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= this.Url.Content("~/");
        if (!this.ModelState.IsValid)
        {
            return this.Page();
        }

        bool apiKeyOk = false;
        try
        {
            this.pterodactylService.GetServers(this.Input.PterodactylApiKey!);
            apiKeyOk = true;
        }
        catch { }

        PterodactylUserModel? targetUser = await this.pterodactylContext.Users.FirstOrDefaultAsync(u => u.Username == this.Input.Username!);

        if (!apiKeyOk || targetUser == null || !BCrypt.Net.BCrypt.Verify(this.Input.Password!, targetUser.Password))
        {
            this.ModelState.AddModelError(string.Empty, "Verify that your username, password and API key are the same as in Pterodactyl");
            return this.Page();
        }

        PterodactylPavlovServerControllerUser user = this.createUser();
        user.PterodactylApiKey = this.Input.PterodactylApiKey!;
        await this.userStore.SetUserNameAsync(user, this.Input.Username, CancellationToken.None);
        //await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
        IdentityResult result = await this.userManager.CreateAsync(user);

        if (result.Succeeded)
        {
            this.logger.LogInformation("User created a new account.");

            //var userId = await _userManager.GetUserIdAsync(user);
            //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            //code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            //var callbackUrl = Url.Page(
            //    "/Account/ConfirmEmail",
            //    pageHandler: null,
            //    values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
            //    protocol: Request.Scheme);

            //await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
            //    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            //if (_userManager.Options.SignIn.RequireConfirmedAccount)
            //{
            //    return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
            //}
            //else
            //{
            await this.signInManager.SignInAsync(user, false);
            return this.LocalRedirect(returnUrl);
            //}
        }

        foreach (IdentityError error in result.Errors)
        {
            this.ModelState.AddModelError(string.Empty, error.Description);
        }

        // If we got this far, something failed, redisplay form
        return this.Page();
    }

    private PterodactylPavlovServerControllerUser createUser()
    {
        try
        {
            return Activator.CreateInstance<PterodactylPavlovServerControllerUser>();
        }
        catch
        {
            throw new InvalidOperationException($"Can't create an instance of '{nameof(PterodactylPavlovServerControllerUser)}'. " + $"Ensure that '{nameof(PterodactylPavlovServerControllerUser)}' is not an abstract class and has a parameterless constructor, or alternatively " + "override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
        }
    }

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
        //[Display(Name = "Email")]
        //public string Email { get; set; }

        [Required]
        [Display(Name = "Username")]
        public string? Username { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string? Password { get; set; }

        [Required]
        [Display(Name = "Pterodactyl API key")]
        public string? PterodactylApiKey { get; set; }
    }

    //private IUserEmailStore<PterodactylPavlovServerControllerUser> GetEmailStore()
    //{
    //    if (!_userManager.SupportsUserEmail)
    //    {
    //        throw new NotSupportedException("The default UI requires a user store with email support.");
    //    }
    //    return (IUserEmailStore<PterodactylPavlovServerControllerUser>)_userStore;
    //}
}
