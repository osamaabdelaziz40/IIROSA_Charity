using FluentValidation;
using IIROSA.Application.DTOs.TechnicalSupport;

namespace IIROSA.Application.Validators.TechnicalSupport;

/// <summary>
/// Validator for <see cref="UpdateSupportTicketDto"/> (UC-CST-04).
/// </summary>
/// <remarks>
/// Keeps the update path as strict as the create path — without it every length/presence
/// rule was bypassable by creating a minimal ticket and then amending it.
/// Registered automatically via AddValidatorsFromAssembly (IIROSA.Application).
/// </remarks>
public class UpdateSupportTicketValidator : AbstractValidator<UpdateSupportTicketDto>
{
    public UpdateSupportTicketValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Ticket id is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required")
            .MaximumLength(4000).WithMessage("Message cannot exceed 4000 characters");

        // Lookup FKs are Restrict — 0 would reach SQL and surface as a 500 leaking
        // constraint detail instead of a field error.
        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category is required");

        RuleFor(x => x.PriorityId)
            .GreaterThan(0).WithMessage("Priority is required");

        // Optional on the wire — null leaves the status unchanged (edit form has no status control).
        RuleFor(x => x.StatusId)
            .Must(id => !id.HasValue || id.Value > 0).WithMessage("Status is invalid");
    }
}
