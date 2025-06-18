using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
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
    public class DocumentsController : Controller
    {
        private readonly SPELS_TRACKING_SYSTEMContext _context;
        private readonly PermissionService _permissionService;
        private readonly IHubContext<NotificationHub> _hubContext;

        public DocumentsController(SPELS_TRACKING_SYSTEMContext context, PermissionService permissionService, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _permissionService = permissionService;
            _hubContext = hubContext;
        }

        // GET: Documents
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
            ViewBag.StatusList = Enum.GetValues(typeof(StatusType))
                .Cast<StatusType>()
                .Select(e => new SelectListItem
                {
                    Value = ((int)e).ToString(),
                    Text = EnumHelper.GetEnumDisplayName(e)
                })
                .ToList();

            ViewBag.SubmissionList = Enum.GetValues(typeof(SubmissionType))
                .Cast<SubmissionType>()
                .Select(e => new SelectListItem
                {
                    Value = ((int)e).ToString(),
                    Text = EnumHelper.GetEnumDisplayName(e)
                })
                .ToList();

            var listDocs = await _context.Document
            .Include(d => d.SpecialEligibility)
            .Include(d => d.OtherFOs)
            .Include(d => d.ReleasingStages)
            .ToListAsync();

            var document = id.HasValue ? await _context.Document.FindAsync(id) : new Document();

            if (document == null)
            {
                return NotFound();
            }

            // Get the logged-in user from session
            string username = HttpContext.Session.GetString("Username");
            var admin = HttpContext.Session.GetInt32("IsAdmin");

            var user = await _context.User
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var enabledStages = await _context.RolePermission
                .Where(rp => rp.RoleID == user.RoleID && rp.CanAccess)
                .Select(rp => rp.StageName)
                .ToListAsync();

            ViewBag.EnabledStages = enabledStages;

            var documentsPermission = await _context.RolePermission
                .FirstOrDefaultAsync(rp => rp.RoleID == user.RoleID && rp.StageName == "Documents");

            if (admin != 1)
            {
                if (documentsPermission == null || !documentsPermission.CanAccess)
                {
                    return RedirectToAction("Login", "Account");
                }

                ViewBag.CanAdd = documentsPermission?.CanAdd ?? false;
                ViewBag.CanExport = documentsPermission?.CanExport ?? false;
                ViewBag.CanEdit = documentsPermission?.CanEdit ?? false;
                ViewBag.CanRemove = documentsPermission?.CanRemove ?? false;
            }
            else
            {
                ViewBag.CanRemove = true;
                ViewBag.CanEdit = true;
                ViewBag.CanAdd = true; 
                ViewBag.CanExport = true;
            }

            var docsVM = new DocumentVM
            {   
                Documents = listDocs,
                Document = document
            };
            return View(docsVM);
        }

        public async Task<IActionResult> ExportToExcel(DateTime fromDate, DateTime toDate, string fileName)
        {   
            var documents = await _context.Document
                .Where(d => d.DateReceived >= fromDate && d.DateReceived <= toDate)
                .Include(d => d.SpecialEligibility)
                .Include(d => d.ReleasingStages)
                .ToListAsync();

            // Log if no data is found
            if (!documents.Any())
            {
                return BadRequest("No records found within the selected date range.");
            }

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Document");

                worksheet.Cell(1, 1).Value = "Last Name";
                worksheet.Cell(1, 2).Value = "First Name";
                worksheet.Cell(1, 3).Value = "Middle Name";
                worksheet.Cell(1, 4).Value = "Special Eligibility";
                worksheet.Cell(1, 5).Value = "Status";
                worksheet.Cell(1, 6).Value = "Remarks";
                worksheet.Cell(1, 7).Value = "Date Received";
                worksheet.Cell(1, 8).Value = "Date Entered";
                worksheet.Cell(1, 9).Value = "Date Approved";

                var header = worksheet.Range("A1:I1");
                header.Style.Font.Bold = true;
                    
                int row = 2;
                foreach (var doc in documents)
                {
                    worksheet.Cell(row, 1).Value = doc.Lastname;
                    worksheet.Cell(row, 2).Value = doc.Firstname;
                    worksheet.Cell(row, 3).Value = string.Equals(doc.Middlename, "N/A", StringComparison.OrdinalIgnoreCase) ? "" : doc.Middlename ?? "";
                    worksheet.Cell(row, 4).Value = doc.SpecialEligibility?.SpecialEligibilityName ?? "N/A";
                    worksheet.Cell(row, 5).Value = doc.StatusTypeName?.ToString() ?? "N/A";
                    worksheet.Cell(row, 6).Value = string.Equals(doc.Remarks, "N/A", StringComparison.OrdinalIgnoreCase) ? "" : doc.Remarks ?? "";
                    worksheet.Cell(row, 7).Value = doc.DateReceived.ToString("MM/dd/yyyy");
                    worksheet.Cell(row, 8).Value = doc.DateEntered.ToString("MM/dd/yyyy");
                    worksheet.Cell(row, 9).Value = doc.ReleasingStages
                        .OrderByDescending(a => a.DateApproved)
                        .FirstOrDefault()?.DateApproved.ToString("MM/dd/yyyy") ?? "";
                        
                    row++;
                }
                        
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);

                    return File(stream.ToArray(),
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                $"{fileName}.xlsx");
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DocumentVM vm)
        {
            vm.Document.DateEntered = DateTime.Now;
            _context.Document.Add(vm.Document);
            await _context.SaveChangesAsync();

            var evaluation = new EvaluationStage
            {
                DocumentID = vm.Document.DocumentID,
                DateActed = DateTime.Now
            };
            _context.Add(evaluation);
            await _context.SaveChangesAsync();

            var history = new DocumentHistory
            {
                DocumentID = vm.Document.DocumentID,
                ActionType = "Created",
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

            try
            {
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", "A new document was created.");
            }
            catch (Exception ex)
            {
                // Log exception
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult GetDocs(int getID)
        {
            var document = _context.Document
                .Where(d => d.DocumentID == getID)
                .Select(d => new
                {
                    documentid = d.DocumentID,
                    lastname = d.Lastname,
                    firstname = d.Firstname,
                    middlename = d.Middlename,
                    suffix = d.Suffix,
                    gender = d.GenderType.ToString(),
                    submission = d.SubmissionType.ToString(),
                    otherfosid = d.OtherFOsID,
                    dateofbirth = d.DateofBirth.ToString("yyyy-MM-dd"),
                    placeofbirth = d.PlaceofBirth,
                    specialeligibilityid = d.SpecialEligibilityID,
                    school = d.School,
                    address = d.Address,
                    provinceid = d.ProvinceID,
                    positionid = d.PositionID,
                    toe = d.TypeofEligibility,
                    othertoe = d.OtherEligibility,
                    remarks = d.Remarks,
                    status = d.StatusType.ToString(),
                    datereceived = d.DateReceived.ToString("MMMM dd, yyyy").ToUpper(),
                    dateentered = d.DateEntered.ToString("MMMM dd, yyyy").ToUpper()
                })
                .FirstOrDefault();

            return Json(document);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DocumentVM vm)
        {
            var docs = await _context.Document.FindAsync(vm.Document.DocumentID);
            if (docs == null)
            {
                return NotFound();
            }

            docs.SubmissionType = vm.Document.SubmissionType;
            docs.OtherFOsID = vm.Document.OtherFOsID;
            docs.Lastname = vm.Document.Lastname;
            docs.Firstname = vm.Document.Firstname;
            docs.Middlename = vm.Document.Middlename;
            docs.Suffix = vm.Document.Suffix;
            docs.GenderType = vm.Document.GenderType;
            docs.DateofBirth = vm.Document.DateofBirth;
            docs.PlaceofBirth = vm.Document.PlaceofBirth;
            docs.SpecialEligibilityID = vm.Document.SpecialEligibilityID;
            docs.School = vm.Document.School;
            docs.Address = vm.Document.Address;
            docs.ProvinceID = vm.Document.ProvinceID;
            docs.PositionID = vm.Document.PositionID;
            docs.TypeofEligibility = vm.Document.TypeofEligibility;
            docs.OtherEligibility = vm.Document.OtherEligibility;
            docs.Remarks = vm.Document.Remarks;
            docs.StatusType = vm.Document.StatusType;

            _context.Document.Update(docs);
            await _context.SaveChangesAsync();

            var history = new DocumentHistory
            {
                DocumentID = docs.DocumentID,
                ActionType = "Edited",
                ActedBy = HttpContext.Session.GetString("Fullname"),
                Timestamp = DateTime.Now
            };
            _context.DocumentHistory.Add(history);
            await _context.SaveChangesAsync();

            // Cleanup: Keep only the latest 100 entries
            var totalHistory = _context.DocumentHistory.Count();
            if (totalHistory > 100)
            {
                var oldestHistory = await _context.DocumentHistory
                    .OrderBy(h => h.Timestamp)  // Order by the oldest Timestamp
                    .FirstOrDefaultAsync();          // Get the oldest record

                if (oldestHistory != null)
                {
                    _context.DocumentHistory.Remove(oldestHistory);  // Remove the oldest entry
                    await _context.SaveChangesAsync();
                }
            }

            try
            {
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", "A document was edited.");
            }
            catch (Exception ex)
            {
                // Log exception
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<JsonResult> Delete(int intDelete)
        {
            bool result = false;
            var docs = await _context.Document.FindAsync(intDelete);
            if (docs != null)
            {
                result = true;
                _context.Document.Remove(docs);
                await _context.SaveChangesAsync();

                try
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveMessage", "A new document was deleted.");
                }
                catch (Exception ex)
                {
                    // Log exception
                }
            }
            return Json(result);
        }
    }
}
