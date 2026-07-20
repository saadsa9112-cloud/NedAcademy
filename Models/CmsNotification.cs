using System.ComponentModel.DataAnnotations;

namespace NedAcademyWebsite.Models;

public class CmsNotification
{
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Message { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Type { get; set; } = "Info"; // Info, Warning, Success, Error

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public bool IsRead { get; set; } = false;

    [StringLength(255)]
    public string TargetUrl { get; set; } = string.Empty;
}
