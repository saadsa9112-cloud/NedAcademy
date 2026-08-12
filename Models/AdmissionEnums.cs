namespace NedAcademyWebsite.Models;

public enum DegreeLevel
{
    Certificate = 1,
    Diploma = 2,
    AssociateDegree = 3,
    Bachelor = 4,
    Master = 5,
    MPhil = 6,
    PhD = 7,
    ShortCourse = 8
}

public enum AdmissionSessionStatus
{
    Draft = 1,
    Upcoming = 2,
    OpenForApplications = 3,
    QuotaReview = 4,
    MeritEvaluation = 5,
    Closed = 6,
    Archived = 7
}

public enum ApplicationStatus
{
    Draft = 1,
    Submitted = 2,
    UnderReview = 3,
    DocumentsPending = 4,
    DocumentsVerified = 5,
    InterviewScheduled = 6,
    InterviewCompleted = 7,
    MeritWaiting = 8,
    Selected = 9,
    ConditionalOffer = 10,
    FeePending = 11,
    FeeVerified = 12,
    Approved = 13,
    Rejected = 14,
    Withdrawn = 15,
    Cancelled = 16,
    Archived = 17,
    RequiresRevision = 18,
    TestScheduled = 19,
    TestQualified = 20,
    TestNotQualified = 21
}

public enum DocumentType
{
    CNIC_BForm = 1,
    Passport = 2,
    MatricMarkSheet = 3,
    IntermediateMarkSheet = 4,
    BachelorsDegree = 5,
    MastersDegree = 6,
    EquivalenceCertificate = 7,
    Domicile = 8,
    NOC = 9,
    EntryTestScoreCard = 10,
    PassportPhoto = 11,
    ExperienceCertificate = 12,
    FatherCNIC = 13,
    MotherCNIC = 14,
    GuardianCNIC = 15,
    CharacterCertificate = 16,
    MigrationCertificate = 17,
    Transcript = 18,
    Other = 99
}

public enum DocumentVerificationStatus
{
    Pending = 1,
    Verified = 2,
    Rejected = 3,
    ReuploadRequired = 4,
    NeedReupload = 4,
    Missing = 5
}

public enum VoucherStatus
{
    Generated = 1,
    Pending = 2,
    Paid = 3,
    Expired = 4,
    Cancelled = 5,
    Reactivated = 6
}

public enum PaymentStatus
{
    Unpaid = 1,
    PendingVerification = 2,
    Paid = 3,
    Failed = 4,
    Refunded = 5,
    Waived = 6,
    Expired = 7,
    Cancelled = 8,
    Reactivated = 9
}

public enum ApplicantGender
{
    Male = 1,
    Female = 2,
    Other = 3
}

public enum ApplicantAccountStatus
{
    PendingVerification = 1,
    Active = 2,
    Suspended = 3,
    Blocked = 4,
    Inactive = 5,
    Rejected = 6,
    Deleted = 7
}

