using FluentValidation;
using IIROSA.Application.DTOs.SeasonalAid;

namespace IIROSA.Application.Validators.SeasonalAid;

/// <summary>
/// Validator for CreateSeasonalAidDistributionDto (UC-9.5 / the UC-PRJ-08 record flow).
/// </summary>
public class CreateSeasonalAidDistributionValidator : AbstractValidator<CreateSeasonalAidDistributionDto>
{
    public CreateSeasonalAidDistributionValidator()
    {
        RuleFor(x => x.BeneficiaryId)
            .NotEmpty().WithMessage("Beneficiary ID is required");

        RuleFor(x => x.DistributionDate)
            .NotEmpty().WithMessage("Distribution date is required");

        RuleFor(x => x.AmountDistributed)
            .GreaterThan(0).WithMessage("Amount distributed must be greater than zero");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .MaximumLength(3).WithMessage("Currency code cannot exceed 3 characters");

        RuleFor(x => x.ReceivedBy)
            .NotEmpty().WithMessage("Received by name is required")
            .MaximumLength(100).WithMessage("Received by name cannot exceed 100 characters");

        RuleFor(x => x.RecipientRelationship)
            .MaximumLength(50).WithMessage("Recipient relationship cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.RecipientRelationship));

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters");

        RuleFor(x => x.DistributionMethod)
            .MaximumLength(50).WithMessage("Distribution method cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.DistributionMethod));

        RuleFor(x => x.DistributorName)
            .MaximumLength(100).WithMessage("Distributor name cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.DistributorName));
    }
}
