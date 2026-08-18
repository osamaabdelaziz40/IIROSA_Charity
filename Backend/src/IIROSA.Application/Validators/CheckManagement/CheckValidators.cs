using FluentValidation;
using IIROSA.Application.DTOs.CheckManagement;

namespace IIROSA.Application.Validators.CheckManagement;

/// <summary>
/// Validator for CreateCheckDto (UC-11.1)
/// </summary>
public class CreateCheckValidator : AbstractValidator<CreateCheckDto>
{
    public CreateCheckValidator()
    {
        // Required fields
        RuleFor(x => x.CheckNumber)
            .NotEmpty().WithMessage("Check number is required")
            .MaximumLength(50).WithMessage("Check number cannot exceed 50 characters");

        RuleFor(x => x.CheckDate)
            .NotEmpty().WithMessage("Check date is required")
            .GreaterThanOrEqualTo(DateTime.Today.AddMonths(-1)).WithMessage("Check date cannot be more than 1 month in the past");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0");

        RuleFor(x => x.BeneficiaryName)
            .NotEmpty().WithMessage("Beneficiary name is required")
            .MaximumLength(200).WithMessage("Beneficiary name cannot exceed 200 characters");

        // Optional fields with constraints
        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(x => x.CheckDate).WithMessage("Due date cannot be before check date")
            .When(x => x.DueDate.HasValue);

        RuleFor(x => x.Currency)
            .Must(BeValidCurrency).WithMessage("Currency must be EGP, SAR, or USD");

        RuleFor(x => x.BeneficiaryType)
            .Must(BeValidBeneficiaryType).WithMessage("Beneficiary type must be Individual, Company, Charity, Supplier, or Employee")
            .When(x => !string.IsNullOrEmpty(x.BeneficiaryType));

        RuleFor(x => x.BeneficiaryAddress)
            .MaximumLength(300).WithMessage("Beneficiary address cannot exceed 300 characters");

        RuleFor(x => x.BeneficiaryPhone)
            .MaximumLength(50).WithMessage("Beneficiary phone cannot exceed 50 characters");

        RuleFor(x => x.BeneficiaryEmail)
            .EmailAddress().WithMessage("Invalid email format")
            .When(x => !string.IsNullOrEmpty(x.BeneficiaryEmail));

        RuleFor(x => x.BeneficiaryIdNumber)
            .MaximumLength(50).WithMessage("Beneficiary ID number cannot exceed 50 characters");

        RuleFor(x => x.AmountInWords)
            .MaximumLength(500).WithMessage("Amount in words cannot exceed 500 characters");

        RuleFor(x => x.PaymentReason)
            .Must(BeValidPaymentReason).WithMessage("Payment reason must be Salary, Supplier Refund, Expense, or Other")
            .When(x => !string.IsNullOrEmpty(x.PaymentReason));

        RuleFor(x => x.PaymentDescription)
            .MaximumLength(1000).WithMessage("Payment description cannot exceed 1000 characters");

        RuleFor(x => x.BankBranch)
            .MaximumLength(100).WithMessage("Bank branch cannot exceed 100 characters");

        RuleFor(x => x.AccountNumber)
            .MaximumLength(50).WithMessage("Account number cannot exceed 50 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters");
    }

    private bool BeValidCurrency(string currency)
    {
        return currency == "EGP" || currency == "SAR" || currency == "USD";
    }

    private bool BeValidBeneficiaryType(string? type)
    {
        return type == "Individual" || type == "Company" || type == "Charity" ||
               type == "Supplier" || type == "Employee";
    }

    private bool BeValidPaymentReason(string? reason)
    {
        return reason == "Salary" || reason == "Supplier Refund" || reason == "Expense" || reason == "Other";
    }
}

/// <summary>
/// Validator for UpdateCheckDto
/// </summary>
public class UpdateCheckValidator : AbstractValidator<UpdateCheckDto>
{
    public UpdateCheckValidator()
    {
        RuleFor(x => x.CheckNumber)
            .NotEmpty().WithMessage("Check number is required")
            .MaximumLength(50).WithMessage("Check number cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.CheckNumber));

        RuleFor(x => x.CheckDate)
            .GreaterThanOrEqualTo(DateTime.Today.AddMonths(-1)).WithMessage("Check date cannot be more than 1 month in the past")
            .When(x => x.CheckDate.HasValue);

        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(x => x.CheckDate).WithMessage("Due date cannot be before check date")
            .When(x => x.DueDate.HasValue && x.CheckDate.HasValue);

        RuleFor(x => x.Currency)
            .Must(BeValidCurrency).WithMessage("Currency must be EGP, SAR, or USD")
            .When(x => !string.IsNullOrEmpty(x.Currency));

        RuleFor(x => x.BeneficiaryType)
            .Must(BeValidBeneficiaryType).WithMessage("Beneficiary type must be Individual, Company, Charity, Supplier, or Employee")
            .When(x => !string.IsNullOrEmpty(x.BeneficiaryType));

        RuleFor(x => x.BeneficiaryName)
            .MaximumLength(200).WithMessage("Beneficiary name cannot exceed 200 characters");

        RuleFor(x => x.BeneficiaryAddress)
            .MaximumLength(300).WithMessage("Beneficiary address cannot exceed 300 characters");

        RuleFor(x => x.BeneficiaryPhone)
            .MaximumLength(50).WithMessage("Beneficiary phone cannot exceed 50 characters");

        RuleFor(x => x.BeneficiaryEmail)
            .EmailAddress().WithMessage("Invalid email format")
            .When(x => !string.IsNullOrEmpty(x.BeneficiaryEmail));

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0")
            .When(x => x.Amount.HasValue);

        RuleFor(x => x.AmountInWords)
            .MaximumLength(500).WithMessage("Amount in words cannot exceed 500 characters");

        RuleFor(x => x.PaymentReason)
            .Must(BeValidPaymentReason).WithMessage("Payment reason must be Salary, Supplier Refund, Expense, or Other")
            .When(x => !string.IsNullOrEmpty(x.PaymentReason));

        RuleFor(x => x.PaymentDescription)
            .MaximumLength(1000).WithMessage("Payment description cannot exceed 1000 characters");

        RuleFor(x => x.BankBranch)
            .MaximumLength(100).WithMessage("Bank branch cannot exceed 100 characters");

        RuleFor(x => x.AccountNumber)
            .MaximumLength(50).WithMessage("Account number cannot exceed 50 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters");
    }

    private bool BeValidCurrency(string currency)
    {
        return currency == "EGP" || currency == "SAR" || currency == "USD";
    }

    private bool BeValidBeneficiaryType(string? type)
    {
        return type == "Individual" || type == "Company" || type == "Charity" ||
               type == "Supplier" || type == "Employee";
    }

    private bool BeValidPaymentReason(string? reason)
    {
        return reason == "Salary" || reason == "Supplier Refund" || reason == "Expense" || reason == "Other";
    }
}

/// <summary>
/// Validator for SetCheckAmountDto (UC-11.3)
/// </summary>
public class SetCheckAmountValidator : AbstractValidator<SetCheckAmountDto>
{
    public SetCheckAmountValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Must(BeValidCurrency).WithMessage("Currency must be EGP, SAR, or USD");
    }

    private bool BeValidCurrency(string currency)
    {
        return currency == "EGP" || currency == "SAR" || currency == "USD";
    }
}

/// <summary>
/// Validator for SetCheckDateDto (UC-11.4)
/// </summary>
public class SetCheckDateValidator : AbstractValidator<SetCheckDateDto>
{
    public SetCheckDateValidator()
    {
        RuleFor(x => x.CheckDate)
            .NotEmpty().WithMessage("Check date is required");

        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(x => x.CheckDate).WithMessage("Due date cannot be before check date")
            .When(x => x.DueDate.HasValue);
    }
}

/// <summary>
/// Validator for MarkCheckClearedDto (UC-11.5)
/// </summary>
public class MarkCheckClearedValidator : AbstractValidator<MarkCheckClearedDto>
{
    public MarkCheckClearedValidator()
    {
        RuleFor(x => x.ClearanceDate)
            .NotEmpty().WithMessage("Clearance date is required")
            .GreaterThanOrEqualTo(DateTime.Today.AddMonths(-6)).WithMessage("Clearance date cannot be more than 6 months in the past");

        RuleFor(x => x.BankReference)
            .MaximumLength(100).WithMessage("Bank reference cannot exceed 100 characters");

        RuleFor(x => x.ClearanceNotes)
            .MaximumLength(500).WithMessage("Clearance notes cannot exceed 500 characters");
    }
}

/// <summary>
/// Validator for VoidCheckDto (UC-11.6)
/// </summary>
public class VoidCheckValidator : AbstractValidator<VoidCheckDto>
{
    public VoidCheckValidator()
    {
        RuleFor(x => x.VoidReason)
            .NotEmpty().WithMessage("Void reason is required")
            .Must(BeValidVoidReason).WithMessage("Void reason must be Lost, Stopped, Error, Expired, or Other");

        RuleFor(x => x.VoidDate)
            .NotEmpty().WithMessage("Void date is required");

        RuleFor(x => x.VoidNotes)
            .NotEmpty().WithMessage("Void notes are required")
            .MaximumLength(500).WithMessage("Void notes cannot exceed 500 characters");
    }

    private bool BeValidVoidReason(string reason)
    {
        return reason == "Lost" || reason == "Stopped" || reason == "Error" ||
               reason == "Expired" || reason == "Other";
    }
}

/// <summary>
/// Validator for CheckReportFilterDto (UC-11.10)
/// </summary>
public class CheckReportFilterValidator : AbstractValidator<CheckReportFilterDto>
{
    public CheckReportFilterValidator()
    {
        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required")
            .LessThanOrEqualTo(x => x.EndDate).WithMessage("Start date must be before end date");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required");

        RuleFor(x => x.CheckStatus)
            .Must(BeValidCheckStatus).WithMessage("Check status must be Pending, Issued, Cleared, or Void")
            .When(x => !string.IsNullOrEmpty(x.CheckStatus));

        RuleFor(x => x.GroupBy)
            .Must(BeValidGroupBy).WithMessage("Group by must be Status, Bank, or Beneficiary");
    }

    private bool BeValidCheckStatus(string? status)
    {
        return status == "Pending" || status == "Issued" || status == "Cleared" || status == "Void";
    }

    private bool BeValidGroupBy(string groupBy)
    {
        return groupBy == "Status" || groupBy == "Bank" || groupBy == "Beneficiary";
    }
}
