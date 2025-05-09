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
    public class ProvincesController : Controller
    {
        private readonly SPELS_TRACKING_SYSTEMContext _context;

        public ProvincesController(SPELS_TRACKING_SYSTEMContext context)
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

            [HttpPost]
            public JsonResult Delete(int intDelete)
            {
                bool result = false;
                var province = _context.Province.Find(intDelete);
                if (province != null)
                {
                    result = true;
                    _context.Province.Remove(province);
                    _context.SaveChanges();
                }
                return Json(result);
            }
    }
}
