using FluentValidation;
using IIROSA.Application.DTOs.Reports;

namespace IIROSA.Application.Validators.Reports;

/// <summary>
/// UC-RPT-03 — page-bounds validation for the excluded-orphans filter
/// (the report's only failure class; the charity scope resolves server-side).
/// </summary>
public class ExcludedOrphansFilterValidator : AbstractValidator<ExcludedOrphansFilterDto>
{
    public ExcludedOrphansFilterValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be at least 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 200).WithMessage("PageSize must be between 1 and 200");
    }
}
