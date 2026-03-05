using Microsoft.AspNetCore.Mvc;
using MobileRechargeApp.Models; 

namespace MobileRechargeApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
        public IActionResult About() => View();
        public IActionResult Contact() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Contact(ContactViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // If validation fails, return to the view with error messages
                return View(model);
            }

            // If valid, redirect to a success page
            return RedirectToAction("Index");
        }

        public IActionResult CustomerCare() => View();
        public IActionResult SiteMap() => View();
    }
}