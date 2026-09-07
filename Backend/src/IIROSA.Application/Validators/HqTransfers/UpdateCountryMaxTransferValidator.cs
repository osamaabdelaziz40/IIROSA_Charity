using FluentValidation;
using IIROSA.Application.DTOs.HqTransfers;

namespace IIROSA.Application.Validators.HqTransfers;

/// <summary>
/// UC-TRF-07: the country must be identified; a present ceiling must be positive. NULL is
/// explicitly legal — it means unlimited (never treated as zero).
/// </summary>
public class UpdateCountryMaxTransferValidator : AbstractValidator<UpdateCountryMaxTransferDto>
{
    public UpdateCountryMaxTransferValidator()
    {
        RuleFor(x => x.CountryId)
            .NotEmpty()
            .WithMessage("Country is required");

        RuleFor(x => x.MaxTransferAmount)
            .GreaterThan(0)
            .When(x => x.MaxTransferAmount.HasValue)
            .WithMessage("Maximum transfer amount must be greater than zero when provided")
            // decimal(18,2) column — a 0.005 ceiling would store as 0.00 and block the
            // whole country; refuse out-of-scale values as a field error, not a save-time 500
            .ScalePrecision(2, 18)
            .When(x => x.MaxTransferAmount.HasValue)
            .WithMessage("Maximum transfer amount must not exceed 2 decimal places and 18 digits");
    }
}
