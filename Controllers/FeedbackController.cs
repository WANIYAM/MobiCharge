using Microsoft.AspNetCore.Mvc;
using MobileRechargeApp.Data;
using MobileRechargeApp.Models;

namespace MobileRechargeApp.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly ApplicationDbContext _db;

        public FeedbackController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index() => View();

        [HttpPost]
        public async Task<IActionResult> Index(Feedback model)
        {
            if (!ModelState.IsValid) return View(model);
            _db.Feedbacks.Add(model);
            await _db.SaveChangesAsync();
            ViewBag.Success = "Thank you for your feedback!";
            return View();
        }
    }
}