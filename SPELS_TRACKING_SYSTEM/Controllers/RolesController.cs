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
    public class RolesController : Controller
    {
        private readonly SPELS_TRACKING_SYSTEMContext _context;

        public RolesController(SPELS_TRACKING_SYSTEMContext context)
        {
            _context = context;
        }

        // GET: Roles
        public async Task<IActionResult> Index(int? id)
        {
            var listRole = await _context.Role
            .ToListAsync();

            var role = id.HasValue ? await _context.Role.FindAsync(id) : new Role();
            if (role == null)
            {
                return NotFound();
            }

            string username = HttpContext.Session.GetString("Username");

            var user = await _context.User
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null || !user.Role.IsAdmin)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Account");
            }

            var roleVM = new RoleVM
            {
                Roles = listRole,
                Role = role
            };

            return View(roleVM);
        }

        [HttpPost]
        public async Task<IActionResult> Create(RoleVM roleVM)
        {
            _context.Role.Add(roleVM.Role);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult GetRole(int getID)
        {
            var role = _context.Role
                .Include(r => r.RolePermissions)
                .Where(r => r.RoleID == getID)
                .Select(r => new
                {
                    roleid = r.RoleID,
                    rolename = r.RoleName,
                    isadmin = r.IsAdmin,
                    rolepermission = r.RolePermissions.Select(rp => new
                    {
                        stageName = rp.StageName,
                        isEnabled = rp.CanAccess,
                        canAdd = rp.CanAdd,
                        canForward = rp.CanForward,
                        canEdit = rp.CanEdit,
                        canExport = rp.CanExport,
                        canRemove = rp.CanRemove
                    }).ToList()
                })
                .FirstOrDefault();

            return Json(role);
        }

        [HttpPost]
        public IActionResult Edit(RoleVM roleVM)
        {
            var role = _context.Role.Find(roleVM.Role.RoleID);
            if (role == null)
            {
                return NotFound();
            }

            role.RoleName = roleVM.Role.RoleName;
            _context.Role.Update(role);

            foreach (var stage in roleVM.Stages)
            {
                var permission = _context.RolePermission
                    .FirstOrDefault(p => p.RoleID == role.RoleID && p.StageName == stage.StageName);

                if (permission == null)
                {
                    permission = new RolePermission
                    {
                        RoleID = role.RoleID,
                        StageName = stage.StageName,
                        CanAccess = stage.CanAccess,
                        CanForward = stage.CanForward,
                        CanAdd = stage.CanAdd,
                        CanEdit = stage.CanEdit,
                        CanExport = stage.CanExport,
                        CanRemove = stage.CanRemove
                    };
                    _context.RolePermission.Add(permission);
                }
                else
                {
                    permission.CanAccess = stage.CanAccess;
                    permission.CanForward = stage.CanForward;
                    permission.CanAdd = stage.CanAdd;
                    permission.CanEdit = stage.CanEdit;
                    permission.CanExport = stage.CanExport;
                    permission.CanRemove = stage.CanRemove;
                }
            }

            _context.SaveChanges();
            return RedirectToAction("Index");
        }


        [HttpPost]
        public JsonResult Delete(int intDelete)
        {
            bool result = false;
            var role = _context.Role.Find(intDelete);
            if (role != null)
            {
                result = true;
                _context.Role.Remove(role);
                _context.SaveChanges();
            }
            return Json(result);
        }
    }
}
