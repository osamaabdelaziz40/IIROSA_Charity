using FluentValidation;
using IIROSA.Application.DTOs.Reports;

namespace IIROSA.Application.Validators.Reports;

/// <summary>
/// UC-RPT-07/08 — page bounds for the read variant; extract rules for the extract variant
/// (<c>ReportNo &gt; 0</c> when requested; <c>DateTo ≥ DateFrom</c> when both set). No
/// DB-dependent checks here — the charity narrow is authorised in the service.
/// </summary>
public class MezaCardsFilterValidator : AbstractValidator<MezaCardsFilterDto>
{
    public MezaCardsFilterValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be at least 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 200).WithMessage("PageSize must be between 1 and 200");

        // §23.U.8 extract keys — ReportNo is the extract discriminator and must be positive.
        RuleFor(x => x.ReportNo)
            .GreaterThan(0).WithMessage("ReportNo must be a positive number")
            .When(x => x.ReportNo.HasValue);

        RuleFor(x => x.DateTo)
            .GreaterThanOrEqualTo(x => x.DateFrom.Value)
            .WithMessage("DateTo must be on or after DateFrom")
            .When(x => x.DateFrom.HasValue && x.DateTo.HasValue);
    }
}
