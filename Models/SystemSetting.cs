using System.ComponentModel.DataAnnotations;

namespace NedAcademyWebsite.Models;

public class SystemSetting
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string UniversityName { get; set; } = "NED Academy";

    [Required]
    [StringLength(150)]
    public string Slogan { get; set; } = "Pioneering the Next Frontier of Learning & Technology";

    [StringLength(255)]
    public string LogoPath { get; set; } = string.Empty;

    [StringLength(255)]
    public string LogoDarkPath { get; set; } = string.Empty;

    [StringLength(255)]
    public string FaviconPath { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string ContactEmail { get; set; } = "info@nedacademy.edu";

    [Required]
    [StringLength(50)]
    public string ContactPhone { get; set; } = "+92-21-99261261";

    [Required]
    [StringLength(255)]
    public string Address { get; set; } = "NED Academy, NED University of Engineering & Technology, Main University Road, Karachi - 75270, Pakistan.";

    public double MapsLatitude { get; set; } = 24.9317;
    public double MapsLongitude { get; set; } = 67.1136;

    [StringLength(255)]
    public string SocialFacebook { get; set; } = "#";

    [StringLength(255)]
    public string SocialX { get; set; } = "#";

    [StringLength(255)]
    public string SocialLinkedIn { get; set; } = "#";

    [StringLength(255)]
    public string SocialYouTube { get; set; } = "#";

    // Hero Section CMS
    [Required]
    [StringLength(150)]
    public string HeroTitle { get; set; } = "Pioneering the Next Frontier of Learning & Technology.";

    [Required]
    public string HeroSubtitle { get; set; } = "NED Academy represents excellence, industry-integrated paths, and cutting-edge research. We train creators, architects, and visionaries to lead global technical revolutions.";

    [StringLength(255)]
    public string HeroVideoPath { get; set; } = string.Empty;

    [StringLength(255)]
    public string HeroImagePath { get; set; } = string.Empty;

    // About Sections CMS
    public string AboutManifesto { get; set; } = string.Empty;
    public string AboutVision { get; set; } = string.Empty;
    public string AboutMission { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string FooterCopyrightText { get; set; } = "NED Academy. All rights reserved.";
}
