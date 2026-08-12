using Microsoft.EntityFrameworkCore;
using NedAcademyWebsite.Models;

namespace NedAcademyWebsite.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Faculty> FacultyMembers => Set<Faculty>();
    public DbSet<DegreeProgram> DegreePrograms => Set<DegreeProgram>();
    public DbSet<Leadership> LeadershipRegistry => Set<Leadership>();
    public DbSet<ResearchItem> ResearchItems => Set<ResearchItem>();
    public DbSet<CampusFacility> CampusFacilities => Set<CampusFacility>();
    public DbSet<NewsItem> NewsItems => Set<NewsItem>();
    public DbSet<EventItem> EventItems => Set<EventItem>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<DownloadFile> DownloadFiles => Set<DownloadFile>();
    public DbSet<FAQItem> FAQItems => Set<FAQItem>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
    public DbSet<MediaItem> MediaItems => Set<MediaItem>();
    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();
    public DbSet<NavMenuItem> NavMenuItems => Set<NavMenuItem>();
    public DbSet<SectionSetting> SectionSettings => Set<SectionSetting>();
    public DbSet<PageSection> PageSections => Set<PageSection>();
    public DbSet<CustomForm> CustomForms => Set<CustomForm>();
    public DbSet<FormField> FormFields => Set<FormField>();
    public DbSet<ContentRevision> ContentRevisions => Set<ContentRevision>();
    public DbSet<CmsNotification> CmsNotifications => Set<CmsNotification>();
    public DbSet<BackupRecord> BackupRecords => Set<BackupRecord>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // Enterprise Admission System Entities
    public DbSet<Applicant> Applicants => Set<Applicant>();
    public DbSet<ApplicantProfile> ApplicantProfiles => Set<ApplicantProfile>();
    public DbSet<ApplicantGuardianInfo> ApplicantGuardianInfos => Set<ApplicantGuardianInfo>();
    public DbSet<ApplicantAcademicRecord> ApplicantAcademicRecords => Set<ApplicantAcademicRecord>();
    public DbSet<ApplicantMessage> ApplicantMessages => Set<ApplicantMessage>();
    public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();
    public DbSet<EnrollmentRecord> EnrollmentRecords => Set<EnrollmentRecord>();
    public DbSet<AdmissionDecision> AdmissionDecisions => Set<AdmissionDecision>();
    public DbSet<ApplicationTimelineLog> ApplicationTimelineLogs => Set<ApplicationTimelineLog>();
    public DbSet<ApplicantNotification> ApplicantNotifications => Set<ApplicantNotification>();
    public DbSet<AdmissionSession> AdmissionSessions => Set<AdmissionSession>();
    public DbSet<AdmissionSessionProgram> AdmissionSessionPrograms => Set<AdmissionSessionProgram>();
    public DbSet<AdmissionApplication> AdmissionApplications => Set<AdmissionApplication>();
    public DbSet<ApplicationDocument> ApplicationDocuments => Set<ApplicationDocument>();
    public DbSet<ApplicationStatusHistory> ApplicationStatusHistories => Set<ApplicationStatusHistory>();
    public DbSet<FeeVoucher> FeeVouchers => Set<FeeVoucher>();
    public DbSet<AdmissionAuditLog> AdmissionAuditLogs => Set<AdmissionAuditLog>();

    // New 10/10 ERP DbSets
    public DbSet<DocumentVersionHistory> DocumentVersionHistories => Set<DocumentVersionHistory>();
    public DbSet<AdmissionTestSchedule> AdmissionTestSchedules => Set<AdmissionTestSchedule>();
    public DbSet<AdmissionTestResult> AdmissionTestResults => Set<AdmissionTestResult>();
    public DbSet<ScholarshipRule> ScholarshipRules => Set<ScholarshipRule>();
    public DbSet<FeeStructureConfig> FeeStructureConfigs => Set<FeeStructureConfig>();
    public DbSet<ApplicationInternalNote> ApplicationInternalNotes => Set<ApplicationInternalNote>();
    public DbSet<CommunicationTemplate> CommunicationTemplates => Set<CommunicationTemplate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Applicant -> ApplicantProfile 1-to-1 relationship
        modelBuilder.Entity<ApplicantProfile>()
            .HasOne(ap => ap.Applicant)
            .WithOne(a => a.Profile)
            .HasForeignKey<ApplicantProfile>(ap => ap.ApplicantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Applicant -> ApplicantGuardianInfo 1-to-1 relationship
        modelBuilder.Entity<ApplicantGuardianInfo>()
            .HasOne(g => g.Applicant)
            .WithOne(a => a.GuardianInfo)
            .HasForeignKey<ApplicantGuardianInfo>(g => g.ApplicantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Applicant -> ApplicantAcademicRecord 1-to-Many relationship
        modelBuilder.Entity<ApplicantAcademicRecord>()
            .HasOne(ar => ar.Applicant)
            .WithMany(a => a.AcademicRecords)
            .HasForeignKey(ar => ar.ApplicantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Applicant -> ApplicantMessage 1-to-Many relationship
        modelBuilder.Entity<ApplicantMessage>()
            .HasOne(m => m.Applicant)
            .WithMany(a => a.Messages)
            .HasForeignKey(m => m.ApplicantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Applicant -> SupportTicket 1-to-Many relationship
        modelBuilder.Entity<SupportTicket>()
            .HasOne(st => st.Applicant)
            .WithMany(a => a.SupportTickets)
            .HasForeignKey(st => st.ApplicantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure Department -> Faculty relationship
        modelBuilder.Entity<Faculty>()
            .HasOne(f => f.Department)
            .WithMany(d => d.FacultyMembers)
            .HasForeignKey(f => f.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Department -> DegreeProgram relationship
        modelBuilder.Entity<DegreeProgram>()
            .HasOne(p => p.Department)
            .WithMany(d => d.Programs)
            .HasForeignKey(p => p.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure CustomForm -> FormField relationship
        modelBuilder.Entity<FormField>()
            .HasOne(f => f.CustomForm)
            .WithMany(c => c.Fields)
            .HasForeignKey(f => f.CustomFormId)
            .OnDelete(DeleteBehavior.Cascade);

        // ==========================================
        // ENTERPRISE ADMISSION SYSTEM CONFIGURATIONS
        // ==========================================

        // Applicant Indexes & Constraints
        modelBuilder.Entity<Applicant>()
            .HasIndex(a => a.Email)
            .IsUnique();

        modelBuilder.Entity<Applicant>()
            .HasIndex(a => a.CNIC_Passport)
            .IsUnique();

        // AdmissionApplication Indexes & Foreign Keys
        modelBuilder.Entity<AdmissionApplication>()
            .HasIndex(a => a.ApplicationNumber)
            .IsUnique();

        modelBuilder.Entity<AdmissionApplication>()
            .HasOne(a => a.Applicant)
            .WithMany(ap => ap.Applications)
            .HasForeignKey(a => a.ApplicantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AdmissionApplication>()
            .HasOne(a => a.AdmissionSession)
            .WithMany(s => s.Applications)
            .HasForeignKey(a => a.AdmissionSessionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AdmissionApplication>()
            .HasOne(a => a.DegreeProgram)
            .WithMany()
            .HasForeignKey(a => a.DegreeProgramId)
            .OnDelete(DeleteBehavior.Restrict);

        // ApplicationDocument Foreign Keys
        modelBuilder.Entity<ApplicationDocument>()
            .HasOne(d => d.AdmissionApplication)
            .WithMany(a => a.UploadedDocuments)
            .HasForeignKey(d => d.AdmissionApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ApplicationDocument>()
            .HasOne(d => d.Applicant)
            .WithMany(ap => ap.Documents)
            .HasForeignKey(d => d.ApplicantId)
            .OnDelete(DeleteBehavior.Restrict);

        // ApplicationStatusHistory Foreign Key
        modelBuilder.Entity<ApplicationStatusHistory>()
            .HasOne(h => h.AdmissionApplication)
            .WithMany(a => a.StatusHistory)
            .HasForeignKey(h => h.AdmissionApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        // FeeVoucher Indexes & Foreign Keys
        modelBuilder.Entity<FeeVoucher>()
            .HasIndex(v => v.VoucherNumber)
            .IsUnique();

        modelBuilder.Entity<FeeVoucher>()
            .HasOne(v => v.AdmissionApplication)
            .WithMany(a => a.FeeVouchers)
            .HasForeignKey(v => v.AdmissionApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<FeeVoucher>()
            .HasOne(v => v.Applicant)
            .WithMany(ap => ap.FeeVouchers)
            .HasForeignKey(v => v.ApplicantId)
            .OnDelete(DeleteBehavior.Restrict);

        // AdmissionSessionProgram Relationships
        modelBuilder.Entity<AdmissionSessionProgram>()
            .HasOne(sp => sp.AdmissionSession)
            .WithMany(s => s.SessionPrograms)
            .HasForeignKey(sp => sp.AdmissionSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AdmissionSessionProgram>()
            .HasOne(sp => sp.DegreeProgram)
            .WithMany()
            .HasForeignKey(sp => sp.DegreeProgramId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
