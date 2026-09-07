using FluentValidation;

using IIROSA.Application.DTOs.Reports;

namespace IIROSA.Application.Validators.Reports;

/// <summary>
/// §23.U.39 أيتام الأسر بتاريخ — the as-at date is mandatory; page bounds mirror the epic's
/// other report validators. Invoked in the service (platform rule).
/// </summary>
public class FamilyOrphansByDateFilterValidator : AbstractValidator<FamilyOrphansByDateFilterDto>
{
    public FamilyOrphansByDateFilterValidator()
    {
        RuleFor(x => x.Date)
            .NotEmpty()
            .WithMessage("The as-at date is required");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);
    }
}
