using FluentValidation;
using IIROSA.Application.DTOs.Family;

namespace IIROSA.Application.Validators.Family;

/// <summary>
/// §11.S.2 mandatory flags for a housing family create (UC-HOU-03).
/// Mirrors <see cref="CreateRefugeeFamilyValidator"/> (the epic-7 pattern): validates the
/// shared CreateFamilyDto — the housing branch is reached with FamilyType=Housing, so the
/// refugee-only fields stay nullable and only the §11.S.2 contract is enforced.
/// Invoked from FamilyService (create branch) and AddNewHousingFamilyAsync; NOT registered in
/// DI as IValidator&lt;CreateFamilyDto&gt; — that slot belongs to the refugee validator.
/// </summary>
public class CreateHousingFamilyValidator : AbstractValidator<CreateFamilyDto>
{
    public CreateHousingFamilyValidator()
    {
        // بيانات الأسرة — §11.S.2 mandatory set
        RuleFor(x => x.CityVillage)          // القرية / الحي
            .NotEmpty().WithMessage("القرية / الحي مطلوب");
        RuleFor(x => x.RegionId)             // المنطقة /المحافظة
            .NotNull().WithMessage("المنطقة /المحافظة مطلوبة");
        RuleFor(x => x.CenterId)             // المركز/ المدينة
            .NotNull().WithMessage("المركز/ المدينة مطلوب");
        RuleFor(x => x.Address)              // العنوان التفصيلى
            .NotEmpty().WithMessage("العنوان التفصيلى مطلوب");
        RuleFor(x => x.PhoneNumber)          // بيانات الإتصال (default phone row)
            .NotEmpty().WithMessage("بيانات الإتصال مطلوبة");
        RuleFor(x => x.RentAmount)           // قيمة الإيجار
            .NotNull().WithMessage("قيمة الإيجار مطلوبة");
        RuleFor(x => x.IncomeTypeId)         // نوع الدخل
            .NotNull().WithMessage("نوع الدخل مطلوب");
        RuleFor(x => x.HousingBuildingId)    // رقم العماره
            .NotNull().WithMessage("رقم العماره مطلوب");
        RuleFor(x => x.HousingFlatId)        // رقم الشقه
            .NotNull().WithMessage("رقم الشقه مطلوب");

        // Charity scope — a housing family is always owned by a charity (server-pinned for
        // Charity callers; HQ must name one, matching the refugee rule).
        RuleFor(x => x.CharityId)
            .NotNull().WithMessage("الجمعية مطلوبة");

        // اضافة معيل (guardian block) — §11.S.2 mandatories. The service mirrors the first
        // multi-guardian row onto Provider before validating, so row 1 always runs through
        // the Provider rules; rows 2+ run the same mandatory set per row below.
        RuleFor(x => x.Provider)
            .NotNull().WithMessage("بيانات المعيل مطلوبة");
        When(x => x.Provider != null, () =>
        {
            RuleFor(x => x.Provider!.FullName)              // الاسم (أول/ثانى/ثالث/رباعي composed)
                .NotEmpty().WithMessage("اسم المعيل مطلوب");
            RuleFor(x => x.Provider!.NationalId)            // الرقم القومي
                .NotEmpty().WithMessage("الرقم القومي للمعيل مطلوب");
            RuleFor(x => x.Provider!.DateOfBirth)           // تاريخ الميلاد
                .NotNull().WithMessage("تاريخ ميلاد المعيل مطلوب");
            RuleFor(x => x.Provider!.NationalityCountryId)  // الجنسية
                .NotNull().WithMessage("جنسية المعيل مطلوبة");
            RuleFor(x => x.Provider!.Job)                   // نوع العمل
                .NotEmpty().WithMessage("نوع عمل المعيل مطلوب");
            RuleFor(x => x.Provider!.ReasonOfRelationId)    // السبب
                .NotNull().WithMessage("سبب العلاقة مطلوب");
            RuleFor(x => x.Provider!.RelationId)            // نوعها
                .NotNull().WithMessage("نوع العلاقة مطلوب");
            RuleFor(x => x.Provider!.MainRelation)          // العلاقة (الاب / الام)
                .NotEmpty().WithMessage("العلاقة مطلوبة");
            RuleFor(x => x.Provider!.EducationLevelId)      // المؤهل الدراسى
                .NotNull().WithMessage("المؤهل الدراسى للمعيل مطلوب");
            RuleFor(x => x.Provider!.SocialStatusId)        // الحالة الاجتماعية
                .NotNull().WithMessage("الحالة الاجتماعية للمعيل مطلوبة");
            RuleFor(x => x.Provider!.HealthStatusId)        // الحالة الصحية
                .NotNull().WithMessage("الحالة الصحية للمعيل مطلوبة");
            RuleFor(x => x.Provider!.Phone)
                .NotEmpty().WithMessage("هاتف المعيل مطلوب");
        });

        // §11.S.2 multi-guardian rows (اضافة الاباء · AddNewParent() always) — the same
        // mandatory set per row; rows are keyed by id on the update (edit-sync contract).
        RuleForEach(x => x.Providers).ChildRules(guardian =>
        {
            guardian.RuleFor(g => g.FullName)
                .NotEmpty().WithMessage("اسم المعيل مطلوب");
            guardian.RuleFor(g => g.NationalId)
                .NotEmpty().WithMessage("الرقم القومي للمعيل مطلوب");
            guardian.RuleFor(g => g.DateOfBirth)
                .NotNull().WithMessage("تاريخ ميلاد المعيل مطلوب");
            guardian.RuleFor(g => g.NationalityCountryId)
                .NotNull().WithMessage("جنسية المعيل مطلوبة");
            guardian.RuleFor(g => g.Job)
                .NotEmpty().WithMessage("نوع عمل المعيل مطلوب");
            guardian.RuleFor(g => g.ReasonOfRelationId)
                .NotNull().WithMessage("سبب العلاقة مطلوب");
            guardian.RuleFor(g => g.RelationId)
                .NotNull().WithMessage("نوع العلاقة مطلوب");
            guardian.RuleFor(g => g.MainRelation)
                .NotEmpty().WithMessage("العلاقة مطلوبة");
            guardian.RuleFor(g => g.EducationLevelId)
                .NotNull().WithMessage("المؤهل الدراسى للمعيل مطلوب");
            guardian.RuleFor(g => g.SocialStatusId)
                .NotNull().WithMessage("الحالة الاجتماعية للمعيل مطلوبة");
            guardian.RuleFor(g => g.HealthStatusId)
                .NotNull().WithMessage("الحالة الصحية للمعيل مطلوبة");
            guardian.RuleFor(g => g.Phone)
                .NotEmpty().WithMessage("هاتف المعيل مطلوب");
        });

        // اضافة ابن (child block) — §11.S.2 mandatories per child
        RuleForEach(x => x.Orphans).ChildRules(orphan =>
        {
            orphan.RuleFor(o => o.FullName)                 // الاسم الاول
                .NotEmpty().WithMessage("اسم الابن مطلوب");
            orphan.RuleFor(o => o.DateOfBirth)              // تاريخ الميلاد
                .NotEmpty().WithMessage("تاريخ ميلاد الابن مطلوب");
            orphan.RuleFor(o => o.NationalId)               // الرقم القومى — max 14
                .NotEmpty().WithMessage("الرقم القومى للابن مطلوب")
                .MaximumLength(14).WithMessage("الرقم القومى لا يزيد عن 14 رقما");
            orphan.RuleFor(o => o.Gender)                   // النوع (ذكر / انثى)
                .NotEmpty().WithMessage("نوع الابن مطلوب");
            orphan.RuleFor(o => o.HealthStatusId)           // الحالة الصحية
                .NotNull().WithMessage("الحالة الصحية للابن مطلوبة");
            orphan.RuleFor(o => o.SocialStatusId)           // الحالة الاجتماعية
                .NotNull().WithMessage("الحالة الاجتماعية للابن مطلوبة");
            orphan.RuleFor(o => o.EducationLevelId)         // المرحلة الدراسية
                .NotNull().WithMessage("المرحلة الدراسية مطلوبة");
            orphan.RuleFor(o => o.EducationalQualificationId) // حاصل على مؤهل دراسى (review D3 2026-08-24)
                .NotNull().WithMessage("مؤهل الابن الدراسى مطلوب");
            orphan.RuleFor(o => o.GradeClass)               // الصف الدراسي
                .NotEmpty().WithMessage("الصف الدراسي مطلوب");
            orphan.RuleFor(o => o.Profession)               // نوعية العمل
                .NotEmpty().WithMessage("نوعية عمل الابن مطلوبة");
            orphan.RuleFor(o => o.DepartmentName)           // القسم
                .NotEmpty().WithMessage("القسم مطلوب");
            orphan.RuleFor(o => o.FacultyName)              // الكلية
                .NotEmpty().WithMessage("الكلية مطلوبة");
            orphan.RuleFor(o => o.SchoolName)               // المؤسسة التعليمية
                .NotEmpty().WithMessage("اسم الموسسة التعليمية مطلوب");
        });
    }
}
