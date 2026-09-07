using FluentValidation;
using IIROSA.Application.DTOs.Family;

namespace IIROSA.Application.Validators.Family;

/// <summary>
/// UC-FAM-10 decision validation — a refusal is not accepted without a reason (§10.U.10); an
/// approval carries the request's own snapshot, so nothing else is needed.
/// </summary>
public class ApproveGuardianChangeRequestValidator : AbstractValidator<ApproveGuardianChangeRequestDto>
{
    public ApproveGuardianChangeRequestValidator()
    {
        // Must(IsNullOrWhiteSpace-false) rather than NotEmpty: NotEmpty passes whitespace-only
        // strings, and the service stores dto.RejectionReason.Trim() — i.e. "" in the database.
        RuleFor(x => x.RejectionReason)
            .Must(v => !string.IsNullOrWhiteSpace(v)).When(x => !x.IsApproved)
            .WithMessage("A rejection reason is required when refusing a guardian-change request")
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.RejectionReason));
    }
}
