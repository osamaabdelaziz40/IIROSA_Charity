using FluentValidation;
using IIROSA.Application.DTOs.Reports;

namespace IIROSA.Application.Validators.Reports;

/// <summary>
/// UC-RPT-11 — page bounds only; the charity narrow is optional (HQ may ask for all) and is
/// authorised in the service, never here. Nothing else is filterable per §23.S.7.
/// </summary>
public class FamilyProjectsFilterValidator : AbstractValidator<FamilyProjectsFilterDto>
{
    public FamilyProjectsFilterValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be at least 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 200).WithMessage("PageSize must be between 1 and 200");
    }
}
