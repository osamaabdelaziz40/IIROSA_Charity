using FluentValidation;
using IIROSA.Application.DTOs.Reports;

namespace IIROSA.Application.Validators.Reports;

/// <summary>
/// UC-ORR-14 / UC-RPT-19 — the only failures this read can produce: an unset or inverted
/// window in window mode, an over-long batch number, or an out-of-range page. Invoked with
/// ValidateAndThrowAsync in ReportService (service-layer rule).
/// </summary>
public class NonRenewedReportsRequestValidator : AbstractValidator<NonRenewedReportsRequestDto>
{
    public NonRenewedReportsRequestValidator()
    {
        // Batch mode (18-19): a batchId sent, or no window at all — an absent batch means
        // the current one (the latest payment by GroupDate), and the dates are ignored.
        RuleFor(r => r.BatchId)
            .MaximumLength(50);

        // Window mode (§14.U.14): the window is the chase-list semantic — both ends required.
        When(r => string.IsNullOrWhiteSpace(r.BatchId)
               && (r.DateFrom != default(DateTime) || r.DateTo != default(DateTime)), () =>
        {
            RuleFor(r => r.DateFrom)
                .NotEqual(default(DateTime))
                .WithMessage("DateFrom is required");

            RuleFor(r => r.DateTo)
                .NotEqual(default(DateTime))
                .WithMessage("DateTo is required");

            RuleFor(r => r)
                .Must(r => r.DateTo.Date >= r.DateFrom.Date)
                .WithMessage("DateTo must be greater than or equal to DateFrom")
                .WithName("DateTo");
        });

        RuleFor(r => r.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(r => r.PageSize)
            .InclusiveBetween(1, 100);
    }
}
