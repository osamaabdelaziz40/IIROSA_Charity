using FluentValidation;
using IIROSA.Application.DTOs.Reports;

namespace IIROSA.Application.Validators.Reports;

/// <summary>
/// UC-RPT-21 (§23.U.21) — the failures this read can produce: a missing or implausible
/// update-start date (من فضلك ادخل تاريخ بدا التحديث is the screen's one required field),
/// or out-of-range paging. Invoked with ValidateAndThrowAsync in ReportService
/// (service-layer rule).
/// </summary>
public class FamilyUpdateTrackingValidator : AbstractValidator<FamilyUpdateTrackingFilterDto>
{
    public FamilyUpdateTrackingValidator()
    {
        RuleFor(r => r.Date)
            .NotEqual(default(DateTime))
            .WithMessage("Date is required")
            .Must(d => d.Year >= 2000 && d <= DateTime.UtcNow.AddYears(1))
            .WithMessage("Date is outside the sane range");

        RuleFor(r => r.BatchNo)
            .MaximumLength(50);

        RuleFor(r => r.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(r => r.PageSize)
            .InclusiveBetween(1, 500);
    }
}
