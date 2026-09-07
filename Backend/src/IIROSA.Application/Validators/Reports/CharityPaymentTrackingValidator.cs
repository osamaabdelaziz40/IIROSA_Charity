using FluentValidation;
using IIROSA.Application.DTOs.Reports;

namespace IIROSA.Application.Validators.Reports;

/// <summary>
/// UC-RPT-20 (§23.S.12) — the only failures this read can produce: out-of-range paging,
/// an over-long batch number, or an implausible update-start date (the date is 18-21's
/// print input, so it is range-checked but never required). Invoked with
/// ValidateAndThrowAsync in ReportService (service-layer rule).
/// </summary>
public class CharityPaymentTrackingValidator : AbstractValidator<CharityPaymentTrackingFilterDto>
{
    public CharityPaymentTrackingValidator()
    {
        RuleFor(r => r.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(r => r.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(r => r.BatchId)
            .MaximumLength(50);

        RuleFor(r => r.DateOfStartingUpdate)
            .Must(d => d == null || (d.Value.Year >= 2000 && d.Value <= DateTime.UtcNow.AddYears(1)))
            .WithMessage("DateOfStartingUpdate is outside the sane range");
    }
}
