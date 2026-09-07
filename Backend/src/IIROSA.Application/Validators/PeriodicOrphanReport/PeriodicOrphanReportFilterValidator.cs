using FluentValidation;
using IIROSA.Application.DTOs.PeriodicOrphanReport;

namespace IIROSA.Application.Validators.PeriodicOrphanReport;

/// <summary>
/// Validator for PeriodicOrphanReportFilterDto — §23.S.1 vocabulary contract (UC-RPT-02).
/// The status tokens are closed vocabularies: an unknown token must fail loudly (400)
/// rather than silently match nothing. Free-text Contains filters (SchoolType,
/// EducationDegree, MedicalStatus) mirror stored legacy values and stay open.
/// Paging bounds are not validated here — GetPagedAsync clamps them (existing behavior).
/// </summary>
public class PeriodicOrphanReportFilterValidator : AbstractValidator<PeriodicOrphanReportFilterDto>
{
    private static readonly string[] AndOrValues = { "and", "or" };
    private static readonly string[] MaritalValues = { "married", "single", "deceased" };
    private static readonly string[] EducationalValues = { "studying", "graduated", "dropout" };

    public PeriodicOrphanReportFilterValidator()
    {
        RuleFor(x => x.AndOr)
            .Must(v => string.IsNullOrWhiteSpace(v)
                       || AndOrValues.Contains(v.Trim().ToLowerInvariant()))
            .WithMessage("AndOr must be 'and' or 'or'");

        RuleFor(x => x.MaritalStatus)
            .Must(v => string.IsNullOrWhiteSpace(v)
                       || MaritalValues.Contains(v.Trim().ToLowerInvariant()))
            .WithMessage("MaritalStatus must be one of: married, single, deceased");

        RuleFor(x => x.EducationalStatus)
            .Must(v => string.IsNullOrWhiteSpace(v)
                       || EducationalValues.Contains(v.Trim().ToLowerInvariant()))
            .WithMessage("EducationalStatus must be one of: studying, graduated, dropout");
    }
}
