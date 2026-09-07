using FluentValidation;
using IIROSA.Application.DTOs.Reports;

namespace IIROSA.Application.Validators.Reports;

/// <summary>
/// UC-RPT-06 — page bounds only; the charity narrow is authorised in the service
/// (pin-never-widen), never here.
/// </summary>
public class WidowSponsorshipFilterValidator : AbstractValidator<WidowSponsorshipFilterDto>
{
    public WidowSponsorshipFilterValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be at least 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 200).WithMessage("PageSize must be between 1 and 200");
    }
}
