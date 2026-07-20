using System.ComponentModel.DataAnnotations;

namespace NedAcademyWebsite.Models;

public class DegreeProgram
{
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Code { get; set; } = "BS-SE";

    [StringLength(150)]
    public string Slug { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string DegreeLevel { get; set; } = "Undergraduate"; // Undergraduate, Postgraduate, Certification

    public int DepartmentId { get; set; }
    public Department? Department { get; set; }

    [Required]
    [StringLength(50)]
    public string Duration { get; set; } = "4 Years (8 Semesters)";

    [Required]
    public string Eligibility { get; set; } = string.Empty;

    public string Overview { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public string LearningOutcomes { get; set; } = string.Empty;
    public string CareerOpportunities { get; set; } = string.Empty;

    [StringLength(255)]
    public string CurriculumPdfPath { get; set; } = string.Empty;

    [StringLength(255)]
    public string ThumbnailPath { get; set; } = string.Empty;

    [StringLength(255)]
    public string BannerPath { get; set; } = string.Empty;

    [StringLength(255)]
    public string BrochurePath { get; set; } = string.Empty;

    public int DisplayOrder { get; set; } = 0;
    public bool IsFeatured { get; set; } = true;
    public bool IsPublished { get; set; } = true;

    [StringLength(150)]
    public string MetaTitle { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
}
