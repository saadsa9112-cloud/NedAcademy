using System.ComponentModel.DataAnnotations;

namespace NedAcademyWebsite.Models;

public class BackupRecord
{
    public int Id { get; set; }

    [Required]
    [StringLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string BackupType { get; set; } = "Database"; // Database, MediaAssets, FullSystem

    public long SizeBytes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Required]
    [StringLength(255)]
    public string FilePath { get; set; } = string.Empty;
}
