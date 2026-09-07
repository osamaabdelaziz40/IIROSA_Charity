using FluentValidation;
using IIROSA.Application.DTOs.Family;

namespace IIROSA.Application.Validators.Family;

/// <summary>
/// UC-REF-03 §12.S.2 mandatory flags for registering a REFUGEE family. Invoked by
/// FamilyService.CreateFamilyAsync only when FamilyType == Refugee — the regular-family
/// path keeps its current behaviour (registered automatically via AddValidatorsFromAssembly).
/// </summary>
public class CreateRefugeeFamilyValidator : AbstractValidator<CreateFamilyDto>
{
    public CreateRefugeeFamilyValidator()
    {
        // بيانات الأسرة — the §12.S.2 mandatory list
        RuleFor(x => x.CityVillage)
            .NotEmpty().WithMessage("القرية / الحي مطلوب");

        RuleFor(x => x.RegionId)
            .NotNull().WithMessage("المنطقة /المحافظة مطلوبة");

        RuleFor(x => x.CenterId)
            .NotNull().WithMessage("المركز/ المدينة مطلوب");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("العنوان التفصيلى مطلوب");

        RuleFor(x => x.HouseOwnershipId)
            .NotNull().WithMessage("ملكية السكن مطلوبة");

        RuleFor(x => x.HouseStatusId)
            .NotNull().WithMessage("حالة محتويات السكن مطلوبة");

        RuleFor(x => x.HousingTypeId)
            .NotNull().WithMessage("نوع السكن مطلوب");

        RuleFor(x => x.IncomeTypeId)
            .NotNull().WithMessage("نوع الدخل مطلوب");

        // قيمة الإيجار — §12.S.2 lists it mandatory unconditionally (the FORM shows/hides it via
        // the legacy DisplayRentValue() ownership rule; the API enforces it flat, per spec)
        RuleFor(x => x.RentAmount)
            .NotNull().WithMessage("قيمة الإيجار مطلوبة")
            .GreaterThan(0).WithMessage("قيمة الإيجار يجب أن تكون أكبر من صفر");

        // Charity scope — HQ must name the owning charity; a Charity caller is pinned by the controller
        RuleFor(x => x.CharityId)
            .NotNull().WithMessage("Charity is required for a refugee family");

        // اضافة معيل — a refugee family carries a provider, not a father/mother. §12.U.3 main
        // flow step 3: the guardian block itself is MANDATORY on this register — a guardian-less
        // refugee family must be refused, not persisted with HeadOfFamily unset.
        RuleFor(x => x.Provider)
            .NotNull().WithMessage("بيانات المعيل مطلوبة");

        When(x => x.Provider != null, () =>
        {
            RuleFor(x => x.Provider!.FullName)
                .NotEmpty().WithMessage("إسم المعيل مطلوب")
                .Must(name => name != null && name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length >= 2)
                .WithMessage("يجب إدخال الاسم الثانى والاسم الثالث على الأقل");

            RuleFor(x => x.Provider!.NationalId)
                .NotEmpty().WithMessage("الرقم القومي للمعيل مطلوب");

            RuleFor(x => x.Provider!.DateOfBirth)
                .NotNull().WithMessage("تاريخ ميلاد المعيل مطلوب");

            RuleFor(x => x.Provider!.NationalityCountryId)
                .NotNull().WithMessage("جنسية المعيل مطلوبة");

            RuleFor(x => x.Provider!.RelationshipToFamily)
                .NotEmpty().WithMessage("العلاقة (نوعها) مطلوبة");

            RuleFor(x => x.Provider!.ReasonOfRelationId)
                .NotNull().WithMessage("سبب العلاقة مطلوب");
        });

        // اضافة ابن — national id capped at 14 digits (§12.S.2)
        RuleForEach(x => x.Orphans)
            .ChildRules(orphan =>
            {
                orphan.RuleFor(o => o.NationalId)
                    .MaximumLength(14)
                    .When(o => !string.IsNullOrEmpty(o.NationalId))
                    .WithMessage("الرقم القومي للأبن لا يتجاوز 14 رقماً");
            });
    }
}
