using FluentValidation;

using IIROSA.Application.DTOs.Reports;

namespace IIROSA.Application.Validators.Reports;

/// <summary>
/// UC-RPT-28 (§23.U.28 أيتام لم يصرف لهم) — the batch is the report's frame: PaymentId
/// mandatory; page bounds mirror the epic's other report validators.
/// </summary>
public class OrphansWithoutPaymentFilterValidator : AbstractValidator<OrphansWithoutPaymentFilterDto>
{
    public OrphansWithoutPaymentFilterValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty()
            .WithMessage("Payment batch is required");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);
    }
}
