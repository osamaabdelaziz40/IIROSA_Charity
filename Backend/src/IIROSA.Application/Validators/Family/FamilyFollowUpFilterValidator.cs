using FluentValidation;
using IIROSA.Application.DTOs.Family;

namespace IIROSA.Application.Validators.Family;

/// <summary>
/// UC-FAM-11 report-date validation — the follow-up report runs on one concrete day that has
/// already happened; a missing or future date is refused.
/// </summary>
public class FamilyFollowUpFilterValidator : AbstractValidator<FamilyFollowUpFilterDto>
{
    public FamilyFollowUpFilterValidator()
    {
        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("A report date is required")
            .Must(date => date.Date <= DateTime.UtcNow.Date)
            .WithMessage("The report date cannot be in the future");
    }
}
