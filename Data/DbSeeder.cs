using Microsoft.EntityFrameworkCore;
using NedAcademyWebsite.Models;

namespace NedAcademyWebsite.Data;

public static class DbSeeder
{
    public static void Seed(ApplicationDbContext context)
    {
        // 0. Ensure Phase 3 Tables Exist in SQL Database
        try
        {
            context.Database.ExecuteSqlRaw(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ApplicantGuardianInfos')
                BEGIN
                    CREATE TABLE [ApplicantGuardianInfos] (
                        [Id] int NOT NULL IDENTITY,
                        [ApplicantId] int NOT NULL,
                        [FatherName] nvarchar(120) NOT NULL,
                        [FatherCNIC] nvarchar(50) NULL,
                        [FatherOccupation] nvarchar(100) NULL,
                        [FatherPhone] nvarchar(30) NULL,
                        [GuardianName] nvarchar(120) NOT NULL,
                        [GuardianRelation] nvarchar(50) NOT NULL,
                        [GuardianPhone] nvarchar(30) NOT NULL,
                        [GuardianCNIC] nvarchar(50) NULL,
                        [GuardianOccupation] nvarchar(100) NULL,
                        [MonthlyIncome] decimal(18,2) NOT NULL DEFAULT 0.0,
                        [EmergencyAddress] nvarchar(250) NULL,
                        [LastUpdatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
                        CONSTRAINT [PK_ApplicantGuardianInfos] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_ApplicantGuardianInfos_Applicants_ApplicantId] FOREIGN KEY ([ApplicantId]) REFERENCES [Applicants] ([Id]) ON DELETE CASCADE
                    );
                    CREATE UNIQUE INDEX [IX_ApplicantGuardianInfos_ApplicantId] ON [ApplicantGuardianInfos] ([ApplicantId]);
                END;

                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ApplicantAcademicRecords')
                BEGIN
                    CREATE TABLE [ApplicantAcademicRecords] (
                        [Id] int NOT NULL IDENTITY,
                        [ApplicantId] int NOT NULL,
                        [Level] nvarchar(50) NOT NULL,
                        [DegreeOrCertificateTitle] nvarchar(150) NOT NULL,
                        [BoardOrUniversity] nvarchar(150) NOT NULL,
                        [PassingYear] int NOT NULL,
                        [RollNumber] nvarchar(50) NOT NULL,
                        [TotalMarks] float NOT NULL,
                        [ObtainedMarks] float NOT NULL,
                        [Percentage] float NOT NULL,
                        [GradeOrGPA] nvarchar(10) NOT NULL,
                        [MarksheetPath] nvarchar(250) NULL,
                        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
                        CONSTRAINT [PK_ApplicantAcademicRecords] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_ApplicantAcademicRecords_Applicants_ApplicantId] FOREIGN KEY ([ApplicantId]) REFERENCES [Applicants] ([Id]) ON DELETE CASCADE
                    );
                    CREATE INDEX [IX_ApplicantAcademicRecords_ApplicantId] ON [ApplicantAcademicRecords] ([ApplicantId]);
                END;

                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ApplicantMessages')
                BEGIN
                    CREATE TABLE [ApplicantMessages] (
                        [Id] int NOT NULL IDENTITY,
                        [ApplicantId] int NOT NULL,
                        [SenderType] nvarchar(50) NOT NULL,
                        [SenderName] nvarchar(100) NULL,
                        [Subject] nvarchar(200) NOT NULL,
                        [MessageBody] nvarchar(max) NOT NULL,
                        [AttachmentPath] nvarchar(250) NULL,
                        [IsRead] bit NOT NULL DEFAULT 0,
                        [SentAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
                        CONSTRAINT [PK_ApplicantMessages] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_ApplicantMessages_Applicants_ApplicantId] FOREIGN KEY ([ApplicantId]) REFERENCES [Applicants] ([Id]) ON DELETE CASCADE
                    );
                    CREATE INDEX [IX_ApplicantMessages_ApplicantId] ON [ApplicantMessages] ([ApplicantId]);
                END;

                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SupportTickets')
                BEGIN
                    CREATE TABLE [SupportTickets] (
                        [Id] int NOT NULL IDENTITY,
                        [TicketNumber] nvarchar(50) NOT NULL,
                        [ApplicantId] int NOT NULL,
                        [Subject] nvarchar(200) NOT NULL,
                        [Category] nvarchar(50) NOT NULL,
                        [Priority] nvarchar(30) NOT NULL,
                        [Description] nvarchar(max) NOT NULL,
                        [Status] nvarchar(30) NOT NULL,
                        [ResolutionNotes] nvarchar(max) NULL,
                        [AssignedOfficerId] nvarchar(max) NULL,
                        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
                        [ResolvedAt] datetime2 NULL,
                        CONSTRAINT [PK_SupportTickets] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_SupportTickets_Applicants_ApplicantId] FOREIGN KEY ([ApplicantId]) REFERENCES [Applicants] ([Id]) ON DELETE CASCADE
                    );
                    CREATE INDEX [IX_SupportTickets_ApplicantId] ON [SupportTickets] ([ApplicantId]);
                END;

                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'EnrollmentRecords')
                BEGIN
                    CREATE TABLE [EnrollmentRecords] (
                        [Id] int NOT NULL IDENTITY,
                        [ApplicantId] int NOT NULL,
                        [AdmissionApplicationId] int NOT NULL,
                        [GeneratedStudentId] nvarchar(50) NOT NULL,
                        [GeneratedRollNumber] nvarchar(50) NOT NULL,
                        [DegreeProgramId] int NOT NULL,
                        [DepartmentId] int NOT NULL,
                        [Batch] nvarchar(30) NOT NULL DEFAULT 'Batch 2026',
                        [Semester] nvarchar(30) NOT NULL DEFAULT '1st Semester',
                        [EnrollmentStatus] nvarchar(30) NOT NULL DEFAULT 'Active',
                        [ActivationDate] datetime2 NOT NULL DEFAULT GETUTCDATE(),
                        [FutureUmsSyncStatus] nvarchar(30) NOT NULL DEFAULT 'PendingSync',
                        [SyncMetadataJson] nvarchar(max) NULL,
                        CONSTRAINT [PK_EnrollmentRecords] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_EnrollmentRecords_Applicants_ApplicantId] FOREIGN KEY ([ApplicantId]) REFERENCES [Applicants] ([Id]) ON DELETE CASCADE
                    );
                    CREATE UNIQUE INDEX [IX_EnrollmentRecords_ApplicantId] ON [EnrollmentRecords] ([ApplicantId]);
                    CREATE INDEX [IX_EnrollmentRecords_AdmissionApplicationId] ON [EnrollmentRecords] ([AdmissionApplicationId]);
                END;

                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AdmissionDecisions')
                BEGIN
                    CREATE TABLE [AdmissionDecisions] (
                        [Id] int NOT NULL IDENTITY,
                        [AdmissionApplicationId] int NOT NULL,
                        [DecisionType] nvarchar(50) NOT NULL,
                        [Comments] nvarchar(500) NULL,
                        [OfficerId] nvarchar(100) NOT NULL,
                        [Timestamp] datetime2 NOT NULL DEFAULT GETUTCDATE(),
                        CONSTRAINT [PK_AdmissionDecisions] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_AdmissionDecisions_AdmissionApplications_AdmissionApplicationId] FOREIGN KEY ([AdmissionApplicationId]) REFERENCES [AdmissionApplications] ([Id]) ON DELETE CASCADE
                    );
                    CREATE INDEX [IX_AdmissionDecisions_AdmissionApplicationId] ON [AdmissionDecisions] ([AdmissionApplicationId]);
                END;

                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ApplicationTimelineLogs')
                BEGIN
                    CREATE TABLE [ApplicationTimelineLogs] (
                        [Id] int NOT NULL IDENTITY,
                        [AdmissionApplicationId] int NOT NULL,
                        [StageName] nvarchar(100) NOT NULL,
                        [Description] nvarchar(200) NOT NULL,
                        [PerformedBy] nvarchar(100) NOT NULL DEFAULT 'System',
                        [Timestamp] datetime2 NOT NULL DEFAULT GETUTCDATE(),
                        CONSTRAINT [PK_ApplicationTimelineLogs] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_ApplicationTimelineLogs_AdmissionApplications_AdmissionApplicationId] FOREIGN KEY ([AdmissionApplicationId]) REFERENCES [AdmissionApplications] ([Id]) ON DELETE CASCADE
                    );
                    CREATE INDEX [IX_ApplicationTimelineLogs_AdmissionApplicationId] ON [ApplicationTimelineLogs] ([AdmissionApplicationId]);
                END;

                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ApplicantNotifications')
                BEGIN
                    CREATE TABLE [ApplicantNotifications] (
                        [Id] int NOT NULL IDENTITY,
                        [ApplicantId] int NOT NULL,
                        [Title] nvarchar(150) NOT NULL,
                        [Message] nvarchar(max) NOT NULL,
                        [NotificationType] nvarchar(50) NOT NULL DEFAULT 'Info',
                        [IsRead] bit NOT NULL DEFAULT 0,
                        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
                        CONSTRAINT [PK_ApplicantNotifications] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_ApplicantNotifications_Applicants_ApplicantId] FOREIGN KEY ([ApplicantId]) REFERENCES [Applicants] ([Id]) ON DELETE CASCADE
                    );
                    CREATE INDEX [IX_ApplicantNotifications_ApplicantId] ON [ApplicantNotifications] ([ApplicantId]);
                END;

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ApplicantProfiles') AND name = 'FullName')
                    ALTER TABLE [ApplicantProfiles] ADD [FullName] nvarchar(120) NULL;

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ApplicantProfiles') AND name = 'FatherName')
                    ALTER TABLE [ApplicantProfiles] ADD [FatherName] nvarchar(120) NULL;

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ApplicantProfiles') AND name = 'CNIC_Passport')
                    ALTER TABLE [ApplicantProfiles] ADD [CNIC_Passport] nvarchar(50) NULL;

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ApplicantProfiles') AND name = 'Domicile')
                    ALTER TABLE [ApplicantProfiles] ADD [Domicile] nvarchar(80) NULL;

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ApplicantProfiles') AND name = 'PermanentAddress')
                    ALTER TABLE [ApplicantProfiles] ADD [PermanentAddress] nvarchar(250) NULL;

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ApplicantProfiles') AND name = 'PostalAddress')
                    ALTER TABLE [ApplicantProfiles] ADD [PostalAddress] nvarchar(250) NULL;

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ApplicantProfiles') AND name = 'GuardianName')
                    ALTER TABLE [ApplicantProfiles] ADD [GuardianName] nvarchar(120) NULL;

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ApplicantProfiles') AND name = 'GuardianPhone')
                    ALTER TABLE [ApplicantProfiles] ADD [GuardianPhone] nvarchar(30) NULL;

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ApplicantProfiles') AND name = 'EmergencyContactName')
                    ALTER TABLE [ApplicantProfiles] ADD [EmergencyContactName] nvarchar(120) NULL;

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ApplicantProfiles') AND name = 'EmergencyContactPhone')
                    ALTER TABLE [ApplicantProfiles] ADD [EmergencyContactPhone] nvarchar(30) NULL;

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ApplicationDocuments') AND name = 'ReplacementRequested')
                    ALTER TABLE [ApplicationDocuments] ADD [ReplacementRequested] bit NOT NULL DEFAULT 0;

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ApplicationDocuments') AND name = 'RejectionReason')
                    ALTER TABLE [ApplicationDocuments] ADD [RejectionReason] nvarchar(500) NULL;

                -- Missing FeeVouchers Columns
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('FeeVouchers') AND name = 'AdmissionFee')
                    ALTER TABLE [FeeVouchers] ADD [AdmissionFee] decimal(18,2) NOT NULL DEFAULT 15000;

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('FeeVouchers') AND name = 'RegistrationFee')
                    ALTER TABLE [FeeVouchers] ADD [RegistrationFee] decimal(18,2) NOT NULL DEFAULT 5000;

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('FeeVouchers') AND name = 'SecurityDeposit')
                    ALTER TABLE [FeeVouchers] ADD [SecurityDeposit] decimal(18,2) NOT NULL DEFAULT 5000;

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('FeeVouchers') AND name = 'LibraryFee')
                    ALTER TABLE [FeeVouchers] ADD [LibraryFee] decimal(18,2) NOT NULL DEFAULT 2500;

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('FeeVouchers') AND name = 'LabFee')
                    ALTER TABLE [FeeVouchers] ADD [LabFee] decimal(18,2) NOT NULL DEFAULT 2500;

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('FeeVouchers') AND name = 'StudentActivityFee')
                    ALTER TABLE [FeeVouchers] ADD [StudentActivityFee] decimal(18,2) NOT NULL DEFAULT 1000;

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('FeeVouchers') AND name = 'SemesterFee')
                    ALTER TABLE [FeeVouchers] ADD [SemesterFee] decimal(18,2) NOT NULL DEFAULT 125000;

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('FeeVouchers') AND name = 'SubTotalAmount')
                    ALTER TABLE [FeeVouchers] ADD [SubTotalAmount] decimal(18,2) NOT NULL DEFAULT 155000;

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('FeeVouchers') AND name = 'ScholarshipPercentageApplied')
                    ALTER TABLE [FeeVouchers] ADD [ScholarshipPercentageApplied] float NOT NULL DEFAULT 0;

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('FeeVouchers') AND name = 'ScholarshipDiscountAmount')
                    ALTER TABLE [FeeVouchers] ADD [ScholarshipDiscountAmount] decimal(18,2) NOT NULL DEFAULT 0;

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('FeeVouchers') AND name = 'ManualDiscountAmount')
                    ALTER TABLE [FeeVouchers] ADD [ManualDiscountAmount] decimal(18,2) NOT NULL DEFAULT 0;

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('FeeVouchers') AND name = 'FineAmount')
                    ALTER TABLE [FeeVouchers] ADD [FineAmount] decimal(18,2) NOT NULL DEFAULT 0;

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('FeeVouchers') AND name = 'NetPayableAmount')
                    ALTER TABLE [FeeVouchers] ADD [NetPayableAmount] decimal(18,2) NOT NULL DEFAULT 155000;

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('FeeVouchers') AND name = 'ExpiryDate')
                    ALTER TABLE [FeeVouchers] ADD [ExpiryDate] datetime2 NOT NULL DEFAULT GETUTCDATE();

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('FeeVouchers') AND name = 'VoucherStatus')
                    ALTER TABLE [FeeVouchers] ADD [VoucherStatus] int NOT NULL DEFAULT 1;

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('FeeVouchers') AND name = 'PreviousVoucherId')
                    ALTER TABLE [FeeVouchers] ADD [PreviousVoucherId] int NULL;

                -- Missing AdmissionAuditLogs Columns
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AdmissionAuditLogs') AND name = 'OldValue')
                    ALTER TABLE [AdmissionAuditLogs] ADD [OldValue] nvarchar(max) NULL;

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AdmissionAuditLogs') AND name = 'NewValue')
                    ALTER TABLE [AdmissionAuditLogs] ADD [NewValue] nvarchar(max) NULL;

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AdmissionAuditLogs') AND name = 'UserAgent')
                    ALTER TABLE [AdmissionAuditLogs] ADD [UserAgent] nvarchar(250) NULL;

                -- New ERP Tables
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DocumentVersionHistories')
                BEGIN
                    CREATE TABLE [DocumentVersionHistories] (
                        [Id] int NOT NULL IDENTITY,
                        [ApplicationDocumentId] int NOT NULL,
                        [VersionNumber] int NOT NULL DEFAULT 1,
                        [FileName] nvarchar(250) NOT NULL,
                        [FilePath] nvarchar(500) NOT NULL,
                        [RejectionReason] nvarchar(500) NULL,
                        [UploadedBy] nvarchar(100) NOT NULL DEFAULT 'Applicant',
                        [UploadedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
                        CONSTRAINT [PK_DocumentVersionHistories] PRIMARY KEY ([Id])
                    );
                END;

                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AdmissionTestSchedules')
                BEGIN
                    CREATE TABLE [AdmissionTestSchedules] (
                        [Id] int NOT NULL IDENTITY,
                        [AdmissionApplicationId] int NOT NULL,
                        [ApplicantId] int NOT NULL,
                        [TestDate] datetime2 NOT NULL,
                        [TestTime] nvarchar(20) NOT NULL DEFAULT '09:00 AM',
                        [DurationHours] int NOT NULL DEFAULT 2,
                        [CampusName] nvarchar(150) NOT NULL DEFAULT 'NED Main Campus',
                        [BuildingName] nvarchar(150) NOT NULL DEFAULT 'Department of Computer Science',
                        [RoomNumber] nvarchar(50) NOT NULL DEFAULT 'Lab 101',
                        [ReportingTime] nvarchar(20) NOT NULL DEFAULT '08:30 AM',
                        [Instructions] nvarchar(500) NULL,
                        [RequiredItems] nvarchar(250) NULL,
                        [DressCode] nvarchar(150) NULL,
                        [QrCodeData] nvarchar(250) NULL,
                        [GoogleMapsUrl] nvarchar(500) NULL,
                        [ScheduledByUserId] nvarchar(100) NOT NULL DEFAULT 'AdmissionOfficer',
                        [ScheduledAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
                        CONSTRAINT [PK_AdmissionTestSchedules] PRIMARY KEY ([Id])
                    );
                END;

                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AdmissionTestResults')
                BEGIN
                    CREATE TABLE [AdmissionTestResults] (
                        [Id] int NOT NULL IDENTITY,
                        [AdmissionApplicationId] int NOT NULL,
                        [TotalMarks] float NOT NULL DEFAULT 100,
                        [ObtainedMarks] float NOT NULL DEFAULT 0,
                        [PassingPercentage] float NOT NULL DEFAULT 50,
                        [CalculatedPercentage] float NOT NULL DEFAULT 0,
                        [IsQualified] bit NOT NULL DEFAULT 0,
                        [Remarks] nvarchar(500) NULL,
                        [EvaluatedByUserId] nvarchar(100) NOT NULL DEFAULT 'AdmissionOfficer',
                        [EvaluatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
                        CONSTRAINT [PK_AdmissionTestResults] PRIMARY KEY ([Id])
                    );
                END;

                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ScholarshipRules')
                BEGIN
                    CREATE TABLE [ScholarshipRules] (
                        [Id] int NOT NULL IDENTITY,
                        [ScholarshipName] nvarchar(120) NOT NULL,
                        [Description] nvarchar(250) NULL,
                        [MinimumPercentage] float NOT NULL DEFAULT 0,
                        [ScholarshipPercentage] float NOT NULL DEFAULT 0,
                        [EffectiveStartDate] datetime2 NULL,
                        [EffectiveEndDate] datetime2 NULL,
                        [IsEnabled] bit NOT NULL DEFAULT 1,
                        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
                        CONSTRAINT [PK_ScholarshipRules] PRIMARY KEY ([Id])
                    );
                END;

                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'FeeStructureConfigs')
                BEGIN
                    CREATE TABLE [FeeStructureConfigs] (
                        [Id] int NOT NULL IDENTITY,
                        [AdmissionFee] decimal(18,2) NOT NULL DEFAULT 15000,
                        [RegistrationFee] decimal(18,2) NOT NULL DEFAULT 5000,
                        [SecurityDeposit] decimal(18,2) NOT NULL DEFAULT 5000,
                        [LibraryFee] decimal(18,2) NOT NULL DEFAULT 2500,
                        [LabFee] decimal(18,2) NOT NULL DEFAULT 2500,
                        [StudentActivityFee] decimal(18,2) NOT NULL DEFAULT 1000,
                        [SemesterFee] decimal(18,2) NOT NULL DEFAULT 125000,
                        [LastUpdatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
                        [UpdatedByUserId] nvarchar(100) NOT NULL DEFAULT 'SystemAdmin',
                        CONSTRAINT [PK_FeeStructureConfigs] PRIMARY KEY ([Id])
                    );
                END;

                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ApplicationInternalNotes')
                BEGIN
                    CREATE TABLE [ApplicationInternalNotes] (
                        [Id] int NOT NULL IDENTITY,
                        [AdmissionApplicationId] int NOT NULL,
                        [AuthorUserId] nvarchar(100) NOT NULL,
                        [NoteText] nvarchar(max) NOT NULL,
                        [AttachmentPath] nvarchar(250) NULL,
                        [MentionedAdminUser] nvarchar(100) NULL,
                        [Timestamp] datetime2 NOT NULL DEFAULT GETUTCDATE(),
                        CONSTRAINT [PK_ApplicationInternalNotes] PRIMARY KEY ([Id])
                    );
                END;

                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CommunicationTemplates')
                BEGIN
                    CREATE TABLE [CommunicationTemplates] (
                        [Id] int NOT NULL IDENTITY,
                        [TemplateKey] nvarchar(100) NOT NULL,
                        [Subject] nvarchar(200) NOT NULL,
                        [BodyHtml] nvarchar(max) NOT NULL,
                        [Channel] nvarchar(50) NOT NULL DEFAULT 'Email',
                        [PlaceholderGuide] nvarchar(500) NULL,
                        [LastUpdated] datetime2 NOT NULL DEFAULT GETUTCDATE(),
                        CONSTRAINT [PK_CommunicationTemplates] PRIMARY KEY ([Id])
                    );
                END;

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AdmissionApplications') AND name = 'AssignedDate')
                BEGIN
                    ALTER TABLE [AdmissionApplications] ADD [AssignedDate] datetime2 NULL;
                    ALTER TABLE [AdmissionApplications] ADD [AssignedByUserId] nvarchar(100) NULL;
                END;

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AdmissionTestResults') AND name = 'CalculatedScholarshipPercentage')
                BEGIN
                    ALTER TABLE [AdmissionTestResults] ADD [CalculatedScholarshipPercentage] float NOT NULL DEFAULT 0;
                    ALTER TABLE [AdmissionTestResults] ADD [ApprovedScholarshipPercentage] float NOT NULL DEFAULT 0;
                    ALTER TABLE [AdmissionTestResults] ADD [OverrideReason] nvarchar(500) NULL;
                    ALTER TABLE [AdmissionTestResults] ADD [OverrideApprovedByUserId] nvarchar(100) NULL;
                END;
            ");
        }
        catch { }

        // 1. Seed System Settings
        if (!context.SystemSettings.Any())
        {
            context.SystemSettings.Add(new SystemSetting
            {
                UniversityName = "NED Academy",
                Slogan = "Pioneering the Next Frontier of Learning & Technology",
                LogoPath = "/uploads/logo.png",
                LogoDarkPath = "/uploads/logo_dark.png",
                FaviconPath = "/uploads/favicon.ico",
                ContactEmail = "info@nedacademy.edu",
                ContactPhone = "+92-21-99261261",
                Address = "NED Academy, NED University of Engineering & Technology, Main University Road, Karachi - 75270, Pakistan.",
                MapsLatitude = 24.9317,
                MapsLongitude = 67.1136,
                SocialFacebook = "https://facebook.com/nedacademy",
                SocialX = "https://x.com/nedacademy",
                SocialLinkedIn = "https://linkedin.com/company/nedacademy",
                SocialYouTube = "https://youtube.com/nedacademy",
                HeroTitle = "Pioneering the Next Frontier of Learning & Technology.",
                HeroSubtitle = "NED Academy represents excellence, industry-integrated paths, and cutting-edge research. We train creators, architects, and visionaries to lead global technical revolutions.",
                HeroVideoPath = "",
                HeroImagePath = "/uploads/hero_widget.png",
                AboutManifesto = "NED Academy was established to bridge the gap between abstract academic theories and modern engineering outputs. Grounded in the prestigious history of technical competence, we deliver accelerated programs to prepare architects of tomorrow.",
                AboutMission = "To enable future pioneers with project-first knowledge, fostering robust engineering models, high-performance computing competencies, and strategic industrial integration.",
                AboutVision = "To become a premier global hub of innovation, recognized for raising engineering standards, supporting open-source development, and producing creative technical leaders.",
                FooterCopyrightText = "NED Academy. All rights reserved."
            });
            context.SaveChanges();
        }

        // 2. Seed Departments
        if (!context.Departments.Any())
        {
            context.Departments.AddRange(
                new Department { Code = "CS-AI", Name = "Computer Science & AI" },
                new Department { Code = "ARCH-GOV", Name = "Architecture & Governance" },
                new Department { Code = "CIVIL-INFRA", Name = "Civil & Infrastructure" }
            );
            context.SaveChanges();
        }

        // Retrieve seeded departments for foreign keys
        var deptCs = context.Departments.FirstOrDefault(d => d.Code == "CS-AI");
        var deptArch = context.Departments.FirstOrDefault(d => d.Code == "ARCH-GOV");
        var deptCivil = context.Departments.FirstOrDefault(d => d.Code == "CIVIL-INFRA");

        // 3. Seed Faculty Members
        if (!context.FacultyMembers.Any() && deptCs != null && deptArch != null && deptCivil != null)
        {
            context.FacultyMembers.AddRange(
                new Faculty
                {
                    Name = "Prof. Dr. Muhammad Ali Ismail",
                    PhotoPath = "MI",
                    Designation = "Professor / CS Chairman",
                    Qualification = "Ph.D. in Computer Engineering (NED UET)",
                    DepartmentId = deptCs.Id,
                    Biography = "Dr. Muhammad Ali Ismail has over two decades of research experience in parallel architectures, HPC structures, and grid designs.",
                    ResearchAreas = "High-performance computing (HPC), distributed AI systems, GPU cluster designs.",
                    Email = "chairman-cs@nedacademy.edu"
                },
                new Faculty
                {
                    Name = "Dr. Danish Farooq",
                    PhotoPath = "DF",
                    Designation = "Associate Professor",
                    Qualification = "Ph.D. in Systems Security (Bonn, Germany)",
                    DepartmentId = deptCs.Id,
                    Biography = "Dr. Danish Farooq is a systems security researcher focusing on hardware interfaces security and network pen-testing.",
                    ResearchAreas = "Firmware hardening, smart systems validation pipelines, IoT pen-tests.",
                    Email = "dfarooq@nedacademy.edu"
                },
                new Faculty
                {
                    Name = "Dr. Maria Kazi",
                    PhotoPath = "MK",
                    Designation = "Assistant Professor",
                    Qualification = "Ph.D. in Deep Learning (Imperial, UK)",
                    DepartmentId = deptCs.Id,
                    Biography = "Dr. Maria Kazi works on transformer networks optimization, distributed deep learning weight pipelines, and natural language tasks.",
                    ResearchAreas = "Transformers architectures, neural processing, large language models.",
                    Email = "mkazi@nedacademy.edu"
                },
                new Faculty
                {
                    Name = "Prof. Dr. Noman Ahmed",
                    PhotoPath = "NA",
                    Designation = "Dean of Planning",
                    Qualification = "Ph.D. in Urban Governance (Loughborough, UK)",
                    DepartmentId = deptArch.Id,
                    Biography = "Dr. Noman Ahmed is a senior academic policy expert, researching public governance and sustainable civic systems.",
                    ResearchAreas = "Educational ethics, sustainable infrastructure models, planning boards.",
                    Email = "dean@nedacademy.edu"
                },
                new Faculty
                {
                    Name = "Prof. Dr. Sarosh H. Lodi",
                    PhotoPath = "SL",
                    Designation = "Professor / Advisor Lead",
                    Qualification = "Ph.D. in structural mechanics (UC Irvine, USA)",
                    DepartmentId = deptCivil.Id,
                    Biography = "Dr. Sarosh H. Lodi has led infrastructure design grids and structural diagnostics research globally.",
                    ResearchAreas = "Infrastructure modeling, smart grid load analysis.",
                    Email = "slodi@nedacademy.edu"
                }
            );
            context.SaveChanges();
        }

        // 4. Seed Leadership Registry
        if (!context.LeadershipRegistry.Any())
        {
            context.LeadershipRegistry.AddRange(
                new Leadership
                {
                    Name = "Hafiz Muhammad Saad",
                    Designation = "Founder & Patron-in-Chief",
                    Qualification = "B.E. in Software Engineering & Creative Systems (NED)",
                    Biography = "Technology represents the ultimate vehicle for human scalability and impact. NED Academy is our pledge to bypass structural lags in technical education, training next-generation systems engineers to build tomorrow's computational frameworks.",
                    PhotoPath = "HMS",
                    Email = "founder@nedacademy.edu",
                    Rank = 1
                },
                new Leadership
                {
                    Name = "Prof. Dr. Sarosh H. Lodi",
                    Designation = "Chancellor Advisor",
                    Qualification = "Ph.D. in Structural Engineering (UC Irvine, USA)",
                    Biography = "Coordinates the general academic steering board and guides smart infrastructure syllabus tracks.",
                    PhotoPath = "SL",
                    Email = "director@nedacademy.edu",
                    Rank = 2
                },
                new Leadership
                {
                    Name = "Prof. Dr. Noman Ahmed",
                    Designation = "Dean of Planning Board",
                    Qualification = "Ph.D. in Governance (Loughborough, UK)",
                    Biography = "Directs strategic educational guidelines and verifies ethics constraints inside syllabus frameworks.",
                    PhotoPath = "NA",
                    Email = "dean@nedacademy.edu",
                    Rank = 3
                },
                new Leadership
                {
                    Name = "Prof. Dr. Muhammad Ali Ismail",
                    Designation = "Chairman of Computing",
                    Qualification = "Ph.D. in Computer Engineering (NED UET)",
                    Biography = "Supervises computer engineering modules and handles high-performance computing lab allocations.",
                    PhotoPath = "MI",
                    Email = "chairman-cs@nedacademy.edu",
                    Rank = 4
                }
            );
            context.SaveChanges();
        }

        // 5. Seed Degree Programs
        if (!context.DegreePrograms.Any() && deptCs != null)
        {
            context.DegreePrograms.AddRange(
                new DegreeProgram
                {
                    Name = "BS Software Engineering (Accelerated & Systems)",
                    Code = "BS-SE",
                    DepartmentId = deptCs.Id,
                    Duration = "4 Years (8 Semesters)",
                    Eligibility = "Higher Secondary School Certificate (Pre-Engineering) with minimum 60% marks, or equivalent (A-Levels with physics, chemistry, and mathematics).",
                    Description = "Focuses on large-scale distributed systems, compiler design, cloud-native microservices, and database engine structures. Rather than learning just coding, students learn how to design architectures that manage billions of requests securely.",
                    LearningOutcomes = "Design resilient microservices using containerization and serverless patterns\nDevelop highly optimized compiler passes and custom low-overhead database systems\nDeploy secure automation pipelines with automated unit testing integration",
                    CareerOpportunities = "Systems Engineer\nCloud Solutions Architect\nBackend Engineer\nDevSecOps Manager",
                    ThumbnailPath = "",
                    BannerPath = "",
                    BrochurePath = "Prospectus_2026.pdf"
                },
                new DegreeProgram
                {
                    Name = "BS Artificial Intelligence & High-Performance Computing",
                    Code = "BS-AI",
                    DepartmentId = deptCs.Id,
                    Duration = "4 Years (8 Semesters)",
                    Eligibility = "Higher Secondary School Certificate (Pre-Engineering / Computer Science) with minimum 60% marks, or equivalent.",
                    Description = "Combines deep learning models with compute infrastructure. Students build neural network architectures and learn how to optimize training runs across large multi-GPU clusters, design custom vector databases, and manage distributed ML pipelines.",
                    LearningOutcomes = "Design and train custom neural network architectures using PyTorch/TensorFlow\nWrite parallel programs utilizing CUDA and MPI library routines for GPU grids\nStructure scalable data pipelines with vector database optimization",
                    CareerOpportunities = "Machine Learning Architect\nHPC Operations Analyst\nData Platform Engineer\nAI Research Scientist",
                    ThumbnailPath = "",
                    BannerPath = "",
                    BrochurePath = "Prospectus_2026.pdf"
                },
                new DegreeProgram
                {
                    Name = "BS Cyber Security & Smart Systems",
                    Code = "BS-CY",
                    DepartmentId = deptCs.Id,
                    Duration = "4 Years (8 Semesters)",
                    Eligibility = "Higher Secondary School Certificate (Pre-Engineering / Computer Science) with minimum 60% marks, or equivalent.",
                    Description = "Focuses on adversarial security engineering, firmware analysis, smart grid cybersecurity, and distributed hardware security. Students learn penetration testing methodologies, cryptography, threat modeling, and secure code auditing.",
                    LearningOutcomes = "Perform comprehensive threat modeling and vulnerability scans across cloud clusters\nConduct firmware analysis and secure hardware interfaces\nImplement cryptographic key architectures and secure communication lines",
                    CareerOpportunities = "Penetration Tester\nApplication Security Architect\nCryptographic System Engineer\nSmart Systems Consultant",
                    ThumbnailPath = "",
                    BannerPath = "",
                    BrochurePath = "Prospectus_2026.pdf"
                }
            );
            context.SaveChanges();
        }

        // 6. Seed Research Items
        if (!context.ResearchItems.Any())
        {
            context.ResearchItems.AddRange(
                new ResearchItem
                {
                    Category = "Lab",
                    Title = "HPC & GPU Compute Cluster Lab",
                    Description = "Focuses on compiling passes for multi-GPU arrays, optimizing distributed transformer models, and designing custom high-throughput memory pipelines.",
                    ImagePath = "",
                    Link = ""
                },
                new ResearchItem
                {
                    Category = "Lab",
                    Title = "Threat Intelligence & Pen-Testing Lab",
                    Description = "Researches firmware hardening strategies, smart grid vulnerabilities, secure key exchange mechanisms, and automated threat modeling architectures.",
                    ImagePath = "",
                    Link = ""
                },
                new ResearchItem
                {
                    Category = "Lab",
                    Title = "Smart Grid & Decarbonized Infrastructure Lab",
                    Description = "Focuses on structural health simulation models, digital twins of civic resources, and decentralized energy distribution algorithms.",
                    ImagePath = "",
                    Link = ""
                },
                new ResearchItem
                {
                    Category = "Project",
                    Title = "Decentralized Load Balancer (DLB)",
                    Description = "A decentralized system utility engineered to distribute routing workloads across micro-clusters dynamically. Operates with sub-millisecond latencies and low memory footprints.",
                    ImagePath = "",
                    Link = ""
                },
                new ResearchItem
                {
                    Category = "Project",
                    Title = "High-Throughput Training Pipelines",
                    Description = "Researches optimized weights training passes across local GPU arrays, compiling custom memory allocators to prevent parallel processing bottlenecks.",
                    ImagePath = "",
                    Link = ""
                }
            );
            context.SaveChanges();
        }

        // 7. Seed Campus Facilities
        if (!context.CampusFacilities.Any())
        {
            context.CampusFacilities.AddRange(
                new CampusFacility { Title = "Modern Classrooms", Description = "Equipped with high-resolution laser displays, acoustic dampening layouts, and active modular desks for group sprint activities.", IconClass = "fa-solid fa-chalkboard-user" },
                new CampusFacility { Title = "Resource Library", Description = "Features high-speed search terminals, digital journal access databases, private research cabins, and reference textbooks.", IconClass = "fa-solid fa-book-open" },
                new CampusFacility { Title = "Advanced Laboratories", Description = "Smart grid simulators, systems validation testing utilities, and computational physics testing suites.", IconClass = "fa-solid fa-flask" },
                new CampusFacility { Title = "HPC Computer Labs", Description = "Equipped with high-end workstations connected directly to local GPU servers for executing machine learning models.", IconClass = "fa-solid fa-server" },
                new CampusFacility { Title = "Innovation Center", Description = "A startup sandbox area with whiteboard walls, projector sets, and meeting booths designed for project incubation.", IconClass = "fa-solid fa-lightbulb" },
                new CampusFacility { Title = "Auditorium", Description = "A premium, 500-seat theater layout equipped with linear acoustic setups, hosting tech symposiums and orientations.", IconClass = "fa-solid fa-people-roof" },
                new CampusFacility { Title = "Sports Complex", Description = "Includes a modern indoor gym, basketball courts, and outdoor running tracks for wellness activities.", IconClass = "fa-solid fa-volleyball" },
                new CampusFacility { Title = "Cafeteria", Description = "A hygienic dining hub offering diverse cuisines, fast internet access, and comfortable collaboration layouts.", IconClass = "fa-solid fa-utensils" },
                new CampusFacility { Title = "Campus Mosque", Description = "A beautifully designed, spacious prayer area built for spiritual tranquility and daily congregations.", IconClass = "fa-solid fa-mosque" },
                new CampusFacility { Title = "Medical Center", Description = "Offers 24/7 medical response parameters, emergency first aid tools, and wellness diagnosis consults.", IconClass = "fa-solid fa-house-medical" }
            );
            context.SaveChanges();
        }

        // 8. Seed Download Files
        if (!context.DownloadFiles.Any())
        {
            context.DownloadFiles.AddRange(
                new DownloadFile { Title = "University Prospectus 2026", Category = "admissions", FilePath = "Prospectus_2026.pdf", SizeString = "PDF | 4.2 MB", Extension = ".pdf" },
                new DownloadFile { Title = "Admission Guidelines Form", Category = "admissions", FilePath = "Admission_Form.pdf", SizeString = "PDF | 1.1 MB", Extension = ".pdf" },
                new DownloadFile { Title = "Academic Calendar Fall 2026", Category = "academic", FilePath = "Academic_Calendar_Fall2026.pdf", SizeString = "PDF | 0.9 MB", Extension = ".pdf" },
                new DownloadFile { Title = "Research Incubation Guidelines", Category = "policy", FilePath = "Research_Policy_Handbook.pdf", SizeString = "PDF | 2.5 MB", Extension = ".pdf" },
                new DownloadFile { Title = "Student Honor Code & Ethics", Category = "policy", FilePath = "Student_Honor_Code.pdf", SizeString = "PDF | 1.5 MB", Extension = ".pdf" }
            );
            context.SaveChanges();
        }

        // 9. Seed FAQ Items
        if (!context.FAQItems.Any())
        {
            context.FAQItems.AddRange(
                new FAQItem { Category = "General", Question = "How long is the admission processing cycle?", Answer = "The admissions processing cycle generally takes 3 to 4 weeks. Once applications are filed on the online portal, aptitude assessments are conducted, followed by panel interviews for qualifying applicants. Final placement letters are dispatched within 5 days of interviews." },
                new FAQItem { Category = "Academics", Question = "What programming languages are taught in BS Software Engineering?", Answer = "Our curriculum focuses on concepts rather than specific syntaxes. However, students build systems using Rust, C++, C, Go, and Python. We prioritize low-level memory control, network protocol programming, and cloud concurrency." },
                new FAQItem { Category = "General", Question = "Does NED Academy provide credits transfer policies?", Answer = "Yes. Credit transfer applications are reviewed on a case-by-case basis by the Academic Directors Board. Courses completed at HEC-recognized national universities or foreign institutions with a grade of B or above are eligible for review." },
                new FAQItem { Category = "Technical", Question = "Are there separate computing grids for undergraduate research?", Answer = "Yes. Senior-year student groups and research assistants are assigned dedicated compute slices on our high-performance cluster. GPU allocation requests are authorized through dynamic lab supervisors." },
                new FAQItem { Category = "Academics", Question = "What is the student-faculty ratio in computer science streams?", Answer = "We maintain a strict 12:1 student-to-faculty ratio across all computational engineering classes. This ensures that every learner receives individual code reviews and architecture design feedback." }
            );
            context.SaveChanges();
        }

        // 10. Seed News Items
        if (!context.NewsItems.Any())
        {
            context.NewsItems.AddRange(
                new NewsItem
                {
                    Title = "Decentralized Smart Grid Optimizer Launched",
                    Category = "Research",
                    Content = "Our smart systems engineering cohort, under Prof. Dr. Sarosh H. Lodi, has successfully validated prototypes of a decentralized load optimizer. The research will be published in upcoming global engineering journals.",
                    PublishDate = DateTime.Now.AddDays(-1),
                    Status = "Published",
                    FeaturedImagePath = ""
                },
                new NewsItem
                {
                    Title = "CS Department Deploys GPU Training Cluster",
                    Category = "General",
                    Content = "Under Dr. Muhammad Ali Ismail's oversight, a local multi-GPU cluster has been wired up to support student deep learning tasks. The cluster offers parallel acceleration capacities for weights training.",
                    PublishDate = DateTime.Now.AddDays(-5),
                    Status = "Published",
                    FeaturedImagePath = ""
                },
                new NewsItem
                {
                    Title = "Incubation Agreement Finalized with Cloud Partners",
                    Category = "Partnership",
                    Content = "NED Academy has locked in a strategic alliance to provide senior-year students with cloud compute credits and mentorship lines with global solution architects.",
                    PublishDate = DateTime.Now.AddDays(-12),
                    Status = "Published",
                    FeaturedImagePath = ""
                }
            );
            context.SaveChanges();
        }

        // 11. Seed Event Items
        if (!context.EventItems.Any())
        {
            context.EventItems.AddRange(
                new EventItem { Title = "AI & Cloud Architecture Symposium", Date = DateTime.Today.AddDays(25), TimeString = "10:00 AM - 04:00 PM", Venue = "Main Auditorium", BannerImagePath = "" },
                new EventItem { Title = "Fall Semester Orientation 2026", Date = DateTime.Today.AddDays(42), TimeString = "09:30 AM - 12:30 PM", Venue = "Main Campus Plaza", BannerImagePath = "" },
                new EventItem { Title = "Annual Tech & Research Expo", Date = DateTime.Today.AddDays(64), TimeString = "09:00 AM - 05:00 PM", Venue = "CS Innovation Wing", BannerImagePath = "" }
            );
            context.SaveChanges();
        }

        // 12. Seed Enterprise Admission Sessions & Offered Programs
        if (!context.AdmissionSessions.Any())
        {
            var session = new AdmissionSession
            {
                Title = "Fall 2026 Admissions",
                AcademicYear = "2026-2027",
                Term = "Fall",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(4),
                ApplicationDeadline = DateTime.UtcNow.AddMonths(2),
                Status = AdmissionSessionStatus.OpenForApplications,
                MaxCapacity = 1500,
                ProcessingFeeAmount = 2500.00m,
                CreatedAt = DateTime.UtcNow
            };
            context.AdmissionSessions.Add(session);
            context.SaveChanges();

            var programs = context.DegreePrograms.ToList();
            foreach (var prog in programs)
            {
                context.AdmissionSessionPrograms.Add(new AdmissionSessionProgram
                {
                    AdmissionSessionId = session.Id,
                    DegreeProgramId = prog.Id,
                    CapacitySeats = 60,
                    AvailableSeats = 60,
                    MinimumEligibilityMarksPercentage = 50.0,
                    IsActive = true
                });
            }
            context.SaveChanges();
        }

        // 13. Seed Demo Applicant Account
        if (!context.Applicants.Any(a => a.Email == "applicant@ned.edu.pk"))
        {
            string salt = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(16));
            byte[] bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes("Applicant123!" + salt));
            string pwdHash = Convert.ToHexString(bytes);

            var demoApplicant = new Applicant
            {
                FullName = "Demo Applicant",
                Email = "applicant@ned.edu.pk",
                PasswordSalt = salt,
                PasswordHash = pwdHash,
                PhoneNumber = "+92 3001234567",
                CNIC_Passport = "42101-1234567-1",
                City = "Karachi",
                Address = "NED University Campus, Karachi",
                AccountStatus = ApplicantAccountStatus.Active,
                IsEmailVerified = true,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };
            context.Applicants.Add(demoApplicant);
            context.SaveChanges();

            context.ApplicantProfiles.Add(new ApplicantProfile
            {
                ApplicantId = demoApplicant.Id,
                Nationality = "Pakistani",
                Gender = ApplicantGender.Male,
                City = "Karachi",
                Address = "NED University Campus, Karachi",
                Country = "Pakistan",
                ProfileCompletionPercentage = 30,
                LastUpdatedAt = DateTime.UtcNow
            });
            context.SaveChanges();
        }

        // 14. Seed Default Scholarship Rules (CMS Manageable Defaults)
        if (!context.ScholarshipRules.Any())
        {
            context.ScholarshipRules.AddRange(
                new ScholarshipRule { ScholarshipName = "Merit Scholarship", Description = "Awarded to top test scorers with 90%+ calculated score", MinimumPercentage = 90.0, ScholarshipPercentage = 100.0, IsEnabled = true },
                new ScholarshipRule { ScholarshipName = "Academic Excellence Scholarship", Description = "Awarded to high achievers with 80%+ score", MinimumPercentage = 80.0, ScholarshipPercentage = 60.0, IsEnabled = true },
                new ScholarshipRule { ScholarshipName = "University Scholarship", Description = "Default university scholarship for 75%+ score", MinimumPercentage = 75.0, ScholarshipPercentage = 40.0, IsEnabled = true }
            );
            context.SaveChanges();
        }

        // 15. Seed Fee Structure Configuration (CMS Manageable Defaults)
        if (!context.FeeStructureConfigs.Any())
        {
            context.FeeStructureConfigs.Add(new FeeStructureConfig
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
            });
            context.SaveChanges();
        }
    }
}
