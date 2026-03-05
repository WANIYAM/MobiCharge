using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MobileRechargeApp.Models
{
    public class TopUpPlan
    {
        [Key]
        public int PlanId { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TalkTime { get; set; }

        public int Validity { get; set; }
        public string? Description { get; set; }
    }
}