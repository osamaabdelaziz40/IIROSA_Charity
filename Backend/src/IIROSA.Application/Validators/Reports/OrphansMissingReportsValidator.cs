using FluentValidation;

using IIROSA.Application.DTOs.Reports;

namespace IIROSA.Application.Validators.Reports;

/// <summary>
/// UC-RPT-15 (§23.S.14 أيتام مكودون مطلوب لهم تقرير) — paging bounds only: the charity key
/// is the single optional filter; the chase window is fixed server-side (trailing 12 months).
/// </summary>
public class OrphansMissingReportsValidator : AbstractValidator<OrphansMissingReportsFilterDto>
{
    public OrphansMissingReportsValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
                .WithMessage("Page must be at least 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 200)
                .WithMessage("PageSize must be between 1 and 200");
    }
}
