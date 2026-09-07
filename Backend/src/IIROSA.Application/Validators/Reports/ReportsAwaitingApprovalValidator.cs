using FluentValidation;

using IIROSA.Application.DTOs.Reports;

namespace IIROSA.Application.Validators.Reports;

/// <summary>
/// UC-RPT-17 (§23.S.16 تقارير في انتظار الموافقة) — paging bounds only: the charity key
/// is the single optional filter; the pending state is fixed server-side.
/// </summary>
public class ReportsAwaitingApprovalValidator : AbstractValidator<ReportsAwaitingApprovalFilterDto>
{
    public ReportsAwaitingApprovalValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
                .WithMessage("Page must be at least 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 200)
                .WithMessage("PageSize must be between 1 and 200");
    }
}
