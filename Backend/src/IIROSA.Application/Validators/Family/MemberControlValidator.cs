using FluentValidation;
using IIROSA.Application.DTOs.Family;

namespace IIROSA.Application.Validators.Family;

/// <summary>
/// UC-FAM-07/08 member-control validation — conditional on the legacy Action contract:
/// attaching (1) needs a target family code, detaching (0) needs a justification note.
/// </summary>
public class MemberControlValidator : AbstractValidator<MemberControlDto>
{
    public MemberControlValidator()
    {
        RuleFor(x => x.MemberType)
            .InclusiveBetween(1, 2)
            .WithMessage("MemberType must be 1 (orphan) or 2 (guardian)");

        RuleFor(x => x.Action)
            .InclusiveBetween(0, 1)
            .WithMessage("Action must be 0 (detach to a new holding family) or 1 (attach to an existing family)");

        RuleFor(x => x.TargetFamilyCode)
            .NotEmpty().When(x => x.Action == 1)
            .WithMessage("A target family code is required to attach a member")
            .MaximumLength(50).When(x => !string.IsNullOrEmpty(x.TargetFamilyCode));

        RuleFor(x => x.Justification)
            .NotEmpty().When(x => x.Action == 0)
            .WithMessage("A justification is required when detaching a member to a new holding family")
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Justification));
    }
}
