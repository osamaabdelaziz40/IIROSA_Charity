using FluentValidation;
using IIROSA.Application.DTOs.OrphanPayment;

namespace IIROSA.Application.Validators.OrphanPayment;

/// <summary>
/// §15.S.2 mandatory field set for editing a payment batch (UC-PAY-04 / 10-4).
/// Mirrors CreateOrphanPaymentValidator — the id comes from the route, not the body.
/// </summary>
public class UpdateOrphanPaymentValidator : AbstractValidator<UpdateOrphanPaymentDto>
{
    public UpdateOrphanPaymentValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.GroupName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.PaymentDate)
            .NotEmpty();

        RuleFor(x => x.PaymentPeriodFrom)
            .NotEmpty();

        RuleFor(x => x.PaymentPeriodTo)
            .NotEmpty();

        RuleFor(x => x)
            .Must(x => x.PaymentPeriodFrom <= x.PaymentPeriodTo)
            .OverridePropertyName(nameof(UpdateOrphanPaymentDto.PaymentPeriodFrom))
            .WithMessage("Payment period start date must be before end date");

        RuleFor(x => x.BatchNo)
            .MaximumLength(50)
            // Review P26: '/' and '%' make by-batch-no/{batchNo} unreachable (route
            // segment / URL decoding) — numbers must stay URL-segment-safe.
            .Matches("^[^/%]*$").WithMessage("Batch number cannot contain '/' or '%'");

        RuleFor(x => x.Description)
            .MaximumLength(1000);

        RuleFor(x => x.Notes)
            .MaximumLength(2000);

        RuleFor(x => x.Currency)
            .MaximumLength(10);
    }
}
