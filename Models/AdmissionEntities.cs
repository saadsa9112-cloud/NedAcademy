using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NedAcademyWebsite.Models;

public class Applicant
{
    public int Id { get; set; }

    public string? ApplicationUserId { get; set; } // Future Identity integration

    [Required]
    [StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    [StringLength(100)]
    public string? PasswordSalt { get; set; }

    [Required]
    [StringLength(30)]
    public string PhoneNumber { get; set; } = string.Empty;

    [StringLength(50)]
    public string? CNIC_Passport { get; set; }

    public DateTime DateOfBirth { get; set; }

    public ApplicantGender Gender { get; set; } = ApplicantGender.Male;

    [StringLength(60)]
    public string Nationality { get; set; } = "Pakistani";

    [StringLength(80)]
    public string City { get; set; } = string.Empty;

    [StringLength(250)]
    public string Address { get; set; } = string.Empty;

    public bool IsEmailVerified { get; set; } = false;

    public bool IsPhoneVerified { get; set; } = false;

    [StringLength(100)]
    public string? EmailVerificationToken { get; set; }

    public DateTime? EmailVerificationTokenExpiry { get; set; }

    [StringLength(100)]
    public string? PasswordResetToken { get; set; }

    public DateTime? PasswordResetTokenExpiry { get; set; }

    public ApplicantAccountStatus AccountStatus { get; set; } = ApplicantAccountStatus.Active;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAt { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; } = false;

    // Navigation Profile
    public ApplicantProfile? Profile { get; set; }
    public ApplicantGuardianInfo? GuardianInfo { get; set; }
    public EnrollmentRecord? EnrollmentRecord { get; set; }

    // Navigation collections
    public ICollection<AdmissionApplication> Applications { get; set; } = new List<AdmissionApplication>();
    public ICollection<FeeVoucher> FeeVouchers { get; set; } = new List<FeeVoucher>();
    public ICollection<ApplicationDocument> Documents { get; set; } = new List<ApplicationDocument>();
    public ICollection<ApplicantAcademicRecord> AcademicRecords { get; set; } = new List<ApplicantAcademicRecord>();
    public ICollection<ApplicantMessage> Messages { get; set; } = new List<ApplicantMessage>();
    public ICollection<SupportTicket> SupportTickets { get; set; } = new List<SupportTicket>();
    public ICollection<ApplicantNotification> Notifications { get; set; } = new List<ApplicantNotification>();
}

public class ApplicantProfile
{
    public int Id { get; set; }

    public int ApplicantId { get; set; }
    public Applicant? Applicant { get; set; }

    [StringLength(120)]
    public string? FullName { get; set; }

    [StringLength(120)]
    public string? FatherName { get; set; }

    [StringLength(50)]
    public string? CNIC_Passport { get; set; }

    [StringLength(80)]
    public string? Domicile { get; set; }

    [StringLength(250)]
    public string? PermanentAddress { get; set; }

    [StringLength(250)]
    public string? PostalAddress { get; set; }

    [StringLength(120)]
    public string? GuardianName { get; set; }

    [StringLength(30)]
    public string? GuardianPhone { get; set; }

    [StringLength(120)]
    public string? EmergencyContactName { get; set; }

    [StringLength(30)]
    public string? EmergencyContactPhone { get; set; }

    [StringLength(250)]
    public string? ProfilePhotoPath { get; set; }

    [StringLength(50)]
    public string? CNIC { get; set; }

    [StringLength(50)]
    public string? PassportNo { get; set; }

    [StringLength(60)]
    public string Nationality { get; set; } = "Pakistani";

    public ApplicantGender Gender { get; set; } = ApplicantGender.Male;

    public DateTime? DateOfBirth { get; set; }

    [StringLength(250)]
    public string? Address { get; set; }

    [StringLength(80)]
    public string? City { get; set; }

    [StringLength(80)]
    public string? Province { get; set; }

    [StringLength(20)]
    public string? PostalCode { get; set; }

    [StringLength(80)]
    public string Country { get; set; } = "Pakistan";

    public int ProfileCompletionPercentage { get; set; } = 30; // 30% upon Account Creation

    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
}

public class AdmissionSession
{
    public int Id { get; set; }

    [Required]
    [StringLength(120)]
    public string Title { get; set; } = string.Empty; // e.g. "Fall 2026 Admissions"

    [Required]
    [StringLength(30)]
    public string AcademicYear { get; set; } = string.Empty; // e.g. "2026-2027"

    [Required]
    [StringLength(30)]
    public string Term { get; set; } = string.Empty; // "Fall", "Spring", "Summer"

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public DateTime ApplicationDeadline { get; set; }

    public AdmissionSessionStatus Status { get; set; } = AdmissionSessionStatus.OpenForApplications;

    public int MaxCapacity { get; set; } = 1500;

    [Column(TypeName = "decimal(18,2)")]
    public decimal ProcessingFeeAmount { get; set; } = 2500.00m;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation collections
    public ICollection<AdmissionSessionProgram> SessionPrograms { get; set; } = new List<AdmissionSessionProgram>();
    public ICollection<AdmissionApplication> Applications { get; set; } = new List<AdmissionApplication>();
}

public class AdmissionSessionProgram
{
    public int Id { get; set; }

    public int AdmissionSessionId { get; set; }
    public AdmissionSession? AdmissionSession { get; set; }

    public int DegreeProgramId { get; set; }
    public DegreeProgram? DegreeProgram { get; set; }

    public int CapacitySeats { get; set; } = 60;

    public int AvailableSeats { get; set; } = 60;

    [Column(TypeName = "decimal(18,2)")]
    public decimal? CustomProcessingFee { get; set; }

    public double MinimumEligibilityMarksPercentage { get; set; } = 50.0;

    public bool IsActive { get; set; } = true;
}

public class AdmissionApplication
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string ApplicationNumber { get; set; } = string.Empty; // e.g. APP-2026-89412

    public int ApplicantId { get; set; }
    public Applicant? Applicant { get; set; }

    public int AdmissionSessionId { get; set; }
    public AdmissionSession? AdmissionSession { get; set; }

    public int DegreeProgramId { get; set; }
    public DegreeProgram? DegreeProgram { get; set; }

    public ApplicationStatus CurrentStatus { get; set; } = ApplicationStatus.Draft;

    public int CurrentStepNumber { get; set; } = 1; // Multi-step progress (1: Personal, 2: Academic, 3: Program, 4: Docs, 5: Review)

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime? SubmissionDate { get; set; }

    public DateTime LastStatusUpdateDate { get; set; } = DateTime.UtcNow;

    public double CalculatedMeritScore { get; set; } = 0.0;

    public string? Remarks { get; set; }

    public string? AssignedOfficerId { get; set; }
    public DateTime? AssignedDate { get; set; }
    public string? AssignedByUserId { get; set; }

    public bool IsDeleted { get; set; } = false;

    // Navigation collections
    public ICollection<ApplicationDocument> UploadedDocuments { get; set; } = new List<ApplicationDocument>();
    public ICollection<ApplicationStatusHistory> StatusHistory { get; set; } = new List<ApplicationStatusHistory>();
    public ICollection<FeeVoucher> FeeVouchers { get; set; } = new List<FeeVoucher>();
    public ICollection<ApplicationInternalNote> InternalNotes { get; set; } = new List<ApplicationInternalNote>();
}

public class ApplicationDocument
{
    public int Id { get; set; }

    public int AdmissionApplicationId { get; set; }
    public AdmissionApplication? AdmissionApplication { get; set; }

    public int ApplicantId { get; set; }
    public Applicant? Applicant { get; set; }

    public DocumentType DocumentType { get; set; }

    [Required]
    [StringLength(200)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string FilePath { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    [StringLength(50)]
    public string FileType { get; set; } = string.Empty;

    public DateTime UploadDate { get; set; } = DateTime.UtcNow;

    public DocumentVerificationStatus VerificationStatus { get; set; } = DocumentVerificationStatus.Pending;

    [StringLength(250)]
    public string? VerificationRemarks { get; set; }

    public string? VerifiedByUserId { get; set; }

    public DateTime? VerificationDate { get; set; }

    public bool ReplacementRequested { get; set; } = false;

    [StringLength(500)]
    public string? RejectionReason { get; set; }
}

public class ApplicationStatusHistory
{
    public int Id { get; set; }

    public int AdmissionApplicationId { get; set; }
    public AdmissionApplication? AdmissionApplication { get; set; }

    public ApplicationStatus PreviousStatus { get; set; }

    public ApplicationStatus NewStatus { get; set; }

    [StringLength(100)]
    public string ChangedByUserId { get; set; } = string.Empty;

    [StringLength(50)]
    public string ChangedByRole { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Comments { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class FeeVoucher
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string VoucherNumber { get; set; } = string.Empty; // e.g. VOC-2026-78419

    public int AdmissionApplicationId { get; set; }
    public AdmissionApplication? AdmissionApplication { get; set; }

    public int ApplicantId { get; set; }
    public Applicant? Applicant { get; set; }

    [Required]
    [StringLength(50)]
    public string VoucherType { get; set; } = "ProcessingFee"; // "ProcessingFee" / "AdmissionFee"

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    // Itemized Enterprise Fee Breakdown
    [Column(TypeName = "decimal(18,2)")]
    public decimal AdmissionFee { get; set; } = 15000;

    [Column(TypeName = "decimal(18,2)")]
    public decimal RegistrationFee { get; set; } = 5000;

    [Column(TypeName = "decimal(18,2)")]
    public decimal SecurityDeposit { get; set; } = 5000;

    [Column(TypeName = "decimal(18,2)")]
    public decimal LibraryFee { get; set; } = 2500;

    [Column(TypeName = "decimal(18,2)")]
    public decimal LabFee { get; set; } = 2500;

    [Column(TypeName = "decimal(18,2)")]
    public decimal StudentActivityFee { get; set; } = 1000;

    [Column(TypeName = "decimal(18,2)")]
    public decimal SemesterFee { get; set; } = 125000;

    [Column(TypeName = "decimal(18,2)")]
    public decimal SubTotalAmount { get; set; } = 155000;

    public double ScholarshipPercentageApplied { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal ScholarshipDiscountAmount { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal ManualDiscountAmount { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal FineAmount { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal NetPayableAmount { get; set; } = 155000;

    public DateTime IssueDate { get; set; } = DateTime.UtcNow;

    public DateTime DueDate { get; set; }

    public DateTime ExpiryDate { get; set; }

    public VoucherStatus VoucherStatus { get; set; } = VoucherStatus.Generated;

    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;

    public int? PreviousVoucherId { get; set; }

    [StringLength(50)]
    public string? PaymentMethod { get; set; }

    [StringLength(100)]
    public string? TransactionReference { get; set; }

    public DateTime? PaidDate { get; set; }

    [StringLength(500)]
    public string? PaymentProofPath { get; set; }

    public string? VerifiedByUserId { get; set; }

    public DateTime? VerifiedDate { get; set; }

    [StringLength(250)]
    public string? Remarks { get; set; }
}

public class AdmissionAuditLog
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Action { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string EntityName { get; set; } = string.Empty;

    [StringLength(50)]
    public string EntityId { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string PerformedBy { get; set; } = string.Empty;

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    [StringLength(50)]
    public string? IpAddress { get; set; }

    [StringLength(250)]
    public string? UserAgent { get; set; }

    [Required]
    public string Details { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class DocumentVersionHistory
{
    public int Id { get; set; }

    public int ApplicationDocumentId { get; set; }
    public ApplicationDocument? ApplicationDocument { get; set; }

    public int VersionNumber { get; set; } = 1;

    [Required]
    [StringLength(250)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string FilePath { get; set; } = string.Empty;

    [StringLength(500)]
    public string? RejectionReason { get; set; }

    [StringLength(100)]
    public string UploadedBy { get; set; } = "Applicant";

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}

public class AdmissionTestSchedule
{
    public int Id { get; set; }

    public int AdmissionApplicationId { get; set; }
    public AdmissionApplication? AdmissionApplication { get; set; }

    public int ApplicantId { get; set; }
    public Applicant? Applicant { get; set; }

    public DateTime TestDate { get; set; }

    [StringLength(20)]
    public string TestTime { get; set; } = "09:00 AM";

    public int DurationHours { get; set; } = 2; // 1, 2, 3

    [StringLength(150)]
    public string CampusName { get; set; } = "NED Main Campus";

    [StringLength(150)]
    public string BuildingName { get; set; } = "Department of Computer Science";

    [StringLength(50)]
    public string RoomNumber { get; set; } = "Lab 101";

    [StringLength(20)]
    public string ReportingTime { get; set; } = "08:30 AM";

    [StringLength(500)]
    public string Instructions { get; set; } = "Bring original CNIC, Admit Card printout, and clipboard.";

    [StringLength(250)]
    public string RequiredItems { get; set; } = "CNIC/B-Form, Admit Card, Ballpoint Pens";

    [StringLength(150)]
    public string DressCode { get; set; } = "Formal / Academic Attire";

    [StringLength(250)]
    public string QrCodeData { get; set; } = string.Empty;

    [StringLength(500)]
    public string? GoogleMapsUrl { get; set; }

    public string ScheduledByUserId { get; set; } = "AdmissionOfficer";

    public DateTime ScheduledAt { get; set; } = DateTime.UtcNow;
}

public class AdmissionTestResult
{
    public int Id { get; set; }

    public int AdmissionApplicationId { get; set; }
    public AdmissionApplication? AdmissionApplication { get; set; }

    public double TotalMarks { get; set; } = 100;

    public double ObtainedMarks { get; set; }

    public double PassingPercentage { get; set; } = 50;

    public double CalculatedPercentage { get; set; }

    public bool IsQualified { get; set; }

    public double CalculatedScholarshipPercentage { get; set; } = 0;
    public double ApprovedScholarshipPercentage { get; set; } = 0;
    [StringLength(500)]
    public string? OverrideReason { get; set; }
    public string? OverrideApprovedByUserId { get; set; }

    [StringLength(500)]
    public string? Remarks { get; set; }

    public string EvaluatedByUserId { get; set; } = "AdmissionOfficer";

    public DateTime EvaluatedAt { get; set; } = DateTime.UtcNow;
}

public class ApplicationInternalNote
{
    public int Id { get; set; }

    public int AdmissionApplicationId { get; set; }
    public AdmissionApplication? AdmissionApplication { get; set; }

    [Required]
    [StringLength(100)]
    public string AuthorUserId { get; set; } = string.Empty;

    [Required]
    public string NoteText { get; set; } = string.Empty;

    [StringLength(250)]
    public string? AttachmentPath { get; set; }

    [StringLength(100)]
    public string? MentionedAdminUser { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class CommunicationTemplate
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string TemplateKey { get; set; } = string.Empty; // e.g. "ApplicationSubmitted", "TestScheduled", "ScholarshipAwarded"

    [Required]
    [StringLength(200)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    public string BodyHtml { get; set; } = string.Empty;

    [StringLength(50)]
    public string Channel { get; set; } = "Email"; // "Email", "SMS"

    [StringLength(500)]
    public string? PlaceholderGuide { get; set; } // e.g. "{{StudentName}}, {{Program}}, {{TestDate}}, {{ScholarshipPercentage}}"

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public class ScholarshipRule
{
    public int Id { get; set; }

    [Required]
    [StringLength(120)]
    public string ScholarshipName { get; set; } = string.Empty;

    [StringLength(250)]
    public string Description { get; set; } = string.Empty;

    public double MinimumPercentage { get; set; } // e.g. 75, 80, 90

    public double ScholarshipPercentage { get; set; } // e.g. 40, 60, 100

    public DateTime? EffectiveStartDate { get; set; }

    public DateTime? EffectiveEndDate { get; set; }

    public bool IsEnabled { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class FeeStructureConfig
{
    public int Id { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal AdmissionFee { get; set; } = 15000;

    [Column(TypeName = "decimal(18,2)")]
    public decimal RegistrationFee { get; set; } = 5000;

    [Column(TypeName = "decimal(18,2)")]
    public decimal SecurityDeposit { get; set; } = 5000;

    [Column(TypeName = "decimal(18,2)")]
    public decimal LibraryFee { get; set; } = 2500;

    [Column(TypeName = "decimal(18,2)")]
    public decimal LabFee { get; set; } = 2500;

    [Column(TypeName = "decimal(18,2)")]
    public decimal StudentActivityFee { get; set; } = 1000;

    [Column(TypeName = "decimal(18,2)")]
    public decimal SemesterFee { get; set; } = 125000;

    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

    public string UpdatedByUserId { get; set; } = "Admin";
}

public class ApplicantAcademicRecord
{
    public int Id { get; set; }

    public int ApplicantId { get; set; }
    public Applicant? Applicant { get; set; }

    [Required]
    [StringLength(50)]
    public string Level { get; set; } = "Matriculation"; // Matric, Intermediate, Diploma, O-Level, A-Level, Bachelor

    [Required]
    [StringLength(150)]
    public string DegreeOrCertificateTitle { get; set; } = string.Empty; // SSC Science, Pre-Engineering, Pre-Medical, ICS, etc.

    [Required]
    [StringLength(150)]
    public string BoardOrUniversity { get; set; } = string.Empty; // BISE Karachi, FBISE, Cambridge, etc.

    public int PassingYear { get; set; }

    [StringLength(50)]
    public string RollNumber { get; set; } = string.Empty;

    public double TotalMarks { get; set; }

    public double ObtainedMarks { get; set; }

    public double Percentage { get; set; }

    [StringLength(10)]
    public string GradeOrGPA { get; set; } = string.Empty;

    [StringLength(250)]
    public string? MarksheetPath { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ApplicantGuardianInfo
{
    public int Id { get; set; }

    public int ApplicantId { get; set; }
    public Applicant? Applicant { get; set; }

    [Required]
    [StringLength(120)]
    public string FatherName { get; set; } = string.Empty;

    [StringLength(50)]
    public string? FatherCNIC { get; set; }

    [StringLength(100)]
    public string? FatherOccupation { get; set; }

    [StringLength(30)]
    public string? FatherPhone { get; set; }

    [Required]
    [StringLength(120)]
    public string GuardianName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string GuardianRelation { get; set; } = "Father"; // Father, Mother, Brother, Uncle, Legal Guardian

    [Required]
    [StringLength(30)]
    public string GuardianPhone { get; set; } = string.Empty;

    [StringLength(50)]
    public string? GuardianCNIC { get; set; }

    [StringLength(100)]
    public string? GuardianOccupation { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal MonthlyIncome { get; set; } = 0m;

    [StringLength(250)]
    public string? EmergencyAddress { get; set; }

    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
}

public class ApplicantMessage
{
    public int Id { get; set; }

    public int ApplicantId { get; set; }
    public Applicant? Applicant { get; set; }

    [Required]
    [StringLength(50)]
    public string SenderType { get; set; } = "Applicant"; // "Applicant" or "Admin"

    [StringLength(100)]
    public string? SenderName { get; set; }

    [Required]
    [StringLength(200)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    public string MessageBody { get; set; } = string.Empty;

    [StringLength(250)]
    public string? AttachmentPath { get; set; }

    public bool IsRead { get; set; } = false;

    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}

public class SupportTicket
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string TicketNumber { get; set; } = string.Empty; // e.g. TCK-2026-98124

    public int ApplicantId { get; set; }
    public Applicant? Applicant { get; set; }

    [Required]
    [StringLength(200)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Category { get; set; } = "General"; // General, Application, Document, Payment, Technical

    [Required]
    [StringLength(30)]
    public string Priority { get; set; } = "Medium"; // Low, Medium, High, Urgent

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    public string Status { get; set; } = "Open"; // Open, InProgress, Resolved, Closed

    public string? ResolutionNotes { get; set; }

    public string? AssignedOfficerId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ResolvedAt { get; set; }
}

public class EnrollmentRecord
{
    public int Id { get; set; }

    public int ApplicantId { get; set; }
    public Applicant? Applicant { get; set; }

    public int AdmissionApplicationId { get; set; }
    public AdmissionApplication? AdmissionApplication { get; set; }

    [Required]
    [StringLength(50)]
    public string GeneratedStudentId { get; set; } = string.Empty; // e.g. STD-2026-08912

    [Required]
    [StringLength(50)]
    public string GeneratedRollNumber { get; set; } = string.Empty; // e.g. 2026-CS-041

    public int DegreeProgramId { get; set; }
    public DegreeProgram? DegreeProgram { get; set; }

    public int DepartmentId { get; set; }
    public Department? Department { get; set; }

    [Required]
    [StringLength(30)]
    public string Batch { get; set; } = "Batch 2026";

    [Required]
    [StringLength(30)]
    public string Semester { get; set; } = "1st Semester";

    [Required]
    [StringLength(30)]
    public string EnrollmentStatus { get; set; } = "Active"; // Active, ProvisionallyEnrolled, Withdrawn

    public DateTime ActivationDate { get; set; } = DateTime.UtcNow;

    [StringLength(30)]
    public string FutureUmsSyncStatus { get; set; } = "PendingSync"; // PendingSync, Synced, Failed

    public string? SyncMetadataJson { get; set; }
}

public class AdmissionDecision
{
    public int Id { get; set; }

    public int AdmissionApplicationId { get; set; }
    public AdmissionApplication? AdmissionApplication { get; set; }

    [Required]
    [StringLength(50)]
    public string DecisionType { get; set; } = "Approve"; // Approve, Reject, Hold, RequestAdditionalDocuments, WaitList, InterviewRequired, MedicalVerificationRequired, FeePending

    [StringLength(500)]
    public string? Comments { get; set; }

    [Required]
    [StringLength(100)]
    public string OfficerId { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class ApplicationTimelineLog
{
    public int Id { get; set; }

    public int AdmissionApplicationId { get; set; }
    public AdmissionApplication? AdmissionApplication { get; set; }

    [Required]
    [StringLength(100)]
    public string StageName { get; set; } = string.Empty; // AccountCreated, ProfileCompleted, ApplicationStarted, DocumentsUploaded, ApplicationSubmitted, DocumentVerification, AcademicVerification, InterviewScheduled, FeeVerification, Selected, EnrollmentCompleted, StudentPortalActivated

    [Required]
    [StringLength(200)]
    public string Description { get; set; } = string.Empty;

    [StringLength(100)]
    public string PerformedBy { get; set; } = "System";

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class ApplicantNotification
{
    public int Id { get; set; }

    public int ApplicantId { get; set; }
    public Applicant? Applicant { get; set; }

    [Required]
    [StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Message { get; set; } = string.Empty;

    [StringLength(50)]
    public string NotificationType { get; set; } = "Info"; // Info, Success, Warning, Danger

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
