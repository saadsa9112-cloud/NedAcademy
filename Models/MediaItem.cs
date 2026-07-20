using System.ComponentModel.DataAnnotations;

namespace NedAcademyWebsite.Models;

public class MediaItem
{
    public int Id { get; set; }

    [Required]
    [StringLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string FilePath { get; set; } = string.Empty; // Location on disk under wwwroot/uploads/

    [Required]
    [StringLength(50)]
    public string FileType { get; set; } = "Image"; // Image, Video, PDF, Document

    public long SizeBytes { get; set; }

    public DateTime UploadDate { get; set; } = DateTime.Now;
}
