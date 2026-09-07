using FluentValidation;
using IIROSA.Application.DTOs.CheckManagement;

namespace IIROSA.Application.Validators.CheckManagement;

/// <summary>
/// UC-CHQ-02 — issue a cheque. The mandatory set of §16.S.2:
/// البنك، اسم المستفيد، تاريخ الشيك، رقم الشيك، العملة، المبلغ.
/// </summary>
public class CreateCheckValidator : AbstractValidator<CreateCheckDto>
{
    public CreateCheckValidator()
    {
        RuleFor(x => x.BankId)
            .NotNull().WithMessage("Bank is required");

        RuleFor(x => x.BeneficiaryName)
            .NotEmpty().WithMessage("Beneficiary name is required")
            .MaximumLength(200).WithMessage("Beneficiary name cannot exceed 200 characters");

        RuleFor(x => x.CheckDate)
            .NotEmpty().WithMessage("Check date is required");

        RuleFor(x => x.CheckNumber)
            .NotEmpty().WithMessage("Check number is required")
            .MaximumLength(50).WithMessage("Check number cannot exceed 50 characters");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .MaximumLength(10).WithMessage("Currency cannot exceed 10 characters")
            .Matches("^[A-Za-z]{3}$").WithMessage("Currency must be a 3-letter ISO code")
            .When(x => !string.IsNullOrEmpty(x.Currency), ApplyConditionTo.CurrentValidator);

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0");

        RuleFor(x => x.ChequeType)
            .Must(t => t == "Orphans" || t == "Individuals")
            .WithMessage("Cheque type must be Orphans or Individuals")
            .When(x => !string.IsNullOrEmpty(x.ChequeType));

        // Optional fields — length guards only.
        RuleFor(x => x.BeneficiaryType).MaximumLength(50);
        RuleFor(x => x.BeneficiaryAddress).MaximumLength(500);
        RuleFor(x => x.BeneficiaryPhone).MaximumLength(30);
        RuleFor(x => x.BeneficiaryEmail)
            .EmailAddress().WithMessage("Invalid email format")
            .When(x => !string.IsNullOrEmpty(x.BeneficiaryEmail));
        RuleFor(x => x.BeneficiaryIdNumber).MaximumLength(100);
        RuleFor(x => x.BankBranch).MaximumLength(200);
        RuleFor(x => x.AccountNumber).MaximumLength(50);
        RuleFor(x => x.AmountInWords).MaximumLength(500);
        RuleFor(x => x.Comment).MaximumLength(2000);
    }
}

/// <summary>
/// UC-CHQ-04 — update a cheque; same field contract plus the record id.
/// </summary>
public class UpdateCheckValidator : AbstractValidator<UpdateCheckDto>
{
    public UpdateCheckValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Check id is required");

        RuleFor(x => x.BankId)
            .NotNull().WithMessage("Bank is required");

        RuleFor(x => x.BeneficiaryName)
            .NotEmpty().WithMessage("Beneficiary name is required")
            .MaximumLength(200).WithMessage("Beneficiary name cannot exceed 200 characters");

        RuleFor(x => x.CheckDate)
            .NotEmpty().WithMessage("Check date is required");

        RuleFor(x => x.CheckNumber)
            .NotEmpty().WithMessage("Check number is required")
            .MaximumLength(50).WithMessage("Check number cannot exceed 50 characters");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .MaximumLength(10).WithMessage("Currency cannot exceed 10 characters")
            .Matches("^[A-Za-z]{3}$").WithMessage("Currency must be a 3-letter ISO code")
            .When(x => !string.IsNullOrEmpty(x.Currency), ApplyConditionTo.CurrentValidator);

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0");

        RuleFor(x => x.ChequeType)
            .Must(t => t == "Orphans" || t == "Individuals")
            .WithMessage("Cheque type must be Orphans or Individuals")
            .When(x => !string.IsNullOrEmpty(x.ChequeType));

        RuleFor(x => x.BeneficiaryType).MaximumLength(50);
        RuleFor(x => x.BeneficiaryAddress).MaximumLength(500);
        RuleFor(x => x.BeneficiaryPhone).MaximumLength(30);
        RuleFor(x => x.BeneficiaryEmail)
            .EmailAddress().WithMessage("Invalid email format")
            .When(x => !string.IsNullOrEmpty(x.BeneficiaryEmail));
        RuleFor(x => x.BeneficiaryIdNumber).MaximumLength(100);
        RuleFor(x => x.BankBranch).MaximumLength(200);
        RuleFor(x => x.AccountNumber).MaximumLength(50);
        RuleFor(x => x.AmountInWords).MaximumLength(500);
        RuleFor(x => x.Comment).MaximumLength(2000);
    }
}

/// <summary>
/// Register/statement filter — page bounds and the cheque-type vocabulary.
/// </summary>
public class CheckFilterValidator : AbstractValidator<CheckFilterDto>
{
    public CheckFilterValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200);
        RuleFor(x => x.ChequeType)
            .Must(t => t == "Orphans" || t == "Individuals")
            .WithMessage("Cheque type must be Orphans or Individuals")
            .When(x => !string.IsNullOrEmpty(x.ChequeType));
        RuleFor(x => x.DateTo)
            .GreaterThanOrEqualTo(x => x.DateFrom)
            .WithMessage("Date to cannot be before date from")
            .When(x => x.DateFrom.HasValue && x.DateTo.HasValue);
    }
}
