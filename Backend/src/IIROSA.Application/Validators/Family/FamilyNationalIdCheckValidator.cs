using FluentValidation;
using IIROSA.Application.DTOs.Family;

namespace IIROSA.Application.Validators.Family;

/// <summary>
/// UC-SYS-12 — the family/guardian national-id check needs the id itself; every other
/// scoping rule (charity claim vs explicit charityId, familyId exclusion) is resolved
/// server-side in the service, not by the payload shape.
/// </summary>
public class FamilyNationalIdCheckValidator : AbstractValidator<CheckFamilyNationalIdDto>
{
    public FamilyNationalIdCheckValidator()
    {
        RuleFor(x => x.NationalId)
            .NotEmpty().WithMessage("National id is required")
            .Must(nid => nid.Trim().Length > 0).WithMessage("National id is required");
    }
}
