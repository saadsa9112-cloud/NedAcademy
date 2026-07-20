using System.ComponentModel.DataAnnotations;

namespace NedAcademyWebsite.Models;

public class PageSection
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string PageKey { get; set; } = "Home"; // Home, About, Admissions, Programs, Research, Faculty, Facilities, Gallery, News, Contact

    [Required]
    [StringLength(50)]
    public string SectionKey { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string SectionTitle { get; set; } = string.Empty;

    public string SectionSubtitle { get; set; } = string.Empty;

    public bool IsVisible { get; set; } = true;
    public int SortOrder { get; set; } = 0;

    [StringLength(50)]
    public string BackgroundStyle { get; set; } = "Dark"; // Dark, Glass, Light, Gradient

    [StringLength(50)]
    public string ContainerWidth { get; set; } = "Container"; // Container, FullWidth

    [StringLength(50)]
    public string PaddingStyle { get; set; } = "py-5";

    [StringLength(50)]
    public string AnimationType { get; set; } = "reveal-up";
}
