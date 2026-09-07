using FluentValidation;

using IIROSA.Application.DTOs.Reports;

namespace IIROSA.Application.Validators.Reports;

/// <summary>
/// UC-RPT-31 (§23.U.31 كروت الاستلام) — shape only: the batch number is mandatory;
/// whether a caller may use CharityId is the service's scope decision, not the validator's.
/// </summary>
public class ReceiptCardsFilterValidator : AbstractValidator<ReceiptCardsFilterDto>
{
    public ReceiptCardsFilterValidator()
    {
        RuleFor(x => x.OrpCheckBatchNo)
            .NotEmpty()
            .WithMessage("Batch number is required");
    }
}
