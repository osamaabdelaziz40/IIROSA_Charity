using FluentValidation;
using IIROSA.Application.DTOs.OfficeProjectManagement;

namespace IIROSA.Application.Validators.OfficeProjectManagement;

// Date note: §18.S carries no future-only constraint on the date fields, and the register holds
// backdated projects, so the earlier `.GreaterThanOrEqualTo(DateTime.Today)` rules were removed
// when the validators were wired into the service — they would have rejected every historical row.

/// <summary>
/// Validator for CreateOfficeProjectDto (UC-OFP-03)
/// </summary>
public class CreateOfficeProjectValidator : AbstractValidator<CreateOfficeProjectDto>
{
    public CreateOfficeProjectValidator()
    {
        // Required fields
        RuleFor(x => x.ProjectName)
            .NotEmpty().WithMessage("Project name is required")
            .MaximumLength(200).WithMessage("Project name cannot exceed 200 characters");

        RuleFor(x => x.ProjectDate)
            .NotEmpty().WithMessage("Project date is required");

        RuleFor(x => x.OfficeProjectTypeId)
            .NotEmpty().WithMessage("Project type is required")
            .GreaterThan(0).WithMessage("Project type must be a positive value");

        // Optional fields with constraints
        RuleFor(x => x.ProjectHint)
            .MaximumLength(1000).WithMessage("Project hint cannot exceed 1000 characters");

        RuleFor(x => x.VillageName)
            .MaximumLength(100).WithMessage("Village name cannot exceed 100 characters");

        RuleFor(x => x.ProjectCostEGP)
            .GreaterThanOrEqualTo(0).WithMessage("Project cost in EGP must be non-negative")
            .When(x => x.ProjectCostEGP.HasValue);

        RuleFor(x => x.ProjectCostSAR)
            .GreaterThanOrEqualTo(0).WithMessage("Project cost in SAR must be non-negative")
            .When(x => x.ProjectCostSAR.HasValue);

        RuleFor(x => x.DonorName)
            .MaximumLength(200).WithMessage("Donor name cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.DonorName));

        RuleFor(x => x.BeneficiariesCount)
            .GreaterThan(0).WithMessage("Beneficiaries count must be greater than 0")
            .When(x => x.BeneficiariesCount.HasValue);

        RuleFor(x => x.BeneficiariesType)
            .Must(BeValidBeneficiariesType).WithMessage("Beneficiaries type must be Families, Individuals, or Both")
            .When(x => !string.IsNullOrEmpty(x.BeneficiariesType));

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters");

        RuleFor(x => x.ProjectEndDate)
            .GreaterThanOrEqualTo(x => x.ProjectDate)
            .WithMessage("Project end date must be on or after the project date")
            .When(x => x.ProjectEndDate.HasValue);

        // Location cascade validation (Country → Region → Center)
        RuleFor(x => x.CenterId)
            .Must((model, centerId) => !centerId.HasValue || model.RegionId.HasValue)
            .WithMessage("Region must be specified when Center is selected");

        RuleFor(x => x.RegionId)
            .Must((model, regionId) => !regionId.HasValue || model.CountryId.HasValue)
            .WithMessage("Country must be specified when Region is selected");
    }

    private static bool BeValidBeneficiariesType(string? type)
    {
        return type == "Families" || type == "Individuals" || type == "Both";
    }
}

/// <summary>
/// Validator for UpdateOfficeProjectDto (UC-OFP-04). Every field is optional — null means "leave
/// unchanged" — so each rule runs only when a value is supplied.
/// </summary>
public class UpdateOfficeProjectValidator : AbstractValidator<UpdateOfficeProjectDto>
{
    public UpdateOfficeProjectValidator()
    {
        // Optional fields with constraints
        RuleFor(x => x.ProjectName)
            .NotEmpty().WithMessage("Project name is required")
            .MaximumLength(200).WithMessage("Project name cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.ProjectName));

        RuleFor(x => x.OfficeProjectTypeId)
            .GreaterThan(0).WithMessage("Project type must be a positive value")
            .When(x => x.OfficeProjectTypeId.HasValue);

        RuleFor(x => x.ProjectHint)
            .MaximumLength(1000).WithMessage("Project hint cannot exceed 1000 characters");

        RuleFor(x => x.VillageName)
            .MaximumLength(100).WithMessage("Village name cannot exceed 100 characters");

        RuleFor(x => x.ProjectCostEGP)
            .GreaterThanOrEqualTo(0).WithMessage("Project cost in EGP must be non-negative")
            .When(x => x.ProjectCostEGP.HasValue);

        RuleFor(x => x.ProjectCostSAR)
            .GreaterThanOrEqualTo(0).WithMessage("Project cost in SAR must be non-negative")
            .When(x => x.ProjectCostSAR.HasValue);

        RuleFor(x => x.DonorName)
            .MaximumLength(200).WithMessage("Donor name cannot exceed 200 characters");

        RuleFor(x => x.BeneficiariesCount)
            .GreaterThan(0).WithMessage("Beneficiaries count must be greater than 0")
            .When(x => x.BeneficiariesCount.HasValue);

        RuleFor(x => x.BeneficiariesType)
            .Must(BeValidBeneficiariesType).WithMessage("Beneficiaries type must be Families, Individuals, or Both")
            .When(x => !string.IsNullOrEmpty(x.BeneficiariesType));

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters");

        RuleFor(x => x.ProjectEndDate)
            .GreaterThanOrEqualTo(x => x.ProjectDate)
            .WithMessage("Project end date must be on or after the project date")
            .When(x => x.ProjectEndDate.HasValue && x.ProjectDate.HasValue);

        // Location cascade validation (Country → Region → Center)
        RuleFor(x => x.CenterId)
            .Must((model, centerId) => !centerId.HasValue || model.RegionId.HasValue)
            .WithMessage("Region must be specified when Center is selected");

        RuleFor(x => x.RegionId)
            .Must((model, regionId) => !regionId.HasValue || model.CountryId.HasValue)
            .WithMessage("Country must be specified when Region is selected");
    }

    private static bool BeValidBeneficiariesType(string? type)
    {
        return type == "Families" || type == "Individuals" || type == "Both";
    }
}
