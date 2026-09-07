using FluentValidation;

using IIROSA.Application.DTOs.Reports;

namespace IIROSA.Application.Validators.Reports;

/// <summary>
/// UC-RPT-29 (§23.U.29 المستلمون / غير المستلمين / الموقوفون) — shape only: the batch
/// number is mandatory and the variant must be one of the three discriminator values;
/// whether a caller may use CharityId is the service's scope decision, not the validator's.
/// </summary>
public class PaymentsOutcomeFilterValidator : AbstractValidator<PaymentsOutcomeFilterDto>
{
    private static readonly string[] Variants = { "received", "notReceived", "stopped" };

    public PaymentsOutcomeFilterValidator()
    {
        RuleFor(x => x.OrpCheckBatchNo)
            .NotEmpty()
            .WithMessage("Batch number is required");

        RuleFor(x => x.Variant)
            .Must(v => Variants.Contains(v, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Variant must be one of: received, notReceived, stopped");
    }
}
