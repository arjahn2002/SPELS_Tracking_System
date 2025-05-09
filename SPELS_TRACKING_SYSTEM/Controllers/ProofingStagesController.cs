using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SPELS_TRACKING_SYSTEM.Data;
using SPELS_TRACKING_SYSTEM.Models;
using SPELS_TRACKING_SYSTEM.Services;
using SPELS_TRACKING_SYSTEM.ViewModels;
using SPELS_TRACKING_SYSTEM.Helper;

namespace SPELS_TRACKING_SYSTEM.Controllers
{
    public class ProofingStagesController : Controller
    {
        private readonly SPELS_TRACKING_SYSTEMContext _context;
        private readonly PermissionService _permissionService;

        public ProofingStagesController(SPELS_TRACKING_SYSTEMContext context, PermissionService permissionService)
        {
            _context = context;
            _permissionService = permissionService;
        }

        // GET: ProofingStages
        public async Task<IActionResult> Index(int? id)
        {
            ViewBag.GenderList = Enum.GetValues(typeof(GenderType))
                .Cast<GenderType>()
                .Select(e => new SelectListItem
                {
                    Value = ((int)e).ToString(),
                    Text = EnumHelper.GetEnumDisplayName(e)
                })
                .ToList();
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

            var listProofing = await _context.ProofingStage
                .Include(e => e.Document)
                .ThenInclude(e => e.SpecialEligibility)
                .ToListAsync();

            var proofing = id.HasValue ? await _context.ProofingStage.FindAsync(id) : new ProofingStage();

            if (proofing == null)
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

                var proofingPermission = await _context.RolePermission
                        .FirstOrDefaultAsync(rp => rp.RoleID == user.RoleID && rp.StageName == "ProofingStages");

                if (proofingPermission == null || !proofingPermission.CanAccess)
                {
                    // Block access if no permission
                    return RedirectToAction("Login", "Account");
                }

                ViewBag.CanForward = proofingPermission?.CanForward ?? false;
            }

            var proofingVM = new ProofingVM
            {
                Proofings = listProofing,
                Proofing = proofing
            };

            return View(proofingVM);
        }

        public IActionResult GetDocs(int getID)
        {
            var proofing = _context.ProofingStage
                .Where(e => e.ProofingID == getID)
                .Include(d => d.Document)
                .Select(e => new
                {
                    proofingid = e.ProofingID,
                    documentid = e.Document.DocumentID,
                    lastname = e.Document.Lastname,
                    firstname = e.Document.Firstname,
                    middlename = e.Document.Middlename,
                    suffix = e.Document.Suffix,
                    gender = e.Document.GenderType.ToString(),
                    submission = e.Document.SubmissionType.ToString(),
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
                    status = e.Document.StatusType.ToString(),
                    dateacted = e.DateActed.ToString("MMMM dd, yyyy hh:mm tt").ToUpper()
                })
                .FirstOrDefault();

            return Json(proofing);
        }

        [HttpPost]
        public async Task<IActionResult> NextStage(ProofingVM proofingVM)
        {
            var docs = await _context.Document.FindAsync(proofingVM.Proofing.DocumentID);
            if (docs != null)
            {
                switch (docs.StatusType)
                {
                    case 2:
                        var proofingStage = await _context.ProofingStage
                            .Include(d => d.Document)
                            .FirstOrDefaultAsync(r => r.ProofingID == proofingVM.Proofing.ProofingID);

                        if (proofingStage != null) _context.ProofingStage.Remove(proofingStage);

                        docs.Remarks = "N/A";
                        docs.StatusType = 3;
                        _context.Document.Update(docs);
                        await _context.SaveChangesAsync();

                        // Move to the posting model
                        var posting = new PostingStage
                        {
                            DocumentID = docs.DocumentID,
                            DateActed = DateTime.Now
                        };
                        _context.PostingStage.Add(posting);
                        await _context.SaveChangesAsync();

                        var history = new DocumentHistory
                        {
                            DocumentID = docs.DocumentID,
                            ActionType = "Forwarded",
                            ActedBy = HttpContext.Session.GetString("Fullname"),
                            Timestamp = DateTime.Now
                        };
                        _context.DocumentHistory.Add(history);
                        await _context.SaveChangesAsync();

                        // Cleanup: Keep only the latest 100 entries
                        var totalHistory = _context.DocumentHistory.Count();
                        if (totalHistory > 100)
                        {
                            var oldestHistory = _context.DocumentHistory
                                .OrderBy(h => h.Timestamp)  // Order by the oldest Timestamp
                                .FirstOrDefault();          // Get the oldest record

                            if (oldestHistory != null)
                            {
                                _context.DocumentHistory.Remove(oldestHistory);  // Remove the oldest entry
                                await _context.SaveChangesAsync();
                            }
                        }
                        return RedirectToAction(nameof(Index));

                    default:
                        return RedirectToAction(nameof(Index));

                };
            };
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> RejectDocs(ProofingVM proofingVM)
        {
            var docs = await _context.Document.FindAsync(proofingVM.Proofing.DocumentID);
            if (docs != null)
            {
                switch (docs.StatusType)
                {
                    case 2:
                        var proofingStage = await _context.ProofingStage
                            .Include(d => d.Document)
                            .FirstOrDefaultAsync(r => r.ProofingID == proofingVM.Proofing.ProofingID);

                        if (proofingStage != null) _context.ProofingStage.Remove(proofingStage);

                        docs.Remarks = proofingVM.Proofing.Document.Remarks;
                        docs.StatusType = 1;
                        _context.Document.Update(docs);
                        await _context.SaveChangesAsync();

                        var evaluation = new EvaluationStage
                        {
                            DocumentID = docs.DocumentID,
                            DateActed = DateTime.Now
                        };
                        _context.EvaluationStage.Add(evaluation);
                        await _context.SaveChangesAsync();

                        var history = new DocumentHistory
                        {
                            DocumentID = docs.DocumentID,
                            ActionType = "Compliance",
                            ActedBy = HttpContext.Session.GetString("Fullname"),
                            Timestamp = DateTime.Now
                        };
                        _context.DocumentHistory.Add(history);
                        await _context.SaveChangesAsync();

                        // Cleanup: Keep only the latest 100 entries
                        var totalHistory = _context.DocumentHistory.Count();
                        if (totalHistory > 100)
                        {
                            var oldestHistory = _context.DocumentHistory
                                .OrderBy(h => h.Timestamp)  // Order by the oldest Timestamp
                                .FirstOrDefault();          // Get the oldest record

                            if (oldestHistory != null)
                            {
                                _context.DocumentHistory.Remove(oldestHistory);  // Remove the oldest entry
                                await _context.SaveChangesAsync();
                            }
                        }
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
