using FluentValidation;
using IIROSA.Application.DTOs.Family;

namespace IIROSA.Application.Validators.Family;

/// <summary>
/// Validator for <see cref="OrphanSearchFilterDto"/> (UC-ORP-02/03/07).
/// </summary>
/// <remarks>
/// Registered automatically via AddValidatorsFromAssembly (IIROSA.Application). Invoked in the
/// service layer, as every other validator in this assembly is.
/// </remarks>
public class OrphanSearchFilterValidator : AbstractValidator<OrphanSearchFilterDto>
{
    public OrphanSearchFilterValidator()
    {
        RuleFor(x => x.Search)
            .MaximumLength(200).WithMessage("Search term cannot exceed 200 characters");

        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be 1 or greater");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 200).WithMessage("Page size must be between 1 and 200");

        RuleFor(x => x.CodingStatus)
            .Must(value => value == null || value.Equals("Pending", StringComparison.OrdinalIgnoreCase)
                                      || value.Equals("Coded", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Coding status must be 'Pending' or 'Coded'");
    }
}

/// <summary>
/// Validator for <see cref="OrphanEligibilityCheckDto"/> (UC-ORP-01).
/// </summary>
public class OrphanEligibilityCheckValidator : AbstractValidator<OrphanEligibilityCheckDto>
{
    public OrphanEligibilityCheckValidator()
    {
        // Optional since P3: with only a family id given, the check reduces to the family gate.
        // Length mirrors Orphan.NationalId (50) — where they disagree the database wins.
        RuleFor(x => x.NationalId)
            .MaximumLength(50).WithMessage("National ID cannot exceed 50 characters");
    }
}

/// <summary>
/// Validator for <see cref="OrphanCodeCheckFilterDto"/> (UC-ORP-05).
/// </summary>
public class OrphanCodeCheckFilterValidator : AbstractValidator<OrphanCodeCheckFilterDto>
{
    public OrphanCodeCheckFilterValidator()
    {
        // NotEmpty alone lets a blank string of spaces through (P5) — require real content.
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required")
            .Must(code => code!.Trim().Length > 0).WithMessage("Code cannot be only whitespace")
            .MaximumLength(50).WithMessage("Code cannot exceed 50 characters");
    }
}

/// <summary>
/// Validator for <see cref="AssignOrphanCodeDto"/> (UC-ORP-06).
/// </summary>
public class AssignOrphanCodeValidator : AbstractValidator<AssignOrphanCodeDto>
{
    public AssignOrphanCodeValidator()
    {
        RuleFor(x => x.OrphanId)
            .NotEmpty().WithMessage("Orphan is required");

        // NotEmpty alone lets a blank string of spaces through (P5) — require real content.
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required")
            .Must(code => code!.Trim().Length > 0).WithMessage("Code cannot be only whitespace")
            .MaximumLength(50).WithMessage("Code cannot exceed 50 characters");
    }
}

/// <summary>
/// Validator for <see cref="PhoneCheckFilterDto"/> (UC-ORP-10).
/// </summary>
public class PhoneCheckFilterValidator : AbstractValidator<PhoneCheckFilterDto>
{
    public PhoneCheckFilterValidator()
    {
        // Digits, a leading +, spaces and hyphens — the shapes the five phone columns hold.
        // At least one digit is required (P7): a bare "---" or "   " is not a phone number.
        // Trim only at compare time: folding a trunk-prefix zero is locale-dependent and unsafe,
        // so the rule is an exact match after trimming (recorded in the 8-10 completion notes).
        RuleFor(x => x.Number)
            .NotEmpty().WithMessage("Phone number is required")
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters")
            .Matches(@"^\+?(?=.*[0-9])[0-9\s\-]{3,20}$").WithMessage("Phone number must contain digits only, optionally prefixed with +");
    }
}
