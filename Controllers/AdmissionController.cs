using Microsoft.AspNetCore.Mvc;
using NedAcademyWebsite.Models;
using NedAcademyWebsite.Services;

namespace NedAcademyWebsite.Controllers;

[Route("Admission")]
public class AdmissionController : Controller
{
    private readonly IAdmissionService _admissionService;
    private readonly IProgramService _programService;
    private readonly ISettingService _settingService;
    private readonly ILogger<AdmissionController> _logger;

    public AdmissionController(
        IAdmissionService admissionService,
        IProgramService programService,
        ISettingService settingService,
        ILogger<AdmissionController> logger)
    {
        _admissionService = admissionService;
        _programService = programService;
        _settingService = settingService;
        _logger = logger;
    }

    // Centralized Entry Endpoint: /Admission or /Admission?program=slug
    [HttpGet("")]
    [HttpGet("Index")]
    [HttpGet("Index/{program}")]
    public async Task<IActionResult> Index(string? program)
    {
        try
        {
            var activeSession = await _admissionService.GetActiveAdmissionSessionAsync();
            var settings = await _settingService.GetSettingsAsync();

            DegreeProgram? selectedProgram = null;
            if (!string.IsNullOrEmpty(program))
            {
                var allPrograms = await _programService.GetProgramsAsync();
                selectedProgram = allPrograms.FirstOrDefault(p => 
                    p.Slug.Equals(program, StringComparison.OrdinalIgnoreCase) || 
                    p.Code.Equals(program, StringComparison.OrdinalIgnoreCase) ||
                    p.Id.ToString() == program);
            }

            ViewBag.ActiveSession = activeSession;
            ViewBag.SelectedProgram = selectedProgram;
            ViewBag.SystemSettings = settings;

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading admission entry portal");
            TempData["ErrorMessage"] = "Unable to load admission portal. Please try again later.";
            return RedirectToAction("Admissions", "Home");
        }
    }

    // Applicant Registration Entry: /Admission/Register
    [HttpGet("Register")]
    public async Task<IActionResult> Register(string? program)
    {
        var activeSession = await _admissionService.GetActiveAdmissionSessionAsync();
        var settings = await _settingService.GetSettingsAsync();

        DegreeProgram? selectedProgram = null;
        if (!string.IsNullOrEmpty(program))
        {
            var allPrograms = await _programService.GetProgramsAsync();
            selectedProgram = allPrograms.FirstOrDefault(p => 
                p.Slug.Equals(program, StringComparison.OrdinalIgnoreCase) || 
                p.Code.Equals(program, StringComparison.OrdinalIgnoreCase));
        }

        ViewBag.ActiveSession = activeSession;
        ViewBag.SelectedProgram = selectedProgram;
        ViewBag.SystemSettings = settings;

        return View();
    }

    // Handle Applicant Account Creation POST
    [HttpPost("Register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(Applicant applicant, string password, string confirmPassword, string countryCode, string? programSlug)
    {
        // Remove non-posted properties from ModelState
        ModelState.Remove("PasswordHash");
        ModelState.Remove("Profile");
        ModelState.Remove("CNIC_Passport");

        if (password != confirmPassword)
        {
            ModelState.AddModelError("Password", "Password and Confirm Password do not match.");
        }

        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
        {
            ModelState.AddModelError("Password", "Password must be at least 6 characters long.");
        }

        DegreeProgram? selectedProgram = null;
        if (!string.IsNullOrEmpty(programSlug))
        {
            var allPrograms = await _programService.GetProgramsAsync();
            selectedProgram = allPrograms.FirstOrDefault(p => 
                p.Slug.Equals(programSlug, StringComparison.OrdinalIgnoreCase) || 
                p.Code.Equals(programSlug, StringComparison.OrdinalIgnoreCase));
        }

        if (!ModelState.IsValid)
        {
            ViewBag.ActiveSession = await _admissionService.GetActiveAdmissionSessionAsync();
            ViewBag.SelectedProgram = selectedProgram;
            ViewBag.SystemSettings = await _settingService.GetSettingsAsync();
            return View(applicant);
        }

        var existing = await _admissionService.GetApplicantByEmailAsync(applicant.Email);
        if (existing != null)
        {
            ModelState.AddModelError("Email", "An applicant account with this email address already exists.");
            ViewBag.ActiveSession = await _admissionService.GetActiveAdmissionSessionAsync();
            ViewBag.SelectedProgram = selectedProgram;
            ViewBag.SystemSettings = await _settingService.GetSettingsAsync();
            return View(applicant);
        }

        // Format phone number with country code
        if (!string.IsNullOrEmpty(countryCode) && !applicant.PhoneNumber.StartsWith("+"))
        {
            applicant.PhoneNumber = $"{countryCode} {applicant.PhoneNumber.TrimStart('0')}";
        }

        // Duplicate Phone Number Check
        var existingPhone = await _admissionService.GetApplicantByPhoneAsync(applicant.PhoneNumber);
        if (existingPhone != null)
        {
            ModelState.AddModelError("PhoneNumber", "An applicant account with this phone number already exists.");
            ViewBag.ActiveSession = await _admissionService.GetActiveAdmissionSessionAsync();
            ViewBag.SelectedProgram = selectedProgram;
            ViewBag.SystemSettings = await _settingService.GetSettingsAsync();
            return View(applicant);
        }

        var registered = await _admissionService.RegisterApplicantAsync(applicant, password);
        TempData["SuccessMessage"] = "Applicant account created successfully! Please log in to complete your admission application.";

        return RedirectToAction(nameof(Login), new { program = programSlug });
    }

    // Applicant Login Entry: /Admission/Login
    [HttpGet("Login")]
    public async Task<IActionResult> Login(string? program)
    {
        ViewBag.SelectedProgram = program;
        ViewBag.SystemSettings = await _settingService.GetSettingsAsync();
        return View();
    }

    // Handle Applicant Login POST
    [HttpPost("Login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password, bool rememberMe, string? program)
    {
        var applicant = await _admissionService.AuthenticateApplicantAsync(email, password);
        if (applicant == null)
        {
            ViewBag.ErrorMessage = "Invalid email address or password, or account is suspended/blocked.";
            ViewBag.SystemSettings = await _settingService.GetSettingsAsync();
            return View();
        }

        // Store Session & Claims Authentication
        HttpContext.Session.SetInt32("ApplicantId", applicant.Id);
        HttpContext.Session.SetString("ApplicantName", applicant.FullName);

        return RedirectToAction(nameof(Dashboard), new { program });
    }

    // Forgot Password GET: /Admission/ForgotPassword
    [HttpGet("ForgotPassword")]
    public async Task<IActionResult> ForgotPassword()
    {
        ViewBag.SystemSettings = await _settingService.GetSettingsAsync();
        return View();
    }

    // Forgot Password POST: /Admission/ForgotPassword
    [HttpPost("ForgotPassword")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(string email)
    {
        ViewBag.SystemSettings = await _settingService.GetSettingsAsync();
        if (string.IsNullOrWhiteSpace(email))
        {
            ViewBag.ErrorMessage = "Please enter your registered email address.";
            return View();
        }

        var token = await _admissionService.GeneratePasswordResetTokenAsync(email);
        if (token != null)
        {
            ViewBag.SuccessMessage = $"Password reset token generated. Verification Token: {token} (Valid for 24 hours).";
            ViewBag.ResetToken = token;
            ViewBag.TargetEmail = email;
        }
        else
        {
            ViewBag.ErrorMessage = "If an account exists with this email address, a password reset link has been dispatched.";
        }

        return View();
    }

    // Reset Password GET: /Admission/ResetPassword
    [HttpGet("ResetPassword")]
    public async Task<IActionResult> ResetPassword(string email, string token)
    {
        ViewBag.Email = email;
        ViewBag.Token = token;
        ViewBag.SystemSettings = await _settingService.GetSettingsAsync();
        return View();
    }

    // Reset Password POST: /Admission/ResetPassword
    [HttpPost("ResetPassword")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(string email, string token, string newPassword, string confirmPassword)
    {
        ViewBag.SystemSettings = await _settingService.GetSettingsAsync();
        if (newPassword != confirmPassword)
        {
            ViewBag.ErrorMessage = "Passwords do not match.";
            ViewBag.Email = email;
            ViewBag.Token = token;
            return View();
        }

        bool success = await _admissionService.ResetPasswordWithTokenAsync(email, token, newPassword);
        if (!success)
        {
            ViewBag.ErrorMessage = "Invalid or expired password reset token.";
            ViewBag.Email = email;
            ViewBag.Token = token;
            return View();
        }

        TempData["SuccessMessage"] = "Your password has been reset successfully! Please log in with your new password.";
        return RedirectToAction(nameof(Login));
    }

    // Applicant Dashboard Entry: /Admission/Dashboard
    [HttpGet("Dashboard")]
    public async Task<IActionResult> Dashboard(string? program)
    {
        int? applicantId = HttpContext.Session.GetInt32("ApplicantId");
        if (!applicantId.HasValue)
        {
            return RedirectToAction(nameof(Login));
        }

        var applicant = await _admissionService.GetApplicantByIdAsync(applicantId.Value);
        var profile = await _admissionService.GetApplicantProfileAsync(applicantId.Value);
        var applications = await _admissionService.GetApplicationsByApplicantAsync(applicantId.Value);
        var activeSession = await _admissionService.GetActiveAdmissionSessionAsync();
        var enrollment = await _admissionService.GetEnrollmentRecordByApplicantAsync(applicantId.Value);
        var notifications = await _admissionService.GetApplicantNotificationsAsync(applicantId.Value);
        var documents = await _admissionService.GetDocumentsByApplicantAsync(applicantId.Value);

        var latestApp = applications.FirstOrDefault();
        var timelineLogs = latestApp != null 
            ? await _admissionService.GetTimelineLogsAsync(latestApp.Id) 
            : new List<ApplicationTimelineLog>();

        AdmissionTestSchedule? testSchedule = null;
        AdmissionTestResult? testResult = null;
        if (latestApp != null)
        {
            testSchedule = await _admissionService.GetTestScheduleByApplicationAsync(latestApp.Id);
            testResult = await _admissionService.GetTestResultByApplicationAsync(latestApp.Id);
        }

        ViewBag.Applicant = applicant;
        ViewBag.ApplicantProfile = profile;
        ViewBag.Applications = applications;
        ViewBag.ActiveSession = activeSession;
        ViewBag.EnrollmentRecord = enrollment;
        ViewBag.Notifications = notifications;
        ViewBag.Documents = documents;
        ViewBag.TimelineLogs = timelineLogs;
        ViewBag.TestSchedule = testSchedule;
        ViewBag.TestResult = testResult;
        ViewBag.SelectedProgramSlug = program;
        ViewBag.SystemSettings = await _settingService.GetSettingsAsync();

        return View();
    }

    // Student Portal Showcase Page: /Admission/StudentPortal
    [HttpGet("StudentPortal")]
    public async Task<IActionResult> StudentPortal()
    {
        int? applicantId = HttpContext.Session.GetInt32("ApplicantId");
        if (!applicantId.HasValue)
        {
            return RedirectToAction(nameof(Login));
        }

        var applicant = await _admissionService.GetApplicantByIdAsync(applicantId.Value);
        var enrollment = await _admissionService.GetEnrollmentRecordByApplicantAsync(applicantId.Value);
        var applications = await _admissionService.GetApplicationsByApplicantAsync(applicantId.Value);
        var latestApp = applications.FirstOrDefault();

        // FAILSAFE: If latestApp is Approved but enrollment is missing, auto-provision
        if (enrollment == null && latestApp != null && latestApp.CurrentStatus == ApplicationStatus.Approved)
        {
            enrollment = await _admissionService.ProcessAdmissionDecisionAsync(latestApp.Id, "Approve", "Auto-provisioned on student portal access", "System");
        }

        bool isApprovedOrActive = (enrollment != null && enrollment.EnrollmentStatus == "Active") || (latestApp != null && latestApp.CurrentStatus == ApplicationStatus.Approved);

        if (!isApprovedOrActive)
        {
            TempData["ErrorMessage"] = "Student Portal Account is locked. Admission Confirmation & Fee Verification required.";
            return RedirectToAction(nameof(Dashboard));
        }

        ViewBag.Applicant = applicant;
        ViewBag.EnrollmentRecord = enrollment;
        ViewBag.Application = latestApp;
        ViewBag.SystemSettings = await _settingService.GetSettingsAsync();

        return View();
    }

    // Profile Completion Wizard GET: /Admission/ProfileWizard
    [HttpGet("ProfileWizard")]
    public async Task<IActionResult> ProfileWizard()
    {
        int? applicantId = HttpContext.Session.GetInt32("ApplicantId");
        if (!applicantId.HasValue)
        {
            return RedirectToAction(nameof(Login));
        }

        var applicant = await _admissionService.GetApplicantByIdAsync(applicantId.Value);
        var guardian = await _admissionService.GetGuardianInfoAsync(applicantId.Value);

        ViewBag.Applicant = applicant;
        ViewBag.Guardian = guardian;
        ViewBag.SystemSettings = await _settingService.GetSettingsAsync();

        return View();
    }

    // Profile Completion Wizard POST: /Admission/SaveProfileWizard
    [HttpPost("SaveProfileWizard")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveProfileWizard(
        string FullName,
        string FatherName,
        string CNIC_Passport,
        DateTime DateOfBirth,
        string Gender,
        string Nationality,
        string Domicile,
        string PermanentAddress,
        string PostalAddress,
        string City,
        string Province,
        string GuardianName,
        string GuardianRelation,
        string GuardianCNIC,
        string GuardianOccupation,
        string GuardianPhone,
        decimal MonthlyIncome,
        string EmergencyContactName,
        string EmergencyContactPhone,
        string EmergencyAddress,
        IFormFile? profilePhoto)
    {
        int? applicantId = HttpContext.Session.GetInt32("ApplicantId");
        if (!applicantId.HasValue)
        {
            return RedirectToAction(nameof(Login));
        }

        var profile = await _admissionService.GetApplicantProfileAsync(applicantId.Value) ?? new ApplicantProfile { ApplicantId = applicantId.Value };

        profile.FullName = FullName;
        profile.FatherName = FatherName;
        profile.CNIC_Passport = CNIC_Passport;
        profile.DateOfBirth = DateOfBirth;
        profile.Gender = Enum.TryParse<ApplicantGender>(Gender, true, out var g) ? g : ApplicantGender.Male;
        profile.Nationality = Nationality;
        profile.Domicile = Domicile;
        profile.PermanentAddress = PermanentAddress;
        profile.PostalAddress = PostalAddress;
        profile.City = City;
        profile.Province = Province;
        profile.GuardianName = GuardianName;
        profile.GuardianPhone = GuardianPhone;
        profile.EmergencyContactName = EmergencyContactName;
        profile.EmergencyContactPhone = EmergencyContactPhone;
        profile.ProfileCompletionPercentage = 100; // Complete Profile Wizard

        // Handle Profile Photo Upload
        if (profilePhoto != null && profilePhoto.Length > 0)
        {
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "applicants");
            Directory.CreateDirectory(uploadsFolder);
            string uniqueFileName = $"{applicantId.Value}_profile_{Guid.NewGuid():N}{Path.GetExtension(profilePhoto.FileName)}";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await profilePhoto.CopyToAsync(stream);
            }
            profile.ProfilePhotoPath = $"/uploads/applicants/{uniqueFileName}";
        }

        await _admissionService.UpdateApplicantProfileAsync(profile);

        // Save Guardian Info
        var guardian = await _admissionService.GetGuardianInfoAsync(applicantId.Value) ?? new ApplicantGuardianInfo { ApplicantId = applicantId.Value };
        guardian.FatherName = FatherName;
        guardian.FatherCNIC = GuardianCNIC;
        guardian.FatherOccupation = GuardianOccupation;
        guardian.FatherPhone = GuardianPhone;
        guardian.GuardianName = GuardianName;
        guardian.GuardianRelation = GuardianRelation;
        guardian.GuardianPhone = GuardianPhone;
        guardian.GuardianCNIC = GuardianCNIC;
        guardian.GuardianOccupation = GuardianOccupation;
        guardian.MonthlyIncome = MonthlyIncome;
        guardian.EmergencyAddress = EmergencyAddress;

        await _admissionService.SaveGuardianInfoAsync(guardian);
        await _admissionService.AddNotificationAsync(applicantId.Value, "Profile Completed 100%", "Your applicant profile details were updated and verified successfully.", "Success");

        TempData["SuccessMessage"] = "Profile completed successfully! Your profile completion is now 100%.";
        return RedirectToAction(nameof(Dashboard));
    }

    // Multi-Step Admission Application Wizard: /Admission/Apply
    [HttpGet("Apply")]
    public async Task<IActionResult> Apply(string? program)
    {
        int? applicantId = HttpContext.Session.GetInt32("ApplicantId");
        if (!applicantId.HasValue)
        {
            return RedirectToAction(nameof(Login), new { program });
        }

        var applicant = await _admissionService.GetApplicantByIdAsync(applicantId.Value);
        if (applicant == null) return RedirectToAction(nameof(Login));

        var activeSession = await _admissionService.GetActiveAdmissionSessionAsync();
        var profile = await _admissionService.GetApplicantProfileAsync(applicantId.Value);
        var guardian = await _admissionService.GetGuardianInfoAsync(applicantId.Value);
        var academicRecords = await _admissionService.GetAcademicRecordsAsync(applicantId.Value);
        var documents = await _admissionService.GetDocumentsByApplicantAsync(applicantId.Value);
        var existingApps = await _admissionService.GetApplicationsByApplicantAsync(applicantId.Value);

        DegreeProgram? selectedProgram = null;
        if (!string.IsNullOrEmpty(program))
        {
            var allPrograms = await _programService.GetProgramsAsync();
            selectedProgram = allPrograms.FirstOrDefault(p => 
                p.Slug.Equals(program, StringComparison.OrdinalIgnoreCase) || 
                p.Code.Equals(program, StringComparison.OrdinalIgnoreCase));
        }

        var activePrograms = await _programService.GetProgramsAsync();

        ViewBag.Applicant = applicant;
        ViewBag.ApplicantProfile = profile;
        ViewBag.GuardianInfo = guardian;
        ViewBag.AcademicRecords = academicRecords;
        ViewBag.UploadedDocuments = documents;
        ViewBag.ActiveSession = activeSession;
        ViewBag.SelectedProgram = selectedProgram;
        ViewBag.AllPrograms = activePrograms;
        ViewBag.ExistingApplications = existingApps;
        ViewBag.SystemSettings = await _settingService.GetSettingsAsync();

        return View();
    }

    // Submit Application POST: /Admission/SubmitApplication
    [HttpPost("SubmitApplication")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitApplication(
        int degreeProgramId,
        string sscBoard, string sscTitle, int sscYear, string sscRoll, double sscTotal, double sscObtained,
        string hsscBoard, string hsscTitle, int hsscYear, string hsscRoll, double hsscTotal, double hsscObtained,
        string fatherName, string fatherCnic, string fatherOccupation, string guardianName, string guardianRelation, string guardianPhone, decimal monthlyIncome,
        string declarationSigned)
    {
        int? applicantId = HttpContext.Session.GetInt32("ApplicantId");
        if (!applicantId.HasValue) return RedirectToAction(nameof(Login));

        var activeSession = await _admissionService.GetActiveAdmissionSessionAsync();
        if (activeSession == null)
        {
            TempData["ErrorMessage"] = "No active admission session available.";
            return RedirectToAction(nameof(Dashboard));
        }

        // Save Academic Records (Matric & Inter)
        var sscRecord = new ApplicantAcademicRecord
        {
            ApplicantId = applicantId.Value,
            Level = "Matriculation",
            DegreeOrCertificateTitle = string.IsNullOrWhiteSpace(sscTitle) ? "SSC Science" : sscTitle,
            BoardOrUniversity = string.IsNullOrWhiteSpace(sscBoard) ? "BISE Karachi" : sscBoard,
            PassingYear = sscYear > 1900 ? sscYear : 2022,
            RollNumber = sscRoll ?? "",
            TotalMarks = sscTotal > 0 ? sscTotal : 1100,
            ObtainedMarks = sscObtained,
            Percentage = sscTotal > 0 ? Math.Round((sscObtained / sscTotal) * 100, 2) : 0
        };
        await _admissionService.SaveAcademicRecordAsync(sscRecord);

        var hsscRecord = new ApplicantAcademicRecord
        {
            ApplicantId = applicantId.Value,
            Level = "Intermediate",
            DegreeOrCertificateTitle = string.IsNullOrWhiteSpace(hsscTitle) ? "HSSC Pre-Engineering" : hsscTitle,
            BoardOrUniversity = string.IsNullOrWhiteSpace(hsscBoard) ? "BISE Karachi" : hsscBoard,
            PassingYear = hsscYear > 1900 ? hsscYear : 2024,
            RollNumber = hsscRoll ?? "",
            TotalMarks = hsscTotal > 0 ? hsscTotal : 1100,
            ObtainedMarks = hsscObtained,
            Percentage = hsscTotal > 0 ? Math.Round((hsscObtained / hsscTotal) * 100, 2) : 0
        };
        await _admissionService.SaveAcademicRecordAsync(hsscRecord);

        // Save Guardian Info
        var guardian = new ApplicantGuardianInfo
        {
            ApplicantId = applicantId.Value,
            FatherName = fatherName ?? "Father",
            FatherCNIC = fatherCnic,
            FatherOccupation = fatherOccupation,
            GuardianName = string.IsNullOrWhiteSpace(guardianName) ? (fatherName ?? "Father") : guardianName,
            GuardianRelation = string.IsNullOrWhiteSpace(guardianRelation) ? "Father" : guardianRelation,
            GuardianPhone = guardianPhone ?? "",
            MonthlyIncome = monthlyIncome
        };
        await _admissionService.SaveGuardianInfoAsync(guardian);

        // Create & Submit Application Draft
        var app = await _admissionService.CreateApplicationDraftAsync(applicantId.Value, activeSession.Id, degreeProgramId);
        await _admissionService.SubmitApplicationAsync(app.Id);

        TempData["SuccessMessage"] = $"Congratulations! Your admission application ({app.ApplicationNumber}) has been submitted successfully.";
        return RedirectToAction(nameof(Dashboard));
    }

    // ==========================================
    // PROFILE MANAGEMENT MODULE
    // ==========================================

    [HttpGet("Profile")]
    public async Task<IActionResult> Profile()
    {
        int? applicantId = HttpContext.Session.GetInt32("ApplicantId");
        if (!applicantId.HasValue) return RedirectToAction(nameof(Login));

        var applicant = await _admissionService.GetApplicantByIdAsync(applicantId.Value);
        var profile = await _admissionService.GetApplicantProfileAsync(applicantId.Value);
        var guardian = await _admissionService.GetGuardianInfoAsync(applicantId.Value);

        ViewBag.Applicant = applicant;
        ViewBag.ApplicantProfile = profile;
        ViewBag.GuardianInfo = guardian;
        ViewBag.SystemSettings = await _settingService.GetSettingsAsync();

        return View();
    }

    [HttpPost("SaveProfile")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveProfile(string fullName, string city, string address, string nationality, string cnic, DateTime dateOfBirth)
    {
        int? applicantId = HttpContext.Session.GetInt32("ApplicantId");
        if (!applicantId.HasValue) return RedirectToAction(nameof(Login));

        var applicant = await _admissionService.GetApplicantByIdAsync(applicantId.Value);
        if (applicant != null)
        {
            applicant.FullName = fullName;
            applicant.City = city;
            applicant.Address = address;
            applicant.Nationality = nationality;
            applicant.CNIC_Passport = cnic;
            applicant.DateOfBirth = dateOfBirth;

            var profile = await _admissionService.GetApplicantProfileAsync(applicantId.Value);
            if (profile != null)
            {
                profile.City = city;
                profile.Address = address;
                profile.CNIC = cnic;
                profile.DateOfBirth = dateOfBirth;
                profile.ProfileCompletionPercentage = 100;
                await _admissionService.UpdateApplicantProfileAsync(profile);
            }
        }

        TempData["SuccessMessage"] = "Your applicant profile has been updated successfully.";
        return RedirectToAction(nameof(Profile));
    }

    // ==========================================
    // DOCUMENTS CENTER MODULE
    // ==========================================

    [HttpGet("Documents")]
    public async Task<IActionResult> Documents()
    {
        int? applicantId = HttpContext.Session.GetInt32("ApplicantId");
        if (!applicantId.HasValue) return RedirectToAction(nameof(Login));

        var applicant = await _admissionService.GetApplicantByIdAsync(applicantId.Value);
        var documents = await _admissionService.GetDocumentsByApplicantAsync(applicantId.Value);

        ViewBag.Applicant = applicant;
        ViewBag.Documents = documents;
        ViewBag.SystemSettings = await _settingService.GetSettingsAsync();

        return View();
    }

    [HttpPost("UploadDocument")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadDocument(DocumentType documentType, IFormFile file)
    {
        int? applicantId = HttpContext.Session.GetInt32("ApplicantId");
        if (!applicantId.HasValue) return RedirectToAction(nameof(Login));

        if (file == null || file.Length == 0)
        {
            TempData["ErrorMessage"] = "Please select a valid file to upload.";
            return RedirectToAction(nameof(Documents));
        }

        if (file.Length > 5 * 1024 * 1024)
        {
            TempData["ErrorMessage"] = "File size exceeds the 5MB limit.";
            return RedirectToAction(nameof(Documents));
        }

        string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext != ".pdf" && ext != ".jpg" && ext != ".jpeg" && ext != ".png")
        {
            TempData["ErrorMessage"] = "Invalid file format. Only PDF, JPG, and PNG files are allowed.";
            return RedirectToAction(nameof(Documents));
        }

        string uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "documents");
        if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

        string uniqueFileName = $"{applicantId.Value}_{documentType}_{DateTime.UtcNow:yyyyMMddHHmmss}{ext}";
        string filePath = Path.Combine(uploadDir, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var doc = new ApplicationDocument
        {
            ApplicantId = applicantId.Value,
            DocumentType = documentType,
            FileName = file.FileName,
            FilePath = $"/uploads/documents/{uniqueFileName}",
            FileSizeBytes = file.Length,
            FileType = file.ContentType
        };

        var applications = await _admissionService.GetApplicationsByApplicantAsync(applicantId.Value);
        if (applications.Any())
        {
            doc.AdmissionApplicationId = applications.First().Id;
        }

        await _admissionService.UploadDocumentAsync(doc);
        TempData["SuccessMessage"] = $"{documentType} document uploaded successfully.";
        return RedirectToAction(nameof(Documents));
    }

    [HttpPost("DeleteDocument/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDocument(int id)
    {
        int? applicantId = HttpContext.Session.GetInt32("ApplicantId");
        if (!applicantId.HasValue) return RedirectToAction(nameof(Login));

        bool deleted = await _admissionService.DeleteDocumentAsync(id, applicantId.Value);
        if (deleted) TempData["SuccessMessage"] = "Document removed successfully.";
        else TempData["ErrorMessage"] = "Unable to remove document.";

        return RedirectToAction(nameof(Documents));
    }

    [HttpPost("ReuploadDocument")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReuploadDocument(int documentId, IFormFile file)
    {
        int? applicantId = HttpContext.Session.GetInt32("ApplicantId");
        if (!applicantId.HasValue) return RedirectToAction(nameof(Login));

        if (file == null || file.Length == 0)
        {
            TempData["ErrorMessage"] = "Please select a valid file to upload.";
            return RedirectToAction(nameof(Documents));
        }

        string uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "documents");
        if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);

        string fileExt = Path.GetExtension(file.FileName);
        string uniqueFileName = $"DOC_RE_{documentId}_{DateTime.UtcNow:yyyyMMddHHmmss}{fileExt}";
        string filePath = Path.Combine(uploadsDir, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        string relativePath = $"/uploads/documents/{uniqueFileName}";
        await _admissionService.ReuploadDocumentAsync(documentId, relativePath, file.FileName, file.Length, $"Applicant-{applicantId.Value}");

        TempData["SuccessMessage"] = "Replacement document uploaded successfully and sent for re-verification!";
        return RedirectToAction(nameof(Documents));
    }

    // ==========================================
    // FEE VOUCHER & PRINTING MODULE
    // ==========================================

    [HttpGet("FeeVouchers")]
    public async Task<IActionResult> FeeVouchers()
    {
        int? applicantId = HttpContext.Session.GetInt32("ApplicantId");
        if (!applicantId.HasValue) return RedirectToAction(nameof(Login));

        var applicant = await _admissionService.GetApplicantByIdAsync(applicantId.Value);
        var vouchers = await _admissionService.GetVouchersByApplicantAsync(applicantId.Value);

        ViewBag.Applicant = applicant;
        ViewBag.Vouchers = vouchers;
        ViewBag.SystemSettings = await _settingService.GetSettingsAsync();

        return View();
    }

    [HttpGet("PrintVoucher/{id}")]
    public async Task<IActionResult> PrintVoucher(int id)
    {
        int? applicantId = HttpContext.Session.GetInt32("ApplicantId");
        if (!applicantId.HasValue) return RedirectToAction(nameof(Login));

        var voucher = await _admissionService.GetVoucherByIdAsync(id);
        if (voucher == null || voucher.ApplicantId != applicantId.Value)
        {
            TempData["ErrorMessage"] = "Voucher not found.";
            return RedirectToAction(nameof(FeeVouchers));
        }

        ViewBag.Voucher = voucher;
        ViewBag.SystemSettings = await _settingService.GetSettingsAsync();

        return View();
    }

    [HttpPost("UploadPaymentProof")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadPaymentProof(int voucherId, string paymentMethod, string transactionRef, IFormFile proofFile)
    {
        int? applicantId = HttpContext.Session.GetInt32("ApplicantId");
        if (!applicantId.HasValue) return RedirectToAction(nameof(Login));

        string proofPath = "/uploads/payments/receipt_demo.pdf";
        if (proofFile != null && proofFile.Length > 0)
        {
            string uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "payments");
            if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

            string ext = Path.GetExtension(proofFile.FileName).ToLowerInvariant();
            string uniqueName = $"PROOF_{voucherId}_{DateTime.UtcNow:yyyyMMddHHmmss}{ext}";
            string savePath = Path.Combine(uploadDir, uniqueName);

            using (var stream = new FileStream(savePath, FileMode.Create))
            {
                await proofFile.CopyToAsync(stream);
            }
            proofPath = $"/uploads/payments/{uniqueName}";
        }

        bool success = await _admissionService.UploadPaymentProofAsync(voucherId, proofPath, paymentMethod, transactionRef);
        if (success) TempData["SuccessMessage"] = "Payment proof uploaded! Your voucher status is now Pending Verification.";
        else TempData["ErrorMessage"] = "Failed to upload payment proof.";

        return RedirectToAction(nameof(FeeVouchers));
    }

    // ==========================================
    // MESSAGING CENTER MODULE
    // ==========================================

    [HttpGet("Messages")]
    public async Task<IActionResult> Messages()
    {
        int? applicantId = HttpContext.Session.GetInt32("ApplicantId");
        if (!applicantId.HasValue) return RedirectToAction(nameof(Login));

        var applicant = await _admissionService.GetApplicantByIdAsync(applicantId.Value);
        var messages = await _admissionService.GetMessagesAsync(applicantId.Value);
        await _admissionService.MarkMessagesAsReadAsync(applicantId.Value);

        ViewBag.Applicant = applicant;
        ViewBag.Messages = messages;
        ViewBag.SystemSettings = await _settingService.GetSettingsAsync();

        return View();
    }

    [HttpPost("SendMessage")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendMessage(string subject, string messageBody, IFormFile? attachment)
    {
        int? applicantId = HttpContext.Session.GetInt32("ApplicantId");
        if (!applicantId.HasValue) return RedirectToAction(nameof(Login));

        if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(messageBody))
        {
            TempData["ErrorMessage"] = "Subject and message body are required.";
            return RedirectToAction(nameof(Messages));
        }

        string? attachmentPath = null;
        if (attachment != null && attachment.Length > 0)
        {
            string uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "messages");
            if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

            string ext = Path.GetExtension(attachment.FileName).ToLowerInvariant();
            string uniqueName = $"MSG_{applicantId.Value}_{DateTime.UtcNow:yyyyMMddHHmmss}{ext}";
            string savePath = Path.Combine(uploadDir, uniqueName);

            using (var stream = new FileStream(savePath, FileMode.Create))
            {
                await attachment.CopyToAsync(stream);
            }
            attachmentPath = $"/uploads/messages/{uniqueName}";
        }

        var msg = new ApplicantMessage
        {
            ApplicantId = applicantId.Value,
            SenderType = "Applicant",
            SenderName = HttpContext.Session.GetString("ApplicantName") ?? "Applicant",
            Subject = subject,
            MessageBody = messageBody,
            AttachmentPath = attachmentPath
        };

        await _admissionService.SendMessageAsync(msg);
        TempData["SuccessMessage"] = "Your message has been sent to the Admission Desk.";
        return RedirectToAction(nameof(Messages));
    }

    // ==========================================
    // NOTIFICATIONS MODULE
    // ==========================================

    [HttpGet("Notifications")]
    public async Task<IActionResult> Notifications()
    {
        int? applicantId = HttpContext.Session.GetInt32("ApplicantId");
        if (!applicantId.HasValue) return RedirectToAction(nameof(Login));

        var applicant = await _admissionService.GetApplicantByIdAsync(applicantId.Value);
        var applications = await _admissionService.GetApplicationsByApplicantAsync(applicantId.Value);

        ViewBag.Applicant = applicant;
        ViewBag.Applications = applications;
        ViewBag.SystemSettings = await _settingService.GetSettingsAsync();

        return View();
    }

    // ==========================================
    // PORTAL SETTINGS & SECURITY
    // ==========================================

    [HttpGet("Settings")]
    public async Task<IActionResult> Settings()
    {
        int? applicantId = HttpContext.Session.GetInt32("ApplicantId");
        if (!applicantId.HasValue) return RedirectToAction(nameof(Login));

        var applicant = await _admissionService.GetApplicantByIdAsync(applicantId.Value);

        ViewBag.Applicant = applicant;
        ViewBag.SystemSettings = await _settingService.GetSettingsAsync();

        return View();
    }

    [HttpPost("ChangePassword")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
    {
        int? applicantId = HttpContext.Session.GetInt32("ApplicantId");
        if (!applicantId.HasValue) return RedirectToAction(nameof(Login));

        if (newPassword != confirmPassword)
        {
            TempData["ErrorMessage"] = "New password and Confirm password do not match.";
            return RedirectToAction(nameof(Settings));
        }

        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
        {
            TempData["ErrorMessage"] = "New password must be at least 6 characters long.";
            return RedirectToAction(nameof(Settings));
        }

        var applicant = await _admissionService.GetApplicantByIdAsync(applicantId.Value);
        if (applicant == null) return RedirectToAction(nameof(Login));

        var authCheck = await _admissionService.AuthenticateApplicantAsync(applicant.Email, currentPassword);
        if (authCheck == null)
        {
            TempData["ErrorMessage"] = "Current password is incorrect.";
            return RedirectToAction(nameof(Settings));
        }

        // Reset password
        string token = await _admissionService.GeneratePasswordResetTokenAsync(applicant.Email) ?? "";
        bool success = await _admissionService.ResetPasswordWithTokenAsync(applicant.Email, token, newPassword);

        if (success) TempData["SuccessMessage"] = "Your password has been changed successfully.";
        else TempData["ErrorMessage"] = "Unable to change password.";

        return RedirectToAction(nameof(Settings));
    }

    // ==========================================
    // HELP CENTER & SUPPORT TICKETS
    // ==========================================

    [HttpGet("Help")]
    public async Task<IActionResult> Help()
    {
        int? applicantId = HttpContext.Session.GetInt32("ApplicantId");
        if (!applicantId.HasValue) return RedirectToAction(nameof(Login));

        var applicant = await _admissionService.GetApplicantByIdAsync(applicantId.Value);
        var tickets = await _admissionService.GetSupportTicketsAsync(applicantId.Value);

        ViewBag.Applicant = applicant;
        ViewBag.Tickets = tickets;
        ViewBag.SystemSettings = await _settingService.GetSettingsAsync();

        return View();
    }

    [HttpPost("CreateTicket")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTicket(string subject, string category, string priority, string description)
    {
        int? applicantId = HttpContext.Session.GetInt32("ApplicantId");
        if (!applicantId.HasValue) return RedirectToAction(nameof(Login));

        if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(description))
        {
            TempData["ErrorMessage"] = "Subject and description are required.";
            return RedirectToAction(nameof(Help));
        }

        var ticket = new SupportTicket
        {
            ApplicantId = applicantId.Value,
            Subject = subject,
            Category = category ?? "General",
            Priority = priority ?? "Medium",
            Description = description
        };

        await _admissionService.CreateSupportTicketAsync(ticket);
        TempData["SuccessMessage"] = $"Support ticket ({ticket.TicketNumber}) created successfully! Our team will respond shortly.";
        return RedirectToAction(nameof(Help));
    }

    // Applicant Logout: /Admission/Logout
    [HttpGet("Logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        TempData["SuccessMessage"] = "You have been logged out safely.";
        return RedirectToAction(nameof(Index));
    }
}
