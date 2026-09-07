using FluentValidation;

using IIROSA.Application.DTOs.Reports;

namespace IIROSA.Application.Validators.Reports;

/// <summary>
/// UC-RPT-30 (§23.U.30 أرقام الشيكات) — shape only: the batch number is mandatory;
/// whether a caller may use CharityId is the service's scope decision, not the validator's.
/// </summary>
public class ChequeNumbersFilterValidator : AbstractValidator<ChequeNumbersFilterDto>
{
    public ChequeNumbersFilterValidator()
    {
        RuleFor(x => x.OrpCheckBatchNo)
            .NotEmpty()
            .WithMessage("Batch number is required");
    }
}
