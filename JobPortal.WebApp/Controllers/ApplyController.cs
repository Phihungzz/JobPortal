using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobPortal.Data.DataContext;
using JobPortal.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using JobPortal.Data.ViewModel;
using JobPortal.Common;
using JobPortal.WebApp.Services;
using System.Text;

namespace JobPortal.WebApp.Controllers
{
    [Route("apply")]
    public class ApplyController : Controller
    {
        private readonly DataDbContext _context;
        private readonly GeminiCVFilterService _cvFilterService;
        private const float MATCH_SCORE_THRESHOLD = 70.0f;

        public ApplyController(DataDbContext context, GeminiCVFilterService cvFilterService)
        {
            _context = context;
            _cvFilterService = cvFilterService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ListApplies(Guid id)
        {
            var CVs = await (from cv in _context.CVs
                             where cv.AppUserId == id
                             orderby cv.ApplyDate descending
                             select new CVsViewModel()
                             {
                                 CVId = cv.Id,
                                 Certificate = cv.Certificate,
                                 Major = cv.Major,
                                 ApplyDate = cv.ApplyDate,
                                 GraduatedAt = cv.GraduatedAt,
                                 GPA = cv.GPA,
                                 Description = cv.Description,
                                 Introduce = cv.Introduce,
                                 CVEmail = cv.Email,
                                 CVPhone = cv.Phone,
                                 UserId = cv.AppUserId,
                                 CVStatus = cv.Status,
                                 JobName = cv.Job != null ? cv.Job.Name : "Unknown Job",
                                 UserName = cv.AppUser != null ? cv.AppUser.FullName : "Unknown User",
                                 EmployerLogo = cv.Job != null && cv.Job.AppUser != null ? (cv.Job.AppUser.UrlAvatar ?? "default-avatar.png") : "default-avatar.png",
                                 EmployerAddress = cv.EmployerAddress,
                                 EmployerCity = cv.City,
                                 EmployerComment = cv.Comment,
                                 EmployerEmail = cv.EmployerEmail,
                                 EmployerPhone = cv.EmployerPhone,
                                 EmployerRating = cv.EmployerRating,
                                 CommentOn = cv.CommentOn,
                                 Skills = cv.Skills,
                                 ExperienceYears = cv.ExperienceYears,
                                 MatchScore = cv.MatchScore,
                                 CVFilePath = cv.CVFilePath
                             }).ToListAsync();

            CVs = CVs.Where(cv => cv != null).ToList();
            return View(CVs);
        }

        [Route("{slug}/{id}")]
        public IActionResult Apply(string slug, Guid id)
        {
            ViewBag.Slug = slug;
            ViewBag.UserId = id;
            return View();
        }

        [Route("{slug}/{id}")]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Apply(string slug, Guid id, CreateCVViewModel model)
        {
            var job = await _context.Jobs.Where(j => j.Slug == slug).FirstOrDefaultAsync();
            if (job == null)
            {
                ModelState.AddModelError("", "Job not found.");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                if (model.CVFile == null || model.CVFile.Length == 0)
                {
                    ModelState.AddModelError("CVFile", "Please upload a CV file.");
                    return View(model);
                }

                if (!model.CVFile.FileName.EndsWith(".pdf"))
                {
                    ModelState.AddModelError("CVFile", "Only PDF files are allowed.");
                    return View(model);
                }

                if (model.CVFile.Length > 5 * 1024 * 1024) 
                {
                    ModelState.AddModelError("CVFile", "File size must be less than 5MB.");
                    return View(model);
                }

                try
                {
                    string POST_PDF_PATH = "files/cvs/";
                    string pdfFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.CVFile.FileName);
                    var pdfPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", POST_PDF_PATH);
                    if (!Directory.Exists(pdfPath))
                    {
                        Directory.CreateDirectory(pdfPath);
                    }
                    var pdfFilePath = Path.Combine(pdfPath, pdfFileName);
                    using (var fileStream = new FileStream(pdfFilePath, FileMode.Create))
                    {
                        await model.CVFile.CopyToAsync(fileStream);
                    }

                    string cvContent = ExtractTextFromPDF(pdfFilePath);
                    System.Diagnostics.Debug.WriteLine($"Extracted CV Content: {cvContent}");
                    if (string.IsNullOrWhiteSpace(cvContent))
                    {
                        System.Diagnostics.Debug.WriteLine("Warning: Extracted CV content is empty.");
                        ModelState.AddModelError("CVFile", "The uploaded PDF does not contain readable text. Please upload a text-based PDF.");
                        return View(model);
                    }

                    var jobDescription = job.Description ?? job.Name;
                    var jobExperience = job.Experience ?? "At least 2 years of experience required.";
                    if (string.IsNullOrWhiteSpace(jobDescription) || jobDescription.Length < 20)
                    {
                        jobDescription = "Software Engineer position requiring skills in C#, Java, or Python.";
                    }
                    if (string.IsNullOrWhiteSpace(jobExperience))
                    {
                        jobExperience = "At least 2 years of experience required.";
                    }
                    System.Diagnostics.Debug.WriteLine($"Job Description: {jobDescription}");
                    System.Diagnostics.Debug.WriteLine($"Job Experience: {jobExperience}");

                    var (skills, experienceYears, matchScore) = await _cvFilterService.AnalyzeCVAsync(cvContent, jobDescription, jobExperience);
                    System.Diagnostics.Debug.WriteLine($"Gemini API Result - Skills: {skills}, ExperienceYears: {experienceYears}, MatchScore: {matchScore}");

            
                    int cvStatus = matchScore >= MATCH_SCORE_THRESHOLD ? 2 : 0; 

                    CV cv = new CV()
                    {
                        ApplyDate = DateTime.Now,
                        Certificate = model.Certificate,
                        Major = model.Major,
                        GraduatedAt = model.GraduatedAt,
                        GPA = model.GPA,
                        Description = model.Description,
                        Introduce = model.Introduce,
                        Phone = model.Phone,
                        Email = model.Email,
                        AppUserId = id,
                        JobId = job.Id,
                        Status = cvStatus, 
                        Skills = skills,
                        ExperienceYears = experienceYears,
                        MatchScore = matchScore,
                        CVFilePath = $"{POST_PDF_PATH}{pdfFileName}"
                    };
                    _context.CVs.Add(cv);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = cvStatus == 2
                        ? "CV submitted and automatically accepted!"
                        : "CV submitted but automatically rejected due to low match score.";
                    return RedirectToAction("ListApplies", new { id = id });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in Apply: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while processing your application. Please try again.");
                    return View(model);
                }
            }
            return View(model);
        }

        private string ExtractTextFromPDF(string filePath)
        {
            try
            {
                using (var pdfDocument = new iText.Kernel.Pdf.PdfDocument(new iText.Kernel.Pdf.PdfReader(filePath)))
                {
                    var text = new StringBuilder();
                    for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
                    {
                        var page = pdfDocument.GetPage(i);
                        text.Append(iText.Kernel.Pdf.Canvas.Parser.PdfTextExtractor.GetTextFromPage(page));
                    }
                    return text.ToString();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error extracting text from PDF: {ex.Message}");
                return string.Empty;
            }
        }

        [HttpGet("{id}/{CVid}/delete")]
        public IActionResult Delete(Guid id, int CVid)
        {
            try
            {
                var cv = _context.CVs.FirstOrDefault(cv => cv.Id == CVid);
                if (cv == null)
                {
                    TempData["ErrorMessage"] = "CV not found.";
                    return RedirectToAction("ListApplies", new { id = id });
                }

                string pdfFilePath = cv.CVFilePath;
                _context.CVs.Remove(cv);
                _context.SaveChanges();

                if (!string.IsNullOrEmpty(pdfFilePath))
                {
                    string fullPdfPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", pdfFilePath);
                    if (System.IO.File.Exists(fullPdfPath))
                    {
                        System.IO.File.Delete(fullPdfPath);
                    }
                }

                TempData["SuccessMessage"] = "CV deleted successfully!";
                return RedirectToAction("ListApplies", new { id = id });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in Delete: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while deleting the CV.";
                return RedirectToAction("ListApplies", new { id = id });
            }
        }

        [HttpGet("{id}/{CVid}/update/{status}")]
        public IActionResult UpdateCV(Guid id, int CVid, int status)
        {
            try
            {
                var cv = _context.CVs.FirstOrDefault(cv => cv.Id == CVid);
                if (cv == null)
                {
                    TempData["ErrorMessage"] = "CV not found.";
                    return RedirectToAction("ListApplies", new { id = id });
                }

                cv.Status = status;
                _context.CVs.Update(cv);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "CV status updated successfully!";
                return RedirectToAction("ListApplies", new { id = id });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in UpdateCV: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while updating the CV status.";
                return RedirectToAction("ListApplies", new { id = id });
            }
        }
    }
}