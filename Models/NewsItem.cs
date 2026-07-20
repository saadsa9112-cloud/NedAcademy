using System.ComponentModel.DataAnnotations;

namespace NedAcademyWebsite.Models;

public class NewsItem
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Category { get; set; } = "Research"; // Research, Hackathon, Partnership, General

    [Required]
    public string Content { get; set; } = string.Empty;

    public DateTime PublishDate { get; set; } = DateTime.Now;

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Published"; // Published, Draft, Scheduled, Archived

    [StringLength(255)]
    public string FeaturedImagePath { get; set; } = string.Empty;

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
}
