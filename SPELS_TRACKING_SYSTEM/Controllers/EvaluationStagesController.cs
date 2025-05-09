using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using System.Xml.Linq;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using SPELS_TRACKING_SYSTEM.Data;
using SPELS_TRACKING_SYSTEM.Models;
using SPELS_TRACKING_SYSTEM.Services;
using SPELS_TRACKING_SYSTEM.ViewModels;
using SPELS_TRACKING_SYSTEM.Helper;

namespace SPELS_TRACKING_SYSTEM.Controllers
{
    
    public class EvaluationStagesController : Controller
    {
        private readonly SPELS_TRACKING_SYSTEMContext _context;
        private readonly PermissionService _permissionService;

        public EvaluationStagesController(SPELS_TRACKING_SYSTEMContext context, PermissionService permissionService)
        {
            _context = context;
            _permissionService = permissionService;
        }

        // GET: EvaluationStages
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

            var listEvaluation = await _context.EvaluationStage
                .Include(e => e.Document)
                .ThenInclude(e => e.SpecialEligibility)
                .ToListAsync();

            var evaluation = id.HasValue ? await _context.EvaluationStage.FindAsync(id) : new EvaluationStage();

            if (evaluation == null)
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

                var evaluationPermission = await _context.RolePermission
                        .FirstOrDefaultAsync(rp => rp.RoleID == user.RoleID && rp.StageName == "EvaluationStages");

                if (evaluationPermission == null || !evaluationPermission.CanAccess)
                {
                    // Block access if no permission
                    return RedirectToAction("Login", "Account");
                }

                ViewBag.CanForward = evaluationPermission?.CanForward ?? false;
            }

            var evaluationVM = new EvaluationVM
            {
                Evaluations = listEvaluation,
                Evaluation = evaluation
            };

            return View(evaluationVM);
        }

        public IActionResult GetDocs(int getID)
        {
            var evaluation = _context.EvaluationStage
                .Where(e => e.EvaluationID == getID)
                .Include(d => d.Document)
                .Select(e => new
                {
                    evaluationid = e.EvaluationID,
                    documentid = e.DocumentID,
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
                    remarks = e.Document.Remarks,
                    status = e.Document.StatusType.ToString(),
                    dateacted = e.DateActed.ToString("MMMM dd, yyyy hh:mm tt").ToUpper()
                })
                .FirstOrDefault();

            return Json(evaluation);
        }

        [HttpPost]
        public async Task<IActionResult> NextStage(EvaluationVM evaluationVM)
        {
            var docs = await _context.Document.FindAsync(evaluationVM.Evaluation.DocumentID);
            if (docs != null)
            {
                switch (docs.StatusType)
                {
                    case 1:
                        var evaluationStage = await _context.EvaluationStage
                            .Include(d => d.Document)
                            .FirstOrDefaultAsync(r => r.EvaluationID == evaluationVM.Evaluation.EvaluationID);

                        if (evaluationStage != null) _context.EvaluationStage.Remove(evaluationStage);

                        docs.Remarks = "N/A";
                        docs.StatusType = 2;
                        _context.Document.Update(docs);
                        await _context.SaveChangesAsync();

                        // Move to the proofing model
                        var proofing = new ProofingStage
                        {
                            DocumentID = docs.DocumentID,
                            DateActed = DateTime.Now
                        };
                        _context.ProofingStage.Add(proofing);
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
        public async Task<IActionResult> RejectDocs(EvaluationVM evaluationVM)
        {
            var docs = await _context.Document.FindAsync(evaluationVM.Evaluation.DocumentID);
            var evaluation = await _context.EvaluationStage.FindAsync(evaluationVM.Evaluation.EvaluationID);

            if (docs != null && evaluation != null)
            {
                docs.Remarks = evaluationVM.Evaluation.Document.Remarks;
                evaluation.DateActed = evaluationVM.Evaluation.DateActed = DateTime.Now;

                _context.EvaluationStage.Update(evaluation);
                _context.Document.Update(docs);
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
            };
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Edit(DocumentVM docsVM)
        {
            var docs = _context.Document.Find(docsVM.Document.DocumentID);
            if (docs == null)
            {
                return NotFound();
            }

            docs.SubmissionType = docsVM.Document.SubmissionType;
            docs.OtherFOsID = docsVM.Document.OtherFOsID;
            docs.Lastname = docsVM.Document.Lastname;
            docs.Firstname = docsVM.Document.Firstname;
            docs.Middlename = docsVM.Document.Middlename;
            docs.DateofBirth = docsVM.Document.DateofBirth;
            docs.PlaceofBirth = docsVM.Document.PlaceofBirth;
            docs.SpecialEligibilityID = docsVM.Document.SpecialEligibilityID;
            docs.School = docsVM.Document.School;
            docs.Remarks = docsVM.Document.Remarks;
            docs.StatusType = docsVM.Document.StatusType;
            docs.DateReceived = docsVM.Document.DateReceived;

            _context.Document.Update(docs);
            _context.SaveChanges();
            return RedirectToAction("Index");
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
