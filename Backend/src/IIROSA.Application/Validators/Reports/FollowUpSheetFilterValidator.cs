using FluentValidation;

using IIROSA.Application.DTOs.Reports;

namespace IIROSA.Application.Validators.Reports;

/// <summary>
/// §23.U.36 كشوف المتابعة والتسليم — variant membership, window order, page bounds. Invoked in
/// the service (platform rule); bounds mirror the epic's other report validators.
/// </summary>
public class FollowUpSheetFilterValidator : AbstractValidator<FollowUpSheetFilterDto>
{
    public FollowUpSheetFilterValidator()
    {
        RuleFor(x => x.Variant)
            .IsInEnum()
            .WithMessage("Variant must be one of: FollowUp, FollowUpFamily, Tasleem");

        RuleFor(x => x.DateTo)
            .GreaterThanOrEqualTo(x => x.DateFrom)
            .When(x => x.DateFrom.HasValue && x.DateTo.HasValue)
            .WithMessage("To date must not be before the from date");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);
    }
}
