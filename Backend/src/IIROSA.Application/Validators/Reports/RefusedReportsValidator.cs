using FluentValidation;

using IIROSA.Application.DTOs.Reports;

namespace IIROSA.Application.Validators.Reports;

/// <summary>
/// UC-RPT-18 (§23.S.17 تقارير تم رفضها) — paging bounds only: the charity key is the
/// single optional filter; the refused state is fixed server-side.
/// </summary>
public class RefusedReportsValidator : AbstractValidator<RefusedReportsFilterDto>
{
    public RefusedReportsValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
                .WithMessage("Page must be at least 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 200)
                .WithMessage("PageSize must be between 1 and 200");
    }
}
