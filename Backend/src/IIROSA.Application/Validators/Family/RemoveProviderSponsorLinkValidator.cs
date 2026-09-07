using FluentValidation;

using IIROSA.Application.DTOs.Family;

namespace IIROSA.Application.Validators.Family;

/// <summary>
/// UC-FAM-13 — the التعليق is optional but bounded (matches the modal's maxlength).
/// </summary>
public class RemoveProviderSponsorLinkValidator : AbstractValidator<RemoveProviderSponsorLinkDto>
{
    public RemoveProviderSponsorLinkValidator()
    {
        RuleFor(x => x.Comment)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Comment));
    }
}
