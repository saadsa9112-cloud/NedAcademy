using System.ComponentModel.DataAnnotations;

namespace NedAcademyWebsite.Models;

public class SectionSetting
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string SectionKey { get; set; } = string.Empty; // Hero, Programs, Research, Facilities, News, Gallery, Testimonials, Partners, CTA

    [Required]
    [StringLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    public bool IsVisible { get; set; } = true;
    public int SortOrder { get; set; } = 0;

    [StringLength(50)]
    public string BackgroundStyle { get; set; } = "Dark"; // Dark, Glass, Light, Gradient

    [StringLength(50)]
    public string PaddingStyle { get; set; } = "py-5";
}
