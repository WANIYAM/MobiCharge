using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MobileRechargeApp.Data;
using MobileRechargeApp.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace MobileRechargeApp.Controllers
{
    [Authorize]
    public class MyAccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _db;

        public MyAccountController(
            UserManager<IdentityUser> userManager,
            ApplicationDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        // ─── Dashboard ───────────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var claims = await _userManager.GetClaimsAsync(user);

            ViewBag.FullName = claims.FirstOrDefault(c => c.Type == "FullName")?.Value ?? user.UserName;
            ViewBag.MobileNumber = user.UserName;
            ViewBag.PlanType = claims.FirstOrDefault(c => c.Type == "PlanType")?.Value ?? "Prepaid";

            // ✅ DND status from TempData (persisted from DND page)
            ViewBag.DndActive = TempData["DndActive"] ?? false;
            TempData.Keep("DndActive"); // keep it for next request too

            // ✅ Caller Tune from TempData
            ViewBag.SelectedTune = TempData["SelectedTune"] ?? "Not Set";
            TempData.Keep("SelectedTune");

            // ✅ Last 5 transactions for this user
            ViewBag.Transactions = await _db.Transactions
                .Where(t => t.MobileNumber == user.UserName)
                .OrderByDescending(t => t.TxnDate)
                .Take(5)
                .ToListAsync();

            // ✅ For postpaid — bill summary
            if ((ViewBag.PlanType as string) == "Postpaid")
            {
                var now = System.DateTime.Now;
                var cycleStart = new System.DateTime(now.Year, now.Month, 1);
                var cycleEnd = cycleStart.AddMonths(1).AddDays(-1);
                var isPaid = await _db.Transactions.AnyAsync(
                    t => t.MobileNumber == user.UserName &&
                         t.PlanType == "postpaid" &&
                         t.TxnDate >= cycleStart &&
                         t.TxnDate <= cycleEnd);

                var baseRent = 299.00m;
                var callCharges = GenerateCharge(user.UserName, 1, 50, 150);
                var dataCharges = GenerateCharge(user.UserName, 2, 80, 200);
                var smsCharges = GenerateCharge(user.UserName, 3, 10, 50);
                var subtotal = baseRent + callCharges + dataCharges + smsCharges;
                var taxes = System.Math.Round(subtotal * 0.18m, 2);

                ViewBag.BillTotal = System.Math.Round(subtotal + taxes, 2);
                ViewBag.BillIsPaid = isPaid;
                ViewBag.BillDueDate = cycleStart.AddDays(15).ToString("dd MMM yyyy");
            }

            return View();
        }

        // ✅ Same charge generator as RechargeController
        private decimal GenerateCharge(string mobile, int seed, int min, int max)
        {
            int hash = mobile.Aggregate(seed, (acc, c) => acc * 31 + c) & 0x7FFFFFFF;
            return min + (hash % (max - min));
        }

        // ─── Do Not Disturb ──────────────────────────────────────────────────
        public IActionResult DoNotDisturb()
        {
            ViewBag.DndActive = TempData["DndActive"] ?? false;
            return View();
        }

        [HttpPost]
        public IActionResult DoNotDisturb(string action)
        {
            bool isActive = action == "activate";
            ViewBag.DndActive = isActive;
            ViewBag.Action = action;
            ViewBag.Message = isActive
                ? "Do Not Disturb activated! Promotional calls & SMS are now blocked."
                : "Do Not Disturb deactivated. You will receive all calls & messages.";
            TempData["DndActive"] = isActive;
            return View();
        }

        // ─── Caller Tunes ────────────────────────────────────────────────────
        public IActionResult CallerTunes() => View();

        [HttpPost]
        public IActionResult CallerTunes(string tune)
        {
            ViewBag.SelectedTune = tune;
            ViewBag.Message = $"Caller Tune '{tune}' activated successfully!";
            TempData["SelectedTune"] = tune; // ✅ persist for MyAccount Index
            return View();
        }

        // ─── Edit Profile ────────────────────────────────────────────────────
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            var claims = await _userManager.GetClaimsAsync(user);

            var model = new EditProfileViewModel
            {
                MobileNumber = user.UserName,
                FullName = claims.FirstOrDefault(c => c.Type == "FullName")?.Value ?? "",
                PhoneNumber = user.PhoneNumber ?? ""
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            var claims = await _userManager.GetClaimsAsync(user);

            // ✅ Update FullName claim
            var oldClaim = claims.FirstOrDefault(c => c.Type == "FullName");
            var newClaim = new Claim("FullName", model.FullName ?? "");

            if (oldClaim != null)
                await _userManager.ReplaceClaimAsync(user, oldClaim, newClaim);
            else
                await _userManager.AddClaimAsync(user, newClaim);

            // ✅ Update password if provided
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);
                    return View(model);
                }
            }

            ViewBag.Success = "Profile updated successfully!";
            return View(model);
        }
    }
}