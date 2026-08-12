using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NedAcademyWebsite.Data;
using NedAcademyWebsite.Models;
using NedAcademyWebsite.Services;

namespace NedAcademyWebsite.Controllers;

[Authorize]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly IAuditLogService _auditLog;
    private readonly IStructureService _structureService;
    private readonly IPageBuilderService _pageBuilderService;
    private readonly ISearchService _searchService;
    private readonly IAdmissionService _admissionService;

    public AdminController(
        ApplicationDbContext context,
        IWebHostEnvironment env,
        IAuditLogService auditLog,
        IStructureService structureService,
        IPageBuilderService pageBuilderService,
        ISearchService searchService,
        IAdmissionService admissionService)
    {
        _context = context;
        _env = env;
        _auditLog = auditLog;
        _structureService = structureService;
        _pageBuilderService = pageBuilderService;
        _searchService = searchService;
        _admissionService = admissionService;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction(nameof(Index));
        }
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
    {
        if (username == "admin" && password == "admin123")
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, "Administrator")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
            await _auditLog.LogAsync("Login", "User", null, "Administrative user logged into CMS Console");
            
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError(string.Empty, "Invalid administrative credential parameters.");
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    public async Task<IActionResult> Logout()
    {
        await _auditLog.LogAsync("Logout", "User", null, "Administrative user logged out");
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.ProgramsCount = await _context.DegreePrograms.CountAsync(p => !p.IsDeleted);
        ViewBag.FacultyCount = await _context.FacultyMembers.CountAsync(f => !f.IsDeleted);
        ViewBag.ResearchCount = await _context.ResearchItems.CountAsync();
        ViewBag.MessagesCount = await _context.ContactMessages.CountAsync();
        ViewBag.DownloadsCount = await _context.DownloadFiles.CountAsync();
        ViewBag.MediaAssetsCount = await _context.MediaAssets.CountAsync(m => !m.IsDeleted);
        ViewBag.RecentLogs = await _auditLog.GetLogsAsync(7);
        ViewBag.RecentUploads = await _context.MediaAssets.Where(m => !m.IsDeleted).OrderByDescending(m => m.UploadDate).Take(4).ToListAsync();

        ViewBag.ApplicationsCount = await _context.AdmissionApplications.CountAsync(a => !a.IsDeleted);
        ViewBag.PendingApplicationsCount = await _context.AdmissionApplications.CountAsync(a => !a.IsDeleted && (a.CurrentStatus == ApplicationStatus.Submitted || a.CurrentStatus == ApplicationStatus.UnderReview));
        ViewBag.RecentApplications = await _context.AdmissionApplications
            .Include(a => a.Applicant)
            .Include(a => a.DegreeProgram)
            .Where(a => !a.IsDeleted)
            .OrderByDescending(a => a.CreatedDate)
            .Take(5)
            .ToListAsync();

        var recentMessages = await _context.ContactMessages
            .OrderByDescending(m => m.ReceivedDate)
            .Take(5)
            .ToListAsync();

        return View(recentMessages);
    }

    [HttpGet]
    public async Task<IActionResult> GlobalSearch(string q)
    {
        var results = await _searchService.SearchAsync(q);
        return Json(results);
    }

    #region Universal Page Builder
    public async Task<IActionResult> PageBuilder(string pageKey = "Home")
    {
        ViewBag.PageKey = pageKey;
        var sections = await _pageBuilderService.GetPageSectionsAsync(pageKey);
        return View(sections);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SavePageSections(string pageKey, List<PageSection> sections)
    {
        await _pageBuilderService.UpdatePageSectionsAsync(pageKey, sections);
        await _auditLog.LogAsync("Update", "PageBuilder", null, $"Updated section layout parameters for {pageKey} page");
        TempData["Success"] = $"Page section settings for '{pageKey}' saved successfully.";
        return RedirectToAction(nameof(PageBuilder), new { pageKey });
    }
    #endregion

    #region Website Structure Manager (Homepage Sections)
    public async Task<IActionResult> Structure()
    {
        var sections = await _structureService.GetHomepageSectionsAsync();
        return View(sections);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveStructure(List<SectionSetting> sections)
    {
        await _structureService.UpdateSectionOrderAsync(sections);
        await _auditLog.LogAsync("Update", "Structure", null, "Updated homepage section layout rules and visibility");
        TempData["Success"] = "Homepage layout configuration updated.";
        return RedirectToAction(nameof(Structure));
    }
    #endregion

    #region Dynamic Form Builder
    public async Task<IActionResult> FormBuilder()
    {
        var forms = await _context.CustomForms.Include(f => f.Fields).ToListAsync();
        if (!forms.Any())
        {
            var defaultForm = new CustomForm
            {
                FormName = "Online Admissions Application",
                FormKey = "admissions",
                Description = "Prospective student application form fields",
                IsActive = true,
                Fields = new List<FormField>
                {
                    new FormField { FieldName = "FullName", FieldLabel = "Full Name", FieldType = "Text", IsRequired = true, SortOrder = 1 },
                    new FormField { FieldName = "CNIC", FieldLabel = "CNIC / B-Form Number", FieldType = "Text", IsRequired = true, SortOrder = 2 },
                    new FormField { FieldName = "DegreeLevel", FieldLabel = "Target Degree Level", FieldType = "Dropdown", OptionsCsv = "BS,MS,PhD", IsRequired = true, SortOrder = 3 }
                }
            };
            _context.CustomForms.Add(defaultForm);
            await _context.SaveChangesAsync();
            forms = await _context.CustomForms.Include(f => f.Fields).ToListAsync();
        }
        return View(forms);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateFormField(FormField model)
    {
        if (ModelState.IsValid)
        {
            _context.FormFields.Add(model);
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Create", "FormField", model.Id, $"Added new form field '{model.FieldLabel}'");
            TempData["Success"] = "Form field registered.";
        }
        return RedirectToAction(nameof(FormBuilder));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteFormField(int id)
    {
        var field = await _context.FormFields.FindAsync(id);
        if (field != null)
        {
            _context.FormFields.Remove(field);
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Delete", "FormField", id, $"Removed field '{field.FieldLabel}'");
            TempData["Success"] = "Form field removed.";
        }
        return RedirectToAction(nameof(FormBuilder));
    }
    #endregion

    #region Dynamic Navigation Manager
    public async Task<IActionResult> Navigation()
    {
        var list = await _context.NavMenuItems.OrderBy(n => n.SortOrder).ToListAsync();
        return View(list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateNavMenu(NavMenuItem model)
    {
        if (ModelState.IsValid)
        {
            _context.NavMenuItems.Add(model);
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Create", "NavMenuItem", model.Id, $"Created navigation link '{model.MenuName}'");
            TempData["Success"] = "Navigation item created.";
        }
        return RedirectToAction(nameof(Navigation));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteNavMenu(int id)
    {
        var nav = await _context.NavMenuItems.FindAsync(id);
        if (nav != null)
        {
            _context.NavMenuItems.Remove(nav);
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Delete", "NavMenuItem", id, $"Deleted navigation item '{nav.MenuName}'");
            TempData["Success"] = "Navigation item deleted.";
        }
        return RedirectToAction(nameof(Navigation));
    }
    #endregion

    #region Audit Logs Viewer
    public async Task<IActionResult> AuditLogs()
    {
        var logs = await _auditLog.GetLogsAsync(100);
        return View(logs);
    }
    #endregion

    #region Soft Delete Recycle Bin
    public async Task<IActionResult> RecycleBin()
    {
        ViewBag.Programs = await _context.DegreePrograms.IgnoreQueryFilters().Where(p => p.IsDeleted).ToListAsync();
        ViewBag.Faculty = await _context.FacultyMembers.IgnoreQueryFilters().Where(f => f.IsDeleted).ToListAsync();
        ViewBag.News = await _context.NewsItems.IgnoreQueryFilters().Where(n => n.IsDeleted).ToListAsync();
        ViewBag.Media = await _context.MediaAssets.IgnoreQueryFilters().Where(m => m.IsDeleted).ToListAsync();

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RestoreItem(string entity, int id)
    {
        if (entity == "Program")
        {
            var item = await _context.DegreePrograms.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == id);
            if (item != null) { item.IsDeleted = false; item.DeletedAt = null; }
        }
        else if (entity == "Faculty")
        {
            var item = await _context.FacultyMembers.IgnoreQueryFilters().FirstOrDefaultAsync(f => f.Id == id);
            if (item != null) { item.IsDeleted = false; item.DeletedAt = null; }
        }
        else if (entity == "News")
        {
            var item = await _context.NewsItems.IgnoreQueryFilters().FirstOrDefaultAsync(n => n.Id == id);
            if (item != null) { item.IsDeleted = false; item.DeletedAt = null; }
        }
        else if (entity == "Media")
        {
            var item = await _context.MediaAssets.IgnoreQueryFilters().FirstOrDefaultAsync(m => m.Id == id);
            if (item != null) { item.IsDeleted = false; item.DeletedAt = null; }
        }

        await _context.SaveChangesAsync();
        await _auditLog.LogAsync("Restore", entity, id, $"Restored item #{id} from Recycle Bin");
        TempData["Success"] = $"{entity} record restored successfully.";
        return RedirectToAction(nameof(RecycleBin));
    }
    #endregion

    #region Backup & Restore Manager
    public async Task<IActionResult> Backups()
    {
        var records = await _context.BackupRecords.OrderByDescending(b => b.CreatedAt).ToListAsync();
        return View(records);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBackup(string type = "Database")
    {
        var fileName = $"Backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
        var backupFolder = Path.Combine(_env.WebRootPath, "backups");
        if (!Directory.Exists(backupFolder)) Directory.CreateDirectory(backupFolder);

        var fullPath = Path.Combine(backupFolder, fileName);
        System.IO.File.WriteAllText(fullPath, $"-- Snapshot Backup Created At {DateTime.Now}\n-- System: NED Academy CMS Engine");

        var rec = new BackupRecord
        {
            FileName = fileName,
            BackupType = type,
            SizeBytes = new FileInfo(fullPath).Length,
            CreatedAt = DateTime.Now,
            FilePath = "/backups/" + fileName
        };

        _context.BackupRecords.Add(rec);
        await _context.SaveChangesAsync();
        await _auditLog.LogAsync("Create", "BackupRecord", rec.Id, $"Generated {type} snapshot archive {fileName}");

        TempData["Success"] = "Backup snapshot created successfully.";
        return RedirectToAction(nameof(Backups));
    }
    #endregion

    #region System Settings (10 Grouped Tabs)
    public async Task<IActionResult> Settings()
    {
        var settings = await _context.SystemSettings.FirstOrDefaultAsync();
        if (settings == null)
        {
            settings = new SystemSetting();
            _context.SystemSettings.Add(settings);
            await _context.SaveChangesAsync();
        }
        return View(settings);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Settings(SystemSetting model)
    {
        if (ModelState.IsValid)
        {
            _context.Update(model);
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Update", "SystemSetting", model.Id, "Updated global CMS parameters & metadata");
            TempData["Success"] = "System settings updated successfully.";
            return RedirectToAction(nameof(Settings));
        }
        return View(model);
    }
    #endregion

    #region Departments CRUD
    public async Task<IActionResult> Departments()
    {
        var list = await _context.Departments.ToListAsync();
        return View(list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDepartment(Department model)
    {
        if (ModelState.IsValid)
        {
            _context.Departments.Add(model);
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Create", "Department", model.Id, $"Created department '{model.Name}'");
            TempData["Success"] = "Department created successfully.";
        }
        return RedirectToAction(nameof(Departments));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditDepartment(Department model)
    {
        if (ModelState.IsValid)
        {
            _context.Departments.Update(model);
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Update", "Department", model.Id, $"Updated department '{model.Name}'");
            TempData["Success"] = "Department updated successfully.";
        }
        return RedirectToAction(nameof(Departments));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDepartment(int id)
    {
        var dept = await _context.Departments.FindAsync(id);
        if (dept != null)
        {
            _context.Departments.Remove(dept);
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Delete", "Department", id, $"Deleted department '{dept.Name}'");
            TempData["Success"] = "Department deleted successfully.";
        }
        return RedirectToAction(nameof(Departments));
    }
    #endregion

    #region Degree Programs CRUD
    public async Task<IActionResult> Programs()
    {
        var list = await _context.DegreePrograms.Where(p => !p.IsDeleted).Include(p => p.Department).ToListAsync();
        ViewBag.Departments = await _context.Departments.ToListAsync();
        return View(list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateProgram(DegreeProgram model)
    {
        if (ModelState.IsValid)
        {
            if (string.IsNullOrEmpty(model.Slug)) model.Slug = model.Name.ToLower().Replace(" ", "-");
            _context.DegreePrograms.Add(model);
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Create", "DegreeProgram", model.Id, $"Added degree program '{model.Name}' ({model.Code})");
            TempData["Success"] = "Degree program created successfully.";
        }
        return RedirectToAction(nameof(Programs));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProgram(DegreeProgram model)
    {
        if (ModelState.IsValid)
        {
            model.UpdatedDate = DateTime.Now;
            _context.DegreePrograms.Update(model);
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Update", "DegreeProgram", model.Id, $"Updated program '{model.Name}'");
            TempData["Success"] = "Degree program updated successfully.";
        }
        return RedirectToAction(nameof(Programs));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProgram(int id)
    {
        var prog = await _context.DegreePrograms.FindAsync(id);
        if (prog != null)
        {
            prog.IsDeleted = true;
            prog.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("SoftDelete", "DegreeProgram", id, $"Moved program '{prog.Name}' to Recycle Bin");
            TempData["Success"] = "Program moved to Recycle Bin.";
        }
        return RedirectToAction(nameof(Programs));
    }
    #endregion

    #region Faculty CRUD
    public async Task<IActionResult> Faculty()
    {
        var list = await _context.FacultyMembers.Where(f => !f.IsDeleted).Include(f => f.Department).OrderBy(f => f.Id).ToListAsync();
        ViewBag.Departments = await _context.Departments.ToListAsync();
        return View(list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateFaculty(Faculty model)
    {
        if (ModelState.IsValid)
        {
            _context.FacultyMembers.Add(model);
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Create", "Faculty", model.Id, $"Added faculty profile for '{model.Name}'");
            TempData["Success"] = "Faculty profile created successfully.";
        }
        return RedirectToAction(nameof(Faculty));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditFaculty(Faculty model)
    {
        if (ModelState.IsValid)
        {
            _context.FacultyMembers.Update(model);
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Update", "Faculty", model.Id, $"Updated faculty profile '{model.Name}'");
            TempData["Success"] = "Faculty profile updated successfully.";
        }
        return RedirectToAction(nameof(Faculty));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteFaculty(int id)
    {
        var f = await _context.FacultyMembers.FindAsync(id);
        if (f != null)
        {
            f.IsDeleted = true;
            f.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("SoftDelete", "Faculty", id, $"Moved faculty '{f.Name}' to Recycle Bin");
            TempData["Success"] = "Faculty profile moved to Recycle Bin.";
        }
        return RedirectToAction(nameof(Faculty));
    }
    #endregion

    #region Leadership CRUD
    public async Task<IActionResult> Leadership()
    {
        var list = await _context.LeadershipRegistry.OrderBy(l => l.Rank).ToListAsync();
        return View(list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateLeadership(Leadership model)
    {
        if (ModelState.IsValid)
        {
            _context.LeadershipRegistry.Add(model);
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Create", "Leadership", model.Id, $"Created board profile '{model.Name}'");
            TempData["Success"] = "Board profile created successfully.";
        }
        return RedirectToAction(nameof(Leadership));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditLeadership(Leadership model)
    {
        if (ModelState.IsValid)
        {
            _context.LeadershipRegistry.Update(model);
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Update", "Leadership", model.Id, $"Updated board profile '{model.Name}'");
            TempData["Success"] = "Board profile updated successfully.";
        }
        return RedirectToAction(nameof(Leadership));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLeadership(int id)
    {
        var l = await _context.LeadershipRegistry.FindAsync(id);
        if (l != null)
        {
            _context.LeadershipRegistry.Remove(l);
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Delete", "Leadership", id, $"Deleted board profile '{l.Name}'");
            TempData["Success"] = "Leadership profile deleted.";
        }
        return RedirectToAction(nameof(Leadership));
    }
    #endregion

    #region Research CRUD
    public async Task<IActionResult> Research()
    {
        var list = await _context.ResearchItems.ToListAsync();
        return View(list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateResearch(ResearchItem model)
    {
        if (ModelState.IsValid)
        {
            _context.ResearchItems.Add(model);
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Create", "ResearchItem", model.Id, $"Added research project '{model.Title}'");
            TempData["Success"] = "Research item created successfully.";
        }
        return RedirectToAction(nameof(Research));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditResearch(ResearchItem model)
    {
        if (ModelState.IsValid)
        {
            _context.ResearchItems.Update(model);
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Update", "ResearchItem", model.Id, $"Updated research '{model.Title}'");
            TempData["Success"] = "Research item updated successfully.";
        }
        return RedirectToAction(nameof(Research));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteResearch(int id)
    {
        var r = await _context.ResearchItems.FindAsync(id);
        if (r != null)
        {
            _context.ResearchItems.Remove(r);
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Delete", "ResearchItem", id, $"Deleted research item '{r.Title}'");
            TempData["Success"] = "Research item deleted.";
        }
        return RedirectToAction(nameof(Research));
    }
    #endregion

    #region Campus Facilities CRUD
    public async Task<IActionResult> Facilities()
    {
        var list = await _context.CampusFacilities.ToListAsync();
        return View(list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateFacility(CampusFacility model)
    {
        if (ModelState.IsValid)
        {
            _context.CampusFacilities.Add(model);
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Create", "CampusFacility", model.Id, $"Registered facility '{model.Title}'");
            TempData["Success"] = "Facility created successfully.";
        }
        return RedirectToAction(nameof(Facilities));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditFacility(CampusFacility model)
    {
        if (ModelState.IsValid)
        {
            _context.CampusFacilities.Update(model);
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Update", "CampusFacility", model.Id, $"Updated facility '{model.Title}'");
            TempData["Success"] = "Facility updated successfully.";
        }
        return RedirectToAction(nameof(Facilities));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteFacility(int id)
    {
        var f = await _context.CampusFacilities.FindAsync(id);
        if (f != null)
        {
            _context.CampusFacilities.Remove(f);
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Delete", "CampusFacility", id, $"Deleted facility '{f.Title}'");
            TempData["Success"] = "Facility deleted.";
        }
        return RedirectToAction(nameof(Facilities));
    }
    #endregion

    #region News & Events CRUD
    public async Task<IActionResult> News()
    {
        var news = await _context.NewsItems.Where(n => !n.IsDeleted).OrderByDescending(n => n.PublishDate).ToListAsync();
        ViewBag.Events = await _context.EventItems.OrderBy(e => e.Date).ToListAsync();
        return View(news);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateNews(NewsItem model)
    {
        if (ModelState.IsValid)
        {
            _context.NewsItems.Add(model);
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Create", "NewsItem", model.Id, $"Published article '{model.Title}'");
            TempData["Success"] = "News item published successfully.";
        }
        return RedirectToAction(nameof(News));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditNews(NewsItem model)
    {
        if (ModelState.IsValid)
        {
            _context.NewsItems.Update(model);
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Update", "NewsItem", model.Id, $"Updated news article '{model.Title}'");
            TempData["Success"] = "News item updated successfully.";
        }
        return RedirectToAction(nameof(News));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteNews(int id)
    {
        var n = await _context.NewsItems.FindAsync(id);
        if (n != null)
        {
            n.IsDeleted = true;
            n.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("SoftDelete", "NewsItem", id, $"Moved news '{n.Title}' to Recycle Bin");
            TempData["Success"] = "News item moved to Recycle Bin.";
        }
        return RedirectToAction(nameof(News));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEvent(EventItem model)
    {
        if (ModelState.IsValid)
        {
            _context.EventItems.Add(model);
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Create", "EventItem", model.Id, $"Scheduled event '{model.Title}'");
            TempData["Success"] = "Event scheduled successfully.";
        }
        return RedirectToAction(nameof(News));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditEvent(EventItem model)
    {
        if (ModelState.IsValid)
        {
            _context.EventItems.Update(model);
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Update", "EventItem", model.Id, $"Updated event '{model.Title}'");
            TempData["Success"] = "Event details updated successfully.";
        }
        return RedirectToAction(nameof(News));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var ev = await _context.EventItems.FindAsync(id);
        if (ev != null)
        {
            _context.EventItems.Remove(ev);
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Delete", "EventItem", id, $"Cancelled event '{ev.Title}'");
            TempData["Success"] = "Event deleted successfully.";
        }
        return RedirectToAction(nameof(News));
    }
    #endregion

    #region Resource Downloads CRUD
    public async Task<IActionResult> Downloads()
    {
        var list = await _context.DownloadFiles.OrderBy(d => d.Title).ToListAsync();
        return View(list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDownload(DownloadFile model)
    {
        if (ModelState.IsValid)
        {
            _context.DownloadFiles.Add(model);
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Create", "DownloadFile", model.Id, $"Registered download asset '{model.Title}'");
            TempData["Success"] = "Resource file registered successfully.";
        }
        return RedirectToAction(nameof(Downloads));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditDownload(DownloadFile model)
    {
        if (ModelState.IsValid)
        {
            _context.DownloadFiles.Update(model);
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Update", "DownloadFile", model.Id, $"Updated download resource '{model.Title}'");
            TempData["Success"] = "Resource updated successfully.";
        }
        return RedirectToAction(nameof(Downloads));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDownload(int id)
    {
        var d = await _context.DownloadFiles.FindAsync(id);
        if (d != null)
        {
            _context.DownloadFiles.Remove(d);
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Delete", "DownloadFile", id, $"Deleted download file '{d.Title}'");
            TempData["Success"] = "Resource file deleted.";
        }
        return RedirectToAction(nameof(Downloads));
    }
    #endregion

    #region FAQs CRUD
    public async Task<IActionResult> FAQs()
    {
        var list = await _context.FAQItems.ToListAsync();
        return View(list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateFAQ(FAQItem model)
    {
        if (ModelState.IsValid)
        {
            _context.FAQItems.Add(model);
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Create", "FAQItem", model.Id, $"Created FAQ item");
            TempData["Success"] = "FAQ item created successfully.";
        }
        return RedirectToAction(nameof(FAQs));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditFAQ(FAQItem model)
    {
        if (ModelState.IsValid)
        {
            _context.FAQItems.Update(model);
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Update", "FAQItem", model.Id, $"Updated FAQ item");
            TempData["Success"] = "FAQ updated successfully.";
        }
        return RedirectToAction(nameof(FAQs));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteFAQ(int id)
    {
        var faq = await _context.FAQItems.FindAsync(id);
        if (faq != null)
        {
            _context.FAQItems.Remove(faq);
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Delete", "FAQItem", id, $"Deleted FAQ item");
            TempData["Success"] = "FAQ deleted.";
        }
        return RedirectToAction(nameof(FAQs));
    }
    #endregion

    #region Contact Messages
    public async Task<IActionResult> Messages()
    {
        var list = await _context.ContactMessages.OrderByDescending(m => m.ReceivedDate).ToListAsync();
        return View(list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkMessageRead(int id)
    {
        var msg = await _context.ContactMessages.FindAsync(id);
        if (msg != null)
        {
            msg.IsRead = true;
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
        return Json(new { success = false });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ArchiveMessage(int id)
    {
        var msg = await _context.ContactMessages.FindAsync(id);
        if (msg != null)
        {
            msg.IsArchived = !msg.IsArchived;
            await _context.SaveChangesAsync();
            return Json(new { success = true, archived = msg.IsArchived });
        }
        return Json(new { success = false });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteMessage(int id)
    {
        var msg = await _context.ContactMessages.FindAsync(id);
        if (msg != null)
        {
            _context.ContactMessages.Remove(msg);
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Delete", "ContactMessage", id, $"Removed user message from '{msg.Name}'");
            TempData["Success"] = "Message deleted successfully.";
        }
        return RedirectToAction(nameof(Messages));
    }
    #endregion

    #region Media Library & Unified MediaAssets
    public async Task<IActionResult> Media()
    {
        var items = await _context.MediaAssets.Where(m => !m.IsDeleted).OrderByDescending(m => m.UploadDate).ToListAsync();
        return View(items);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadMedia(IFormFile file, string title, string category = "General")
    {
        if (file != null && file.Length > 0)
        {
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, fileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            var media = new MediaAsset
            {
                Title = title,
                FileName = file.FileName,
                FilePath = "/uploads/" + fileName,
                FileType = file.ContentType.StartsWith("image/") ? "Image" : (file.ContentType.StartsWith("video/") ? "Video" : "Document"),
                Category = category,
                SizeBytes = file.Length,
                UploadDate = DateTime.Now
            };

            _context.MediaAssets.Add(media);
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Upload", "MediaAsset", media.Id, $"Uploaded media asset '{title}' to {category} folder");
            TempData["Success"] = "File uploaded to media library.";
        }
        return RedirectToAction(nameof(Media));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteMedia(int id)
    {
        var item = await _context.MediaAssets.FindAsync(id);
        if (item != null)
        {
            item.IsDeleted = true;
            item.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("SoftDelete", "MediaAsset", id, $"Moved media asset '{item.Title}' to Recycle Bin");
            TempData["Success"] = "File moved to Recycle Bin.";
        }
        return RedirectToAction(nameof(Media));
    }
    #endregion

    #region Admission Digital Admission File & Decision Workflow
    // GET: /Admin/Admissions or /Admin/Applications
    [HttpGet("Admin/Admissions")]
    [HttpGet("Admin/Applications")]
    public async Task<IActionResult> Admissions(string? status, string? search)
    {
        var query = _context.AdmissionApplications
            .Include(a => a.Applicant)
            .Include(a => a.DegreeProgram)
            .Where(a => !a.IsDeleted);

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<ApplicationStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(a => a.CurrentStatus == parsedStatus);
        }

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(a => a.ApplicationNumber.Contains(search) || (a.Applicant != null && a.Applicant.FullName.Contains(search)) || (a.Applicant != null && a.Applicant.Email.Contains(search)));
        }

        var list = await query.OrderByDescending(a => a.CreatedDate).ToListAsync();
        ViewBag.CurrentStatus = status;
        ViewBag.Search = search;
        return View(list);
    }

    // GET: /Admin/ApplicantDetails/{id}
    [HttpGet("Admin/ApplicantDetails/{id}")]
    public async Task<IActionResult> ApplicantDetails(int id, string? tab = "overview")
    {
        var app = await _context.AdmissionApplications
            .Include(a => a.Applicant)
            .Include(a => a.DegreeProgram)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (app == null) return NotFound();

        var profile = await _admissionService.GetApplicantProfileAsync(app.ApplicantId);
        var guardian = await _admissionService.GetGuardianInfoAsync(app.ApplicantId);
        var academics = await _admissionService.GetAcademicRecordsAsync(app.ApplicantId);
        var documents = await _admissionService.GetDocumentsByApplicationAsync(app.Id);
        var vouchers = await _admissionService.GetVouchersByApplicantAsync(app.ApplicantId);
        var timeline = await _admissionService.GetTimelineLogsAsync(app.Id);
        var enrollment = await _admissionService.GetEnrollmentRecordByApplicantAsync(app.ApplicantId);

        var decisions = await _context.AdmissionDecisions
            .Where(d => d.AdmissionApplicationId == id)
            .OrderByDescending(d => d.Timestamp)
            .ToListAsync();

        var auditLogs = await _context.AdmissionAuditLogs
            .Where(l => l.EntityId == id.ToString() || l.EntityId == app.ApplicantId.ToString())
            .OrderByDescending(l => l.Timestamp)
            .ToListAsync();

        var testSchedule = await _admissionService.GetTestScheduleByApplicationAsync(app.Id);
        var testResult = await _admissionService.GetTestResultByApplicationAsync(app.Id);

        ViewBag.ApplicantProfile = profile;
        ViewBag.Guardian = guardian;
        ViewBag.Academics = academics;
        ViewBag.Documents = documents;
        var internalNotes = await _admissionService.GetInternalNotesByApplicationAsync(app.Id);

        ViewBag.FeeVouchers = vouchers;
        ViewBag.TimelineLogs = timeline;
        ViewBag.EnrollmentRecord = enrollment;
        ViewBag.Decisions = decisions;
        ViewBag.AuditLogs = auditLogs;
        ViewBag.TestSchedule = testSchedule;
        ViewBag.TestResult = testResult;
        ViewBag.InternalNotes = internalNotes;
        ViewBag.ActiveTab = string.IsNullOrWhiteSpace(tab) ? "overview" : tab.ToLower().Trim();

        return View(app);
    }

    // POST: /Admin/SaveInternalNote
    [HttpPost("Admin/SaveInternalNote")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveInternalNote(int applicationId, string noteText, string? activeTab = "audit")
    {
        string officerId = User.Identity?.Name ?? "AdmissionAdmin";
        if (!string.IsNullOrWhiteSpace(noteText))
        {
            await _admissionService.SaveInternalNoteAsync(applicationId, officerId, noteText.Trim(), null, null);
            TempData["Success"] = "Internal note saved successfully.";
        }
        return RedirectToAction(nameof(ApplicantDetails), new { id = applicationId, tab = activeTab ?? "audit" });
    }

    // POST: /Admin/AssignOfficer
    [HttpPost("Admin/AssignOfficer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignOfficer(int applicationId, string officerId, string? activeTab = "overview")
    {
        string currentAdmin = User.Identity?.Name ?? "SuperAdmin";
        if (!string.IsNullOrWhiteSpace(officerId))
        {
            await _admissionService.AssignOfficerAsync(applicationId, officerId, currentAdmin);
            TempData["Success"] = $"Application reassigned to officer '{officerId}'.";
        }
        return RedirectToAction(nameof(ApplicantDetails), new { id = applicationId, tab = activeTab ?? "overview" });
    }

    // POST: /Admin/OverrideScholarship
    [HttpPost("Admin/OverrideScholarship")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OverrideScholarship(int applicationId, double overridePercentage, string reason, string? activeTab = "scholarship")
    {
        string currentAdmin = User.Identity?.Name ?? "SuperAdmin";
        if (overridePercentage >= 0 && overridePercentage <= 100)
        {
            await _admissionService.OverrideScholarshipAsync(applicationId, overridePercentage, reason ?? "Approved by Management", currentAdmin);
            TempData["Success"] = $"Scholarship percentage manually updated to {overridePercentage}%.";
        }
        return RedirectToAction(nameof(ApplicantDetails), new { id = applicationId, tab = activeTab ?? "scholarship" });
    }

    // POST: /Admin/ProcessDecision
    [HttpPost("Admin/ProcessDecision")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcessDecision(int applicationId, string decisionType, string comments, string? activeTab = "approval")
    {
        string officerId = User.Identity?.Name ?? "AdmissionAdmin";
        var enrollment = await _admissionService.ProcessAdmissionDecisionAsync(applicationId, decisionType, comments, officerId);

        TempData["Success"] = $"Admission Decision '{decisionType}' processed successfully!";
        return RedirectToAction(nameof(ApplicantDetails), new { id = applicationId, tab = activeTab ?? "approval" });
    }

    // POST: /Admin/VerifyDocument
    [HttpPost("Admin/VerifyDocument")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyDocument(int documentId, int applicationId, string status, string remarks, string? activeTab = "documents")
    {
        string officerId = User.Identity?.Name ?? "AdmissionAdmin";
        if (Enum.TryParse<DocumentVerificationStatus>(status, true, out var parsedStatus))
        {
            await _admissionService.VerifyDocumentAsync(documentId, parsedStatus, remarks, officerId);
            TempData["Success"] = $"Document verification updated to '{status}'.";
        }
        return RedirectToAction(nameof(ApplicantDetails), new { id = applicationId, tab = activeTab ?? "documents" });
    }

    // POST: /Admin/VerifyFeePayment
    [HttpPost("Admin/VerifyFeePayment")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyFeePayment(int voucherId, int applicationId, string? paymentMethod, string? transactionRef, string? activeTab = "fee")
    {
        string officerId = User.Identity?.Name ?? "AdmissionAdmin";
        await _admissionService.VerifyFeePaymentAsync(voucherId, paymentMethod ?? "BankTransfer", transactionRef ?? "TXN-VERIFIED", officerId);
        TempData["Success"] = "Fee voucher payment approved and verified successfully!";
        return RedirectToAction(nameof(ApplicantDetails), new { id = applicationId, tab = activeTab ?? "fee" });
    }

    // POST: /Admin/GenerateFeeVoucher
    [HttpPost("Admin/GenerateFeeVoucher")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateFeeVoucher(int applicationId, string voucherType, decimal amount, string? activeTab = "fee")
    {
        string officerId = User.Identity?.Name ?? "AdmissionAdmin";
        await _admissionService.GenerateFeeVoucherAsync(applicationId, voucherType, amount, DateTime.UtcNow.AddDays(14));
        TempData["Success"] = $"Fee voucher ({voucherType} - PKR {amount:N0}) generated successfully!";
        return RedirectToAction(nameof(ApplicantDetails), new { id = applicationId, tab = activeTab ?? "fee" });
    }

    // POST: /Admin/DeleteFeeVoucher
    [HttpPost("Admin/DeleteFeeVoucher")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteFeeVoucher(int voucherId, int applicationId, string? activeTab = "fee")
    {
        string officerId = User.Identity?.Name ?? "AdmissionAdmin";
        await _admissionService.DeleteFeeVoucherAsync(voucherId, officerId);
        TempData["Success"] = "Fee voucher deleted and removed successfully!";
        return RedirectToAction(nameof(ApplicantDetails), new { id = applicationId, tab = activeTab ?? "fee" });
    }

    // GET: /Admin/Scholarships
    [HttpGet("Admin/Scholarships")]
    public async Task<IActionResult> Scholarships()
    {
        var rules = await _admissionService.GetAllScholarshipRulesAsync();
        return View(rules);
    }

    // POST: /Admin/SaveScholarshipRule
    [HttpPost("Admin/SaveScholarshipRule")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveScholarshipRule(ScholarshipRule rule)
    {
        if (ModelState.IsValid)
        {
            await _admissionService.SaveScholarshipRuleAsync(rule);
            TempData["Success"] = $"Scholarship Rule '{rule.ScholarshipName}' saved successfully!";
        }
        return RedirectToAction(nameof(Scholarships));
    }

    // POST: /Admin/DeleteScholarshipRule
    [HttpPost("Admin/DeleteScholarshipRule")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteScholarshipRule(int id)
    {
        await _admissionService.DeleteScholarshipRuleAsync(id);
        TempData["Success"] = "Scholarship rule removed successfully.";
        return RedirectToAction(nameof(Scholarships));
    }

    // GET: /Admin/FeeConfig
    [HttpGet("Admin/FeeConfig")]
    public async Task<IActionResult> FeeConfig()
    {
        var config = await _admissionService.GetFeeStructureConfigAsync();
        return View(config);
    }

    // POST: /Admin/SaveFeeConfig
    [HttpPost("Admin/SaveFeeConfig")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveFeeConfig(FeeStructureConfig config)
    {
        string officerId = User.Identity?.Name ?? "Admin";
        await _admissionService.SaveFeeStructureConfigAsync(config, officerId);
        TempData["Success"] = "University Fee Structure updated successfully!";
        return RedirectToAction(nameof(FeeConfig));
    }

    // POST: /Admin/ScheduleTest
    [HttpPost("Admin/ScheduleTest")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ScheduleTest(AdmissionTestSchedule schedule, string? activeTab = "schedule")
    {
        schedule.ScheduledByUserId = User.Identity?.Name ?? "AdmissionOfficer";
        await _admissionService.ScheduleAdmissionTestAsync(schedule);
        TempData["Success"] = "Admission Test scheduled successfully!";
        return RedirectToAction(nameof(ApplicantDetails), new { id = schedule.AdmissionApplicationId, tab = activeTab ?? "schedule" });
    }

    // POST: /Admin/RecordTestResult
    [HttpPost("Admin/RecordTestResult")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordTestResult(int applicationId, double obtainedMarks, double totalMarks, string remarks, string? activeTab = "results")
    {
        string officerId = User.Identity?.Name ?? "AdmissionOfficer";
        var result = await _admissionService.RecordTestResultAsync(applicationId, obtainedMarks, totalMarks, remarks, officerId);
        TempData["Success"] = $"Test Result recorded ({obtainedMarks}/{totalMarks} - {result.CalculatedPercentage}%). Status updated!";
        return RedirectToAction(nameof(ApplicantDetails), new { id = applicationId, tab = activeTab ?? "results" });
    }

    // POST: /Admin/ReactivateVoucher
    [HttpPost("Admin/ReactivateVoucher")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReactivateVoucher(int oldVoucherId, int applicationId, DateTime newDueDate, string? activeTab = "fee")
    {
        string officerId = User.Identity?.Name ?? "AdmissionAdmin";
        var voucher = await _admissionService.ReactivateVoucherAsync(oldVoucherId, newDueDate, officerId);
        TempData["Success"] = $"Voucher reactivated with new Voucher #: {voucher.VoucherNumber} (Due: {newDueDate:dd MMM yyyy})!";
        return RedirectToAction(nameof(ApplicantDetails), new { id = applicationId, tab = activeTab ?? "fee" });
    }
    #endregion
}
