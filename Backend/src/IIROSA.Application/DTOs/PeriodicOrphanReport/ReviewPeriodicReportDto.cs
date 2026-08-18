using System.ComponentModel.DataAnnotations;

namespace IIROSA.Application.DTOs.PeriodicOrphanReport;

/// <summary>
/// DTO for reviewing a Periodic Orphan Report - Implements UC-6.13
/// </summary>
public class ReviewPeriodicReportDto
{
    [Required(ErrorMessage = "Report ID is required")]
    public Guid ReportId { get; set; }

    /// <summary>
    /// Approve (true) or Reject (false) - UC-6.13
    /// </summary>
    [Required(ErrorMessage = "Approval decision is required")]
    public bool IsApproved { get; set; }

    /// <summary>
    /// Refuse reason (required when rejecting) - UC-6.13
    /// </summary>
    [RequiredWhen(nameof(IsApproved), false, ErrorMessage = "Refuse reason is required when rejecting")]
    [StringLength(500, ErrorMessage = "Refuse reason cannot exceed 500 characters")]
    public string? RefuseReason { get; set; }

    /// <summary>
    /// Refuse reason ID (optional lookup) - UC-6.13
    /// </summary>
    public int? RefuseReasonId { get; set; }

    /// <summary>
    /// Optional review comments
    /// </summary>
    [StringLength(1000, ErrorMessage = "Review comments cannot exceed 1000 characters")]
    public string? ReviewComments { get; set; }
}

/// <summary>
/// Custom validation attribute for conditional required fields
/// </summary>
public class RequiredWhenAttribute : ValidationAttribute
{
    private readonly string _otherProperty;
    private readonly object _desiredValue;

    public RequiredWhenAttribute(string otherProperty, object desiredValue)
    {
        _otherProperty = otherProperty;
        _desiredValue = desiredValue;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var property = validationContext.ObjectType.GetProperty(_otherProperty);
        if (property == null)
            return new ValidationResult($"Unknown property: {_otherProperty}");

        var otherValue = property.GetValue(validationContext.ObjectInstance);
        if (otherValue != null && otherValue.Equals(_desiredValue))
        {
            if (value == null)
                return new ValidationResult(ErrorMessage);
        }

        return ValidationResult.Success;
    }
}
