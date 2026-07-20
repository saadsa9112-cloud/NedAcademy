using System.ComponentModel.DataAnnotations;

namespace NedAcademyWebsite.Models;

public class CampusFacility
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty; // e.g. Library, Auditorium

    public string Description { get; set; } = string.Empty;

    [StringLength(100)]
    public string IconClass { get; set; } = "fa-solid fa-building"; // FontAwesome icon class

    [StringLength(255)]
    public string ImagePath { get; set; } = string.Empty;
}
