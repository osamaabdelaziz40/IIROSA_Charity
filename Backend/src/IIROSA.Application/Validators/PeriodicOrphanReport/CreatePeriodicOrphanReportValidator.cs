using FluentValidation;
using IIROSA.Application.DTOs.PeriodicOrphanReport;

namespace IIROSA.Application.Validators.PeriodicOrphanReport;

/// <summary>
/// Validator for CreatePeriodicOrphanReportDto (UC-ORR-03, create a periodic report).
/// Orphan, report date and the orphan photo are the mandatory inputs (§14.S.2);
/// everything else is optional narrative the reviewer evaluates. String lengths
/// mirror PeriodicOrphanReportConfiguration; unconfigured columns are nvarchar(max).
/// </summary>
public class CreatePeriodicOrphanReportValidator : AbstractValidator<CreatePeriodicOrphanReportDto>
{
    public CreatePeriodicOrphanReportValidator()
    {
        // §11.U.6 discriminator — a guardian (Parent) report carries the family + guardian
        // instead of an orphan; the orphan rules below then stand down (UC-HOU-08).
        RuleFor(x => x.ChildOrParent)
            .Must(v => string.IsNullOrWhiteSpace(v)
                       || string.Equals(v.Trim(), "Child", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(v.Trim(), "Parent", StringComparison.OrdinalIgnoreCase))
            .WithMessage("childOrParent must be 'Child' or 'Parent'");

        RuleFor(x => x.OrphanId)
            .NotEmpty().WithMessage("Orphan is required")
            .NotEqual(Guid.Empty).WithMessage("Orphan is required")
            .When(x => !IsParentBeneficiary(x));

        RuleFor(x => x.OrphanId)
            .Equal(Guid.Empty).WithMessage("Orphan must not be sent for a guardian (Parent) report")
            .When(IsParentBeneficiary);

        RuleFor(x => x.HousingFamilyId)
            .NotNull().WithMessage("Housing family is required for a guardian (Parent) report")
            .NotEqual(Guid.Empty).WithMessage("Housing family is required for a guardian (Parent) report")
            .When(IsParentBeneficiary);

        RuleFor(x => x.HousingBeneficiaryId)
            .NotNull().WithMessage("Guardian beneficiary is required for a guardian (Parent) report")
            .NotEqual(Guid.Empty).WithMessage("Guardian beneficiary is required for a guardian (Parent) report")
            .When(IsParentBeneficiary);

        // §11.S.4 review flags are data on create — but only one decision may be recorded.
        // NotEqual(true), not Equal(false): IsRefused is nullable, so an omitted flag (null)
        // must pass — only an explicit true conflicts with IsAccepted (found live, 6-8 battery).
        RuleFor(x => x.IsRefused)
            .NotEqual(true).WithMessage("A report cannot be both accepted and refused")
            .When(x => x.IsAccepted == true);

        RuleFor(x => x.RefuseReasonId)
            .NotNull().WithMessage("Refuse reason is required when the report is refused")
            .NotEqual(0).WithMessage("Refuse reason is required when the report is refused")
            .When(x => x.IsRefused == true);

        RuleFor(x => x.ReportDate)
            .NotEmpty().WithMessage("Report date is required")
            .Must(d => d.Date <= DateTime.UtcNow.Date)
            .WithMessage("Report date cannot be in the future");

        RuleFor(x => x.OrphanImageId)
            .NotEmpty().WithMessage("Orphan photo is required")
            .NotEqual(Guid.Empty).WithMessage("Orphan photo is required");

        RuleFor(x => x.ReportPeriodFrom)
            .LessThanOrEqualTo(x => x.ReportPeriodTo)
            .When(x => x.ReportPeriodFrom.HasValue && x.ReportPeriodTo.HasValue)
            .WithMessage("Report period start must be on or before the period end");

        // Lengths matching the entity configuration (the rest are nvarchar(max))
        RuleFor(x => x.PrayerStatus).MaximumLength(100);
        RuleFor(x => x.MannersStatus).MaximumLength(100);
        RuleFor(x => x.QuranParts).MaximumLength(50);

        // Checkbox interlocks (legacy MarriedFun/DieFun/IsOrphanStudentFun)
        RuleFor(x => x.MarriageDate)
            .NotNull().WithMessage("Marriage date is required when the orphan is married")
            .When(x => x.Married == true);
        RuleFor(x => x.DeathDate)
            .NotNull().WithMessage("Death date is required when the orphan is deceased")
            .When(x => x.Dead == true);
        RuleFor(x => x.AnnualFeeForStudy)
            .NotNull().WithMessage("Annual study fee is required for a student-sponsorship request")
            .GreaterThanOrEqualTo(0).WithMessage("Annual study fee cannot be negative")
            .When(x => x.IsOrphanStudent == true);

        // Numeric sanity ranges
        RuleFor(x => x.AnnualFeeForStudy)
            .GreaterThanOrEqualTo(0).WithMessage("Annual study fee cannot be negative")
            .When(x => x.AnnualFeeForStudy.HasValue);
        RuleFor(x => x.StudyingYears)
            .InclusiveBetween(0, 30).WithMessage("Studying years must be between 0 and 30")
            .When(x => x.StudyingYears.HasValue);
        RuleFor(x => x.RestStudyingYears)
            .InclusiveBetween(0, 30).WithMessage("Remaining studying years must be between 0 and 30")
            .When(x => x.RestStudyingYears.HasValue);
        RuleFor(x => x.GraduationYear)
            .InclusiveBetween(1900, 2100).WithMessage("Graduation year must be between 1900 and 2100")
            .When(x => x.GraduationYear.HasValue);
        RuleFor(x => x.DropOutYear)
            .InclusiveBetween(1900, 2100).WithMessage("Drop-out year must be between 1900 and 2100")
            .When(x => x.DropOutYear.HasValue);
    }

    /// <summary>Case-insensitive "Parent" test on the §11.U.6 discriminator (blank ⇒ Child).</summary>
    private static bool IsParentBeneficiary(CreatePeriodicOrphanReportDto dto) =>
        string.Equals(dto.ChildOrParent?.Trim(), "Parent", StringComparison.OrdinalIgnoreCase);
}
