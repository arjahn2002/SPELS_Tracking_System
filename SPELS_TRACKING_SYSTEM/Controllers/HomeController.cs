using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPELS_TRACKING_SYSTEM.Data;
using SPELS_TRACKING_SYSTEM.Models;
using SPELS_TRACKING_SYSTEM.ViewModels;
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

            var userLogin = await _context.User
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (userLogin == null || !userLogin.Role.IsAdmin)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Account");
            }
            var vm = new HomeVM
            {
                DocumentHistories = history
            };

            return View(vm);
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
