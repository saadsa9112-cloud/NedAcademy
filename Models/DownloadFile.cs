using System.ComponentModel.DataAnnotations;

namespace NedAcademyWebsite.Models;

public class DownloadFile
{
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Category { get; set; } = "admissions"; // admissions, academic, policy

    [Required]
    [StringLength(255)]
    public string FilePath { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string SizeString { get; set; } = "PDF | 1.0 MB";

    [Required]
    [StringLength(10)]
    public string Extension { get; set; } = ".pdf";
}
