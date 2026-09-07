using FluentValidation;
using IIROSA.Application.DTOs.PeriodicOrphanReport;

namespace IIROSA.Application.Validators.PeriodicOrphanReport;

/// <summary>
/// Validator for ReviewPeriodicReportDto (UC-ORR-07 accept / UC-ORR-08 refuse).
/// A refusal is never silent: the reviewer must pick a reason from the catalogue
/// or write one (UC-ORR-08 BR). Acceptance carries no reason.
/// </summary>
public class ReviewPeriodicReportValidator : AbstractValidator<ReviewPeriodicReportDto>
{
    public ReviewPeriodicReportValidator()
    {
        RuleFor(x => x.ReportId)
            .NotEmpty().WithMessage("Report id is required")
            .NotEqual(Guid.Empty).WithMessage("Report id is required");

        RuleFor(x => x.RefuseReasonId)
            .NotNull().WithMessage("A refuse reason is required when refusing a report")
            .When(x => !x.IsApproved && string.IsNullOrWhiteSpace(x.RefuseReason));

        RuleFor(x => x.RefuseReason)
            .NotEmpty().WithMessage("A refuse reason is required when refusing a report")
            .MaximumLength(500).WithMessage("Refuse reason cannot exceed 500 characters")
            .When(x => !x.IsApproved && !x.RefuseReasonId.HasValue);

        RuleFor(x => x.ReviewComments)
            .MaximumLength(1000).WithMessage("Review comments cannot exceed 1000 characters");
    }
}
