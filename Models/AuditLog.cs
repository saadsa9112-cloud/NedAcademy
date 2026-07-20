using System.ComponentModel.DataAnnotations;

namespace NedAcademyWebsite.Models;

public class AuditLog
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Action { get; set; } = "Update"; // Create, Update, Delete, Login, SettingChange

    [Required]
    [StringLength(100)]
    public string EntityName { get; set; } = string.Empty;

    public int? EntityId { get; set; }

    [Required]
    public string Details { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; } = DateTime.Now;

    [Required]
    [StringLength(100)]
    public string PerformedBy { get; set; } = "admin";
}
