#nullable disable
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace MobileRechargeApp.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser>  _userManager;
        private readonly ILogger<RegisterModel>      _logger;

        public RegisterModel(
            UserManager<IdentityUser>  userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel>     logger)
        {
            _userManager   = userManager;
            _signInManager = signInManager;
            _logger        = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Mobile number is required")]
            [RegularExpression(@"^[6-9]\d{9}$",
                ErrorMessage = "Enter a valid 10-digit mobile number starting with 6-9")]
            [Display(Name = "Mobile Number")]
            public string MobileNumber { get; set; }

            [Required(ErrorMessage = "Full name is required")]
            [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
            [Display(Name = "Full Name")]
            public string FullName { get; set; }

            // ✅ NEW — Prepaid or Postpaid
            [Required(ErrorMessage = "Please select your plan type")]
            [Display(Name = "Plan Type")]
            public string PlanType { get; set; }

            [Required(ErrorMessage = "Password is required")]
            [StringLength(100, ErrorMessage = "Password must be at least {2} characters.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [Required(ErrorMessage = "Please confirm your password")]
            [DataType(DataType.Password)]
            [Display(Name = "Confirm Password")]
            [Compare("Password", ErrorMessage = "Passwords do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                var user = new IdentityUser
                {
                    UserName       = Input.MobileNumber,
                    PhoneNumber    = Input.MobileNumber,
                    Email          = Input.MobileNumber + "@mobilerecharge.local",
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    // ✅ Save FullName claim
                    await _userManager.AddClaimAsync(user,
                        new Claim("FullName", Input.FullName));

                    // ✅ Save PlanType claim — "Prepaid" or "Postpaid"
                    await _userManager.AddClaimAsync(user,
                        new Claim("PlanType", Input.PlanType));

                    _logger.LogInformation("New user registered: {Mobile} ({Plan})",
                        Input.MobileNumber, Input.PlanType);

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }
    }
}
