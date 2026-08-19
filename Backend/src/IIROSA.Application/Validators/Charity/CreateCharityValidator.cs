using FluentValidation;
using IIROSA.Application.DTOs.Charity;

namespace IIROSA.Application.Validators.Charity;

/// <summary>
/// Validator for <see cref="CreateCharityDto"/> (UC-CHR-03).
/// </summary>
/// <remarks>
/// The Angular form carries its own required/maxlength rules, but those bind only the screen. This
/// is the rule the system actually enforces: any API client, or a form change that drops a
/// validator, is still refused here.
///
/// Lengths mirror <c>CharityConfiguration</c> exactly. Where they disagree the database wins and
/// the save fails with a truncation error the caller cannot act on, so they are kept in step
/// deliberately rather than approximated.
/// </remarks>
public class CreateCharityValidator : AbstractValidator<CreateCharityDto>
{
    public CreateCharityValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Charity name is required")
            .MaximumLength(200).WithMessage("Charity name cannot exceed 200 characters");

        RuleFor(x => x.Code)
            .MaximumLength(50).WithMessage("Charity code cannot exceed 50 characters");

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

        // Location is the basis of country scoping, so an unset id is not merely a missing field —
        // it would leave the record outside every tenancy filter.
        RuleFor(x => x.CountryId)
            .GreaterThan(0).WithMessage("Country is required");

        RuleFor(x => x.RegionId)
            .GreaterThan(0).WithMessage("Region is required");

        RuleFor(x => x.CenterId)
            .GreaterThan(0).WithMessage("Centre is required");

        // BankId is a Restrict foreign key. Without this, 0 reaches SQL and the resulting
        // DbUpdateException surfaces as a 500 carrying constraint and table names, instead of a
        // 400 the operator can act on. [Required] cannot express this: it is a no-op on a
        // non-nullable int.
        RuleFor(x => x.BankId)
            .GreaterThan(0).WithMessage("Bank is required");

        RuleFor(x => x.IBAN)
            .MaximumLength(34).WithMessage("IBAN cannot exceed 34 characters");

        // The remaining columns carry HasMaxLength in CharityConfiguration. Only the length is
        // enforced here: whether each is mandatory is governed by the DataAnnotations on the DTO,
        // which the [ApiController] model-state filter applies before this validator runs. What
        // the database will refuse at write time is bounded here so it returns a field error
        // rather than a truncation 500.
        RuleFor(x => x.StreetName).MaximumLength(200);
        RuleFor(x => x.Village).MaximumLength(100);
        RuleFor(x => x.PostalCode).MaximumLength(20);
        RuleFor(x => x.MailBox).MaximumLength(20);
        RuleFor(x => x.Phone2).MaximumLength(20);
        RuleFor(x => x.HomePhone).MaximumLength(20);
        RuleFor(x => x.Fax).MaximumLength(20);
        RuleFor(x => x.BankAccount).MaximumLength(50);
        RuleFor(x => x.BossName).MaximumLength(100);
        RuleFor(x => x.BossJobName).MaximumLength(100);
        RuleFor(x => x.BossPhone1).MaximumLength(20);
        RuleFor(x => x.BossPhone2).MaximumLength(20);

        // The login is only described when the caller asked for one to be created. Validating these
        // unconditionally would refuse a charity recorded without an account.
        When(x => x.CreateUserAccount, () =>
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required when creating a user account")
                .EmailAddress().WithMessage("Username must be a valid email address");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required when creating a user account")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long");
        });
    }
}
