using System.ComponentModel.DataAnnotations;

namespace MobileRechargeApp.Models
{
    public class Feedback
    {
        [Key]
        public int FeedbackId { get; set; }

        [Required]
        public string? Name { get; set; }

        [Required, EmailAddress]
        public string? Email { get; set; }

        [Required]
        public string? Message { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.Now;
    }
}