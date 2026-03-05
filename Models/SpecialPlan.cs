using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MobileRechargeApp.Models
{
    public class SpecialPlan
    {
        [Key]
        public int PlanId { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        public string? Description { get; set; }
        public int Validity { get; set; }
        public string? Benefits { get; set; }
    }
}