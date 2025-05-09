using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SPELS_TRACKING_SYSTEM.Data;
using SPELS_TRACKING_SYSTEM.Models;
using SPELS_TRACKING_SYSTEM.ViewModels;

namespace SPELS_TRACKING_SYSTEM.Controllers
{
    public class UsersController : Controller
    {
        private readonly SPELS_TRACKING_SYSTEMContext _context;

        public UsersController(SPELS_TRACKING_SYSTEMContext context)
        {
            _context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index(int? id)
        {
            ViewData["RoleID"] = new SelectList(_context.Role, "RoleID", "RoleName");

            var listUser = await _context.User
                .Include(u => u.Role)
                .ToListAsync();

            var user = id.HasValue ? await _context.User.FindAsync(id) : new User();
            if (user == null)
            {
                return NotFound();
            }

            string username = HttpContext.Session.GetString("Username");
            string superAdmin = HttpContext.Session.GetString("SuperAdmin");

            if (superAdmin == "superadmin")
            {

            }
            else
            {
                var userLogin = await _context.User.FirstOrDefaultAsync(u => u.Username == username);

                if (userLogin == null)
                {
                    return RedirectToAction("Login", "Account");
                }
            }

            var userVM = new UserVM
            {
                Users = listUser,
                User = user
            };

            return View(userVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserVM vm)
        {
            // Check if username already exists
            var existingUser = await _context.User.AnyAsync(u => u.Username == vm.Username);
            if (existingUser)
            {
                ModelState.AddModelError("Username", "This username is already taken.");
                vm.Users = await _context.User.ToListAsync();
                return View(vm);
            }
            // Create new user
            var newUser = new User
            {
                Fullname = vm.FullName,
                Username = vm.Username,
                Password = vm.Password,
                RoleID = vm.RoleID
            };

            _context.User.Add(newUser);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult GetUser(int getID)
        {
            var user = _context.User
                .Where(u => u.UserID == getID)
                .Select(u => new
                {
                    userid = u.UserID,
                    fullname = u.Fullname,
                    username = u.Username,
                    password = u.Password,
                    roleid = u.RoleID
                })
                .FirstOrDefault();

            return Json(user);
        }

        [HttpPost]
        public IActionResult Edit(UserVM vm)
        {
            var user = _context.User.Find(vm.User.UserID);
            if (user == null)
            {
                return NotFound();
            }

            user.Fullname = vm.User.Fullname;
            user.Username = vm.User.Username;
            user.Password = vm.User.Password;
            user.RoleID = vm.User.RoleID;

            _context.User.Update(user);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public JsonResult Delete(int intDelete)
        {
            bool result = false;
            var user = _context.User.Find(intDelete);
            if (user != null)
            {
                result = true;
                _context.User.Remove(user);
                _context.SaveChanges();
            }
            return Json(result);
        }
    }
}
