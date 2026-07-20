using System.ComponentModel.DataAnnotations;

namespace NedAcademyWebsite.Models;

public class ResearchItem
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Category { get; set; } = "Lab"; // Lab, Project, Partner

    [Required]
    [StringLength(150)]
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [StringLength(255)]
    public string ImagePath { get; set; } = string.Empty;

    [StringLength(255)]
    public string Link { get; set; } = string.Empty; // External project link
}
