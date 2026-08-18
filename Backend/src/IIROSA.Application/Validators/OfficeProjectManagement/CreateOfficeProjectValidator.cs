using FluentValidation;
using IIROSA.Application.DTOs.OfficeProjectManagement;

namespace IIROSA.Application.Validators.OfficeProjectManagement;

/// <summary>
/// Validator for CreateOfficeProjectDto (UC-7.1)
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
            .NotEmpty().WithMessage("Project date is required")
            .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Project date cannot be in the past");

        RuleFor(x => x.FK_OfficeProjectTypeId)
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

        // Location cascade validation
        RuleFor(x => x.FK_CenterId)
            .Must((model, centerId) => !centerId.HasValue || model.FK_RegionId.HasValue)
            .WithMessage("Region must be specified when Center is selected");

        RuleFor(x => x.FK_RegionId)
            .Must((model, regionId) => !regionId.HasValue || model.FK_CountryId.HasValue)
            .WithMessage("Country must be specified when Region is selected");
    }

    private bool BeValidBeneficiariesType(string? type)
    {
        return type == "Families" || type == "Individuals" || type == "Both";
    }
}

/// <summary>
/// Validator for UpdateOfficeProjectDto (UC-7.8)
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

        RuleFor(x => x.ProjectDate)
            .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Project date cannot be in the past")
            .When(x => x.ProjectDate.HasValue);

        RuleFor(x => x.FK_OfficeProjectTypeId)
            .GreaterThan(0).WithMessage("Project type must be a positive value")
            .When(x => x.FK_OfficeProjectTypeId.HasValue);

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

        // Location cascade validation
        RuleFor(x => x.FK_CenterId)
            .Must((model, centerId) => !centerId.HasValue || model.FK_RegionId.HasValue)
            .WithMessage("Region must be specified when Center is selected");

        RuleFor(x => x.FK_RegionId)
            .Must((model, regionId) => !regionId.HasValue || model.FK_CountryId.HasValue)
            .WithMessage("Country must be specified when Region is selected");
    }

    private bool BeValidBeneficiariesType(string? type)
    {
        return type == "Families" || type == "Individuals" || type == "Both";
    }
}

/// <summary>
/// Validator for SetProjectBudgetDto (UC-7.2)
/// </summary>
public class SetProjectBudgetValidator : AbstractValidator<SetProjectBudgetDto>
{
    public SetProjectBudgetValidator()
    {
        RuleFor(x => x.ProjectCostEGP)
            .GreaterThanOrEqualTo(0).WithMessage("Project cost in EGP must be non-negative")
            .When(x => x.ProjectCostEGP.HasValue);

        RuleFor(x => x.ProjectCostSAR)
            .GreaterThanOrEqualTo(0).WithMessage("Project cost in SAR must be non-negative")
            .When(x => x.ProjectCostSAR.HasValue);

        RuleFor(x => x)
            .Must(x => x.ProjectCostEGP.HasValue || x.ProjectCostSAR.HasValue)
            .WithMessage("At least one cost amount (EGP or SAR) must be specified");
    }
}

/// <summary>
/// Validator for SpecifyProjectDonorDto (UC-7.3)
/// </summary>
public class SpecifyProjectDonorValidator : AbstractValidator<SpecifyProjectDonorDto>
{
    public SpecifyProjectDonorValidator()
    {
        RuleFor(x => x.DonorName)
            .NotEmpty().WithMessage("Donor name is required")
            .MaximumLength(200).WithMessage("Donor name cannot exceed 200 characters");
    }
}

/// <summary>
/// Validator for SetBeneficiariesCountDto (UC-7.4)
/// </summary>
public class SetBeneficiariesCountValidator : AbstractValidator<SetBeneficiariesCountDto>
{
    public SetBeneficiariesCountValidator()
    {
        RuleFor(x => x.BeneficiariesCount)
            .NotEmpty().WithMessage("Beneficiaries count is required")
            .GreaterThan(0).WithMessage("Beneficiaries count must be greater than 0");

        RuleFor(x => x.BeneficiariesType)
            .NotEmpty().WithMessage("Beneficiaries type is required")
            .Must(BeValidBeneficiariesType).WithMessage("Beneficiaries type must be Families, Individuals, or Both");
    }

    private bool BeValidBeneficiariesType(string type)
    {
        return type == "Families" || type == "Individuals" || type == "Both";
    }
}

/// <summary>
/// Validator for AssignProjectLocationDto (UC-7.5)
/// </summary>
public class AssignProjectLocationValidator : AbstractValidator<AssignProjectLocationDto>
{
    public AssignProjectLocationValidator()
    {
        RuleFor(x => x.VillageName)
            .MaximumLength(100).WithMessage("Village name cannot exceed 100 characters");

        // Location cascade validation
        RuleFor(x => x.FK_CenterId)
            .Must((model, centerId) => !centerId.HasValue || model.FK_RegionId.HasValue)
            .WithMessage("Region must be specified when Center is selected");

        RuleFor(x => x.FK_RegionId)
            .Must((model, regionId) => !regionId.HasValue || model.FK_CountryId.HasValue)
            .WithMessage("Country must be specified when Region is selected");
    }
}

/// <summary>
/// Validator for AttachProjectDocumentDto (UC-7.6)
/// </summary>
public class AttachProjectDocumentValidator : AbstractValidator<AttachProjectDocumentDto>
{
    public AttachProjectDocumentValidator()
    {
        RuleFor(x => x.FK_AttachedFileId)
            .NotEmpty().WithMessage("Attached file ID is required");

        RuleFor(x => x.DocumentType)
            .NotEmpty().WithMessage("Document type is required")
            .Must(BeValidDocumentType).WithMessage("Document type must be Proposal, Contract, Progress Report, or Other");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");
    }

    private bool BeValidDocumentType(string type)
    {
        return type == "Proposal" || type == "Contract" || type == "Progress Report" || type == "Other";
    }
}

/// <summary>
/// Validator for UploadProjectReportDto (UC-7.7)
/// </summary>
public class UploadProjectReportValidator : AbstractValidator<UploadProjectReportDto>
{
    public UploadProjectReportValidator()
    {
        RuleFor(x => x.FK_ProjectReportFileId)
            .NotEmpty().WithMessage("Project report file ID is required");

        RuleFor(x => x.ReportType)
            .NotEmpty().WithMessage("Report type is required")
            .Must(BeValidReportType).WithMessage("Report type must be Completion, Progress, or Final");

        RuleFor(x => x.Summary)
            .MaximumLength(1000).WithMessage("Summary cannot exceed 1000 characters");
    }

    private bool BeValidReportType(string type)
    {
        return type == "Completion" || type == "Progress" || type == "Final";
    }
}

/// <summary>
/// Validator for MarkProjectCompletedDto (UC-7.9)
/// </summary>
public class MarkProjectCompletedValidator : AbstractValidator<MarkProjectCompletedDto>
{
    public MarkProjectCompletedValidator()
    {
        RuleFor(x => x.IsFinished)
            .Equal(true).WithMessage("Project must be marked as finished");

        RuleFor(x => x.ProjectEndDate)
            .NotEmpty().WithMessage("Project end date is required")
            .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Project end date cannot be in the past");
    }
}

/// <summary>
/// Validator for SetProjectDatesDto (UC-7.13)
/// </summary>
public class SetProjectDatesValidator : AbstractValidator<SetProjectDatesDto>
{
    public SetProjectDatesValidator()
    {
        RuleFor(x => x.ProjectDate)
            .NotEmpty().WithMessage("Project date is required");

        RuleFor(x => x.ProjectEndDate)
            .Must((model, endDate) => !endDate.HasValue || endDate.Value >= model.ProjectDate)
            .WithMessage("Project end date must be equal to or after project start date");
    }
}

/// <summary>
/// Validator for AssignProjectToCharityDto (UC-7.14)
/// </summary>
public class AssignProjectToCharityValidator : AbstractValidator<AssignProjectToCharityDto>
{
    public AssignProjectToCharityValidator()
    {
        // Charity ID can be null (unassigned) or a valid GUID
        RuleFor(x => x.FK_CharityId)
            .Must(BeValidCharityId).WithMessage("Invalid charity ID");
    }

    private bool BeValidCharityId(Guid? charityId)
    {
        // Allow null (unassigned) or non-empty GUID
        return !charityId.HasValue || charityId.Value != Guid.Empty;
    }
}
