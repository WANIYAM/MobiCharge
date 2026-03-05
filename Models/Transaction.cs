using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MobileRechargeApp.Models
{
    public class Transaction
    {
        [Key]
        public int TxnId { get; set; }

        [Required]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Enter a valid 10-digit mobile number")]
        public string? MobileNumber { get; set; }

        public string? PlanType { get; set; }
        public int PlanId { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        public string? PaymentMethod { get; set; }
        public string? Status { get; set; } = "Success";
        public DateTime TxnDate { get; set; } = DateTime.Now;
    }
}