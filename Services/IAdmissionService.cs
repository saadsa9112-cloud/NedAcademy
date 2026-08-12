using NedAcademyWebsite.Models;

namespace NedAcademyWebsite.Services;

public interface IAdmissionService
{
    // Session & Programs
    Task<AdmissionSession?> GetActiveAdmissionSessionAsync();
    Task<IEnumerable<AdmissionSession>> GetAllAdmissionSessionsAsync();
    Task<AdmissionSession> CreateAdmissionSessionAsync(AdmissionSession session, IEnumerable<int> degreeProgramIds);
    Task<IEnumerable<AdmissionSessionProgram>> GetOfferedProgramsAsync(int sessionId);

    // Applicant Account Management & Authentication
    Task<Applicant?> AuthenticateApplicantAsync(string email, string password);
    Task<Applicant?> GetApplicantByIdAsync(int applicantId);
    Task<Applicant?> GetApplicantByEmailAsync(string email);
    Task<Applicant?> GetApplicantByPhoneAsync(string phone);
    Task<Applicant> RegisterApplicantAsync(Applicant applicant, string rawPassword);
    Task<string?> GeneratePasswordResetTokenAsync(string email);
    Task<bool> ResetPasswordWithTokenAsync(string email, string token, string newPassword);
    Task<bool> VerifyEmailTokenAsync(string email, string token);
    Task<ApplicantProfile?> GetApplicantProfileAsync(int applicantId);
    Task UpdateApplicantProfileAsync(ApplicantProfile profile);

    // Admission Applications Lifecycle
    Task<AdmissionApplication> CreateApplicationDraftAsync(int applicantId, int sessionId, int degreeProgramId);
    Task<AdmissionApplication?> GetApplicationByIdAsync(int applicationId);
    Task<IEnumerable<AdmissionApplication>> GetApplicationsByApplicantAsync(int applicantId);
    Task<IEnumerable<AdmissionApplication>> GetApplicationsForReviewAsync(ApplicationStatus? filterStatus = null);
    Task UpdateApplicationStatusAsync(int applicationId, ApplicationStatus newStatus, string changedByUserId, string changedByRole, string? comments);
    Task SubmitApplicationAsync(int applicationId);

    // Document Management
    Task<ApplicationDocument> UploadDocumentAsync(ApplicationDocument document);
    Task<IEnumerable<ApplicationDocument>> GetDocumentsByApplicationAsync(int applicationId);
    Task<IEnumerable<ApplicationDocument>> GetDocumentsByApplicantAsync(int applicantId);
    Task<bool> DeleteDocumentAsync(int documentId, int applicantId);
    Task VerifyDocumentAsync(int documentId, DocumentVerificationStatus status, string remarks, string verifiedByUserId);

    // Guardian Info & Academic Records
    Task<ApplicantGuardianInfo?> GetGuardianInfoAsync(int applicantId);
    Task SaveGuardianInfoAsync(ApplicantGuardianInfo guardianInfo);
    Task<IEnumerable<ApplicantAcademicRecord>> GetAcademicRecordsAsync(int applicantId);
    Task SaveAcademicRecordAsync(ApplicantAcademicRecord record);
    Task<bool> DeleteAcademicRecordAsync(int recordId, int applicantId);

    // Messaging & Communication Center
    Task<IEnumerable<ApplicantMessage>> GetMessagesAsync(int applicantId);
    Task<ApplicantMessage> SendMessageAsync(ApplicantMessage message);
    Task MarkMessagesAsReadAsync(int applicantId);

    // Help & Support Tickets
    Task<IEnumerable<SupportTicket>> GetSupportTicketsAsync(int applicantId);
    Task<SupportTicket> CreateSupportTicketAsync(SupportTicket ticket);

    // Decoupled Enrollment & Admission Decision Engine
    Task<EnrollmentRecord?> ProcessAdmissionDecisionAsync(int applicationId, string decisionType, string comments, string officerId);
    Task<EnrollmentRecord?> GetEnrollmentRecordByApplicantAsync(int applicantId);

    // Lifecycle Timeline Logs
    Task<IEnumerable<ApplicationTimelineLog>> GetTimelineLogsAsync(int applicationId);
    Task AddTimelineLogAsync(int applicationId, string stageName, string description, string performedBy = "System");

    // Automatic Notifications
    Task<IEnumerable<ApplicantNotification>> GetApplicantNotificationsAsync(int applicantId);
    Task AddNotificationAsync(int applicantId, string title, string message, string type = "Info");

    // Fee Voucher System & Payment Proof
    Task<FeeVoucher> GenerateFeeVoucherAsync(int applicationId, string voucherType, decimal amount, DateTime dueDate);
    Task<IEnumerable<FeeVoucher>> GetVouchersByApplicantAsync(int applicantId);
    Task<FeeVoucher?> GetVoucherByIdAsync(int voucherId);
    Task<bool> UploadPaymentProofAsync(int voucherId, string proofPath, string paymentMethod, string transactionRef);
    Task VerifyFeePaymentAsync(int voucherId, string paymentMethod, string transactionRef, string verifiedByUserId);
    Task<bool> DeleteFeeVoucherAsync(int voucherId, string deletedByUserId);

    // Pass 2: Enterprise ERP Contracts (Versioning, Test Scheduler, Scholarship Engine, Fee Config, Reactivation)
    Task<ApplicationDocument> ReuploadDocumentAsync(int documentId, string newFilePath, string newFileName, long fileSize, string uploadedBy);
    Task<IEnumerable<DocumentVersionHistory>> GetDocumentVersionHistoryAsync(int documentId);

    Task<AdmissionTestSchedule> ScheduleAdmissionTestAsync(AdmissionTestSchedule schedule);
    Task<AdmissionTestSchedule?> GetTestScheduleByApplicationAsync(int applicationId);

    Task<AdmissionTestResult> RecordTestResultAsync(int applicationId, double obtainedMarks, double totalMarks, string remarks, string officerId);
    Task<AdmissionTestResult?> GetTestResultByApplicationAsync(int applicationId);

    Task<IEnumerable<ScholarshipRule>> GetAllScholarshipRulesAsync();
    Task<ScholarshipRule?> GetScholarshipRuleByIdAsync(int ruleId);
    Task SaveScholarshipRuleAsync(ScholarshipRule rule);
    Task DeleteScholarshipRuleAsync(int ruleId);
    Task<ScholarshipRule?> EvaluateScholarshipForPercentageAsync(double percentage);

    Task<FeeStructureConfig> GetFeeStructureConfigAsync();
    Task SaveFeeStructureConfigAsync(FeeStructureConfig config, string updatedByUserId);
    Task<FeeVoucher> GenerateEnterpriseVoucherAsync(int applicationId, string voucherType, double scholarshipPercentage, decimal manualDiscount, decimal fineAmount, DateTime dueDate);
    Task<FeeVoucher> ReactivateVoucherAsync(int oldVoucherId, DateTime newDueDate, string officerId);

    Task BroadcastMultiRoleNotificationAsync(int? applicantId, string title, string message, string targetRole = "All");

    // 10/10 Enterprise Contracts
    Task<ApplicationInternalNote> SaveInternalNoteAsync(int applicationId, string authorUserId, string noteText, string? attachmentPath, string? mentionedUser);
    Task<IEnumerable<ApplicationInternalNote>> GetInternalNotesByApplicationAsync(int applicationId);
    Task AssignOfficerAsync(int applicationId, string officerId, string assignedByUserId);
    Task<AdmissionTestResult> OverrideScholarshipAsync(int applicationId, double overridePercentage, string reason, string approvedByUserId);

    // Audit Logging
    Task RecordAuditLogAsync(string action, string entityName, string entityId, string performedBy, string ipAddress, string details, string? oldValue = null, string? newValue = null, string? userAgent = null);
}
