using System.ComponentModel.DataAnnotations;

namespace NedAcademyWebsite.Models;

public class CustomForm
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string FormName { get; set; } = string.Empty; // e.g. Admissions Application Form

    [Required]
    [StringLength(50)]
    public string FormKey { get; set; } = "admissions"; // admissions, inquiry, feedback

    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public List<FormField> Fields { get; set; } = new();
}

public class FormField
{
    public int Id { get; set; }

    public int CustomFormId { get; set; }
    public CustomForm? CustomForm { get; set; }

    [Required]
    [StringLength(100)]
    public string FieldName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string FieldLabel { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string FieldType { get; set; } = "Text"; // Text, Number, Email, Dropdown, FileUpload, Checkbox, Date, Textarea

    public string Placeholder { get; set; } = string.Empty;

    public string OptionsCsv { get; set; } = string.Empty; // For Dropdowns or Radio options

    public bool IsRequired { get; set; } = false;

    [StringLength(100)]
    public string ValidationRegex { get; set; } = string.Empty;

    public int SortOrder { get; set; } = 0;
}
