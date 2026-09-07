using FluentValidation;

using IIROSA.Application.DTOs.Reports;

namespace IIROSA.Application.Validators.Reports;

/// <summary>
/// UC-FAM-14 — the report key must be one this controller serves, and the tracking sheet
/// needs its activity day (not in the future, same rule as the on-screen filter).
/// </summary>
public class ReportSheetRequestValidator : AbstractValidator<ReportSheetRequestDto>
{
    public static readonly string[] KnownKeys =
    {
        "family-update-tracking",
        "guardian-identification-sheets",
        "widow-identification-sheets"
    };

    public ReportSheetRequestValidator()
    {
        RuleFor(x => x.ReportKey)
            .Must(key => KnownKeys.Contains(key))
            .WithMessage("Unknown report key");

        RuleFor(x => x.Date)
            .NotNull()
            .WithMessage("The report date is required for the follow-up tracking sheet")
            .When(x => x.ReportKey == "family-update-tracking");

        RuleFor(x => x.Date)
            .Must(date => date!.Value.Date <= DateTime.UtcNow.Date)
            .WithMessage("The report date cannot be in the future")
            .When(x => x.Date.HasValue);

        RuleFor(x => x.FamilyCode)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.FamilyCode));
    }
}
