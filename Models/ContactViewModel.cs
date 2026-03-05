using System.ComponentModel.DataAnnotations;

namespace MobileRechargeApp.Models
{
    public class ContactViewModel
    {
        [Required(ErrorMessage = "Please enter your full name")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Name can only contain alphabets and spaces")]
        [StringLength(100, ErrorMessage = "Name is too long")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^\+?[0-9]{10,15}$", ErrorMessage = "Enter a valid phone number (digits only)")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Please provide a subject")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-\.\,]+$", ErrorMessage = "Subject contains invalid characters")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "Please type your message")]
        [MinLength(10, ErrorMessage = "Message must be at least 10 characters")]
        public string Message { get; set; }
    }
}