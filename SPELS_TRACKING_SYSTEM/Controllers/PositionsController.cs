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
    public class PositionsController : Controller
    {
        private readonly SPELS_TRACKING_SYSTEMContext _context;

        public PositionsController(SPELS_TRACKING_SYSTEMContext context)
        {
            _context = context;
        }

        // GET: Positions
        public async Task<IActionResult> Index(int? id)
        {
            var listPosition = await _context.Position
            .ToListAsync();

            var position = id.HasValue ? await _context.Position.FindAsync(id) : new Position();
            if (position == null)
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

            var positionVM = new PositionVM
            {
                Positions = listPosition,
                Position = position
            };

            return View(positionVM);
        }

        [HttpPost]
        public async Task<IActionResult> Create(PositionVM positionVM)
        {
            _context.Position.Add(positionVM.Position);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult GetPosition(int getID)
        {
            var position = _context.Position
                .Where(p => p.PositionID == getID)
                .Select(p => new
                {
                    positionid = p.PositionID,
                    positionname = p.PositionName
                })
                .FirstOrDefault();

            return Json(position);
        }

        [HttpPost]
        public IActionResult Edit(PositionVM positionVM)
        {
            var position = _context.Position.Find(positionVM.Position.PositionID);
            if (position == null)
            {
                return NotFound();
            }

            position.PositionName = positionVM.Position.PositionName;

            _context.Position.Update(position);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public JsonResult Delete(int intDelete)
        {
            bool result = false;
            var position = _context.Position.Find(intDelete);
            if (position != null)
            {
                result = true;
                _context.Position.Remove(position);
                _context.SaveChanges();
            }
            return Json(result);
        }
    }
}
