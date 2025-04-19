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
using MY_CSC_PROJECT.Data;
using MY_CSC_PROJECT.Models;
using MY_CSC_PROJECT.Services;
using MY_CSC_PROJECT.ViewModels;

namespace MY_CSC_PROJECT.Controllers
{
    public class DocumentsController : Controller
    {
        private readonly MY_CSC_PROJECTContext _context;
        private readonly PermissionService _permissionService;

        public DocumentsController(MY_CSC_PROJECTContext context, PermissionService permissionService)
        {
            _context = context;
            _permissionService = permissionService;
        }

        // GET: Documents
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
            string superAdmin = HttpContext.Session.GetString("SuperAdmin");

            if (superAdmin == "superadmin")
            {
                ViewBag.CanRemove = true;  // SuperAdmin can remove documents
                ViewBag.CanEdit = true;  // SuperAdmin can edit documents
                ViewBag.CanAdd = true;  // SuperAdmin can add documents
                ViewBag.CanExport = true;  // SuperAdmin can export documents
            }
            else
            {
                var user = await _context.User.FirstOrDefaultAsync(u => u.Username == username);

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

                if (documentsPermission == null || !documentsPermission.CanAccess)
                {
                    // Block access if no permission
                    return RedirectToAction("Login", "Account");
                }

                ViewBag.CanAdd = documentsPermission?.CanAdd ?? false;
                ViewBag.CanExport = documentsPermission?.CanExport ?? false;
                ViewBag.CanEdit = documentsPermission?.CanEdit ?? false;
                ViewBag.CanRemove = documentsPermission?.CanRemove ?? false;
            }

            var docsVM = new DocumentVM
            {
                Documents = listDocs,
                Document = document
            };
            return View(docsVM);
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

        public async Task<IActionResult> ExportToExcel(DateTime fromDate, DateTime toDate, string fileName)
        {   
            var documents = await _context.Document
                .Where(d => d.DateReceived >= fromDate && d.DateReceived <= toDate)
                .Include(d => d.SpecialEligibility)
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

                var header = worksheet.Range("A1:H1");
                header.Style.Font.Bold = true;
                    
                int row = 2;
                foreach (var doc in documents)
                {
                    worksheet.Cell(row, 1).Value = doc.Lastname ?? "N/A";
                    worksheet.Cell(row, 2).Value = doc.Firstname ?? "N/A";
                    worksheet.Cell(row, 3).Value = doc.Middlename ?? "N/A";
                    worksheet.Cell(row, 4).Value = doc.SpecialEligibility?.SpecialEligibilityName ?? "N/A";
                    worksheet.Cell(row, 5).Value = doc.Status?.ToString() ?? "N/A";
                    worksheet.Cell(row, 6).Value = doc.Remarks ?? "N/A";
                    worksheet.Cell(row, 7).Value = doc.DateReceived.ToString("MM/dd/yyyy");
                    worksheet.Cell(row, 8).Value = doc.DateEntered.ToString("MM/dd/yyyy");
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
        public async Task<IActionResult> Create(DocumentVM docsVM)
        {
            docsVM.Document.DateEntered = DateTime.Now;
            _context.Document.Add(docsVM.Document);
            await _context.SaveChangesAsync();

            var evaluation = new EvaluationStage
            {
                DocumentID = docsVM.Document.DocumentID,
                DateActed = DateTime.Now
            };
            _context.Add(evaluation);
            await _context.SaveChangesAsync();
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
                    gender = d.Gender.ToString(),
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
                    status = d.Status.ToString(),
                    datereceived = d.DateReceived.ToString("MMMM dd, yyyy").ToUpper(),
                    dateentered = d.DateEntered.ToString("MMMM dd, yyyy").ToUpper()
                })
                .FirstOrDefault();

            return Json(document);
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
            docs.Suffix = docsVM.Document.Suffix;
            docs.DateofBirth = docsVM.Document.DateofBirth;
            docs.PlaceofBirth = docsVM.Document.PlaceofBirth;
            docs.SpecialEligibilityID = docsVM.Document.SpecialEligibilityID;
            docs.School = docsVM.Document.School;
            docs.Address = docsVM.Document.Address;
            docs.ProvinceID = docsVM.Document.ProvinceID;
            docs.PositionID = docsVM.Document.PositionID;
            docs.TypeofEligibility = docsVM.Document.TypeofEligibility;
            docs.OtherEligibility = docsVM.Document.OtherEligibility;
            docs.Remarks = docsVM.Document.Remarks;
            docs.Status = docsVM.Document.Status;
            docsVM.Document.DateEntered = DateTime.Now;

            _context.Document.Update(docs);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public JsonResult Delete(int intDelete)
        {
            bool result = false;
            var docs = _context.Document.Find(intDelete);
            if (docs != null)
            {
                result = true;
                _context.Document.Remove(docs);
                _context.SaveChanges();
            }
            return Json(result);
        }
    }
}
