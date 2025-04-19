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
using MY_CSC_PROJECT.Services;
using MY_CSC_PROJECT.ViewModels;

namespace MY_CSC_PROJECT.Controllers
{
    public class PostingStagesController : Controller
    {
        private readonly MY_CSC_PROJECTContext _context;
        private readonly PermissionService _permissionService;

        public PostingStagesController(MY_CSC_PROJECTContext context, PermissionService permissionService)
        {
            _context = context;
            _permissionService = permissionService;
        }

        // GET: PostingStages
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

            var listPosting = await _context.PostingStage
                .Include(e => e.Document)
                .ThenInclude(e => e.SpecialEligibility)
                .ToListAsync();

            var posting = id.HasValue ? await _context.PostingStage.FindAsync(id) : new PostingStage();

            if (posting == null)
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

                var postingPermission = await _context.RolePermission
                        .FirstOrDefaultAsync(rp => rp.RoleID == user.RoleID && rp.StageName == "PostingStages");

                if (postingPermission == null || !postingPermission.CanAccess)
                {
                    // Block access if no permission
                    return RedirectToAction("Login", "Account");
                }

                ViewBag.CanForward = postingPermission?.CanForward ?? false;
            }

            var postingVM = new PostingVM
            {
                Postings = listPosting,
                Posting = posting
            };

            return View(postingVM);
        }

        public IActionResult GetDocs(int getID)
        {
            var posting = _context.PostingStage
                .Where(e => e.PostingID == getID)
                .Include(d => d.Document)
                .Select(e => new
                {
                    evaluationid = e.PostingID,
                    documentid = e.Document.DocumentID,
                    lastname = e.Document.Lastname,
                    firstname = e.Document.Firstname,
                    middlename = e.Document.Middlename,
                    suffix = e.Document.Suffix,
                    gender = e.Document.Gender.ToString(),
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
                    status = e.Document.Status.ToString(),
                    dateacted = e.DateActed.ToString("MMMM dd, yyyy hh:mm tt").ToUpper()
                })
                .FirstOrDefault();

            return Json(posting);
        }

        [HttpPost]
        public async Task<IActionResult> NextStage(PostingVM postingVM)
        {
            var docs = await _context.Document.FindAsync(postingVM.Posting.DocumentID);
            if (docs != null)
            {
                switch (docs.Status)
                {
                    case StatusType.PostingStage:
                        var postingStage = await _context.PostingStage
                            .Include(d => d.Document)
                            .FirstOrDefaultAsync(r => r.PostingID == postingVM.Posting.PostingID);

                        if (postingStage != null) _context.PostingStage.Remove(postingStage);

                        docs.Remarks = "N/A";
                        docs.Status = StatusType.ApprovalStage;
                        _context.Document.Update(docs);
                        await _context.SaveChangesAsync();

                        // Move to the approval model
                        var approval = new ApprovalStage
                        {
                            DocumentID = docs.DocumentID,
                            DateActed = DateTime.Now
                        };
                        _context.ApprovalStage.Add(approval);

                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));

                    default:
                        return RedirectToAction(nameof(Index));

                };
            };
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> RejectDocs(PostingVM postingVM)
        {
            var docs = await _context.Document.FindAsync(postingVM.Posting.DocumentID);
            if (docs != null)
            {
                switch (docs.Status)
                {
                    case StatusType.PostingStage:
                        var postingStage = await _context.PostingStage
                            .Include(d => d.Document)
                            .FirstOrDefaultAsync(r => r.PostingID == postingVM.Posting.PostingID);

                        if (postingStage != null) _context.PostingStage.Remove(postingStage);

                        docs.Remarks = postingVM.Posting.Document.Remarks;
                        docs.Status = StatusType.EvaluationStage;
                        _context.Document.Update(docs);
                        await _context.SaveChangesAsync();

                        var evaluation = new EvaluationStage
                        {
                            DocumentID = docs.DocumentID,
                            DateActed = DateTime.Now
                        };
                        _context.EvaluationStage.Add(evaluation);

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
