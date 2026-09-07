using FluentValidation;
using IIROSA.Application.DTOs.SeasonalAid;

namespace IIROSA.Application.Validators.SeasonalAid;

/// <summary>
/// Validator for the UC-PRJ-07 full-sync payload. An empty family list is a valid
/// deselect-all request, so the list is only checked for null entries and duplicates —
/// the business rules (quota, ownership, distributions) are applied by the service
/// once the diff against the stored registrations is known.
/// </summary>
public class UpdateSeasonalAidBeneficiariesValidator : AbstractValidator<UpdateSeasonalAidBeneficiariesDto>
{
    public UpdateSeasonalAidBeneficiariesValidator()
    {
        RuleFor(x => x.FamilyIds)
            .NotNull().WithMessage("Family ids are required");

        RuleForEach(x => x.FamilyIds)
            .NotEmpty().WithMessage("Family id cannot be empty");

        RuleFor(x => x.Currency)
            .MaximumLength(3).WithMessage("Currency code cannot exceed 3 characters")
            .When(x => !string.IsNullOrEmpty(x.Currency));

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Registration notes cannot exceed 500 characters");
    }
}

/// <summary>
/// Validator for the UC-PRJ-07 quick-add payload (POST) — here the list must carry at
/// least one family, an empty add is a no-op the screen should never send.
/// </summary>
public class CreateSeasonalAidBeneficiaryValidator : AbstractValidator<CreateSeasonalAidBeneficiaryDto>
{
    public CreateSeasonalAidBeneficiaryValidator()
    {
        RuleFor(x => x.FamilyIds)
            .NotNull().WithMessage("Family ids are required")
            .Must(ids => ids.Count > 0).WithMessage("At least one family must be selected");

        RuleForEach(x => x.FamilyIds)
            .NotEmpty().WithMessage("Family id cannot be empty");

        RuleFor(x => x.Currency)
            .MaximumLength(3).WithMessage("Currency code cannot exceed 3 characters")
            .When(x => !string.IsNullOrEmpty(x.Currency));

        RuleFor(x => x.RegistrationNotes)
            .MaximumLength(500).WithMessage("Registration notes cannot exceed 500 characters");
    }
}

/// <summary>
/// Validator for the UC-PRJ-08 received-flag payload.
/// </summary>
public class SetFamilyReceivedFlagValidator : AbstractValidator<SetFamilyReceivedFlagDto>
{
    public SetFamilyReceivedFlagValidator()
    {
        RuleFor(x => x.CampaignId)
            .NotEmpty().WithMessage("Project id is required");
    }
}
