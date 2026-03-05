#nullable disable

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace MobileRechargeApp.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser>  _userManager;
        private readonly ILogger<LoginModel>         _logger;

        public LoginModel(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser>  userManager,
            ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager   = userManager;
            _logger        = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            // ✅ No [EmailAddress] — accepts mobile number OR email
            [Required(ErrorMessage = "Please enter your mobile number or email.")]
            [Display(Name = "Mobile Number / Email")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Please enter your password.")]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
                ModelState.AddModelError(string.Empty, ErrorMessage);

            returnUrl ??= Url.Content("~/");

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                // ✅ Step 1: Find user by username (mobile number) first, then by email
                var user = await _userManager.FindByNameAsync(Input.Email);
                if (user == null)
                    user = await _userManager.FindByEmailAsync(Input.Email);

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid mobile number/email or password.");
                    return Page();
                }

                // ✅ Step 2: Sign in using the actual username
                var result = await _signInManager.PasswordSignInAsync(
                    user.UserName,
                    Input.Password,
                    Input.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in: {Username}", user.UserName);

                    // ✅ Step 3: Redirect admin to dashboard, users to home/returnUrl
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                        return RedirectToAction("Dashboard", "Admin");

                    return LocalRedirect(returnUrl);
                }

                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa",
                        new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }

                ModelState.AddModelError(string.Empty, "Invalid mobile number/email or password.");
                return Page();
            }

            return Page();
        }
    }
}
