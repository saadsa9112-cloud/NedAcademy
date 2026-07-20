using System.ComponentModel.DataAnnotations;

namespace NedAcademyWebsite.Models;

public class Faculty
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(255)]
    public string PhotoPath { get; set; } = string.Empty; // Initials or Image Path

    [Required]
    [StringLength(150)]
    public string Qualification { get; set; } = string.Empty; // e.g. Ph.D.

    [Required]
    [StringLength(100)]
    public string Designation { get; set; } = string.Empty; // e.g. Professor, Dean

    public int DepartmentId { get; set; }
    public Department? Department { get; set; }

    public string Biography { get; set; } = string.Empty;
    public string ResearchAreas { get; set; } = string.Empty; // Stored as plain text or comma-separated

    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
}
