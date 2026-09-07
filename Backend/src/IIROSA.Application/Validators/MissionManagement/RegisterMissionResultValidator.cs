using FluentValidation;
using IIROSA.Application.DTOs.MissionManagement;

namespace IIROSA.Application.Validators.MissionManagement;

/// <summary>
/// Validator for RegisterMissionResultDto (UC-MSN-09, §20.S.3).
/// The read-only fields arrive populated from the loaded mission; the validator enforces
/// only what the actor can change — السبب (Reason) above all — plus the explicit outcome.
/// </summary>
public class RegisterMissionResultValidator : AbstractValidator<RegisterMissionResultDto>
{
    public RegisterMissionResultValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required")
            .MaximumLength(1000).WithMessage("Reason cannot exceed 1000 characters");

        RuleFor(x => x.IsCompleted)
            .NotNull().WithMessage("The completion outcome must be selected");

        // §20.S.3 mandatory editable fields — unconditional; a When(x => x != null) guard
        // would let a missing value sail through unvalidated.
        RuleFor(x => x.Details)
            .NotEmpty().WithMessage("Details are required")
            .MaximumLength(2000).WithMessage("Details cannot exceed 2000 characters");

        RuleFor(x => x.MissionTarget)
            .NotEmpty().WithMessage("Mission target cannot be empty")
            .MaximumLength(200).WithMessage("Mission target cannot exceed 200 characters");

        // Length bounds matching MissionConfiguration's column limits, so oversized input
        // fails validation instead of dying as a SQL error.
        RuleFor(x => x.EntityName)
            .MaximumLength(200).WithMessage("Entity name cannot exceed 200 characters")
            .When(x => x.EntityName != null);

        RuleFor(x => x.ConferenceName)
            .MaximumLength(200).WithMessage("Conference name cannot exceed 200 characters")
            .When(x => x.ConferenceName != null);

        RuleFor(x => x.MissionDetails)
            .MaximumLength(1000).WithMessage("Mission details cannot exceed 1000 characters")
            .When(x => x.MissionDetails != null);

        RuleFor(x => x.MissionLocation)
            .MaximumLength(500).WithMessage("Mission location cannot exceed 500 characters")
            .When(x => x.MissionLocation != null);

        RuleFor(x => x.Village)
            .MaximumLength(100).WithMessage("Village cannot exceed 100 characters")
            .When(x => x.Village != null);
    }
}
