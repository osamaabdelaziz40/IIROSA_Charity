using FluentValidation;

using IIROSA.Application.DTOs.Reports;

namespace IIROSA.Application.Validators.Reports;

/// <summary>
/// UC-RPT-15 drill-down (§23.S.14 ExtractDetails). Review P17 2026-08-26: this endpoint
/// previously bypassed FluentValidation — page bounds were hand-clamped in the controller
/// (Math.Max) and an empty charityId was only caught deep in the service. Registered
/// automatically via AddValidatorsFromAssembly (IIROSA.Application); invoked in the
/// service layer like every sibling report filter.
/// </summary>
public class OrphansMissingReportsDetailValidator : AbstractValidator<OrphansMissingReportsDetailRequestDto>
{
    public OrphansMissingReportsDetailValidator()
    {
        RuleFor(x => x.CharityId)
            .NotEmpty().WithMessage("charityId must be a real charity identifier (it was missing or empty)");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("page is 1-based");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("pageSize must be between 1 and 100");
    }
}
