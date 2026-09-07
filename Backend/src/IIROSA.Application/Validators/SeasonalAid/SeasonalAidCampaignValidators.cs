using FluentValidation;
using IIROSA.Application.DTOs.SeasonalAid;

namespace IIROSA.Application.Validators.SeasonalAid;

/// <summary>
/// Validator for CreateSeasonalAidCampaignDto (UC-PRJ-02)
/// </summary>
public class CreateSeasonalAidCampaignValidator : AbstractValidator<CreateSeasonalAidCampaignDto>
{
    public CreateSeasonalAidCampaignValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Campaign name is required")
            .MaximumLength(200).WithMessage("Campaign name cannot exceed 200 characters");

        RuleFor(x => x.CampaignType)
            .NotEmpty().WithMessage("Campaign type is required")
            .MaximumLength(50).WithMessage("Campaign type cannot exceed 50 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required")
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("End date must be greater than or equal to start date");

        RuleFor(x => x.TotalBudget)
            .GreaterThan(0).WithMessage("Total budget must be greater than zero");

        RuleFor(x => x.BudgetCurrency)
            .NotEmpty().WithMessage("Budget currency is required")
            .MaximumLength(3).WithMessage("Currency code cannot exceed 3 characters");

        RuleFor(x => x.PerFamilyAllocation)
            .GreaterThan(0).WithMessage("Per-family allocation must be greater than zero");

        RuleFor(x => x.MaximumFamilies)
            .GreaterThan(0).WithMessage("Maximum families must be at least 1")
            .When(x => x.MaximumFamilies.HasValue);

        RuleFor(x => x.FamilyType)
            .MaximumLength(50).WithMessage("Family type cannot exceed 50 characters");

        RuleFor(x => x.MinChildrenAge)
            .InclusiveBetween(0, 18).WithMessage("Minimum children age must be between 0 and 18")
            .When(x => x.MinChildrenAge.HasValue);

        RuleFor(x => x.MaxChildrenAge)
            .InclusiveBetween(0, 18).WithMessage("Maximum children age must be between 0 and 18")
            .When(x => x.MaxChildrenAge.HasValue);

        RuleFor(x => x.MaxChildrenAge)
            .GreaterThanOrEqualTo(x => x.MinChildrenAge)
            .WithMessage("Maximum children age must be greater than or equal to minimum children age")
            .When(x => x.MaxChildrenAge.HasValue && x.MinChildrenAge.HasValue);
    }
}

/// <summary>
/// Validator for UpdateSeasonalAidCampaignDto (UC-PRJ-04) — same field contract as create.
/// </summary>
public class UpdateSeasonalAidCampaignValidator : AbstractValidator<UpdateSeasonalAidCampaignDto>
{
    public UpdateSeasonalAidCampaignValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Campaign ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Campaign name is required")
            .MaximumLength(200).WithMessage("Campaign name cannot exceed 200 characters");

        RuleFor(x => x.CampaignType)
            .NotEmpty().WithMessage("Campaign type is required")
            .MaximumLength(50).WithMessage("Campaign type cannot exceed 50 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required")
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("End date must be greater than or equal to start date");

        RuleFor(x => x.TotalBudget)
            .GreaterThan(0).WithMessage("Total budget must be greater than zero");

        RuleFor(x => x.BudgetCurrency)
            .NotEmpty().WithMessage("Budget currency is required")
            .MaximumLength(3).WithMessage("Currency code cannot exceed 3 characters");

        RuleFor(x => x.PerFamilyAllocation)
            .GreaterThan(0).WithMessage("Per-family allocation must be greater than zero");

        RuleFor(x => x.MaximumFamilies)
            .GreaterThan(0).WithMessage("Maximum families must be at least 1")
            .When(x => x.MaximumFamilies.HasValue);

        RuleFor(x => x.MaxChildrenAge)
            .GreaterThanOrEqualTo(x => x.MinChildrenAge)
            .WithMessage("Maximum children age must be greater than or equal to minimum children age")
            .When(x => x.MaxChildrenAge.HasValue && x.MinChildrenAge.HasValue);
    }
}
