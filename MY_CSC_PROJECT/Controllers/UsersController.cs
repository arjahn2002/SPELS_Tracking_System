using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MY_CSC_PROJECT.Data;
using MY_CSC_PROJECT.Models;
using MY_CSC_PROJECT.ViewModels;

namespace MY_CSC_PROJECT.Controllers
{
    public class UsersController : Controller
    {
        private readonly MY_CSC_PROJECTContext _context;

        public UsersController(MY_CSC_PROJECTContext context)
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
        public async Task<IActionResult> Create(UserVM model)
        {
            // Check if username already exists
            var existingUser = await _context.User.AnyAsync(u => u.Username == model.Username);
            if (existingUser)
            {
                ModelState.AddModelError("Username", "This username is already taken.");
                model.Users = await _context.User.ToListAsync();
                return View(model);
            }
            // Create new user
            var newUser = new User
            {
                Username = model.Username,
                Password = model.Password,
                RoleID = model.RoleID
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
                    username = u.Username,
                    password = u.Password,
                    roleid = u.RoleID
                })
                .FirstOrDefault();

            return Json(user);
        }

        [HttpPost]
        public IActionResult Edit(UserVM userVM)
        {
            var user = _context.User.Find(userVM.User.UserID);
            if (user == null)
            {
                return NotFound();
            }

            user.Username = userVM.User.Username;
            user.Password = userVM.User.Password;
            user.RoleID = userVM.User.RoleID;

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

        /*// GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User
                .Include(u => u.UserRole)
                .FirstOrDefaultAsync(m => m.UserID == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            ViewData["UserRoleID"] = new SelectList(_context.UserRole, "RoleID", "RoleID");
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserID,Username,Password,UserRoleID")] User user)
        {
            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserRoleID"] = new SelectList(_context.UserRole, "RoleID", "RoleID", user.UserRoleID);
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            ViewData["UserRoleID"] = new SelectList(_context.UserRole, "RoleID", "RoleID", user.UserRoleID);
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserID,Username,Password,UserRoleID")] User user)
        {
            if (id != user.UserID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.UserID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserRoleID"] = new SelectList(_context.UserRole, "RoleID", "RoleID", user.UserRoleID);
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User
                .Include(u => u.UserRole)
                .FirstOrDefaultAsync(m => m.UserID == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.User.FindAsync(id);
            if (user != null)
            {
                _context.User.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.User.Any(e => e.UserID == id);
        }*/
    }
}
