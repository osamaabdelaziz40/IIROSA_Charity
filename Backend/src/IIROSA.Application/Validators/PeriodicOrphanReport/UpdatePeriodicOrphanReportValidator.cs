using FluentValidation;
using IIROSA.Application.DTOs.PeriodicOrphanReport;

namespace IIROSA.Application.Validators.PeriodicOrphanReport;

/// <summary>
/// Validator for UpdatePeriodicOrphanReportDto (UC-ORR-05, update a periodic report).
/// The update is a full replace of the §14.S.2 fields — the SPA loads the report
/// into the form and PUTs every control — so the same mandatory subset as create
/// applies: an incomplete payload is rejected here rather than wiping the record.
/// </summary>
public class UpdatePeriodicOrphanReportValidator : AbstractValidator<UpdatePeriodicOrphanReportDto>
{
    public UpdatePeriodicOrphanReportValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Report id is required")
            .NotEqual(Guid.Empty).WithMessage("Report id is required");

        RuleFor(x => x.ReportDate)
            .NotEmpty().WithMessage("Report date is required")
            .Must(d => d!.Value.Date <= DateTime.UtcNow.Date)
            .WithMessage("Report date cannot be in the future");

        RuleFor(x => x.OrphanImageId)
            .NotEmpty().WithMessage("Orphan photo is required")
            .NotEqual(Guid.Empty).WithMessage("Orphan photo is required");

        RuleFor(x => x.ReportPeriodFrom)
            .LessThanOrEqualTo(x => x.ReportPeriodTo)
            .When(x => x.ReportPeriodFrom.HasValue && x.ReportPeriodTo.HasValue)
            .WithMessage("Report period start must be on or before the period end");

        RuleFor(x => x.PrayerStatus).MaximumLength(100);
        RuleFor(x => x.MannersStatus).MaximumLength(100);
        RuleFor(x => x.QuranParts).MaximumLength(50);

        RuleFor(x => x.MarriageDate).NotNull()
            .When(x => x.Married == true)
            .WithMessage("Marriage date is required when the orphan is married");
        RuleFor(x => x.DeathDate).NotNull()
            .When(x => x.Dead == true)
            .WithMessage("Death date is required when the orphan is deceased");

        RuleFor(x => x.AnnualFeeForStudy).NotNull()
            .When(x => x.IsOrphanStudent == true)
            .WithMessage("Annual study fee is required for a sponsored student");
        RuleFor(x => x.AnnualFeeForStudy).GreaterThanOrEqualTo(0);
        RuleFor(x => x.StudyingYears).InclusiveBetween(0, 30);
        RuleFor(x => x.RestStudyingYears).InclusiveBetween(0, 30);
        RuleFor(x => x.GraduationYear).InclusiveBetween(1900, 2100);
        RuleFor(x => x.DropOutYear).InclusiveBetween(1900, 2100);
    }
}
