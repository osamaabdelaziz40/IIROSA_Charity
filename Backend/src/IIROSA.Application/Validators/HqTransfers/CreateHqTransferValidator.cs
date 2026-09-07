using FluentValidation;
using IIROSA.Application.DTOs.HqTransfers;

namespace IIROSA.Application.Validators.HqTransfers;

/// <summary>
/// Validator for CreateHqTransferDto (UC-TRF-02 / §22.S.2 — 11 mandatory fields).
/// DB-dependent rules (lookup existence/active) live in the service body, per the epic-14
/// FK-validation precedent.
/// </summary>
public class CreateHqTransferValidator : AbstractValidator<CreateHqTransferDto>
{
    public CreateHqTransferValidator()
    {
        // Mandatory lookups
        RuleFor(x => x.CountryId)
            .NotEmpty().WithMessage("Country is required")
            .GreaterThan(0).WithMessage("Country must be a positive value");

        RuleFor(x => x.DepartmentId)
            .NotEmpty().WithMessage("Department is required")
            .GreaterThan(0).WithMessage("Department must be a positive value");

        // Operation identification
        RuleFor(x => x.OperationNumber)
            .NotEmpty().WithMessage("Operation number is required")
            .Must(v => !string.IsNullOrWhiteSpace(v)).WithMessage("Operation number must not be blank")
            .MaximumLength(50).WithMessage("Operation number cannot exceed 50 characters");

        RuleFor(x => x.FinYear)
            .NotEmpty().WithMessage("Fiscal year is required")
            .Must(v => !string.IsNullOrWhiteSpace(v)).WithMessage("Fiscal year must not be blank")
            .MaximumLength(10).WithMessage("Fiscal year cannot exceed 10 characters");

        RuleFor(x => x.PaymentNumber)
            .NotEmpty().WithMessage("Payment number is required")
            .InclusiveBetween(1, 4).WithMessage("Payment number must be between 1 and 4");

        // Period
        RuleFor(x => x.DateFrom)
            .NotEmpty().WithMessage("From date is required");

        RuleFor(x => x.DateTo)
            .NotEmpty().WithMessage("To date is required")
            .GreaterThanOrEqualTo(x => x.DateFrom).WithMessage("To date must be on or after the from date");

        // Financials — decimal(18,2) column: refuse out-of-scale values here rather than
        // losing them to silent rounding (0.004 → 0.00) or an overflow 500 at save
        RuleFor(x => x.AmountOfPayment)
            .NotEmpty().WithMessage("Payment amount is required")
            .GreaterThan(0).WithMessage("Payment amount must be greater than 0")
            .ScalePrecision(2, 18).WithMessage("Payment amount must not exceed 2 decimal places and 18 digits");

        RuleFor(x => x.Statement)
            .MaximumLength(500).WithMessage("Statement cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Statement));

        RuleFor(x => x.BeneficiariesNumber)
            .NotEmpty().WithMessage("Beneficiaries number is required")
            .GreaterThan(0).WithMessage("Beneficiaries number must be greater than 0");

        // Transaction
        RuleFor(x => x.TransactionNumber)
            .NotEmpty().WithMessage("Transaction number is required")
            .Must(v => !string.IsNullOrWhiteSpace(v)).WithMessage("Transaction number must not be blank")
            .MaximumLength(50).WithMessage("Transaction number cannot exceed 50 characters");

        RuleFor(x => x.TransactionDate)
            .NotEmpty().WithMessage("Transaction date is required");
    }
}
