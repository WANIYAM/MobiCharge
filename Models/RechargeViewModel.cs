using System.ComponentModel.DataAnnotations;

namespace MobileRechargeApp.Models
{
    public class RechargeViewModel
    {
        [Required]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Enter a valid 10-digit mobile number")]
        public string? MobileNumber { get; set; }
    }

    public class PaymentViewModel
    {
        public string? MobileNumber { get; set; }
        public int PlanId { get; set; }
        public string? PlanType { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }

        [Required(ErrorMessage = "Please select a payment method")]
        public string? PaymentMethod { get; set; }
    }
    public class EditProfileViewModel
    {
        [Display(Name = "Mobile Number (Username)")]
        public string MobileNumber { get; set; }

        [Display(Name = "Phone Number")]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Enter a valid 10-digit number")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Display(Name = "New Password")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Display(Name = "Confirm New Password")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
    }
}