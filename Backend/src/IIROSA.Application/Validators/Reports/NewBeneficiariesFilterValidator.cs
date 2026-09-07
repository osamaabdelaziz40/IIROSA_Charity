using FluentValidation;

using IIROSA.Application.DTOs.Reports;

namespace IIROSA.Application.Validators.Reports;

/// <summary>
/// UC-RPT-35 (§23.U.35 الأيتام والأرامل الجدد) — the registration window's start is
/// mandatory, the variant must be one of the four collapsed legacy reports, and the page
/// bounds mirror the epic's other report validators.
/// </summary>
public class NewBeneficiariesFilterValidator : AbstractValidator<NewBeneficiariesFilterDto>
{
    private static readonly string[] Variants = { "orphans", "orphansv2", "widows", "widowsbyfamily" };

    public NewBeneficiariesFilterValidator()
    {
        RuleFor(x => x.DateFrom)
            .NotEmpty()
            .WithMessage("Registration date from is required");

        RuleFor(x => x.DateTo)
            .GreaterThanOrEqualTo(x => x.DateFrom)
            .When(x => x.DateTo.HasValue)
            .WithMessage("Registration date to must not be before the from date");

        RuleFor(x => x.Variant)
            .Must(v => !string.IsNullOrWhiteSpace(v)
                && Variants.Contains(v.ToLowerInvariant()))
            .WithMessage("Variant must be one of: orphans, orphansV2, widows, widowsByFamily");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);
    }
}
