using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MobileRechargeApp.Data;
using MobileRechargeApp.Models;

namespace MobileRechargeApp.Controllers
{
    public class RechargeController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public RechargeController(
            ApplicationDbContext db,
            UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // ─── Online Recharge ────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            // If user is logged in, pre-fill their mobile number
            if (User.Identity!.IsAuthenticated && !User.IsInRole("Admin"))
            {
                var user = await _userManager.GetUserAsync(User);
                ViewBag.LoggedInMobile = user?.UserName ?? "";
            }
            return View();
        }

        [HttpPost]
        public IActionResult Index(RechargeViewModel model, string planType)
        {
            if (!ModelState.IsValid) return View(model);
            return RedirectToAction("Plans", new { mobile = model.MobileNumber, type = planType });
        }

        public async Task<IActionResult> Plans(string mobile, string type)
        {
            ViewBag.Mobile = mobile;
            ViewBag.Type = type;

            if (type == "topup")
            {
                var plans = await _db.TopUpPlans.ToListAsync();
                return View("TopUpPlans", plans);
            }
            else
            {
                var plans = await _db.SpecialPlans.ToListAsync();
                return View("SpecialPlans", plans);
            }
        }

        // ─── Payment ────────────────────────────────────────────────────────
        public async Task<IActionResult> Payment(string mobile, int planId, string planType)
        {
            decimal amount = 0;
            string description = "";

            if (planType == "topup")
            {
                var plan = await _db.TopUpPlans.FindAsync(planId);
                amount = plan!.Amount;
                description = plan.Description ?? "";
            }
            else
            {
                var plan = await _db.SpecialPlans.FindAsync(planId);
                amount = plan!.Amount;
                description = plan.Description ?? "";
            }

            var model = new PaymentViewModel
            {
                MobileNumber = mobile,
                PlanId = planId,
                PlanType = planType,
                Amount = amount,
                Description = description
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Payment(PaymentViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var txn = new Transaction
            {
                MobileNumber = model.MobileNumber,
                PlanType = model.PlanType,
                PlanId = model.PlanId,
                Amount = model.Amount,
                PaymentMethod = model.PaymentMethod,
                Status = "Success",
                TxnDate = DateTime.Now
            };

            _db.Transactions.Add(txn);
            await _db.SaveChangesAsync();

            return RedirectToAction("Receipt", new { txnId = txn.TxnId });
        }

        // ─── Receipt ────────────────────────────────────────────────────────
        public async Task<IActionResult> Receipt(int txnId)
        {
            var txn = await _db.Transactions.FindAsync(txnId);
            return View(txn);
        }

        // ─── Postpaid ────────────────────────────────────────────────────────
        public async Task<IActionResult> Postpaid()
        {
            // ✅ Not logged in — redirect to login page
            if (!User.Identity!.IsAuthenticated)
                return Redirect("/Identity/Account/Login?returnUrl=/Recharge/Postpaid");

            // ✅ Admin — redirect to Admin Dashboard
            if (User.IsInRole("Admin"))
                return RedirectToAction("Dashboard", "Admin");

            var user = await _userManager.GetUserAsync(User);
            var claims = await _userManager.GetClaimsAsync(user!);
            var planType = claims.FirstOrDefault(c => c.Type == "PlanType")?.Value ?? "Prepaid";
            var fullName = claims.FirstOrDefault(c => c.Type == "FullName")?.Value ?? user!.UserName;

            // ✅ Prepaid user — show notice
            if (planType != "Postpaid")
                return View("PrepaidNotice");

            // ✅ Postpaid user — get past bills
            var pastBills = await _db.Transactions
                .Where(t => t.MobileNumber == user!.UserName && t.PlanType == "postpaid")
                .OrderByDescending(t => t.TxnDate)
                .ToListAsync();

            // ✅ Billing cycle = current month
            var now = DateTime.Now;
            var cycleStart = new DateTime(now.Year, now.Month, 1);
            var cycleEnd = cycleStart.AddMonths(1).AddDays(-1);
            var isPaid = pastBills.Any(t => t.TxnDate >= cycleStart && t.TxnDate <= cycleEnd);

            // ✅ Auto-generate bill using mobile number as seed
            var baseRent = 299.00m;
            var callCharges = GenerateCharge(user!.UserName, 1, 50, 150);
            var dataCharges = GenerateCharge(user!.UserName, 2, 80, 200);
            var smsCharges = GenerateCharge(user!.UserName, 3, 10, 50);
            var subtotal = baseRent + callCharges + dataCharges + smsCharges;
            var taxes = Math.Round(subtotal * 0.18m, 2);

            var bill = new PostpaidBillViewModel
            {
                MobileNumber = user!.UserName,
                FullName = fullName,
                PlanName = "Postpaid Unlimited",
                BillingCycle = cycleStart.ToString("dd MMM yyyy") + " – " + cycleEnd.ToString("dd MMM yyyy"),
                BillDate = cycleStart,
                DueDate = cycleStart.AddDays(15),
                BaseRent = baseRent,
                CallCharges = callCharges,
                DataCharges = dataCharges,
                SmsCharges = smsCharges,
                Taxes = taxes,
                TotalAmount = Math.Round(subtotal + taxes, 2),
                IsPaid = isPaid,
                PastBills = pastBills
            };

            return View("PostpaidBill", bill);
        }

        // ✅ Consistent charge per user — same number always gets same bill
        private decimal GenerateCharge(string mobile, int seed, int min, int max)
        {
            int hash = mobile.Aggregate(seed, (acc, c) => acc * 31 + c) & 0x7FFFFFFF;
            return min + (hash % (max - min));
        }

        // ─── Postpaid Pay POST ───────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> PostpaidPay(string mobileNumber, decimal amount, string paymentMethod)
        {
            var txn = new Transaction
            {
                MobileNumber = mobileNumber,
                PlanType = "postpaid",
                PlanId = 0,
                Amount = amount,
                PaymentMethod = paymentMethod,
                Status = "Success",
                TxnDate = DateTime.Now
            };

            _db.Transactions.Add(txn);
            await _db.SaveChangesAsync();

            return RedirectToAction("Receipt", new { txnId = txn.TxnId });
        }
    }
}