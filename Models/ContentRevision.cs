using System.ComponentModel.DataAnnotations;

namespace NedAcademyWebsite.Models;

public class ContentRevision
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string EntityName { get; set; } = string.Empty; // DegreeProgram, HeroSection, PageSection, NewsItem

    public int EntityId { get; set; }

    public int RevisionNumber { get; set; } = 1;

    [Required]
    public string ContentJson { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [StringLength(100)]
    public string CreatedBy { get; set; } = "Admin";

    public string ChangeNotes { get; set; } = string.Empty;
}
