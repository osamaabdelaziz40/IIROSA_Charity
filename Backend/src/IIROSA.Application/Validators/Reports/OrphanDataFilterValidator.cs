using FluentValidation;
using IIROSA.Application.DTOs.Reports;

namespace IIROSA.Application.Validators.Reports;

/// <summary>
/// UC-RPT-01 (§23.S.3) — the only failure this read can produce (AC 6): bad ages or an
/// oversized page. Invoked with ValidateAndThrowAsync in ReportService (service-layer rule).
/// </summary>
public class OrphanDataFilterValidator : AbstractValidator<OrphanDataFilterDto>
{
    public OrphanDataFilterValidator()
    {
        RuleFor(f => f.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(f => f.PageSize)
            .InclusiveBetween(1, 200);

        RuleFor(f => f.AgeFrom)
            .GreaterThanOrEqualTo(1)
                .When(f => f.AgeFrom.HasValue)
            .WithMessage("AgeFrom must be 1 or greater");

        RuleFor(f => f.AgeTo)
            .GreaterThanOrEqualTo(1)
                .When(f => f.AgeTo.HasValue)
            .WithMessage("AgeTo must be 1 or greater");

        // AC 6 — AgeTo lower than AgeFrom refuses the report
        RuleFor(f => f)
            .Must(f => !f.AgeFrom.HasValue || !f.AgeTo.HasValue || f.AgeTo.Value >= f.AgeFrom.Value)
            .WithMessage("AgeTo must be greater than or equal to AgeFrom")
            .WithName("AgeTo");
    }
}
