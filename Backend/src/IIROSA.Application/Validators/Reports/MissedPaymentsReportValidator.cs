using FluentValidation;
using IIROSA.Application.DTOs.Reports;

namespace IIROSA.Application.Validators.Reports;

/// <summary>
/// UC-RPT-22 (§23.U.22) — nothing on this read is mandatory except sane paging; الجمعية is an
/// optional HQ narrow. Invoked with ValidateAndThrowAsync in ReportService (service-layer rule).
/// </summary>
public class MissedPaymentsReportValidator : AbstractValidator<MissedPaymentsReportFilterDto>
{
    public MissedPaymentsReportValidator()
    {
        RuleFor(r => r.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(r => r.PageSize)
            .InclusiveBetween(1, 100);
    }
}
