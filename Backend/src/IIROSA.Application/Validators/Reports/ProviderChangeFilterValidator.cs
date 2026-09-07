using FluentValidation;

using IIROSA.Application.DTOs.Reports;

namespace IIROSA.Application.Validators.Reports;

/// <summary>
/// UC-RPT-12 (§23.S.19 تقارير تعديل المعيل) — paging bounds only: the charity key is the
/// single optional filter and nothing else is filterable on this screen.
/// </summary>
public class ProviderChangeFilterValidator : AbstractValidator<ProviderChangeFilterDto>
{
    public ProviderChangeFilterValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
                .WithMessage("Page must be at least 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 200)
                .WithMessage("PageSize must be between 1 and 200");
    }
}
