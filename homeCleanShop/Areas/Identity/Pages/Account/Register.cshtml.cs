// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using homeCleanShop.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace homeCleanShop.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<homeCleanShopUser> _signInManager;
        private readonly UserManager<homeCleanShopUser> _userManager;
        private readonly IUserStore<homeCleanShopUser> _userStore;
        private readonly IUserEmailStore<homeCleanShopUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RegisterModel(
            UserManager<homeCleanShopUser> userManager,
            IUserStore<homeCleanShopUser> userStore,
            SignInManager<homeCleanShopUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            RoleManager<IdentityRole> rolemanager)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _roleManager = rolemanager;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SelectList RoleSelectList = new SelectList(new List<SelectListItem>
       {
            new SelectListItem { Selected =true, Text = "Select Role", Value = ""}, new SelectListItem { Selected =true, Text = "Admin", Value = "Admin"},
            new SelectListItem { Selected =true, Text = "Customer", Value = "Customer"},
                          }, "Value", "Text", 1);
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required(ErrorMessage = "You must enter your Name")]
            [Display(Name = "Name")]
            public string Name { get; set; }

            [Display(Name = "Address")]
            [Required(ErrorMessage = "You must enter your Address")]
            public string Address { get; set; }

            [Display(Name = "PhoneNumber")]
            [Required(ErrorMessage = "You must enter your Phone Number")]
            public int PhoneNumber { get; set; }

            [Display(Name = "User Role")]
            public string UserRole { get; set; }


        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                user.Name = Input.Name;
                user.Address = Input.Address;
                user.PhoneNumber = Input.PhoneNumber.ToString();
                user.EmailConfirmed = true;
                user.UserRole = Input.UserRole;
                var result = await _userManager.CreateAsync(user, Input.Password);

         
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created a new account with password.");

                        bool roleresult = await _roleManager.RoleExistsAsync("Admin");
                        if (!roleresult)
                        {
                            await _roleManager.CreateAsync(new IdentityRole("Admin"));
                        }
                        roleresult = await _roleManager.RoleExistsAsync("Customer");
                        if (!roleresult)
                        {
                            await _roleManager.CreateAsync(new IdentityRole("Customer"));
                        }
                        await _userManager.AddToRoleAsync(user, Input.UserRole);

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

                        if (_userManager.Options.SignIn.RequireConfirmedAccount)
                        {
                            //return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                            return RedirectToPage("Login");
                        }
                        else
                        {
                            await _signInManager.SignInAsync(user, isPersistent: false);
                            return LocalRedirect(returnUrl);
                        }
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }

                // If we got this far, something failed, redisplay form
                return Page();
     
        }

        private homeCleanShopUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<homeCleanShopUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(homeCleanShopUser)}'. " +
                    $"Ensure that '{nameof(homeCleanShopUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<homeCleanShopUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<homeCleanShopUser>)_userStore;
        }
    }
}
