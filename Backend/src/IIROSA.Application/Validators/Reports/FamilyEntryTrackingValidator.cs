using FluentValidation;

using IIROSA.Application.DTOs.Reports;

namespace IIROSA.Application.Validators.Reports;

/// <summary>
/// UC-RPT-13 (§23.S.10 متابعة إدخلات الأسر والأيتام) — the mode whitelist and page bounds
/// only: both filter keys are optional and the date needs no rule beyond optionality.
/// </summary>
public class FamilyEntryTrackingValidator : AbstractValidator<FamilyEntryTrackingFilterDto>
{
    private static readonly string[] ValidModes = { "totals", "details" };

    public FamilyEntryTrackingValidator()
    {
        RuleFor(x => x.Mode)
            .Must(mode => ValidModes.Contains(mode, StringComparer.OrdinalIgnoreCase))
                .WithMessage("Mode must be either 'totals' or 'details'");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
                .WithMessage("Page must be at least 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 200)
                .WithMessage("PageSize must be between 1 and 200");
    }
}
