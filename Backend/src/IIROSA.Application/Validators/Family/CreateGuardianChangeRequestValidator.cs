using FluentValidation;
using IIROSA.Application.DTOs.Family;

namespace IIROSA.Application.Validators.Family;

/// <summary>
/// UC-FAM-09 raise validation — the new-guardian snapshot and the reason are all mandatory;
/// the reviewer cannot judge a request with blank proposal fields.
/// </summary>
public class CreateGuardianChangeRequestValidator : AbstractValidator<CreateGuardianChangeRequestDto>
{
    public CreateGuardianChangeRequestValidator()
    {
        // Must(...!IsNullOrWhiteSpace) rather than NotEmpty: NotEmpty passes whitespace-only
        // strings, which the service would then Trim() into empty stored snapshots.
        RuleFor(x => x.NewGuardianName)
            .Must(v => !string.IsNullOrWhiteSpace(v)).WithMessage("The new guardian's name is required")
            .MaximumLength(200);

        RuleFor(x => x.NewGuardianNationalId)
            .Must(v => !string.IsNullOrWhiteSpace(v)).WithMessage("The new guardian's national ID is required")
            .MaximumLength(50);

        RuleFor(x => x.Relationship)
            .Must(v => !string.IsNullOrWhiteSpace(v)).WithMessage("The new guardian's relationship to the family is required")
            .MaximumLength(100);

        RuleFor(x => x.Reason)
            .Must(v => !string.IsNullOrWhiteSpace(v)).WithMessage("A reason for the guardian change is required")
            .MaximumLength(500);
    }
}
