using FluentValidation;
using IIROSA.Application.DTOs.OrphanReportCycles;

namespace IIROSA.Application.Validators.OrphanReportCycles;

/// <summary>
/// Create validation (UC-ORC-02): Arabic title mandatory (the primary display name),
/// period mandatory with a strict end-after-start rule (BR-ORC-03). Overlapping cycles
/// are legal and draw no check. The date-vs-completed guards are business rules in the
/// service, not field validation.
/// </summary>
public class CreateOrphanReportCycleValidator : AbstractValidator<CreateOrphanReportCycleDto>
{
    public CreateOrphanReportCycleValidator()
    {
        RuleFor(x => x.TitleAr)
            .NotEmpty().WithMessage("The Arabic title is required.")
            .MaximumLength(250).WithMessage("The title must not exceed 250 characters.");

        RuleFor(x => x.TitleEn)
            .MaximumLength(250).WithMessage("The English title must not exceed 250 characters.");

        RuleFor(x => x.DescriptionAr)
            .MaximumLength(2000).WithMessage("The description must not exceed 2000 characters.");

        RuleFor(x => x.DescriptionEn)
            .MaximumLength(2000).WithMessage("The description must not exceed 2000 characters.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("The start date is required.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("The end date is required.")
            .GreaterThan(x => x.StartDate).WithMessage("The end date must be after the start date.");
    }
}

/// <summary>
/// Update validation (UC-ORC-03): the create field rules plus the target id. BR-ORC-08's
/// completed-read-only and end-date-shortening guards run in the service where the stored
/// cycle can be compared.
/// </summary>
public class UpdateOrphanReportCycleValidator : AbstractValidator<UpdateOrphanReportCycleDto>
{
    public UpdateOrphanReportCycleValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("The cycle id is required.");

        Include(new CreateOrphanReportCycleValidator());
    }
}
