using System.ComponentModel.DataAnnotations;

namespace NedAcademyWebsite.Models;

public class NavMenuItem
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string MenuName { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Url { get; set; } = "#";

    [StringLength(50)]
    public string Icon { get; set; } = "fa-link";

    public int? ParentId { get; set; }
    public NavMenuItem? Parent { get; set; }

    public int SortOrder { get; set; } = 0;
    public bool IsVisible { get; set; } = true;
    public bool IsExternal { get; set; } = false;
}
