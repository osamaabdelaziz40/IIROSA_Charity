using FluentValidation;
using IIROSA.Application.DTOs.IncomingOutgoing;

namespace IIROSA.Application.Validators.Correspondence;

// §21.S.2 field contract (UC-COR-04): the serial is shown read-only and allocated
// server-side — it is deliberately absent from the create/update payloads.

/// <summary>
/// Validator for CreateIncomingDto (epic 16, UC-COR-04 / §21.S.2)
/// </summary>
public class CreateIncomingValidator : AbstractValidator<CreateIncomingDto>
{
    public CreateIncomingValidator()
    {
        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Letter registration date is required");

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Subject is required")
            .MaximumLength(500).WithMessage("Subject cannot exceed 500 characters");

        RuleFor(x => x.DepartmentId)
            .NotEmpty().WithMessage("Routing department is required")
            .GreaterThan(0).WithMessage("Routing department must be a positive value");

        RuleFor(x => x.AssignedUserId)
            .NotEmpty().WithMessage("Assigned employee is required");

        RuleFor(x => x.LetterDescription)
            .NotEmpty().WithMessage("Letter description is required")
            .MaximumLength(4000).WithMessage("Letter description cannot exceed 4000 characters");

        RuleFor(x => x.LetterNumber)
            .NotEmpty().WithMessage("Letter number is required")
            .MaximumLength(100).WithMessage("Letter number cannot exceed 100 characters");

        RuleFor(x => x.LetterDate)
            .NotEmpty().WithMessage("Letter date is required");

        RuleFor(x => x.LetterDate)
            .LessThanOrEqualTo(x => x.Date)
            .WithMessage("Letter date cannot be after the registration date")
            .When(x => x.LetterDate.HasValue && x.Date.HasValue);

        // §21.S.2 — الحاله is one of the mandatory tri-state terms (معلق / تم الرد / تم عمل اللازم);
        // the service still defaults an omitted value to معلق at create
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Letter status is required");
    }
}

/// <summary>
/// Validator for UpdateIncomingDto (epic 16, UC-COR-06) — same field contract as create;
/// the serial and charity ownership are immutable.
/// </summary>
public class UpdateIncomingValidator : AbstractValidator<UpdateIncomingDto>
{
    public UpdateIncomingValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Letter id is required");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Letter registration date is required");

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Subject is required")
            .MaximumLength(500).WithMessage("Subject cannot exceed 500 characters");

        RuleFor(x => x.DepartmentId)
            .NotEmpty().WithMessage("Routing department is required")
            .GreaterThan(0).WithMessage("Routing department must be a positive value");

        RuleFor(x => x.AssignedUserId)
            .NotEmpty().WithMessage("Assigned employee is required");

        RuleFor(x => x.LetterDescription)
            .NotEmpty().WithMessage("Letter description is required")
            .MaximumLength(4000).WithMessage("Letter description cannot exceed 4000 characters");

        RuleFor(x => x.LetterNumber)
            .NotEmpty().WithMessage("Letter number is required")
            .MaximumLength(100).WithMessage("Letter number cannot exceed 100 characters");

        RuleFor(x => x.LetterDate)
            .NotEmpty().WithMessage("Letter date is required");

        RuleFor(x => x.LetterDate)
            .LessThanOrEqualTo(x => x.Date)
            .WithMessage("Letter date cannot be after the registration date")
            .When(x => x.LetterDate.HasValue && x.Date.HasValue);

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Letter status is required");
    }
}
