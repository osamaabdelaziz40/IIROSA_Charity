using FluentValidation;
using IIROSA.Application.DTOs.Charity;

namespace IIROSA.Application.Validators.Charity;

/// <summary>
/// Validator for <see cref="UpdateCharityDto"/> (UC-CHR-05).
/// </summary>
/// <remarks>
/// Mirrors <see cref="CreateCharityValidator"/>. Without it every rule the create path enforces
/// was bypassable by creating a minimal record and then amending it, which made the create-side
/// validation decorative.
/// </remarks>
public class UpdateCharityValidator : AbstractValidator<UpdateCharityDto>
{
    public UpdateCharityValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Charity id is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Charity name is required")
            .MaximumLength(200).WithMessage("Charity name cannot exceed 200 characters");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required")
            .MaximumLength(500).WithMessage("Address cannot exceed 500 characters");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required")
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid email address")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");

        RuleFor(x => x.NGOType)
            .MaximumLength(100).WithMessage("Charity type cannot exceed 100 characters");

        RuleFor(x => x.CountryId)
            .GreaterThan(0).WithMessage("Country is required");

        RuleFor(x => x.RegionId)
            .GreaterThan(0).WithMessage("Region is required");

        RuleFor(x => x.CenterId)
            .GreaterThan(0).WithMessage("Centre is required");

        // Also guarded on create; a Restrict FK, so 0 reaches SQL and surfaces as a 500 that
        // leaks constraint detail rather than a field error.
        RuleFor(x => x.BankId)
            .GreaterThan(0).WithMessage("Bank is required");

        RuleFor(x => x.IBAN)
            .MaximumLength(34).WithMessage("IBAN cannot exceed 34 characters");
    }
}
