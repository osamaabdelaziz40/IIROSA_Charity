using FluentValidation;
using IIROSA.Application.DTOs.OrphanPayment;

namespace IIROSA.Application.Validators.OrphanPayment;

/// <summary>
/// Field set for editing a payment batch (UC-PAY-04 / 10-4).
/// Mirrors CreateOrphanPaymentValidator — every data-entry field mandatory, the
/// id comes from the route, not the body.
/// </summary>
public class UpdateOrphanPaymentValidator : AbstractValidator<UpdateOrphanPaymentDto>
{
    public UpdateOrphanPaymentValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.GroupName)
            .NotEmpty().WithMessage("Group name is required")
            .MaximumLength(200).WithMessage("Group name cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

        RuleFor(x => x.PaymentPeriodFrom)
            .NotEmpty().WithMessage("Payment period start date is required");

        RuleFor(x => x.PaymentPeriodTo)
            .NotEmpty().WithMessage("Payment period end date is required");

        RuleFor(x => x.GroupDate)
            .NotEmpty().WithMessage("Group date is required");

        RuleFor(x => x.PaymentDate)
            .NotEmpty().WithMessage("Payment date is required");

        RuleFor(x => x)
            .Must(x => x.PaymentPeriodFrom <= x.PaymentPeriodTo)
            .OverridePropertyName(nameof(UpdateOrphanPaymentDto.PaymentPeriodFrom))
            .WithMessage("Payment period start date must be before end date");

        RuleFor(x => x.BatchNo)
            .NotEmpty().WithMessage("Batch number is required")
            .MaximumLength(50).WithMessage("Batch number cannot exceed 50 characters")
            // Review P26: '/' and '%' make by-batch-no/{batchNo} unreachable (route
            // segment / URL decoding) — numbers must stay URL-segment-safe.
            .Matches("^[^/%]*$").WithMessage("Batch number cannot contain '/' or '%'");

        RuleFor(x => x.ExchangeRate)
            .NotNull().WithMessage("Exchange rate is required")
            .InclusiveBetween(0, 1000).WithMessage("Exchange rate must be between 0 and 1000");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .MaximumLength(10).WithMessage("Currency cannot exceed 10 characters");

        RuleFor(x => x.ShowOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Show order must be zero or greater");

        RuleFor(x => x.Notes)
            .NotEmpty().WithMessage("Notes are required")
            .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters");
    }
}
