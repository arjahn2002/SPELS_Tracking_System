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
    public class SpecialEligibilitiesController : Controller
    {
        private readonly MY_CSC_PROJECTContext _context;

        public SpecialEligibilitiesController(MY_CSC_PROJECTContext context)
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
    }
}
