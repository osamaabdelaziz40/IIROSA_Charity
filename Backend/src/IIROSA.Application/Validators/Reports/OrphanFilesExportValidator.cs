using FluentValidation;
using IIROSA.Application.DTOs.Reports;

namespace IIROSA.Application.Validators.Reports;

/// <summary>
/// UC-RPT-24 (§23.U.24) — من تاريخ is mandatory per the §23.S.18 table (the legacy binding
/// swap is corrected); الى تاريخ is optional but ≥ من تاريخ. Paging stays small — image
/// manifests are heavy. Invoked with ValidateAndThrowAsync in ReportService (service-layer rule).
/// </summary>
public class OrphanFilesExportValidator : AbstractValidator<OrphanFilesExportFilterDto>
{
    public OrphanFilesExportValidator()
    {
        RuleFor(r => r.DateFrom)
            .NotNull()
            .WithMessage("Date from is required")
            .InclusiveBetween(new DateTime(2000, 1, 1), DateTime.UtcNow.AddYears(1));

        RuleFor(r => r.DateTo)
            .GreaterThanOrEqualTo(r => r.DateFrom!.Value)
            .When(r => r.DateTo.HasValue && r.DateFrom.HasValue)
            .WithMessage("Date to must be on or after date from");

        RuleFor(r => r.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(r => r.PageSize)
            .InclusiveBetween(1, 100);
    }
}
