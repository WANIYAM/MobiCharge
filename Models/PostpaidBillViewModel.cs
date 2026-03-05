namespace MobileRechargeApp.Models
{
    /// <summary>
    /// Represents a system-generated postpaid bill for the current billing cycle
    /// </summary>
    public class PostpaidBillViewModel
    {
        public string MobileNumber { get; set; }
        public string FullName { get; set; }
        public string PlanName { get; set; }
        public string BillingCycle { get; set; }   // e.g. "01 Jan 2026 – 31 Jan 2026"
        public DateTime BillDate { get; set; }
        public DateTime DueDate { get; set; }

        // Usage breakdown
        public decimal CallCharges { get; set; }
        public decimal DataCharges { get; set; }
        public decimal SmsCharges { get; set; }
        public decimal BaseRent { get; set; }
        public decimal Taxes { get; set; }

        // Totals
        public decimal TotalAmount { get; set; }
        public bool IsPaid { get; set; }
        public string Status => IsPaid ? "Paid" : "Due";

        // Past bills
        public IEnumerable<Transaction> PastBills { get; set; } = new List<Transaction>();
    }
}