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
using Microsoft.AspNetCore.SignalR;
using SPELS_TRACKING_SYSTEM.Hubs;

namespace SPELS_TRACKING_SYSTEM.Controllers
{
    public class ReleasingStagesController : Controller
    {
        private readonly SPELS_TRACKING_SYSTEMContext _context;
        private readonly PermissionService _permissionService;
        private readonly IHubContext<NotificationHub> _hubContext;

        public ReleasingStagesController(SPELS_TRACKING_SYSTEMContext context, PermissionService permissionService, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _permissionService = permissionService;
            _hubContext = hubContext;
        }

        // GET: ReleasingStages
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

            var listReleasing = await _context.ReleasingStage
                .Include(e => e.Document)
                .ThenInclude(e => e.SpecialEligibility)
                .ToListAsync();

            var releasing = id.HasValue ? await _context.ReleasingStage.FindAsync(id) : new ReleasingStage();

            if (releasing == null)
            {
                return NotFound();
            }

            string username = HttpContext.Session.GetString("Username");
            var admin = HttpContext.Session.GetInt32("IsAdmin");

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

            var releasingPermission = await _context.RolePermission
                    .FirstOrDefaultAsync(rp => rp.RoleID == user.RoleID && rp.StageName == "ReleasingStages");

            if (admin != 1)
            {
                if (releasingPermission == null || !releasingPermission.CanAccess)
                {
                    // Block access if no permission
                    return RedirectToAction("Login", "Account");
                }
                 
                ViewBag.CanForward = releasingPermission?.CanForward ?? false;
            }
            else
            {
                ViewBag.CanForward = true;
            }

            var releasingVM = new ReleasingVM
            {
                Releasings = listReleasing,
                Releasing = releasing
            };

            return View(releasingVM);
        }

        public IActionResult GetDocs(int getID)
        {
            var authNCert = _context.ReleasingStage
                .Where(e => e.ReleasingID == getID)
                .Include(d => d.Document)
                .Select(e => new
                {
                    releasingid = e.ReleasingID,
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
                    dateapproved = e.DateApproved.ToString("MMMM dd, yyyy hh:mm tt").ToUpper()
                })
                .FirstOrDefault();

            return Json(authNCert);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DocsApprove(ReleasingVM releasingVM)
        {
            var docs = await _context.Document.FindAsync(releasingVM.Releasing.DocumentID);
            var releasing = await _context.ReleasingStage.FindAsync(releasingVM.Releasing.ReleasingID);
            if (docs != null && releasing != null)
            {
                switch (docs.StatusType)
                {
                    case 6:
                        docs.Remarks = "Approved";
                        /*releasing.DateActed = releasingVM.Releasing.DateActed = DateTime.Now;*/

                        _context.Document.Update(docs);
                        _context.ReleasingStage.Update(releasing);
                        await _context.SaveChangesAsync();

                        try
                        {
                            await _hubContext.Clients.All.SendAsync("ReceiveMessage", "A new document was forwarded.");
                        }
                        catch (Exception ex)
                        {
                            // Log exception
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
