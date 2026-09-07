using FluentValidation;
using IIROSA.Application.DTOs.Reports;

namespace IIROSA.Application.Validators.Reports;

/// <summary>
/// UC-RPT-04 / UC-RPT-05 — page-bounds validation for the shared orphan-status filter
/// (the reports' only failure class; the charity scope resolves server-side).
/// </summary>
public class OrphanStatusReportFilterValidator : AbstractValidator<OrphanStatusReportFilterDto>
{
    public OrphanStatusReportFilterValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be at least 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 200).WithMessage("PageSize must be between 1 and 200");
    }
}
