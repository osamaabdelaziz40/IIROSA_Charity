using FluentValidation;
using IIROSA.Application.DTOs.Family;

namespace IIROSA.Application.Validators.Family;

/// <summary>
/// Validator for TransferFamilyDto (UC-FAM-06 نقل الأسرة لجمعية أخرى).
/// Registered automatically via AddValidatorsFromAssembly (IIROSA.Application). Invoked in the
/// service layer before any transfer work begins — an invalid payload must refuse atomically,
/// writing nothing (legacy «Faild Operation» behaviour, meaningful message).
/// </summary>
public class TransferFamilyValidator : AbstractValidator<TransferFamilyDto>
{
    public TransferFamilyValidator()
    {
        // The receiving charity is mandatory (اختر الجمعية)
        RuleFor(x => x.NewCharityId)
            .NotEmpty().WithMessage("Receiving charity is required")
            .NotEqual(Guid.Empty).WithMessage("Receiving charity is required");

        // سبب النقل is optional free text
        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Transfer reason cannot exceed 500 characters");
    }
}
