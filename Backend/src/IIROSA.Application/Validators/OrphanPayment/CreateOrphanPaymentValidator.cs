using FluentValidation;
using IIROSA.Application.DTOs.OrphanPayment;

namespace IIROSA.Application.Validators.OrphanPayment;

/// <summary>
/// Validator for CreateOrphanPaymentDto (UC-PAY-02, §15.S.2 mandatory fields).
/// Wire field mapping (§15.S.2): رقم الدفعة→BatchNo, اسم الدفعة→GroupName,
/// الفترة من/الى→PaymentPeriodFrom/To, تاريخ بدء التوزيع→PaymentDate.
/// </summary>
public class CreateOrphanPaymentValidator : AbstractValidator<CreateOrphanPaymentDto>
{
    public CreateOrphanPaymentValidator()
    {
        RuleFor(x => x.GroupName)
            .NotEmpty().WithMessage("Group name is required")
            .MaximumLength(200).WithMessage("Group name cannot exceed 200 characters");

        RuleFor(x => x.PaymentDate)
            .NotEmpty().WithMessage("Payment date is required");

        RuleFor(x => x.PaymentPeriodFrom)
            .NotEmpty().WithMessage("Payment period start date is required");

        RuleFor(x => x.PaymentPeriodTo)
            .NotEmpty().WithMessage("Payment period end date is required");

        RuleFor(x => x)
            .Must(x => x.PaymentPeriodFrom <= x.PaymentPeriodTo)
            .WithMessage("Payment period start date must be before end date")
            .OverridePropertyName(nameof(CreateOrphanPaymentDto.PaymentPeriodFrom));

        RuleFor(x => x.BatchNo)
            .MaximumLength(50).WithMessage("Batch number cannot exceed 50 characters")
            // Review P26: '/' and '%' make by-batch-no/{batchNo} unreachable (route
            // segment / URL decoding) — numbers must stay URL-segment-safe.
            .Matches("^[^/%]*$").WithMessage("Batch number cannot contain '/' or '%'");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters");

        RuleFor(x => x.Currency)
            .MaximumLength(10).WithMessage("Currency cannot exceed 10 characters");
    }
}
