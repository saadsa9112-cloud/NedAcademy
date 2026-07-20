using System.ComponentModel.DataAnnotations;

namespace NedAcademyWebsite.Models;

public class EventItem
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime Date { get; set; } = DateTime.Today.AddDays(7);

    [Required]
    [StringLength(50)]
    public string TimeString { get; set; } = "10:00 AM - 04:00 PM";

    [Required]
    [StringLength(150)]
    public string Venue { get; set; } = "Main Auditorium";

    [StringLength(255)]
    public string BannerImagePath { get; set; } = string.Empty;
}
