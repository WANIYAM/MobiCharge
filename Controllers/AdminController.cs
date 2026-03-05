using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MobileRechargeApp.Data;
using MobileRechargeApp.Models;

namespace MobileRechargeApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public AdminController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // ─── Dashboard ───────────────────────────────────────────────────────
        public async Task<IActionResult> Dashboard()
        {
            ViewBag.TotalUsers = await _db.Users.CountAsync();
            ViewBag.TotalTransactions = await _db.Transactions.CountAsync();
            ViewBag.TotalRevenue = await _db.Transactions.SumAsync(t => t.Amount);
            ViewBag.TotalFeedbacks = await _db.Feedbacks.CountAsync();
            ViewBag.TotalTopUpPlans = await _db.TopUpPlans.CountAsync();
            ViewBag.TotalSpecialPlans = await _db.SpecialPlans.CountAsync();
            ViewBag.RecentTransactions = await _db.Transactions
                .OrderByDescending(t => t.TxnDate).Take(5).ToListAsync();
            return View();
        }

        // ─── Transactions ────────────────────────────────────────────────────
        public async Task<IActionResult> Transactions(string date = null)
        {
            var query = _db.Transactions.AsQueryable();
            if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out DateTime filterDate))
            {
                query = query.Where(t => t.TxnDate.Date == filterDate.Date);
                ViewBag.FilterDate = date;
            }
            return View(await query.OrderByDescending(t => t.TxnDate).ToListAsync());
        }

        // ✅ Delete Transaction
        [HttpPost]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            var txn = await _db.Transactions.FindAsync(id);
            if (txn != null)
            {
                _db.Transactions.Remove(txn);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Transaction deleted successfully.";
            }
            return RedirectToAction("Transactions");
        }

        // ─── Users ───────────────────────────────────────────────────────────
        public async Task<IActionResult> Users()
        {
            var users = await _db.Users.ToListAsync();
            // Load FullName + PlanType claims for each user
            var result = new List<AdminUserViewModel>();
            foreach (var u in users)
            {
                var claims = await _userManager.GetClaimsAsync(u);
                var roles = await _userManager.GetRolesAsync(u);
                result.Add(new AdminUserViewModel
                {
                    User = u,
                    FullName = claims.FirstOrDefault(c => c.Type == "FullName")?.Value ?? "—",
                    PlanType = claims.FirstOrDefault(c => c.Type == "PlanType")?.Value ?? "—",
                    Role = roles.FirstOrDefault() ?? "User"
                });
            }
            return View(result);
        }

        // ✅ Create User - GET
        public IActionResult CreateUser() => View();

        // ✅ Create User - POST
        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new IdentityUser
            {
                UserName = model.MobileNumber,
                Email = model.Email,
                PhoneNumber = model.MobileNumber
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError("", e.Description);
                return View(model);
            }

            await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("FullName", model.FullName));
            await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("PlanType", model.PlanType));

            TempData["Success"] = $"User {model.MobileNumber} created successfully!";
            return RedirectToAction("Users");
        }

        // ✅ Edit User - GET
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var claims = await _userManager.GetClaimsAsync(user);
            var model = new EditUserViewModel
            {
                Id = user.Id,
                MobileNumber = user.UserName,
                Email = user.Email ?? "",
                FullName = claims.FirstOrDefault(c => c.Type == "FullName")?.Value ?? "",
                PlanType = claims.FirstOrDefault(c => c.Type == "PlanType")?.Value ?? "Prepaid"
            };
            return View(model);
        }

        // ✅ Edit User - POST
        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            user.UserName = model.MobileNumber;
            user.Email = model.Email;
            user.PhoneNumber = model.MobileNumber;
            await _userManager.UpdateAsync(user);

            // Update FullName claim
            var claims = await _userManager.GetClaimsAsync(user);
            var nameClaim = claims.FirstOrDefault(c => c.Type == "FullName");
            if (nameClaim != null) await _userManager.ReplaceClaimAsync(user, nameClaim,
                new System.Security.Claims.Claim("FullName", model.FullName));
            else await _userManager.AddClaimAsync(user,
                new System.Security.Claims.Claim("FullName", model.FullName));

            // Update PlanType claim
            var planClaim = claims.FirstOrDefault(c => c.Type == "PlanType");
            if (planClaim != null) await _userManager.ReplaceClaimAsync(user, planClaim,
                new System.Security.Claims.Claim("PlanType", model.PlanType));
            else await _userManager.AddClaimAsync(user,
                new System.Security.Claims.Claim("PlanType", model.PlanType));

            // Update password if provided
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
            }

            TempData["Success"] = "User updated successfully!";
            return RedirectToAction("Users");
        }

        // ✅ Delete User - POST
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
                TempData["Success"] = "User deleted successfully.";
            }
            return RedirectToAction("Users");
        }

        // ─── Feedbacks ───────────────────────────────────────────────────────
        public async Task<IActionResult> Feedbacks()
        {
            var feedbacks = await _db.Feedbacks.OrderByDescending(f => f.SubmittedAt).ToListAsync();
            return View(feedbacks);
        }

        // ✅ Delete Feedback
        [HttpPost]
        public async Task<IActionResult> DeleteFeedback(int id)
        {
            var fb = await _db.Feedbacks.FindAsync(id);
            if (fb != null)
            {
                _db.Feedbacks.Remove(fb);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Feedback deleted.";
            }
            return RedirectToAction("Feedbacks");
        }

        // ============ TOP UP PLANS ============
        public async Task<IActionResult> TopUpPlans() =>
            View(await _db.TopUpPlans.ToListAsync());

        public IActionResult AddTopUpPlan() => View();

        [HttpPost]
        public async Task<IActionResult> AddTopUpPlan(TopUpPlan model)
        {
            if (!ModelState.IsValid) return View(model);
            _db.TopUpPlans.Add(model);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Top Up Plan added successfully!";
            return RedirectToAction("TopUpPlans");
        }

        public async Task<IActionResult> EditTopUpPlan(int id)
        {
            var plan = await _db.TopUpPlans.FindAsync(id);
            if (plan == null) return NotFound();
            return View(plan);
        }

        [HttpPost]
        public async Task<IActionResult> EditTopUpPlan(TopUpPlan model)
        {
            if (!ModelState.IsValid) return View(model);
            _db.TopUpPlans.Update(model);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Top Up Plan updated!";
            return RedirectToAction("TopUpPlans");
        }

        public async Task<IActionResult> DeleteTopUpPlan(int id)
        {
            var plan = await _db.TopUpPlans.FindAsync(id);
            if (plan != null) { _db.TopUpPlans.Remove(plan); await _db.SaveChangesAsync(); }
            TempData["Success"] = "Top Up Plan deleted!";
            return RedirectToAction("TopUpPlans");
        }

        // ============ SPECIAL PLANS ============
        public async Task<IActionResult> SpecialPlans() =>
            View(await _db.SpecialPlans.ToListAsync());

        public IActionResult AddSpecialPlan() => View();

        [HttpPost]
        public async Task<IActionResult> AddSpecialPlan(SpecialPlan model)
        {
            if (!ModelState.IsValid) return View(model);
            _db.SpecialPlans.Add(model);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Special Plan added successfully!";
            return RedirectToAction("SpecialPlans");
        }

        public async Task<IActionResult> EditSpecialPlan(int id)
        {
            var plan = await _db.SpecialPlans.FindAsync(id);
            if (plan == null) return NotFound();
            return View(plan);
        }

        [HttpPost]
        public async Task<IActionResult> EditSpecialPlan(SpecialPlan model)
        {
            if (!ModelState.IsValid) return View(model);
            _db.SpecialPlans.Update(model);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Special Plan updated!";
            return RedirectToAction("SpecialPlans");
        }

        public async Task<IActionResult> DeleteSpecialPlan(int id)
        {
            var plan = await _db.SpecialPlans.FindAsync(id);
            if (plan != null) { _db.SpecialPlans.Remove(plan); await _db.SaveChangesAsync(); }
            TempData["Success"] = "Special Plan deleted!";
            return RedirectToAction("SpecialPlans");
        }
    }
}