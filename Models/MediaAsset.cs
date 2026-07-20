using System.ComponentModel.DataAnnotations;

namespace NedAcademyWebsite.Models;

public class MediaAsset
{
    public int Id { get; set; }

    [Required]
    [StringLength(255)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string FilePath { get; set; } = string.Empty; // e.g. /uploads/hero_banner.jpg

    [Required]
    [StringLength(50)]
    public string FileType { get; set; } = "Image"; // Image, Video, PDF, Document

    [Required]
    [StringLength(50)]
    public string Category { get; set; } = "General"; // Gallery, Hero, Faculty, Programs, Logos, Brochures, General

    public long SizeBytes { get; set; }

    public DateTime UploadDate { get; set; } = DateTime.Now;

    public bool IsFeatured { get; set; } = false;

    public int DisplayOrder { get; set; } = 0;

    public bool IsDeleted { get; set; } = false;

    public DateTime? DeletedAt { get; set; }
}
