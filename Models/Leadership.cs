using System.ComponentModel.DataAnnotations;

namespace NedAcademyWebsite.Models;

public class Leadership
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Designation { get; set; } = string.Empty; // e.g. Founder, Vice Chancellor

    public string Biography { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Qualification { get; set; } = string.Empty;

    [StringLength(255)]
    public string PhotoPath { get; set; } = string.Empty; // Initials or image url

    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    public int Rank { get; set; } = 10; // For ordering layout presentation
}
