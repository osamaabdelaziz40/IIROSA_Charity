using FluentValidation;
using IIROSA.Application.DTOs.Family;

namespace IIROSA.Application.Validators.Family;

/// <summary>
/// UC-REF-04 §12.S.2 mandatory flags for UPDATING a refugee family. Invoked by
/// FamilyService.UpdateFamilyAsync only when the family is on the Refugee register — the copy
/// there is patch-style (absent ⇒ keep stored value), so the service validates the EFFECTIVE
/// post-copy state, not the raw DTO: clearing a mandatory field cannot silently keep the old
/// value, and an emptied effective field is refused with the §12.S.2 message. Members travel on
/// their own per-member endpoints, so no provider/orphan rules here (unlike the create validator).
/// Instantiated directly in FamilyService (CreateHousingFamilyValidator precedent) so a future
/// second AbstractValidator&lt;UpdateFamilyDto&gt; cannot hijack the resolution.
/// </summary>
public class UpdateRefugeeFamilyValidator : AbstractValidator<UpdateFamilyDto>
{
    public UpdateRefugeeFamilyValidator()
    {
        // بيانات الأسرة — the §12.S.2 mandatory list, checked against the effective state
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

        // قيمة الإيجار — flat rule per spec (same as the create validator)
        RuleFor(x => x.RentAmount)
            .NotNull().WithMessage("قيمة الإيجار مطلوبة")
            .GreaterThan(0).WithMessage("قيمة الإيجار يجب أن تكون أكبر من صفر");
    }
}
