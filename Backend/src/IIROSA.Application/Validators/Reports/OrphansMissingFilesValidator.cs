using FluentValidation;

using IIROSA.Application.DTOs.Reports;

namespace IIROSA.Application.Validators.Reports;

/// <summary>
/// UC-RPT-16 (§23.S.15 أيتام مطلوب لهم ملفات) — paging bounds only: the charity key is the
/// single optional filter; the missing-files rule is fixed server-side.
/// </summary>
public class OrphansMissingFilesValidator : AbstractValidator<OrphansMissingFilesFilterDto>
{
    public OrphansMissingFilesValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
                .WithMessage("Page must be at least 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 200)
                .WithMessage("PageSize must be between 1 and 200");
    }
}
