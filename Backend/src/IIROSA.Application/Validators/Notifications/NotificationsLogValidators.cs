using FluentValidation;
using IIROSA.Application.DTOs.Notifications;

namespace IIROSA.Application.Validators.Notifications;

/// <summary>
/// Validator for CreateNotificationsLogDto (UC-NTF push screen). Title and
/// Message are mandatory; at least one audience flag must be raised and each
/// raised flag must carry at least one recipient — a notification addressed to
/// nobody is a silent no-op the actor would mistake for a delivery.
/// UpdateNotificationsLogDto inherits the same rules (id comes from the route).
/// </summary>
public class CreateNotificationsLogValidator : AbstractValidator<CreateNotificationsLogDto>
{
    public CreateNotificationsLogValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(250).WithMessage("Title cannot exceed 250 characters");

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required")
            .MaximumLength(4000).WithMessage("Message cannot exceed 4000 characters");

        RuleFor(x => x)
            .Must(x => x.IsUser || x.IsCharity)
            .WithMessage("Select at least one target: specific users or charities")
            .OverridePropertyName(nameof(CreateNotificationsLogDto.IsUser));

        RuleFor(x => x.RecipientUserIds)
            .NotEmpty().WithMessage("Select at least one user when targeting users")
            .When(x => x.IsUser);

        RuleFor(x => x.RecipientCharityIds)
            .NotEmpty().WithMessage("Select at least one charity when targeting charities")
            .When(x => x.IsCharity);
    }
}

/// <summary>
/// Validator for UpdateNotificationsLogDto (UC-NTF edit screen) — the same
/// field contract as create.
/// </summary>
public class UpdateNotificationsLogValidator : AbstractValidator<UpdateNotificationsLogDto>
{
    public UpdateNotificationsLogValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Notification id is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(250).WithMessage("Title cannot exceed 250 characters");

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required")
            .MaximumLength(4000).WithMessage("Message cannot exceed 4000 characters");

        RuleFor(x => x)
            .Must(x => x.IsUser || x.IsCharity)
            .WithMessage("Select at least one target: specific users or charities")
            .OverridePropertyName(nameof(UpdateNotificationsLogDto.IsUser));

        RuleFor(x => x.RecipientUserIds)
            .NotEmpty().WithMessage("Select at least one user when targeting users")
            .When(x => x.IsUser);

        RuleFor(x => x.RecipientCharityIds)
            .NotEmpty().WithMessage("Select at least one charity when targeting charities")
            .When(x => x.IsCharity);
    }
}
