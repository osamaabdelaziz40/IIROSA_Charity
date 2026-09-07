using FluentValidation;
using IIROSA.Application.DTOs.HqTransfers;

namespace IIROSA.Application.Validators.HqTransfers;

/// <summary>
/// UC-TRF-08 line rules. The state gating (AC 3) is the legacy «Failed Operation» rule:
/// execution and arrival data may be recorded only once the line is marked تم التنفيذ —
/// a line still لم ينفذ (or unset) carrying any of those fields is refused.
/// Registered automatically via AddValidatorsFromAssembly (IIROSA.Application).
/// </summary>
public class SaveHqTransferDetailLineValidator : AbstractValidator<SaveHqTransferDetailLineDto>
{
    public SaveHqTransferDetailLineValidator()
    {
        RuleFor(x => x.TransferNumber)
            .NotEmpty()
            .WithMessage("Transfer number is required")
            .Must(v => !string.IsNullOrWhiteSpace(v))
            .WithMessage("Transfer number must not be blank")
            .MaximumLength(50)
            .WithMessage("Transfer number must not exceed 50 characters");

        // decimal(18,2) column — refuse out-of-scale values as field errors, not save-time 500s
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than zero")
            .ScalePrecision(2, 18)
            .WithMessage("Amount must not exceed 2 decimal places and 18 digits");

        RuleFor(x => x.ArrivalAmount)
            .GreaterThan(0)
            .When(x => x.ArrivalAmount.HasValue)
            .WithMessage("Arrival amount must be greater than zero when provided")
            .ScalePrecision(2, 18)
            .When(x => x.ArrivalAmount.HasValue)
            .WithMessage("Arrival amount must not exceed 2 decimal places and 18 digits");

        // State gating — ExecutionDate / ArrivalDate / ArrivalAmount only when executed
        RuleFor(x => x.ExecutionDate)
            .Null()
            .When(x => x.IsExecuted != true)
            .WithMessage("Execution date can only be set when the line is marked as executed");

        RuleFor(x => x.ArrivalDate)
            .Null()
            .When(x => x.IsExecuted != true)
            .WithMessage("Arrival date can only be set when the line is marked as executed");

        RuleFor(x => x.ArrivalAmount)
            .Null()
            .When(x => x.IsExecuted != true)
            .WithMessage("Arrival amount can only be set when the line is marked as executed");
    }
}
