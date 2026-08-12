using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NedAcademyWebsite.Data;
using NedAcademyWebsite.Models;
using NedAcademyWebsite.Services;

namespace NedAcademyWebsite.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ISettingService _settingService;
    private readonly IProgramService _programService;
    private readonly IFacultyService _facultyService;
    private readonly ILeadershipService _leadershipService;
    private readonly IResearchService _researchService;
    private readonly ICampusService _campusService;
    private readonly INewsService _newsService;
    private readonly IDownloadService _downloadService;
    private readonly IFAQService _faqService;
    private readonly IContactService _contactService;
    private readonly IAdmissionService _admissionService;

    public HomeController(
        ApplicationDbContext context,
        ISettingService settingService,
        IProgramService programService,
        IFacultyService facultyService,
        ILeadershipService leadershipService,
        IResearchService researchService,
        ICampusService campusService,
        INewsService newsService,
        IDownloadService downloadService,
        IFAQService faqService,
        IContactService contactService,
        IAdmissionService admissionService)
    {
        _context = context;
        _settingService = settingService;
        _programService = programService;
        _facultyService = facultyService;
        _leadershipService = leadershipService;
        _researchService = researchService;
        _campusService = campusService;
        _newsService = newsService;
        _downloadService = downloadService;
        _faqService = faqService;
        _contactService = contactService;
        _admissionService = admissionService;
    }

    public async Task<IActionResult> Index()
    {
        var settings = await _settingService.GetSettingsAsync();
        var programs = await _programService.GetProgramsAsync();
        var faculty = await _facultyService.GetFacultyAsync();
        var research = await _researchService.GetResearchItemsAsync();
        var news = await _newsService.GetNewsAsync();

        ViewBag.ProgramsCount = Math.Max(programs.Count * 5, 15);
        ViewBag.FacultyCount = Math.Max(faculty.Count * 8, 30);
        ViewBag.ResearchCount = Math.Max(research.Count * 4, 18);
        ViewBag.LatestNews = news.Take(3).ToList();

        return View(settings);
    }

    public async Task<IActionResult> About()
    {
        var settings = await _settingService.GetSettingsAsync();
        return View(settings);
    }

    public async Task<IActionResult> Leadership()
    {
        var list = await _leadershipService.GetLeadershipAsync();
        return View(list);
    }

    public IActionResult Academics()
    {
        return View();
    }

    public async Task<IActionResult> Programs()
    {
        var list = await _programService.GetProgramsAsync();
        return View(list);
    }

    public async Task<IActionResult> Faculty()
    {
        var list = await _facultyService.GetFacultyAsync();
        ViewBag.Departments = await _programService.GetDepartmentsAsync();
        return View(list);
    }

    public async Task<IActionResult> Research()
    {
        var list = await _researchService.GetResearchItemsAsync();
        return View(list);
    }

    public async Task<IActionResult> Downloads()
    {
        var list = await _downloadService.GetFilesAsync();
        return View(list);
    }

    public async Task<IActionResult> Admissions()
    {
        var settings = await _settingService.GetSettingsAsync();
        var activeSession = await _admissionService.GetActiveAdmissionSessionAsync();
        var offeredPrograms = activeSession != null 
            ? await _admissionService.GetOfferedProgramsAsync(activeSession.Id) 
            : new List<AdmissionSessionProgram>();
        var faqs = await _faqService.GetFAQsAsync();

        ViewBag.ActiveSession = activeSession;
        ViewBag.OfferedPrograms = offeredPrograms;
        ViewBag.AdmissionFaqs = faqs.Where(f => f.Category == "General" || f.Category == "Academics" || f.Category == "Admissions").ToList();
        ViewBag.SystemSettings = settings;

        return View(settings);
    }

    public IActionResult Scholarships()
    {
        return View();
    }

    public IActionResult StudentLife()
    {
        return View();
    }

    public async Task<IActionResult> Facilities()
    {
        var list = await _campusService.GetFacilitiesAsync();
        return View(list);
    }

    public async Task<IActionResult> News()
    {
        var news = await _newsService.GetNewsAsync();
        ViewBag.Events = await _newsService.GetEventsAsync();
        return View(news);
    }

    public async Task<IActionResult> Gallery()
    {
        var items = await _context.MediaAssets
            .Where(m => !m.IsDeleted && m.Category == "Gallery")
            .OrderBy(m => m.DisplayOrder)
            .AsNoTracking()
            .ToListAsync();
        return View(items);
    }

    public async Task<IActionResult> FAQs()
    {
        var list = await _faqService.GetFAQsAsync();
        return View(list);
    }

    public async Task<IActionResult> Contact()
    {
        var settings = await _settingService.GetSettingsAsync();
        return View(settings);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitContact(ContactMessage model)
    {
        if (ModelState.IsValid)
        {
            await _contactService.SaveMessageAsync(model);
            return Json(new { success = true, message = "Access authorized. Your inquiry has been registered in the database." });
        }
        return Json(new { success = false, message = "Validation failed. Please verify form inputs." });
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
