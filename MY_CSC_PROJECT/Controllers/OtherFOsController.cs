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
    public class OtherFOsController : Controller
    {
        private readonly MY_CSC_PROJECTContext _context;

        public OtherFOsController(MY_CSC_PROJECTContext context)
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
    }
}
