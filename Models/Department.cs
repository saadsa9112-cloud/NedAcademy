using System.ComponentModel.DataAnnotations;

namespace NedAcademyWebsite.Models;

public class Department
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    public string Code { get; set; } = string.Empty; // e.g., CS-AI, CIVIL-INFRA

    public ICollection<Faculty> FacultyMembers { get; set; } = new List<Faculty>();
    public ICollection<DegreeProgram> Programs { get; set; } = new List<DegreeProgram>();
}
