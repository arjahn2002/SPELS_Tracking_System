using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MY_CSC_PROJECT.Data;
using MY_CSC_PROJECT.Models;
using MY_CSC_PROJECT.Services;
using MY_CSC_PROJECT.ViewModels;

namespace MY_CSC_PROJECT.Controllers
{
    public class AccountController : Controller
    {
        private readonly MY_CSC_PROJECTContext _context;
        private readonly PermissionService _permissionService;

        public AccountController(MY_CSC_PROJECTContext context, PermissionService permissionService)
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
            if (model.Username == "admin" && model.Password == "admin123")
            {
                HttpContext.Session.SetString("Username", "admin");
                HttpContext.Session.SetString("SuperAdmin", "superadmin");
                return RedirectToAction("Index", "Documents");
            }
            
            var user = await _context.User
                .FirstOrDefaultAsync(u => u.Username == model.Username && u.Password == model.Password);

            if (user != null)
            {
                HttpContext.Session.SetString("Username", user.Username);

                var rolePermissions = await _context.RolePermission
                    .Where(rp => rp.RoleID == user.RoleID && rp.CanAccess)
                    .Select(rp => rp.StageName)
                    .ToListAsync();

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
