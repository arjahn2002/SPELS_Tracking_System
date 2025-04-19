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
    public class PositionsController : Controller
    {
        private readonly MY_CSC_PROJECTContext _context;

        public PositionsController(MY_CSC_PROJECTContext context)
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

        /*// GET: Positions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var position = await _context.Position
                .FirstOrDefaultAsync(m => m.PositionID == id);
            if (position == null)
            {
                return NotFound();
            }

            return View(position);
        }

        // GET: Positions/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Positions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PositionID,PositionName")] Position position)
        {
            if (ModelState.IsValid)
            {
                _context.Add(position);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(position);
        }

        // GET: Positions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var position = await _context.Position.FindAsync(id);
            if (position == null)
            {
                return NotFound();
            }
            return View(position);
        }

        // POST: Positions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PositionID,PositionName")] Position position)
        {
            if (id != position.PositionID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(position);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PositionExists(position.PositionID))
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
            return View(position);
        }

        // GET: Positions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var position = await _context.Position
                .FirstOrDefaultAsync(m => m.PositionID == id);
            if (position == null)
            {
                return NotFound();
            }

            return View(position);
        }

        // POST: Positions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var position = await _context.Position.FindAsync(id);
            if (position != null)
            {
                _context.Position.Remove(position);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PositionExists(int id)
        {
            return _context.Position.Any(e => e.PositionID == id);
        }*/
    }
}
