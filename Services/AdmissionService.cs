using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using NedAcademyWebsite.Data;
using NedAcademyWebsite.Models;

namespace NedAcademyWebsite.Services;

public class AdmissionService : IAdmissionService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AdmissionService> _logger;

    public AdmissionService(ApplicationDbContext context, ILogger<AdmissionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ==========================================
    // SESSIONS & PROGRAM OFFERINGS
    // ==========================================

    public async Task<AdmissionSession?> GetActiveAdmissionSessionAsync()
    {
        return await _context.AdmissionSessions
            .Include(s => s.SessionPrograms)
                .ThenInclude(sp => sp.DegreeProgram)
            .FirstOrDefaultAsync(s => s.Status == AdmissionSessionStatus.OpenForApplications && s.ApplicationDeadline >= DateTime.UtcNow);
    }

    public async Task<IEnumerable<AdmissionSession>> GetAllAdmissionSessionsAsync()
    {
        return await _context.AdmissionSessions
            .Include(s => s.SessionPrograms)
                .ThenInclude(sp => sp.DegreeProgram)
            .OrderByDescending(s => s.StartDate)
            .ToListAsync();
    }

    public async Task<AdmissionSession> CreateAdmissionSessionAsync(AdmissionSession session, IEnumerable<int> degreeProgramIds)
    {
        _context.AdmissionSessions.Add(session);
        await _context.SaveChangesAsync();

        foreach (var progId in degreeProgramIds)
        {
            _context.AdmissionSessionPrograms.Add(new AdmissionSessionProgram
            {
                AdmissionSessionId = session.Id,
                DegreeProgramId = progId,
                CapacitySeats = 60,
                AvailableSeats = 60,
                IsActive = true
            });
        }
        await _context.SaveChangesAsync();

        await RecordAuditLogAsync("CreateSession", "AdmissionSession", session.Id.ToString(), "Admin", "127.0.0.1", $"Created session {session.Title}");
        return session;
    }

    public async Task<IEnumerable<AdmissionSessionProgram>> GetOfferedProgramsAsync(int sessionId)
    {
        return await _context.AdmissionSessionPrograms
            .Include(sp => sp.DegreeProgram)
                .ThenInclude(dp => dp!.Department)
            .Where(sp => sp.AdmissionSessionId == sessionId && sp.IsActive)
            .ToListAsync();
    }

    // ==========================================
    // APPLICANT MANAGEMENT & AUTHENTICATION
    // ==========================================

    public async Task<Applicant?> AuthenticateApplicantAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) return null;

        var applicant = await GetApplicantByEmailAsync(email.Trim());
        if (applicant == null || !applicant.IsActive || applicant.IsDeleted) return null;
        if (applicant.AccountStatus == ApplicantAccountStatus.Blocked || applicant.AccountStatus == ApplicantAccountStatus.Suspended) return null;

        string hash = HashPasswordWithSalt(password.Trim(), applicant.PasswordSalt);
        if (applicant.PasswordHash != hash) return null;

        applicant.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await RecordAuditLogAsync("ApplicantLogin", "Applicant", applicant.Id.ToString(), applicant.Email, "127.0.0.1", "Applicant authenticated successfully");
        return applicant;
    }

    public async Task<Applicant?> GetApplicantByIdAsync(int applicantId)
    {
        return await _context.Applicants
            .Include(a => a.Profile)
            .Include(a => a.Applications)
                .ThenInclude(app => app.DegreeProgram)
            .Include(a => a.FeeVouchers)
            .Include(a => a.Documents)
            .FirstOrDefaultAsync(a => a.Id == applicantId && !a.IsDeleted);
    }

    public async Task<Applicant?> GetApplicantByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return null;
        string cleanEmail = email.Trim().ToLower();
        return await _context.Applicants
            .Include(a => a.Profile)
            .FirstOrDefaultAsync(a => a.Email.ToLower() == cleanEmail && !a.IsDeleted);
    }

    public async Task<Applicant?> GetApplicantByPhoneAsync(string phone)
    {
        return await _context.Applicants
            .FirstOrDefaultAsync(a => a.PhoneNumber == phone && !a.IsDeleted);
    }

    public async Task<Applicant> RegisterApplicantAsync(Applicant applicant, string rawPassword)
    {
        string salt = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
        applicant.PasswordSalt = salt;
        applicant.PasswordHash = HashPasswordWithSalt(rawPassword, salt);
        applicant.CreatedAt = DateTime.UtcNow;
        applicant.UpdatedAt = DateTime.UtcNow;
        applicant.IsActive = true;
        applicant.IsDeleted = false;
        applicant.AccountStatus = ApplicantAccountStatus.Active;

        _context.Applicants.Add(applicant);
        await _context.SaveChangesAsync();

        // Create initial Applicant Profile (30% completion)
        var profile = new ApplicantProfile
        {
            ApplicantId = applicant.Id,
            Nationality = applicant.Nationality ?? "Pakistani",
            Gender = applicant.Gender,
            City = applicant.City,
            Address = applicant.Address,
            Country = "Pakistan",
            ProfileCompletionPercentage = 30,
            LastUpdatedAt = DateTime.UtcNow
        };
        _context.ApplicantProfiles.Add(profile);
        await _context.SaveChangesAsync();

        await RecordAuditLogAsync("RegisterApplicant", "Applicant", applicant.Id.ToString(), applicant.Email, "127.0.0.1", $"New applicant registered: {applicant.FullName}");
        return applicant;
    }

    public async Task<string?> GeneratePasswordResetTokenAsync(string email)
    {
        var applicant = await GetApplicantByEmailAsync(email);
        if (applicant == null) return null;

        string token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        applicant.PasswordResetToken = token;
        applicant.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(24);
        await _context.SaveChangesAsync();

        await RecordAuditLogAsync("GenerateResetToken", "Applicant", applicant.Id.ToString(), email, "127.0.0.1", "Password reset token generated");
        return token;
    }

    public async Task<bool> ResetPasswordWithTokenAsync(string email, string token, string newPassword)
    {
        var applicant = await GetApplicantByEmailAsync(email);
        if (applicant == null || applicant.PasswordResetToken != token || applicant.PasswordResetTokenExpiry < DateTime.UtcNow)
        {
            return false;
        }

        string salt = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
        applicant.PasswordSalt = salt;
        applicant.PasswordHash = HashPasswordWithSalt(newPassword, salt);
        applicant.PasswordResetToken = null;
        applicant.PasswordResetTokenExpiry = null;
        applicant.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await RecordAuditLogAsync("ResetPassword", "Applicant", applicant.Id.ToString(), email, "127.0.0.1", "Password reset successfully via token");
        return true;
    }

    public async Task<bool> VerifyEmailTokenAsync(string email, string token)
    {
        var applicant = await GetApplicantByEmailAsync(email);
        if (applicant == null || applicant.EmailVerificationToken != token || applicant.EmailVerificationTokenExpiry < DateTime.UtcNow)
        {
            return false;
        }

        applicant.IsEmailVerified = true;
        applicant.EmailVerificationToken = null;
        applicant.EmailVerificationTokenExpiry = null;
        await _context.SaveChangesAsync();

        await RecordAuditLogAsync("VerifyEmail", "Applicant", applicant.Id.ToString(), email, "127.0.0.1", "Applicant email verified");
        return true;
    }

    public async Task<ApplicantProfile?> GetApplicantProfileAsync(int applicantId)
    {
        return await _context.ApplicantProfiles
            .Include(p => p.Applicant)
            .FirstOrDefaultAsync(p => p.ApplicantId == applicantId);
    }

    public async Task UpdateApplicantProfileAsync(ApplicantProfile profile)
    {
        profile.LastUpdatedAt = DateTime.UtcNow;
        _context.ApplicantProfiles.Update(profile);
        await _context.SaveChangesAsync();
    }

    // ==========================================
    // ADMISSION APPLICATIONS LIFECYCLE
    // ==========================================

    public async Task<AdmissionApplication> CreateApplicationDraftAsync(int applicantId, int sessionId, int degreeProgramId)
    {
        string appNum = $"APP-{DateTime.UtcNow:yyyyMM}-{RandomNumberGenerator.GetInt32(10000, 99999)}";

        var application = new AdmissionApplication
        {
            ApplicationNumber = appNum,
            ApplicantId = applicantId,
            AdmissionSessionId = sessionId,
            DegreeProgramId = degreeProgramId,
            CurrentStatus = ApplicationStatus.Draft,
            CurrentStepNumber = 1,
            CreatedDate = DateTime.UtcNow,
            LastStatusUpdateDate = DateTime.UtcNow
        };

        _context.AdmissionApplications.Add(application);
        await _context.SaveChangesAsync();

        // Record Initial Status History
        _context.ApplicationStatusHistories.Add(new ApplicationStatusHistory
        {
            AdmissionApplicationId = application.Id,
            PreviousStatus = ApplicationStatus.Draft,
            NewStatus = ApplicationStatus.Draft,
            ChangedByUserId = applicantId.ToString(),
            ChangedByRole = "Applicant",
            Comments = "Application draft created",
            Timestamp = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        await RecordAuditLogAsync("CreateApplicationDraft", "AdmissionApplication", application.Id.ToString(), $"Applicant-{applicantId}", "127.0.0.1", $"Application draft created with number {appNum}");

        return application;
    }

    public async Task<AdmissionApplication?> GetApplicationByIdAsync(int applicationId)
    {
        return await _context.AdmissionApplications
            .Include(a => a.Applicant)
            .Include(a => a.AdmissionSession)
            .Include(a => a.DegreeProgram)
                .ThenInclude(dp => dp!.Department)
            .Include(a => a.UploadedDocuments)
            .Include(a => a.StatusHistory)
            .Include(a => a.FeeVouchers)
            .FirstOrDefaultAsync(a => a.Id == applicationId && !a.IsDeleted);
    }

    public async Task<IEnumerable<AdmissionApplication>> GetApplicationsByApplicantAsync(int applicantId)
    {
        return await _context.AdmissionApplications
            .Include(a => a.AdmissionSession)
            .Include(a => a.DegreeProgram)
            .Include(a => a.FeeVouchers)
            .Where(a => a.ApplicantId == applicantId && !a.IsDeleted)
            .OrderByDescending(a => a.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<AdmissionApplication>> GetApplicationsForReviewAsync(ApplicationStatus? filterStatus = null)
    {
        var query = _context.AdmissionApplications
            .Include(a => a.Applicant)
            .Include(a => a.AdmissionSession)
            .Include(a => a.DegreeProgram)
            .Include(a => a.UploadedDocuments)
            .Include(a => a.FeeVouchers)
            .Where(a => !a.IsDeleted);

        if (filterStatus.HasValue)
        {
            query = query.Where(a => a.CurrentStatus == filterStatus.Value);
        }

        return await query.OrderByDescending(a => a.LastStatusUpdateDate).ToListAsync();
    }

    public async Task UpdateApplicationStatusAsync(int applicationId, ApplicationStatus newStatus, string changedByUserId, string changedByRole, string? comments)
    {
        var app = await _context.AdmissionApplications.FindAsync(applicationId);
        if (app == null) return;

        var prevStatus = app.CurrentStatus;
        app.CurrentStatus = newStatus;
        app.LastStatusUpdateDate = DateTime.UtcNow;

        _context.ApplicationStatusHistories.Add(new ApplicationStatusHistory
        {
            AdmissionApplicationId = applicationId,
            PreviousStatus = prevStatus,
            NewStatus = newStatus,
            ChangedByUserId = changedByUserId,
            ChangedByRole = changedByRole,
            Comments = comments,
            Timestamp = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        await RecordAuditLogAsync("UpdateStatus", "AdmissionApplication", applicationId.ToString(), changedByUserId, "127.0.0.1", $"Status changed from {prevStatus} to {newStatus}", prevStatus.ToString(), newStatus.ToString());
    }

    public async Task SubmitApplicationAsync(int applicationId)
    {
        var app = await _context.AdmissionApplications.FindAsync(applicationId);
        if (app == null) return;

        app.CurrentStatus = ApplicationStatus.Submitted;
        app.SubmissionDate = DateTime.UtcNow;
        app.LastStatusUpdateDate = DateTime.UtcNow;

        _context.ApplicationStatusHistories.Add(new ApplicationStatusHistory
        {
            AdmissionApplicationId = applicationId,
            PreviousStatus = ApplicationStatus.Draft,
            NewStatus = ApplicationStatus.Submitted,
            ChangedByUserId = app.ApplicantId.ToString(),
            ChangedByRole = "Applicant",
            Comments = "Application officially submitted by applicant",
            Timestamp = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        // Auto Generate Processing Fee Voucher
        var session = await _context.AdmissionSessions.FindAsync(app.AdmissionSessionId);
        decimal feeAmount = session?.ProcessingFeeAmount ?? 2500.00m;
        await GenerateFeeVoucherAsync(applicationId, "ProcessingFee", feeAmount, DateTime.UtcNow.AddDays(7));

        await AddTimelineLogAsync(applicationId, "Submitted", "Application submitted successfully by candidate", $"Applicant-{app.ApplicantId}");
        await RecordAuditLogAsync("SubmitApplication", "AdmissionApplication", applicationId.ToString(), $"Applicant-{app.ApplicantId}", "127.0.0.1", "Application submitted and fee voucher generated");
    }

    // ==========================================
    // DOCUMENT MANAGEMENT
    // ==========================================

    public async Task<ApplicationDocument> UploadDocumentAsync(ApplicationDocument document)
    {
        document.UploadDate = DateTime.UtcNow;
        document.VerificationStatus = DocumentVerificationStatus.Pending;

        _context.ApplicationDocuments.Add(document);
        await _context.SaveChangesAsync();

        await AddTimelineLogAsync(document.AdmissionApplicationId, "DocumentsUploaded", $"Uploaded document {document.FileName} ({document.DocumentType})", $"Applicant-{document.ApplicantId}");
        await RecordAuditLogAsync("UploadDocument", "ApplicationDocument", document.Id.ToString(), $"Applicant-{document.ApplicantId}", "127.0.0.1", $"Uploaded document {document.FileName} ({document.DocumentType})");
        return document;
    }

    public async Task<IEnumerable<ApplicationDocument>> GetDocumentsByApplicationAsync(int applicationId)
    {
        return await _context.ApplicationDocuments
            .Where(d => d.AdmissionApplicationId == applicationId)
            .OrderBy(d => d.DocumentType)
            .ToListAsync();
    }

    public async Task<IEnumerable<ApplicationDocument>> GetDocumentsByApplicantAsync(int applicantId)
    {
        return await _context.ApplicationDocuments
            .Where(d => d.ApplicantId == applicantId)
            .OrderByDescending(d => d.UploadDate)
            .ToListAsync();
    }

    public async Task<bool> DeleteDocumentAsync(int documentId, int applicantId)
    {
        var doc = await _context.ApplicationDocuments
            .FirstOrDefaultAsync(d => d.Id == documentId && d.ApplicantId == applicantId);
        if (doc == null) return false;

        _context.ApplicationDocuments.Remove(doc);
        await _context.SaveChangesAsync();
        await RecordAuditLogAsync("DeleteDocument", "ApplicationDocument", documentId.ToString(), $"Applicant-{applicantId}", "127.0.0.1", $"Deleted document {doc.FileName}");
        return true;
    }

    public async Task VerifyDocumentAsync(int documentId, DocumentVerificationStatus status, string remarks, string verifiedByUserId)
    {
        var doc = await _context.ApplicationDocuments.FindAsync(documentId);
        if (doc == null) return;

        var prevStatus = doc.VerificationStatus.ToString();
        doc.VerificationStatus = status;
        doc.VerificationRemarks = remarks;
        doc.VerifiedByUserId = verifiedByUserId;
        doc.VerificationDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Check if all documents for this application are verified -> Auto transition application status to DocumentsVerified
        var allDocs = await _context.ApplicationDocuments
            .Where(d => d.AdmissionApplicationId == doc.AdmissionApplicationId)
            .ToListAsync();

        if (allDocs.Any() && allDocs.All(d => d.VerificationStatus == DocumentVerificationStatus.Verified))
        {
            await UpdateApplicationStatusAsync(doc.AdmissionApplicationId, ApplicationStatus.DocumentsVerified, verifiedByUserId, "AdmissionOfficer", "All mandatory documents verified successfully");
            await AddTimelineLogAsync(doc.AdmissionApplicationId, "DocumentsVerified", "All candidate documents have been verified by Admission Officer", verifiedByUserId);
        }

        await RecordAuditLogAsync("VerifyDocument", "ApplicationDocument", documentId.ToString(), verifiedByUserId, "127.0.0.1", $"Document verification status set to {status}", prevStatus, status.ToString());
    }

    // ==========================================
    // GUARDIAN INFO & ACADEMIC RECORDS
    // ==========================================

    public async Task<ApplicantGuardianInfo?> GetGuardianInfoAsync(int applicantId)
    {
        return await _context.ApplicantGuardianInfos
            .FirstOrDefaultAsync(g => g.ApplicantId == applicantId);
    }

    public async Task SaveGuardianInfoAsync(ApplicantGuardianInfo guardianInfo)
    {
        var existing = await _context.ApplicantGuardianInfos
            .FirstOrDefaultAsync(g => g.ApplicantId == guardianInfo.ApplicantId);

        if (existing == null)
        {
            guardianInfo.LastUpdatedAt = DateTime.UtcNow;
            _context.ApplicantGuardianInfos.Add(guardianInfo);
        }
        else
        {
            existing.FatherName = guardianInfo.FatherName;
            existing.FatherCNIC = guardianInfo.FatherCNIC;
            existing.FatherOccupation = guardianInfo.FatherOccupation;
            existing.FatherPhone = guardianInfo.FatherPhone;
            existing.GuardianName = guardianInfo.GuardianName;
            existing.GuardianRelation = guardianInfo.GuardianRelation;
            existing.GuardianPhone = guardianInfo.GuardianPhone;
            existing.GuardianCNIC = guardianInfo.GuardianCNIC;
            existing.GuardianOccupation = guardianInfo.GuardianOccupation;
            existing.MonthlyIncome = guardianInfo.MonthlyIncome;
            existing.EmergencyAddress = guardianInfo.EmergencyAddress;
            existing.LastUpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        await RecordAuditLogAsync("SaveGuardianInfo", "ApplicantGuardianInfo", guardianInfo.ApplicantId.ToString(), $"Applicant-{guardianInfo.ApplicantId}", "127.0.0.1", "Saved guardian details");
    }

    public async Task<IEnumerable<ApplicantAcademicRecord>> GetAcademicRecordsAsync(int applicantId)
    {
        return await _context.ApplicantAcademicRecords
            .Where(r => r.ApplicantId == applicantId)
            .OrderBy(r => r.Level)
            .ThenByDescending(r => r.PassingYear)
            .ToListAsync();
    }

    public async Task SaveAcademicRecordAsync(ApplicantAcademicRecord record)
    {
        if (record.Id == 0)
        {
            record.CreatedAt = DateTime.UtcNow;
            _context.ApplicantAcademicRecords.Add(record);
        }
        else
        {
            var existing = await _context.ApplicantAcademicRecords.FindAsync(record.Id);
            if (existing != null && existing.ApplicantId == record.ApplicantId)
            {
                existing.Level = record.Level;
                existing.DegreeOrCertificateTitle = record.DegreeOrCertificateTitle;
                existing.BoardOrUniversity = record.BoardOrUniversity;
                existing.PassingYear = record.PassingYear;
                existing.RollNumber = record.RollNumber;
                existing.TotalMarks = record.TotalMarks;
                existing.ObtainedMarks = record.ObtainedMarks;
                existing.Percentage = record.Percentage;
                existing.GradeOrGPA = record.GradeOrGPA;
                if (!string.IsNullOrEmpty(record.MarksheetPath))
                {
                    existing.MarksheetPath = record.MarksheetPath;
                }
            }
        }

        await _context.SaveChangesAsync();
        await RecordAuditLogAsync("SaveAcademicRecord", "ApplicantAcademicRecord", record.ApplicantId.ToString(), $"Applicant-{record.ApplicantId}", "127.0.0.1", $"Saved academic record for {record.Level}");
    }

    public async Task<bool> DeleteAcademicRecordAsync(int recordId, int applicantId)
    {
        var record = await _context.ApplicantAcademicRecords
            .FirstOrDefaultAsync(r => r.Id == recordId && r.ApplicantId == applicantId);
        if (record == null) return false;

        _context.ApplicantAcademicRecords.Remove(record);
        await _context.SaveChangesAsync();
        await RecordAuditLogAsync("DeleteAcademicRecord", "ApplicantAcademicRecord", recordId.ToString(), $"Applicant-{applicantId}", "127.0.0.1", "Deleted academic record");
        return true;
    }

    // ==========================================
    // MESSAGING & COMMUNICATION CENTER
    // ==========================================

    public async Task<IEnumerable<ApplicantMessage>> GetMessagesAsync(int applicantId)
    {
        return await _context.ApplicantMessages
            .Where(m => m.ApplicantId == applicantId)
            .OrderByDescending(m => m.SentAt)
            .ToListAsync();
    }

    public async Task<ApplicantMessage> SendMessageAsync(ApplicantMessage message)
    {
        message.SentAt = DateTime.UtcNow;
        _context.ApplicantMessages.Add(message);
        await _context.SaveChangesAsync();
        await RecordAuditLogAsync("SendMessage", "ApplicantMessage", message.Id.ToString(), $"Applicant-{message.ApplicantId}", "127.0.0.1", $"Message sent: {message.Subject}");
        return message;
    }

    public async Task MarkMessagesAsReadAsync(int applicantId)
    {
        var unread = await _context.ApplicantMessages
            .Where(m => m.ApplicantId == applicantId && !m.IsRead && m.SenderType == "Admin")
            .ToListAsync();

        foreach (var msg in unread)
        {
            msg.IsRead = true;
        }

        if (unread.Any())
        {
            await _context.SaveChangesAsync();
        }
    }

    // ==========================================
    // SUPPORT TICKETS & HELP CENTER
    // ==========================================

    public async Task<IEnumerable<SupportTicket>> GetSupportTicketsAsync(int applicantId)
    {
        return await _context.SupportTickets
            .Where(t => t.ApplicantId == applicantId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<SupportTicket> CreateSupportTicketAsync(SupportTicket ticket)
    {
        ticket.TicketNumber = $"TCK-{DateTime.UtcNow:yyyyMM}-{RandomNumberGenerator.GetInt32(10000, 99999)}";
        ticket.CreatedAt = DateTime.UtcNow;
        ticket.Status = "Open";

        _context.SupportTickets.Add(ticket);
        await _context.SaveChangesAsync();
        await RecordAuditLogAsync("CreateSupportTicket", "SupportTicket", ticket.Id.ToString(), $"Applicant-{ticket.ApplicantId}", "127.0.0.1", $"Ticket {ticket.TicketNumber} created");
        return ticket;
    }

    // ==========================================
    // FEE VOUCHER SYSTEM
    // ==========================================

    public async Task<FeeVoucher> GenerateFeeVoucherAsync(int applicationId, string voucherType, decimal amount, DateTime dueDate)
    {
        var app = await _context.AdmissionApplications.FindAsync(applicationId);
        if (app == null) throw new InvalidOperationException("Application not found");

        string voucherNum = $"VOC-{DateTime.UtcNow:yyyyMM}-{RandomNumberGenerator.GetInt32(10000, 99999)}";

        var voucher = new FeeVoucher
        {
            VoucherNumber = voucherNum,
            AdmissionApplicationId = applicationId,
            ApplicantId = app.ApplicantId,
            VoucherType = voucherType,
            Amount = amount,
            SubTotalAmount = amount,
            NetPayableAmount = amount,
            IssueDate = DateTime.UtcNow,
            DueDate = dueDate,
            ExpiryDate = dueDate.AddDays(7),
            VoucherStatus = VoucherStatus.Generated,
            PaymentStatus = PaymentStatus.Unpaid
        };

        _context.FeeVouchers.Add(voucher);
        await _context.SaveChangesAsync();

        await AddTimelineLogAsync(applicationId, "FeeVoucherGenerated", $"Fee voucher {voucherNum} generated for amount PKR {amount:N0}", "System");
        await RecordAuditLogAsync("GenerateVoucher", "FeeVoucher", voucher.Id.ToString(), "System", "127.0.0.1", $"Fee voucher {voucherNum} generated for amount {amount}");
        return voucher;
    }

    public async Task<IEnumerable<FeeVoucher>> GetVouchersByApplicantAsync(int applicantId)
    {
        return await _context.FeeVouchers
            .Include(v => v.AdmissionApplication)
                .ThenInclude(app => app!.DegreeProgram)
            .Where(v => v.ApplicantId == applicantId)
            .OrderByDescending(v => v.IssueDate)
            .ToListAsync();
    }

    public async Task<FeeVoucher?> GetVoucherByIdAsync(int voucherId)
    {
        return await _context.FeeVouchers
            .Include(v => v.AdmissionApplication)
            .Include(v => v.Applicant)
            .FirstOrDefaultAsync(v => v.Id == voucherId);
    }

    public async Task<bool> DeleteFeeVoucherAsync(int voucherId, string deletedByUserId)
    {
        var voucher = await _context.FeeVouchers.FindAsync(voucherId);
        if (voucher != null)
        {
            int appId = voucher.AdmissionApplicationId;
            int applicantId = voucher.ApplicantId;
            string vNum = voucher.VoucherNumber;
            string vType = voucher.VoucherType;

            _context.FeeVouchers.Remove(voucher);
            await _context.SaveChangesAsync();

            await AddTimelineLogAsync(appId, "VoucherDeleted", $"Fee Voucher {vNum} ({vType}) cancelled and removed by {deletedByUserId}", deletedByUserId);
            await RecordAuditLogAsync("DeleteVoucher", "FeeVoucher", voucherId.ToString(), deletedByUserId, "127.0.0.1", $"Deleted voucher {vNum} ({vType})");
            return true;
        }
        return false;
    }

    public async Task<bool> UploadPaymentProofAsync(int voucherId, string proofPath, string paymentMethod, string transactionRef)
    {
        var voucher = await _context.FeeVouchers.FindAsync(voucherId);
        if (voucher == null) return false;

        voucher.PaymentProofPath = proofPath;
        voucher.PaymentMethod = paymentMethod;
        voucher.TransactionReference = transactionRef;
        voucher.PaymentStatus = PaymentStatus.PendingVerification;

        await _context.SaveChangesAsync();
        await RecordAuditLogAsync("UploadPaymentProof", "FeeVoucher", voucherId.ToString(), $"Applicant-{voucher.ApplicantId}", "127.0.0.1", $"Uploaded payment receipt for voucher {voucher.VoucherNumber}");
        return true;
    }

    public async Task VerifyFeePaymentAsync(int voucherId, string paymentMethod, string transactionRef, string verifiedByUserId)
    {
        var voucher = await _context.FeeVouchers.FindAsync(voucherId);
        if (voucher == null) return;

        voucher.PaymentStatus = PaymentStatus.Paid;
        voucher.VoucherStatus = VoucherStatus.Paid;
        voucher.PaymentMethod = paymentMethod;
        voucher.TransactionReference = transactionRef;
        voucher.PaidDate = DateTime.UtcNow;
        voucher.VerifiedByUserId = verifiedByUserId;
        voucher.VerifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var app = await _context.AdmissionApplications.FindAsync(voucher.AdmissionApplicationId);
        if (app != null)
        {
            if (voucher.VoucherType.Equals("ProcessingFee", StringComparison.OrdinalIgnoreCase))
            {
                // 1. Processing Fee Paid -> Unlock Admission Test Scheduling
                if (app.CurrentStatus < ApplicationStatus.DocumentsVerified)
                {
                    app.CurrentStatus = ApplicationStatus.DocumentsVerified;
                }
                await AddTimelineLogAsync(app.Id, "ProcessingFeeVerified", $"Processing Fee Voucher {voucher.VoucherNumber} verified. Application is now eligible for Entry Test Scheduling.", verifiedByUserId);
                await AddNotificationAsync(app.ApplicantId, "Processing Fee Verified", $"Your application processing fee payment of PKR {voucher.Amount:N0} has been verified. Entry test scheduling in progress.", "Success");
                await _context.SaveChangesAsync();
            }
            else
            {
                // 2. FirstSemesterFee / AdmissionFee Paid -> Complete Admission Approval & Activate Student Portal
                app.CurrentStatus = ApplicationStatus.FeeVerified;
                await AddTimelineLogAsync(app.Id, "SemesterFeeVerified", $"First Semester Fee Voucher {voucher.VoucherNumber} verified successfully. Admission Confirmed!", verifiedByUserId);
                await AddNotificationAsync(app.ApplicantId, "Semester Fee Verified!", $"Congratulations! Your First Semester Fee payment has been verified. Your Student Portal is now active.", "Success");
                await _context.SaveChangesAsync();

                // Auto-Provision Student Portal & Issue Student ID / Roll Number
                await ProcessAdmissionDecisionAsync(app.Id, "Approve", "Automatic approval upon semester fee verification", verifiedByUserId);
            }
        }

        await RecordAuditLogAsync("VerifyPayment", "FeeVoucher", voucherId.ToString(), verifiedByUserId, "127.0.0.1", $"Voucher {voucher.VoucherNumber} ({voucher.VoucherType}) marked as Paid");
    }

    // ==========================================
    // DECOUPLED ENROLLMENT & DECISION ENGINE
    // ==========================================

    public async Task<EnrollmentRecord?> ProcessAdmissionDecisionAsync(int applicationId, string decisionType, string comments, string officerId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var app = await _context.AdmissionApplications
                .Include(a => a.Applicant)
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (app == null) return null;

            var decision = new AdmissionDecision
            {
                AdmissionApplicationId = applicationId,
                DecisionType = decisionType,
                Comments = comments,
                OfficerId = officerId,
                Timestamp = DateTime.UtcNow
            };
            _context.AdmissionDecisions.Add(decision);
            app.Remarks = comments;
            app.LastStatusUpdateDate = DateTime.UtcNow;

            EnrollmentRecord? record = null;

            if (decisionType.Equals("Approve", StringComparison.OrdinalIgnoreCase))
            {
                app.CurrentStatus = ApplicationStatus.Approved;

                string studentId = $"STD-2026-{app.ApplicantId:D5}";
                string rollNumber = $"2026-CS-{app.Id:D3}";

                record = await _context.EnrollmentRecords.FirstOrDefaultAsync(e => e.ApplicantId == app.ApplicantId);
                if (record == null)
                {
                    record = new EnrollmentRecord
                    {
                        ApplicantId = app.ApplicantId,
                        AdmissionApplicationId = applicationId,
                        GeneratedStudentId = studentId,
                        GeneratedRollNumber = rollNumber,
                        DegreeProgramId = app.DegreeProgramId,
                        DepartmentId = 1,
                        Batch = "Batch 2026",
                        Semester = "1st Semester",
                        EnrollmentStatus = "Active",
                        ActivationDate = DateTime.UtcNow,
                        FutureUmsSyncStatus = "PendingSync",
                        SyncMetadataJson = $"{{\"SyncedAt\": \"{DateTime.UtcNow:o}\", \"Channel\": \"AutoApproval\"}}"
                    };
                    _context.EnrollmentRecords.Add(record);
                }
                else
                {
                    record.EnrollmentStatus = "Active";
                    record.ActivationDate = DateTime.UtcNow;
                }

                await AddTimelineLogAsync(applicationId, "AdmissionConfirmed", $"Admission confirmed! Student ID {studentId} & Roll No {rollNumber} issued", officerId);
                await AddNotificationAsync(app.ApplicantId, "Admission Approved & Portal Activated!", $"Congratulations! Your admission to {app.DegreeProgram?.Name ?? "Program"} is approved. Student ID: {studentId}, Roll No: {rollNumber}", "Success");
            }
            else if (decisionType.Equals("Reject", StringComparison.OrdinalIgnoreCase))
            {
                app.CurrentStatus = ApplicationStatus.Rejected;
                await AddTimelineLogAsync(applicationId, "Rejected", $"Application rejected. Remarks: {comments}", officerId);
                await AddNotificationAsync(app.ApplicantId, "Admission Status Update", "Your application status has been updated. Please check portal details.", "Warning");
            }
            else if (decisionType.Equals("Hold", StringComparison.OrdinalIgnoreCase))
            {
                app.CurrentStatus = ApplicationStatus.UnderReview;
                await AddTimelineLogAsync(applicationId, "OnHold", $"Application placed on hold. Remarks: {comments}", officerId);
                await AddNotificationAsync(app.ApplicantId, "Application On Hold", "Your application is under review by the Admission Committee.", "Info");
            }
            else if (decisionType.Equals("Withdraw", StringComparison.OrdinalIgnoreCase) || decisionType.Equals("CancelAdmission", StringComparison.OrdinalIgnoreCase))
            {
                app.CurrentStatus = ApplicationStatus.Withdrawn;
                record = await _context.EnrollmentRecords.FirstOrDefaultAsync(e => e.ApplicantId == app.ApplicantId);
                if (record != null)
                {
                    record.EnrollmentStatus = "Cancelled";
                }
                await AddTimelineLogAsync(applicationId, "AdmissionWithdrawn", $"Admission Withdrawn / Cancelled. Reason: {comments}", officerId);
                await AddNotificationAsync(app.ApplicantId, "Admission Withdrawn / Cancelled", $"Notice: Your admission has been marked as Withdrawn / Cancelled. Reason: {comments}", "Warning");
            }
            else if (decisionType.Equals("Reopen", StringComparison.OrdinalIgnoreCase))
            {
                app.CurrentStatus = ApplicationStatus.UnderReview;
                record = await _context.EnrollmentRecords.FirstOrDefaultAsync(e => e.ApplicantId == app.ApplicantId);
                if (record != null)
                {
                    record.EnrollmentStatus = "Suspended";
                }
                await AddTimelineLogAsync(applicationId, "DecisionReopened", $"Admission Decision Re-opened for review. Reason: {comments}", officerId);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            await RecordAuditLogAsync("ProcessDecision", "AdmissionApplication", applicationId.ToString(), officerId, "127.0.0.1", $"Decision: {decisionType}");
            return record;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<EnrollmentRecord?> GetEnrollmentRecordByApplicantAsync(int applicantId)
    {
        return await _context.EnrollmentRecords
            .Include(e => e.DegreeProgram)
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.ApplicantId == applicantId);
    }

    // ==========================================
    // LIFECYCLE TIMELINE LOGS
    // ==========================================

    public async Task<IEnumerable<ApplicationTimelineLog>> GetTimelineLogsAsync(int applicationId)
    {
        return await _context.ApplicationTimelineLogs
            .Where(l => l.AdmissionApplicationId == applicationId)
            .OrderBy(l => l.Timestamp)
            .ToListAsync();
    }

    public async Task AddTimelineLogAsync(int applicationId, string stageName, string description, string performedBy = "System")
    {
        var log = new ApplicationTimelineLog
        {
            AdmissionApplicationId = applicationId,
            StageName = stageName,
            Description = description,
            PerformedBy = performedBy,
            Timestamp = DateTime.UtcNow
        };
        _context.ApplicationTimelineLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    // ==========================================
    // AUTOMATIC NOTIFICATIONS
    // ==========================================

    public async Task<IEnumerable<ApplicantNotification>> GetApplicantNotificationsAsync(int applicantId)
    {
        return await _context.ApplicantNotifications
            .Where(n => n.ApplicantId == applicantId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task AddNotificationAsync(int applicantId, string title, string message, string type = "Info")
    {
        var notification = new ApplicantNotification
        {
            ApplicantId = applicantId,
            Title = title,
            Message = message,
            NotificationType = type,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        _context.ApplicantNotifications.Add(notification);
        await _context.SaveChangesAsync();
    }

    // ==========================================
    // AUDIT LOGGING
    // ==========================================

    public async Task RecordAuditLogAsync(string action, string entityName, string entityId, string performedBy, string ipAddress, string details, string? oldValue = null, string? newValue = null, string? userAgent = null)
    {
        try
        {
            var log = new AdmissionAuditLog
            {
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                PerformedBy = performedBy,
                IpAddress = ipAddress,
                UserAgent = userAgent ?? "ASP.NET Core Server",
                OldValue = oldValue,
                NewValue = newValue,
                Details = details,
                Timestamp = DateTime.UtcNow
            };
            _context.AdmissionAuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record admission audit log");
        }
    }

    // ==========================================
    // PASS 2: DOCUMENT RE-UPLOAD & VERSIONING
    // ==========================================

    public async Task<ApplicationDocument> ReuploadDocumentAsync(int documentId, string newFilePath, string newFileName, long fileSize, string uploadedBy)
    {
        var doc = await _context.ApplicationDocuments.FindAsync(documentId);
        if (doc == null) throw new InvalidOperationException("Document not found");

        int currentCount = await _context.DocumentVersionHistories
            .CountAsync(vh => vh.ApplicationDocumentId == documentId);

        // Record history of old file
        var version = new DocumentVersionHistory
        {
            ApplicationDocumentId = documentId,
            VersionNumber = currentCount + 1,
            FileName = doc.FileName,
            FilePath = doc.FilePath,
            RejectionReason = doc.RejectionReason,
            UploadedBy = uploadedBy,
            UploadedAt = doc.UploadDate
        };
        _context.DocumentVersionHistories.Add(version);

        // Update main document with new file
        doc.FileName = newFileName;
        doc.FilePath = newFilePath;
        doc.FileSizeBytes = fileSize;
        doc.UploadDate = DateTime.UtcNow;
        doc.VerificationStatus = DocumentVerificationStatus.Pending;
        doc.RejectionReason = null;

        await _context.SaveChangesAsync();

        await AddTimelineLogAsync(doc.AdmissionApplicationId, "DocumentReuploaded", $"Re-uploaded document {newFileName} (Version #{version.VersionNumber})", uploadedBy);
        await RecordAuditLogAsync("ReuploadDocument", "ApplicationDocument", documentId.ToString(), uploadedBy, "127.0.0.1", $"Re-uploaded document {newFileName}");

        return doc;
    }

    public async Task<IEnumerable<DocumentVersionHistory>> GetDocumentVersionHistoryAsync(int documentId)
    {
        return await _context.DocumentVersionHistories
            .Where(vh => vh.ApplicationDocumentId == documentId)
            .OrderByDescending(vh => vh.VersionNumber)
            .ToListAsync();
    }

    // ==========================================
    // PASS 2: ADMISSION TEST SCHEDULER & DIGITAL SLIP
    // ==========================================

    public async Task<AdmissionTestSchedule> ScheduleAdmissionTestAsync(AdmissionTestSchedule schedule)
    {
        var existing = await _context.AdmissionTestSchedules
            .FirstOrDefaultAsync(s => s.AdmissionApplicationId == schedule.AdmissionApplicationId);

        if (existing == null)
        {
            schedule.ScheduledAt = DateTime.UtcNow;
            schedule.QrCodeData = $"NED-TEST-2026-{schedule.AdmissionApplicationId:D5}-{schedule.TestDate:yyyyMMdd}";
            _context.AdmissionTestSchedules.Add(schedule);
        }
        else
        {
            existing.TestDate = schedule.TestDate;
            existing.TestTime = schedule.TestTime;
            existing.DurationHours = schedule.DurationHours;
            existing.CampusName = schedule.CampusName;
            existing.BuildingName = schedule.BuildingName;
            existing.RoomNumber = schedule.RoomNumber;
            existing.ReportingTime = schedule.ReportingTime;
            existing.Instructions = schedule.Instructions;
            existing.RequiredItems = schedule.RequiredItems;
            existing.DressCode = schedule.DressCode;
            existing.ScheduledByUserId = schedule.ScheduledByUserId;
            schedule = existing;
        }

        await _context.SaveChangesAsync();

        await UpdateApplicationStatusAsync(schedule.AdmissionApplicationId, ApplicationStatus.TestScheduled, schedule.ScheduledByUserId, "AdmissionOfficer", "Admission test scheduled successfully");
        await AddTimelineLogAsync(schedule.AdmissionApplicationId, "AdmissionTestScheduled", $"Admission Test scheduled for {schedule.TestDate:dd MMM yyyy} at {schedule.TestTime} ({schedule.BuildingName}, {schedule.RoomNumber})", schedule.ScheduledByUserId);
        await AddNotificationAsync(schedule.ApplicantId, "Admission Test Scheduled!", $"Your Admission Test is scheduled for {schedule.TestDate:dd MMM yyyy} at {schedule.TestTime}. Download your admit card from dashboard.", "Info");

        return schedule;
    }

    public async Task<AdmissionTestSchedule?> GetTestScheduleByApplicationAsync(int applicationId)
    {
        return await _context.AdmissionTestSchedules
            .FirstOrDefaultAsync(s => s.AdmissionApplicationId == applicationId);
    }

    // ==========================================
    // PASS 2: ADMISSION TEST RESULT & AUTO SCHOLARSHIP
    // ==========================================

    public async Task<AdmissionTestResult> RecordTestResultAsync(int applicationId, double obtainedMarks, double totalMarks, string remarks, string officerId)
    {
        var app = await _context.AdmissionApplications.FindAsync(applicationId);
        if (app == null) throw new InvalidOperationException("Application not found");

        double percentage = Math.Round((obtainedMarks / (totalMarks > 0 ? totalMarks : 100)) * 100, 2);
        bool isQualified = percentage >= 50.0;

        var existing = await _context.AdmissionTestResults
            .FirstOrDefaultAsync(r => r.AdmissionApplicationId == applicationId);

        if (existing == null)
        {
            existing = new AdmissionTestResult
            {
                AdmissionApplicationId = applicationId,
                ObtainedMarks = obtainedMarks,
                TotalMarks = totalMarks,
                CalculatedPercentage = percentage,
                IsQualified = isQualified,
                Remarks = remarks,
                EvaluatedByUserId = officerId,
                EvaluatedAt = DateTime.UtcNow
            };
            _context.AdmissionTestResults.Add(existing);
        }
        else
        {
            existing.ObtainedMarks = obtainedMarks;
            existing.TotalMarks = totalMarks;
            existing.CalculatedPercentage = percentage;
            existing.IsQualified = isQualified;
            existing.Remarks = remarks;
            existing.EvaluatedByUserId = officerId;
            existing.EvaluatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        var newStatus = isQualified ? ApplicationStatus.TestQualified : ApplicationStatus.TestNotQualified;
        await UpdateApplicationStatusAsync(applicationId, newStatus, officerId, "AdmissionOfficer", $"Test result recorded. Percentage: {percentage}%");
        await AddTimelineLogAsync(applicationId, "AdmissionTestCompleted", $"Admission Test result recorded: {obtainedMarks}/{totalMarks} ({percentage}%) - {(isQualified ? "Qualified" : "Not Qualified")}", officerId);

        // Evaluate Dynamic Scholarship
        var scholarship = await EvaluateScholarshipForPercentageAsync(percentage);
        if (scholarship != null && isQualified)
        {
            await AddTimelineLogAsync(applicationId, "ScholarshipAwarded", $"Awarded '{scholarship.ScholarshipName}' ({scholarship.ScholarshipPercentage}% Waiver)", "ScholarshipEngine");
            await AddNotificationAsync(app.ApplicantId, "Scholarship Awarded!", $"Congratulations! Based on your test score of {percentage}%, you have been awarded {scholarship.ScholarshipName} ({scholarship.ScholarshipPercentage}% fee discount)!", "Success");

            // Auto Generate Enterprise Fee Voucher with Scholarship Discount
            await GenerateEnterpriseVoucherAsync(applicationId, "FirstSemesterFee", scholarship.ScholarshipPercentage, 0, 0, DateTime.UtcNow.AddDays(14));
        }
        else if (isQualified)
        {
            // Auto Generate Standard Enterprise Fee Voucher
            await GenerateEnterpriseVoucherAsync(applicationId, "FirstSemesterFee", 0, 0, 0, DateTime.UtcNow.AddDays(14));
        }

        return existing;
    }

    public async Task<AdmissionTestResult?> GetTestResultByApplicationAsync(int applicationId)
    {
        return await _context.AdmissionTestResults
            .FirstOrDefaultAsync(r => r.AdmissionApplicationId == applicationId);
    }

    // ==========================================
    // PASS 2: DYNAMIC SCHOLARSHIP RULES ENGINE
    // ==========================================

    public async Task<IEnumerable<ScholarshipRule>> GetAllScholarshipRulesAsync()
    {
        return await _context.ScholarshipRules
            .OrderByDescending(r => r.MinimumPercentage)
            .ToListAsync();
    }

    public async Task<ScholarshipRule?> GetScholarshipRuleByIdAsync(int ruleId)
    {
        return await _context.ScholarshipRules.FindAsync(ruleId);
    }

    public async Task SaveScholarshipRuleAsync(ScholarshipRule rule)
    {
        if (rule.Id == 0)
        {
            rule.CreatedAt = DateTime.UtcNow;
            _context.ScholarshipRules.Add(rule);
        }
        else
        {
            var existing = await _context.ScholarshipRules.FindAsync(rule.Id);
            if (existing != null)
            {
                existing.ScholarshipName = rule.ScholarshipName;
                existing.Description = rule.Description;
                existing.MinimumPercentage = rule.MinimumPercentage;
                existing.ScholarshipPercentage = rule.ScholarshipPercentage;
                existing.IsEnabled = rule.IsEnabled;
                existing.EffectiveStartDate = rule.EffectiveStartDate;
                existing.EffectiveEndDate = rule.EffectiveEndDate;
            }
        }
        await _context.SaveChangesAsync();
    }

    public async Task DeleteScholarshipRuleAsync(int ruleId)
    {
        var rule = await _context.ScholarshipRules.FindAsync(ruleId);
        if (rule != null)
        {
            _context.ScholarshipRules.Remove(rule);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<ScholarshipRule?> EvaluateScholarshipForPercentageAsync(double percentage)
    {
        var rules = await _context.ScholarshipRules
            .Where(r => r.IsEnabled && r.MinimumPercentage <= percentage)
            .OrderByDescending(r => r.MinimumPercentage)
            .FirstOrDefaultAsync();

        return rules;
    }

    // ==========================================
    // PASS 2: DYNAMIC FEE STRUCTURE & REACTIVATION
    // ==========================================

    public async Task<FeeStructureConfig> GetFeeStructureConfigAsync()
    {
        var config = await _context.FeeStructureConfigs.FirstOrDefaultAsync();
        if (config == null)
        {
            config = new FeeStructureConfig
            {
                AdmissionFee = 15000,
                RegistrationFee = 5000,
                SecurityDeposit = 5000,
                LibraryFee = 2500,
                LabFee = 2500,
                StudentActivityFee = 1000,
                SemesterFee = 125000,
                LastUpdatedAt = DateTime.UtcNow,
                UpdatedByUserId = "SystemAdmin"
            };
            _context.FeeStructureConfigs.Add(config);
            await _context.SaveChangesAsync();
        }
        return config;
    }

    public async Task SaveFeeStructureConfigAsync(FeeStructureConfig config, string updatedByUserId)
    {
        var existing = await GetFeeStructureConfigAsync();
        existing.AdmissionFee = config.AdmissionFee;
        existing.RegistrationFee = config.RegistrationFee;
        existing.SecurityDeposit = config.SecurityDeposit;
        existing.LibraryFee = config.LibraryFee;
        existing.LabFee = config.LabFee;
        existing.StudentActivityFee = config.StudentActivityFee;
        existing.SemesterFee = config.SemesterFee;
        existing.LastUpdatedAt = DateTime.UtcNow;
        existing.UpdatedByUserId = updatedByUserId;

        await _context.SaveChangesAsync();
        await RecordAuditLogAsync("SaveFeeConfig", "FeeStructureConfig", existing.Id.ToString(), updatedByUserId, "127.0.0.1", "Updated university fee structure config");
    }

    public async Task<FeeVoucher> GenerateEnterpriseVoucherAsync(int applicationId, string voucherType, double scholarshipPercentage, decimal manualDiscount, decimal fineAmount, DateTime dueDate)
    {
        var app = await _context.AdmissionApplications.FindAsync(applicationId);
        if (app == null) throw new InvalidOperationException("Application not found");

        var config = await GetFeeStructureConfigAsync();
        decimal subTotal = config.AdmissionFee + config.RegistrationFee + config.SecurityDeposit + config.LibraryFee + config.LabFee + config.StudentActivityFee + config.SemesterFee;
        decimal scholarshipDiscount = (subTotal * (decimal)scholarshipPercentage) / 100m;
        decimal netPayable = subTotal - scholarshipDiscount - manualDiscount + fineAmount;
        if (netPayable < 0) netPayable = 0;

        string voucherNum = $"VOC-2026-{RandomNumberGenerator.GetInt32(10000, 99999)}";

        var voucher = new FeeVoucher
        {
            VoucherNumber = voucherNum,
            AdmissionApplicationId = applicationId,
            ApplicantId = app.ApplicantId,
            VoucherType = voucherType,
            AdmissionFee = config.AdmissionFee,
            RegistrationFee = config.RegistrationFee,
            SecurityDeposit = config.SecurityDeposit,
            LibraryFee = config.LibraryFee,
            LabFee = config.LabFee,
            StudentActivityFee = config.StudentActivityFee,
            SemesterFee = config.SemesterFee,
            SubTotalAmount = subTotal,
            ScholarshipPercentageApplied = scholarshipPercentage,
            ScholarshipDiscountAmount = scholarshipDiscount,
            ManualDiscountAmount = manualDiscount,
            FineAmount = fineAmount,
            Amount = netPayable,
            NetPayableAmount = netPayable,
            IssueDate = DateTime.UtcNow,
            DueDate = dueDate,
            ExpiryDate = dueDate.AddDays(7),
            VoucherStatus = VoucherStatus.Generated,
            PaymentStatus = PaymentStatus.Unpaid
        };

        _context.FeeVouchers.Add(voucher);
        await _context.SaveChangesAsync();

        await AddTimelineLogAsync(applicationId, "FeeVoucherGenerated", $"Official Fee Voucher {voucherNum} generated. Net Payable: PKR {netPayable:N0}", "FeeEngine");
        await AddNotificationAsync(app.ApplicantId, "Fee Voucher Ready", $"Your First Semester Fee Voucher {voucherNum} (Net Payable: PKR {netPayable:N0}) is ready for download.", "Info");

        return voucher;
    }

    public async Task<FeeVoucher> ReactivateVoucherAsync(int oldVoucherId, DateTime newDueDate, string officerId)
    {
        var oldVoucher = await _context.FeeVouchers.FindAsync(oldVoucherId);
        if (oldVoucher == null) throw new InvalidOperationException("Voucher not found");

        oldVoucher.VoucherStatus = VoucherStatus.Reactivated;

        var newVoucher = await GenerateEnterpriseVoucherAsync(
            oldVoucher.AdmissionApplicationId,
            oldVoucher.VoucherType,
            oldVoucher.ScholarshipPercentageApplied,
            oldVoucher.ManualDiscountAmount,
            oldVoucher.FineAmount,
            newDueDate
        );

        newVoucher.PreviousVoucherId = oldVoucherId;
        await _context.SaveChangesAsync();

        await AddTimelineLogAsync(oldVoucher.AdmissionApplicationId, "VoucherReactivated", $"Expired Voucher {oldVoucher.VoucherNumber} reactivated. New Voucher: {newVoucher.VoucherNumber} (Due: {newDueDate:dd MMM yyyy})", officerId);
        await AddNotificationAsync(oldVoucher.ApplicantId, "Fee Voucher Reactivated", $"Your fee voucher has been reactivated with a new due date of {newDueDate:dd MMM yyyy}.", "Info");

        return newVoucher;
    }

    public async Task BroadcastMultiRoleNotificationAsync(int? applicantId, string title, string message, string targetRole = "All")
    {
        if (applicantId.HasValue)
        {
            await AddNotificationAsync(applicantId.Value, title, message, "Info");
        }
        await RecordAuditLogAsync("BroadcastNotification", "MultiRoleNotification", targetRole, "System", "127.0.0.1", $"Notification broadcast: {title} - {message}");
    }

    // ==========================================
    // 10/10 ENTERPRISE ERP IMPLEMENTATIONS
    // ==========================================

    public async Task<ApplicationInternalNote> SaveInternalNoteAsync(int applicationId, string authorUserId, string noteText, string? attachmentPath, string? mentionedUser)
    {
        var note = new ApplicationInternalNote
        {
            AdmissionApplicationId = applicationId,
            AuthorUserId = authorUserId,
            NoteText = noteText,
            AttachmentPath = attachmentPath,
            MentionedAdminUser = mentionedUser,
            Timestamp = DateTime.UtcNow
        };
        _context.ApplicationInternalNotes.Add(note);
        await _context.SaveChangesAsync();

        await RecordAuditLogAsync("AddInternalNote", "ApplicationInternalNote", note.Id.ToString(), authorUserId, "127.0.0.1", $"Internal note added for App #{applicationId}");
        return note;
    }

    public async Task<IEnumerable<ApplicationInternalNote>> GetInternalNotesByApplicationAsync(int applicationId)
    {
        return await _context.ApplicationInternalNotes
            .Where(n => n.AdmissionApplicationId == applicationId)
            .OrderByDescending(n => n.Timestamp)
            .ToListAsync();
    }

    public async Task AssignOfficerAsync(int applicationId, string officerId, string assignedByUserId)
    {
        var app = await _context.AdmissionApplications.FindAsync(applicationId);
        if (app != null)
        {
            string oldOfficer = app.AssignedOfficerId ?? "Unassigned";
            app.AssignedOfficerId = officerId;
            app.AssignedDate = DateTime.UtcNow;
            app.AssignedByUserId = assignedByUserId;
            await _context.SaveChangesAsync();

            await AddTimelineLogAsync(applicationId, "OfficerAssigned", $"Application assigned to Officer: {officerId} by {assignedByUserId}", assignedByUserId);
            await RecordAuditLogAsync("AssignOfficer", "AdmissionApplication", applicationId.ToString(), assignedByUserId, "127.0.0.1", $"Reassigned from {oldOfficer} to {officerId}", oldOfficer, officerId);
        }
    }

    public async Task<AdmissionTestResult> OverrideScholarshipAsync(int applicationId, double overridePercentage, string reason, string approvedByUserId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var result = await _context.AdmissionTestResults.FirstOrDefaultAsync(r => r.AdmissionApplicationId == applicationId);
            if (result == null)
            {
                result = new AdmissionTestResult
                {
                    AdmissionApplicationId = applicationId,
                    TotalMarks = 100,
                    ObtainedMarks = 75,
                    PassingPercentage = 50,
                    CalculatedPercentage = 75,
                    IsQualified = true,
                    EvaluatedByUserId = approvedByUserId,
                    EvaluatedAt = DateTime.UtcNow
                };
                _context.AdmissionTestResults.Add(result);
            }

            double oldPct = result.ApprovedScholarshipPercentage;
            result.ApprovedScholarshipPercentage = overridePercentage;
            result.OverrideReason = reason;
            result.OverrideApprovedByUserId = approvedByUserId;
            await _context.SaveChangesAsync();

            // Update Fee Voucher if semester voucher exists
            var semesterVoucher = await _context.FeeVouchers.FirstOrDefaultAsync(v => v.AdmissionApplicationId == applicationId && (v.VoucherType == "FirstSemesterFee" || v.VoucherType == "AdmissionFee"));
            if (semesterVoucher != null)
            {
                var feeConfig = await GetFeeStructureConfigAsync();
                decimal baseSemesterFee = feeConfig.SemesterFee;
                decimal discountAmount = baseSemesterFee * (decimal)(overridePercentage / 100.0);
                
                semesterVoucher.ScholarshipPercentageApplied = overridePercentage;
                semesterVoucher.ScholarshipDiscountAmount = discountAmount;
                semesterVoucher.NetPayableAmount = semesterVoucher.SubTotalAmount - discountAmount - semesterVoucher.ManualDiscountAmount + semesterVoucher.FineAmount;
                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();

            await AddTimelineLogAsync(applicationId, "ScholarshipOverride", $"Scholarship percentage manually overridden to {overridePercentage}% by {approvedByUserId}. Reason: {reason}", approvedByUserId);
            await AddNotificationAsync(result.AdmissionApplication?.ApplicantId ?? 0, "Scholarship Award Updated", $"Your scholarship award has been updated to {overridePercentage}%.", "Success");
            await RecordAuditLogAsync("ScholarshipOverride", "AdmissionTestResult", result.Id.ToString(), approvedByUserId, "127.0.0.1", $"Overrode scholarship from {oldPct}% to {overridePercentage}%. Reason: {reason}", oldPct.ToString(), overridePercentage.ToString());

            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static string HashPasswordWithSalt(string rawPassword, string? salt)
    {
        string effectiveSalt = salt ?? "NedAcademySalt2026";
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawPassword + effectiveSalt));
        return Convert.ToHexString(bytes);
    }
}
