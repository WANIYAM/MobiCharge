using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MobileRechargeApp.Models
{
    // Used in Users list view
    public class AdminUserViewModel
    {
        public IdentityUser User     { get; set; }
        public string       FullName { get; set; }
        public string       PlanType { get; set; }
        public string       Role     { get; set; }
    }

    // Used in Create User form
    public class CreateUserViewModel
    {
        [Required]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Enter valid 10-digit mobile number")]
        public string MobileNumber { get; set; }

        [Required]
        public string FullName { get; set; }

        public string Email { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; }

        [Required]
        public string PlanType { get; set; } = "Prepaid";
    }

    // Used in Edit User form
    public class EditUserViewModel
    {
        public string Id { get; set; }

        [Required]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Enter valid 10-digit mobile number")]
        public string MobileNumber { get; set; }

        [Required]
        public string FullName { get; set; }

        public string Email { get; set; }

        [Required]
        public string PlanType { get; set; }

        // Optional — only update if filled
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string? NewPassword { get; set; }
    }
}
