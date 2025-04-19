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
    public class ProvincesController : Controller
    {
        private readonly MY_CSC_PROJECTContext _context;

        public ProvincesController(MY_CSC_PROJECTContext context)
        {
            _context = context;
        }

        // GET: Provinces
        public async Task<IActionResult> Index(int? id)
        {
            var listProvince = await _context.Province
            .ToListAsync();

            var province = id.HasValue ? await _context.Province.FindAsync(id) : new Province();
            if (province == null)
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

            var provinceVM = new ProvinceVM
            {
                Provinces = listProvince,
                Province = province
            };

            return View(provinceVM);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProvinceVM provinceVM)
        {
            _context.Province.Add(provinceVM.Province);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult GetProvince(int getID)
        {
            var province = _context.Province
                .Where(p => p.ProvinceID == getID)
                .Select(p => new
                {
                    provinceid = p.ProvinceID,
                    provincename = p.ProvinceName
                })
                .FirstOrDefault();

            return Json(province);
        }

        [HttpPost]
        public IActionResult Edit(ProvinceVM provinceVM)
        {
            var province = _context.Province.Find(provinceVM.Province.ProvinceID);
            if (province == null)
            {
                return NotFound();
            }

            province.ProvinceName = provinceVM.Province.ProvinceName;

            _context.Province.Update(province);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        /*// GET: Provinces/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var province = await _context.Province
                .FirstOrDefaultAsync(m => m.ProvinceID == id);
            if (province == null)
            {
                return NotFound();
            }

            return View(province);
        }

        // GET: Provinces/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Provinces/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProvinceID,ProvinceName")] Province province)
        {
            if (ModelState.IsValid)
            {
                _context.Add(province);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(province);
        }

        // GET: Provinces/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var province = await _context.Province.FindAsync(id);
            if (province == null)
            {
                return NotFound();
            }
            return View(province);
        }

        // POST: Provinces/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProvinceID,ProvinceName")] Province province)
        {
            if (id != province.ProvinceID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(province);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProvinceExists(province.ProvinceID))
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
            return View(province);
        }

        // GET: Provinces/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var province = await _context.Province
                .FirstOrDefaultAsync(m => m.ProvinceID == id);
            if (province == null)
            {
                return NotFound();
            }

            return View(province);
        }

        // POST: Provinces/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var province = await _context.Province.FindAsync(id);
            if (province != null)
            {
                _context.Province.Remove(province);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProvinceExists(int id)
        {
            return _context.Province.Any(e => e.ProvinceID == id);
        }*/
    }
}
