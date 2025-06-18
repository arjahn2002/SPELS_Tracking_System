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
    public class OtherFOsController : Controller
    {
        private readonly SPELS_TRACKING_SYSTEMContext _context;

        public OtherFOsController(SPELS_TRACKING_SYSTEMContext context)
        {
            _context = context;
        }

        // GET: OtherFOs
        public async Task<IActionResult> Index(int? id)
        {
            var listOtherFOs = await _context.OtherFOs
            .ToListAsync();

            var otherFOs = id.HasValue ? await _context.OtherFOs.FindAsync(id) : new OtherFOs();
            if (otherFOs == null)
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

            var otherFOsVM = new OtherFOsVM
            {
                OtherFOs = listOtherFOs,
                FO = otherFOs
            };

            return View(otherFOsVM);
        }

        [HttpPost]
        public async Task<IActionResult> Create(OtherFOsVM otherFOsVM)
        {
            _context.OtherFOs.Add(otherFOsVM.FO);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult GetOtherFOs(int getID)
        {
            var otherFOs = _context.OtherFOs
                .Where(o => o.OtherFOsID == getID)
                .Select(o => new
                {
                    otherfosid = o.OtherFOsID,
                    otherfosname = o.OtherFOsName
                })
                .FirstOrDefault();

            return Json(otherFOs);
        }

        [HttpPost]
        public IActionResult Edit(OtherFOsVM otherFOsVM)
        {
            var otherFOs = _context.OtherFOs.Find(otherFOsVM.FO.OtherFOsID);
            if (otherFOs == null)
            {
                return NotFound();
            }

            otherFOs.OtherFOsName= otherFOsVM.FO.OtherFOsName;

            _context.OtherFOs.Update(otherFOs);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public JsonResult Delete(int intDelete)
        {
            bool result = false;
            var otherfo = _context.OtherFOs.Find(intDelete);
            if (otherfo != null)
            {
                result = true;
                _context.OtherFOs.Remove(otherfo);
                _context.SaveChanges();
            }
            return Json(result);
        }
    }
}
