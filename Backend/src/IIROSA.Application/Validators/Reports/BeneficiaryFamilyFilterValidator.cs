using FluentValidation;
using IIROSA.Application.DTOs.Reports;

namespace IIROSA.Application.Validators.Reports;

/// <summary>
/// UC-RPT-09 — page bounds only; the charity narrow is optional (HQ may ask for all charities)
/// and is authorised in the service, never here.
/// </summary>
public class BeneficiaryFamilyFilterValidator : AbstractValidator<BeneficiaryFamilyFilterDto>
{
    public BeneficiaryFamilyFilterValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be at least 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 200).WithMessage("PageSize must be between 1 and 200");
    }
}
