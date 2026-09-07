using FluentValidation;
using IIROSA.Application.DTOs.IncomingOutgoing;

namespace IIROSA.Application.Validators.Correspondence;

// §21.S.5 field contract (UC-COR-13): serial read-only (server-side allocation), the
// category comes from the table-backed catalogue (UC-COR-17) and the incoming letter
// being replied to (ردا علي خطاب) is mandatory.

/// <summary>
/// Validator for CreateOutgoingDto (epic 16, UC-COR-13 / §21.S.5)
/// </summary>
public class CreateOutgoingValidator : AbstractValidator<CreateOutgoingDto>
{
    public CreateOutgoingValidator()
    {
        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Letter date is required");

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Subject is required")
            .MaximumLength(500).WithMessage("Subject cannot exceed 500 characters");

        RuleFor(x => x.DepartmentId)
            .NotEmpty().WithMessage("Routing department is required")
            .GreaterThan(0).WithMessage("Routing department must be a positive value");

        RuleFor(x => x.OutgoingCategoryId)
            .NotEmpty().WithMessage("Outgoing category is required")
            .GreaterThan(0).WithMessage("Outgoing category must be a positive value");

        // ردا علي خطاب is optional at create (§21.S.5 offers the empty "-" option): the
        // first letter of a charity must be creatable before any reply target exists —
        // mandatory-both-ways would dead-lock an empty register (review P4). Incoming's
        // reply-to (§21.S.2) stays mandatory.
    }
}

/// <summary>
/// Validator for UpdateOutgoingDto (epic 16, UC-COR-15) — same field contract as create;
/// the serial and charity ownership are immutable.
/// </summary>
public class UpdateOutgoingValidator : AbstractValidator<UpdateOutgoingDto>
{
    public UpdateOutgoingValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Letter id is required");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Letter date is required");

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Subject is required")
            .MaximumLength(500).WithMessage("Subject cannot exceed 500 characters");

        RuleFor(x => x.DepartmentId)
            .NotEmpty().WithMessage("Routing department is required")
            .GreaterThan(0).WithMessage("Routing department must be a positive value");

        RuleFor(x => x.OutgoingCategoryId)
            .NotEmpty().WithMessage("Outgoing category is required")
            .GreaterThan(0).WithMessage("Outgoing category must be a positive value");

        // Reply-to optional on update as well (review P4 — same §21.S.5 "-" option)
    }
}
