using System.ComponentModel.DataAnnotations;

namespace NedAcademyWebsite.Models;

public class ContactMessage
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    public string Message { get; set; } = string.Empty;

    public DateTime ReceivedDate { get; set; } = DateTime.Now;

    public bool IsRead { get; set; } = false;

    public bool IsArchived { get; set; } = false;
}
