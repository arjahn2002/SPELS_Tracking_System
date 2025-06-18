using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPELS_TRACKING_SYSTEM.Data;
using SPELS_TRACKING_SYSTEM.Models;
using SPELS_TRACKING_SYSTEM.Services;
using SPELS_TRACKING_SYSTEM.ViewModels;

namespace SPELS_TRACKING_SYSTEM.Controllers
{
    public class AccountController : Controller
    {
        private readonly SPELS_TRACKING_SYSTEMContext _context;
        private readonly PermissionService _permissionService;

        public AccountController(SPELS_TRACKING_SYSTEMContext context, PermissionService permissionService)
        {
            _context = context;
            _permissionService = permissionService;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            var user = await _context.User
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == model.Username && u.Password == model.Password);

            if (user != null)
            {
                HttpContext.Session.SetInt32("IsAdmin", user.Role.IsAdmin ? 1 : 0);
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("Fullname", user.Fullname);

                var rolePermissions = await _context.RolePermission
                    .Where(rp => rp.RoleID == user.RoleID && rp.CanAccess)
                    .Select(rp => rp.StageName)
                    .ToListAsync();

                if (user.Role.IsAdmin == true)
                {
                    return RedirectToAction("Index", "Home");
                }

                if (rolePermissions.Any())
                {
                    HttpContext.Session.SetString("UserPermissions", string.Join(",", rolePermissions));
                    string firstStage = rolePermissions.FirstOrDefault();
                    return RedirectToAction("Index", firstStage);
                }
                else
                {
                    // No permissions assigned — redirect back to login with error
                    ModelState.AddModelError(string.Empty, "You do not have access to any stages. Contact administrator.");
                    return View(model);
                }
            }
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}
