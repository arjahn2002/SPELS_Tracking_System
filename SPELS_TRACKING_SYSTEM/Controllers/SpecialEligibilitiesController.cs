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
    public class SpecialEligibilitiesController : Controller
    {
        private readonly SPELS_TRACKING_SYSTEMContext _context;

        public SpecialEligibilitiesController(SPELS_TRACKING_SYSTEMContext context)
        {
            _context = context;
        }

        // GET: SpecialEligibilities
        public async Task<IActionResult> Index(int? id)
        {
            var listSE = await _context.SpecialEligibility
            .ToListAsync();

            var SE = id.HasValue ? await _context.SpecialEligibility.FindAsync(id) : new SpecialEligibility();
            if (SE == null)
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

            var SEVM = new SpecialEligibilityVM
            {
                SpecialEligibilities = listSE,
                SpecialEligibility = SE
            };

            return View(SEVM);
        }

        [HttpPost]
        public async Task<IActionResult> Create(SpecialEligibilityVM SEVM)
        {
            _context.SpecialEligibility.Add(SEVM.SpecialEligibility);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult GetSE(int getID)
        {
            var SE = _context.SpecialEligibility
                .Where(s => s.SpecialEligibilityID == getID)
                .Select(s => new
                {
                    specialeligibilityid = s.SpecialEligibilityID,
                    specialeligibilityname = s.SpecialEligibilityName
                })
                .FirstOrDefault();

            return Json(SE);
        }

        [HttpPost]
        public IActionResult Edit(SpecialEligibilityVM SEVM)
        {
            var SE = _context.SpecialEligibility.Find(SEVM.SpecialEligibility.SpecialEligibilityID);
            if (SE == null)
            {
                return NotFound();
            }

            SE.SpecialEligibilityName = SEVM.SpecialEligibility.SpecialEligibilityName;

            _context.SpecialEligibility.Update(SE);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public JsonResult Delete(int intDelete)
        {
            bool result = false;
            var se = _context.SpecialEligibility.Find(intDelete);
            if (se != null)
            {
                result = true;
                _context.SpecialEligibility.Remove(se);
                _context.SaveChanges();
            }
            return Json(result);
        }
    }
}
