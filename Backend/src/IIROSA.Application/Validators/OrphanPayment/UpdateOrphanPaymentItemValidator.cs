using FluentValidation;
using IIROSA.Application.DTOs.OrphanPayment;

namespace IIROSA.Application.Validators.OrphanPayment;

/// <summary>
/// Validator for UpdateOrphanPaymentItemDto (§15.1 action model, 10-9).
/// Row id + action are always required; per-action rules:
/// action 0 ⇒ Flag required; actions 3/4 ⇒ ChiqueNum + BenificiaryName required
/// (review P23 — the story letter says 3/4, not 3 only; the service still refuses
/// actions 2..4 loudly until 10-12 ships).
/// </summary>
public class UpdateOrphanPaymentItemValidator : AbstractValidator<UpdateOrphanPaymentItemDto>
{
    public UpdateOrphanPaymentItemValidator()
    {
        RuleFor(x => x.OrphanPaymentItemId)
            .NotEmpty().WithMessage("Orphan payment item id is required");

        RuleFor(x => x.Action)
            .InclusiveBetween(0, 4).WithMessage("Action must be between 0 and 4");

        // Action 0 (stop/resume) needs the direction
        RuleFor(x => x.Flag)
            .NotNull().WithMessage("Flag is required for the stop/resume action")
            .When(x => x.Action == 0);

        // Actions 3/4 (cheque record / clear-and-replace) need their payload
        RuleFor(x => x.ChiqueNum)
            .NotEmpty().WithMessage("Cheque number is required for the cheque action")
            .MaximumLength(50).WithMessage("Cheque number cannot exceed 50 characters")
            .When(x => x.Action == 3 || x.Action == 4);

        RuleFor(x => x.BenificiaryName)
            .NotEmpty().WithMessage("Beneficiary (collector) name is required for the cheque action")
            .MaximumLength(200).WithMessage("Beneficiary name cannot exceed 200 characters")
            .When(x => x.Action == 3 || x.Action == 4);
    }
}
