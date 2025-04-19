using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MY_CSC_PROJECT.Data;
using MY_CSC_PROJECT.Models;
using MY_CSC_PROJECT.ViewModels;

namespace MY_CSC_PROJECT.Controllers
{
    public class ODStagesController : Controller
    {
        private readonly MY_CSC_PROJECTContext _context;

        public ODStagesController(MY_CSC_PROJECTContext context)
        {
            _context = context;
        }

        // GET: ODStages
        public async Task<IActionResult> Index(int? id)
        {
            ViewBag.GenderList = new SelectList(Enum.GetValues(typeof(GenderType)));
            ViewData["SpecialEligibilityID"] = new SelectList(_context.SpecialEligibility, "SpecialEligibilityID", "SpecialEligibilityName");
            ViewData["OtherFOsID"] = new SelectList(_context.OtherFOs, "OtherFOsID", "OtherFOsName");
            ViewData["ProvinceID"] = new SelectList(_context.Province, "ProvinceID", "ProvinceName");
            ViewData["PositionID"] = new SelectList(_context.Position, "PositionID", "PositionName");
            ViewBag.StatusList = new SelectList(Enum.GetValues(typeof(StatusType)));

            ViewBag.SubmissionList = Enum.GetValues(typeof(SubmissionType))
                .Cast<SubmissionType>()
                .Select(e => new SelectListItem
                {
                    Value = ((int)e).ToString(),
                    Text = GetEnumDisplayName(e)
                })
                .ToList();

            var listOD = await _context.ODStage
                .Include(e => e.Document)
                .ThenInclude(e => e.SpecialEligibility)
                .ToListAsync();

            var OD = id.HasValue ? await _context.ODStage.FindAsync(id) : new ODStage();

            if (OD == null)
            {
                return NotFound();
            }

            string username = HttpContext.Session.GetString("Username");
            string superAdmin = HttpContext.Session.GetString("SuperAdmin");

            if (superAdmin == "superadmin")
            {
                ViewBag.CanForward = true;  // SuperAdmin can export documents
            }
            else
            {
                var user = await _context.User.FirstOrDefaultAsync(u => u.Username == username);

                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Fetch enabled stages from RolePermissions table
                var enabledStages = await _context.RolePermission
                    .Where(rp => rp.CanAccess)
                    .Select(rp => rp.StageName)
                    .ToListAsync();

                ViewBag.EnabledStages = enabledStages; // Pass it to the view

                var ODPermission = await _context.RolePermission
                        .FirstOrDefaultAsync(rp => rp.RoleID == user.RoleID && rp.StageName == "ODStages");

                if (ODPermission == null || !ODPermission.CanAccess)
                {
                    // Block access if no permission
                    return RedirectToAction("Login", "Account");
                }

                ViewBag.CanForward = ODPermission?.CanForward ?? false;
            }

            var ODVM = new ODVM
            {
                ODs = listOD,
                OD = OD
            };

            return View(ODVM);
        }

        public IActionResult GetDocs(int getID)
        {
            var approval = _context.ODStage
                .Where(e => e.ODID == getID)
                .Include(d => d.Document)
                .Select(e => new
                {
                    odid = e.ODID,
                    documentid = e.Document.DocumentID,
                    lastname = e.Document.Lastname,
                    firstname = e.Document.Firstname,
                    middlename = e.Document.Middlename,
                    suffix = e.Document.Suffix,
                    gender = e.Document.Gender.ToString(),
                    submission = e.Document.SubmissionType,
                    otherfosid = e.Document.OtherFOsID,
                    dateofbirth = e.Document.DateofBirth.ToString("yyyy-MM-dd"),
                    placeofbirth = e.Document.PlaceofBirth,
                    specialeligibilityid = e.Document.SpecialEligibilityID,
                    school = e.Document.School,
                    address = e.Document.Address,
                    provinceid = e.Document.ProvinceID,
                    positionid = e.Document.PositionID,
                    toe = e.Document.TypeofEligibility,
                    othertoe = e.Document.OtherEligibility,
                    remarks = e.Document.Remarks,
                    status = e.Document.Status.ToString(),
                    dateacted = e.DateActed.ToString("MMMM dd, yyyy hh:mm tt").ToUpper()
                })
                .FirstOrDefault();

            return Json(approval);
        }

        [HttpPost]
        public async Task<IActionResult> NextStage(ODVM odVM)
        {
            var docs = await _context.Document.FindAsync(odVM.OD.DocumentID);
            if (docs != null)
            {
                switch (docs.Status)
                {
                    case StatusType.ODStage:
                        var odStage = await _context.ODStage
                            .Include(d => d.Document)
                            .FirstOrDefaultAsync(r => r.ODID == odVM.OD.ODID);

                        if (odStage != null) _context.ODStage.Remove(odStage);

                        docs.Remarks = "N/A";
                        docs.Status = StatusType.Approved;
                        _context.Document.Update(docs);
                        await _context.SaveChangesAsync();

                        // Move to the Releasing stage
                        var releasing = new ReleasingStage
                        {
                            DocumentID = docs.DocumentID,
                            DateApproved = DateTime.Now
                        };
                        _context.ReleasingStage.Add(releasing);

                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));

                    default:
                        return RedirectToAction(nameof(Index));

                };
            };
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> RejectDocs(ODVM odVM)
        {
            var docs = await _context.Document.FindAsync(odVM.OD.DocumentID);
            var od = await _context.ODStage.FindAsync(odVM.OD.ODID);

            if (docs != null && od != null)
            {
                docs.Remarks = odVM.OD.Document.Remarks;
                od.DateActed = odVM.OD.DateActed = DateTime.Now;

                _context.ODStage.Update(od);
                _context.Document.Update(docs);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            };
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> CorrectionDocs(ODVM odVM)
        {
            var docs = await _context.Document.FindAsync(odVM.OD.DocumentID);
            if (docs != null)
            {
                switch (docs.Status)
                {
                    case StatusType.ODStage:
                        var odStage = await _context.ODStage
                            .Include(d => d.Document)
                            .FirstOrDefaultAsync(r => r.ODID == odVM.OD.ODID);

                        if (odStage != null) _context.ODStage.Remove(odStage);

                        docs.Remarks = odVM.OD.Document.Remarks;
                        docs.Status = StatusType.ProofingStage;
                        _context.Document.Update(docs);
                        await _context.SaveChangesAsync();

                        var proofing = new ProofingStage
                        {
                            DocumentID = docs.DocumentID,
                            DateActed = DateTime.Now
                        };
                        _context.ProofingStage.Add(proofing);

                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));

                    default:
                        return RedirectToAction(nameof(Index));

                };

            };
            return RedirectToAction(nameof(Index));
        }

        private string GetEnumDisplayName(SubmissionType submissionType)
        {
            var type = submissionType.GetType();
            var memberInfo = type.GetMember(submissionType.ToString());

            if (memberInfo.Length > 0)
            {
                var attributes = memberInfo[0].GetCustomAttribute<DisplayAttribute>();
                return attributes?.Name ?? submissionType.ToString();
            }

            return submissionType.ToString();
        }
    }
}
