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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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
    }
}
