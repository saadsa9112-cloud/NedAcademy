using System.ComponentModel.DataAnnotations;

namespace NedAcademyWebsite.Models;

public class FAQItem
{
    public int Id { get; set; }

    [Required]
    [StringLength(255)]
    public string Question { get; set; } = string.Empty;

    [Required]
    public string Answer { get; set; } = string.Empty;

    [StringLength(50)]
    public string Category { get; set; } = "General"; // Admissions, Academics, General, Technical
}
