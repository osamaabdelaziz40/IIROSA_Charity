using FluentValidation;
using IIROSA.Application.DTOs.MissionManagement;

namespace IIROSA.Application.Validators.MissionManagement;

/// <summary>
/// Validator for UpdateMissionDto (UC-MSN-07) — patch-style: a value that IS provided must be
/// valid; mandatory per §20.S.2 is enforced on create. The past-date rule lives in the service,
/// where it can compare against the loaded record — the form always resubmits the unchanged
/// date, so a rule here would block every edit of a past-dated mission.
/// </summary>
public class UpdateMissionValidator : AbstractValidator<UpdateMissionDto>
{
    public UpdateMissionValidator()
    {
        RuleFor(x => x.MissionTarget)
            .NotEmpty().WithMessage("Mission target cannot be empty")
            .MaximumLength(200).WithMessage("Mission target cannot exceed 200 characters")
            .When(x => x.MissionTarget != null);

        RuleFor(x => x.MissionTypeId)
            .GreaterThan(0).WithMessage("Mission type must be a positive value")
            .When(x => x.MissionTypeId.HasValue);

        RuleFor(x => x.MissionTimeTypeId)
            .GreaterThan(0).WithMessage("Mission time type must be a positive value")
            .When(x => x.MissionTimeTypeId.HasValue);

        RuleFor(x => x.MissionInterviewTypeId)
            .GreaterThan(0).WithMessage("Mission interview type must be a positive value")
            .When(x => x.MissionInterviewTypeId.HasValue);

        RuleFor(x => x.AssignedToUserId)
            .NotEqual(Guid.Empty).WithMessage("Assigned user is required")
            .When(x => x.AssignedToUserId.HasValue);

        RuleFor(x => x.EntityName)
            .NotEmpty().WithMessage("Entity name cannot be empty")
            .MaximumLength(200).WithMessage("Entity name cannot exceed 200 characters")
            .When(x => x.EntityName != null);

        RuleFor(x => x.ConferenceName)
            .NotEmpty().WithMessage("Conference name cannot be empty")
            .MaximumLength(200).WithMessage("Conference name cannot exceed 200 characters")
            .When(x => x.ConferenceName != null);

        RuleFor(x => x.MissionDetails)
            .MaximumLength(1000).WithMessage("Mission details cannot exceed 1000 characters")
            .When(x => x.MissionDetails != null);

        RuleFor(x => x.Details)
            .MaximumLength(2000).WithMessage("Details cannot exceed 2000 characters")
            .When(x => x.Details != null);

        RuleFor(x => x.MissionLocation)
            .MaximumLength(500).WithMessage("Mission location cannot exceed 500 characters")
            .When(x => x.MissionLocation != null);

        RuleFor(x => x.Village)
            .MaximumLength(100).WithMessage("Village cannot exceed 100 characters")
            .When(x => x.Village != null);

        // Location cascade (kept from the copied rules): center requires region, region requires country
        RuleFor(x => x.RegionId)
            .NotNull().WithMessage("Region must be specified when Center is selected")
            .When(x => x.CenterId.HasValue);
        RuleFor(x => x.CountryId)
            .NotNull().WithMessage("Country must be specified when Region is selected")
            .When(x => x.RegionId.HasValue);
    }
}
