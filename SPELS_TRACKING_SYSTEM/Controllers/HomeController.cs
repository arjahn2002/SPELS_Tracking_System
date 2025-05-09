using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPELS_TRACKING_SYSTEM.Data;
using SPELS_TRACKING_SYSTEM.Models;
using System.Diagnostics;

namespace SPELS_TRACKING_SYSTEM.Controllers
{
    public class HomeController : Controller
    {
        private readonly SPELS_TRACKING_SYSTEMContext _context;

        public HomeController(SPELS_TRACKING_SYSTEMContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
                var history = _context.DocumentHistory
                .Include(h => h.Document)
                .OrderByDescending(h => h.Timestamp)
                .ToList();

            string username = HttpContext.Session.GetString("Username");
            string superAdmin = HttpContext.Session.GetString("SuperAdmin");
            if (superAdmin == "superadmin")
            {
                
            }
            else
            {
                var user = await _context.User.FirstOrDefaultAsync(u => u.Username == username);

                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }
            }
            return View(history);
        }

        public IActionResult AboutUs()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
