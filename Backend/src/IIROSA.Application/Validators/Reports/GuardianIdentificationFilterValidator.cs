using FluentValidation;

using IIROSA.Application.DTOs.Reports;

namespace IIROSA.Application.Validators.Reports;

/// <summary>
/// §23.U.37 كشوف تعريف العائل والأرامل — variant membership, the single-family parameter rule
/// (§23.U.37's missing-parameter exception), and the start-date shape. Invoked in the service
/// (platform rule).
/// </summary>
public class GuardianIdentificationFilterValidator : AbstractValidator<GuardianIdentificationSheetFilterDto>
{
    public GuardianIdentificationFilterValidator()
    {
        RuleFor(x => x.Variant)
            .IsInEnum()
            .WithMessage("Variant must be one of: AllGuardians, WidowsOnly, SingleFamily");

        // §23.U.37 exception flow — أسرة محددة without a family is a required parameter missing.
        RuleFor(x => x.FamilyId)
            .NotNull()
            .NotEmpty()
            .When(x => x.Variant == GuardianIdentificationSheetVariant.SingleFamily)
            .WithMessage("Family is required for the single-family variant");
    }
}
