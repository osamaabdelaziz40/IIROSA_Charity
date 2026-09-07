using FluentValidation;
using IIROSA.Application.DTOs.MissionManagement;

namespace IIROSA.Application.Validators.MissionManagement;

/// <summary>
/// Validator for CreateMissionDto (UC-MSN-06, §20.S.2 mandatory fields).
/// §20.S.2 lists اسم الجهة and اسم الجهة المنظمة both bound to EntityName — one field,
/// one control; the validator enforces it once.
/// </summary>
public class CreateMissionValidator : AbstractValidator<CreateMissionDto>
{
    public CreateMissionValidator()
    {
        // Required fields per §20.S.2
        RuleFor(x => x.MissionTarget)
            .NotEmpty().WithMessage("Mission target is required")
            .MaximumLength(200).WithMessage("Mission target cannot exceed 200 characters");

        RuleFor(x => x.MissionDate)
            .NotEmpty().WithMessage("Mission date is required")
            .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Mission date cannot be in the past");

        RuleFor(x => x.MissionTypeId)
            .NotEmpty().WithMessage("Mission type is required")
            .GreaterThan(0).WithMessage("Mission type must be a positive value");

        RuleFor(x => x.MissionTimeTypeId)
            .NotEmpty().WithMessage("Mission time type is required")
            .GreaterThan(0).WithMessage("Mission time type must be a positive value");

        RuleFor(x => x.MissionInterviewTypeId)
            .NotEmpty().WithMessage("Mission interview type is required")
            .GreaterThan(0).WithMessage("Mission interview type must be a positive value");

        RuleFor(x => x.AssignedToUserId)
            .NotEmpty().WithMessage("Assigned user is required")
            .NotEqual(Guid.Empty).WithMessage("Assigned user is required");

        RuleFor(x => x.EntityName)
            .NotEmpty().WithMessage("Entity name is required")
            .MaximumLength(200).WithMessage("Entity name cannot exceed 200 characters");

        RuleFor(x => x.ConferenceName)
            .NotEmpty().WithMessage("Conference name is required")
            .MaximumLength(200).WithMessage("Conference name cannot exceed 200 characters");

        RuleFor(x => x.MissionDetails)
            .NotEmpty().WithMessage("Mission details are required")
            .MaximumLength(1000).WithMessage("Mission details cannot exceed 1000 characters");

        RuleFor(x => x.Details)
            .NotEmpty().WithMessage("Details are required")
            .MaximumLength(2000).WithMessage("Details cannot exceed 2000 characters");

        RuleFor(x => x.MissionLocation)
            .NotEmpty().WithMessage("Mission location is required")
            .MaximumLength(500).WithMessage("Mission location cannot exceed 500 characters");

        RuleFor(x => x.Village)
            .NotEmpty().WithMessage("Village is required")
            .MaximumLength(100).WithMessage("Village cannot exceed 100 characters");

        RuleFor(x => x.RegionId)
            .NotEmpty().WithMessage("Region is required")
            .GreaterThan(0).WithMessage("Region must be a positive value");

        RuleFor(x => x.CenterId)
            .NotEmpty().WithMessage("Center is required")
            .GreaterThan(0).WithMessage("Center must be a positive value");

        // Location cascade (kept from the copied rules)
        RuleFor(x => x.RegionId)
            .NotNull().WithMessage("Region must be specified when Center is selected")
            .When(x => x.CenterId.HasValue);
        RuleFor(x => x.CountryId)
            .NotNull().WithMessage("Country must be specified when Region is selected")
            .When(x => x.RegionId.HasValue);
    }
}
