using AutoMapper;
using IIROSA.Application.DTOs.Reports;
using IIROSA.Domain.Entities;

namespace IIROSA.Application.Profiles;

/// <summary>
/// UC-RPT-01 — Orphan → OrphanDataListDto. In-memory (the query materialises a page with its
/// include chain, then maps) because the guardian fallback and age computation are composite
/// logic, not column-for-column picks. Lookup labels resolve NameAr ?? NameEn.
/// CharityName is filled by ReportService.ResolveCharityNamesAsync — not here.
/// </summary>
public class ReportProfile : Profile
{
    public ReportProfile()
    {
        CreateMap<Orphan, OrphanDataListDto>()
            .ConvertUsing((source, _) => MapOrphanData(source));
    }

    private static OrphanDataListDto MapOrphanData(Orphan source)
    {
        var family = source.Family;
        var provider = family?.Provider;
        var father = family?.Father;
        var mother = family?.Mother;

        return new OrphanDataListDto
        {
            // identity
            Code = source.Code,
            FullName = source.FullName,
            NationalId = source.NationalId,
            DateOfBirth = source.DateOfBirth,
            Age = ComputeAge(source.DateOfBirth, DateTime.Today),
            Gender = source.Gender,

            // guardian — Provider first, then Father, then Mother (first non-empty wins)
            GuardianName = FirstNonEmpty(provider?.FullName, father?.FullName, mother?.FullName),
            GuardianRelationship = string.IsNullOrWhiteSpace(provider?.RelationshipToFamily)
                ? family?.ProviderType
                : provider!.RelationshipToFamily,
            GuardianNationalId = FirstNonEmpty(provider?.NationalId, father?.NationalId, mother?.NationalId),
            GuardianEducationLevel = FirstNonEmpty(
                Label(provider?.EducationLevel?.NameAr, provider?.EducationLevel?.NameEn),
                Label(father?.EducationLevel?.NameAr, father?.EducationLevel?.NameEn),
                Label(mother?.EducationLevel?.NameAr, mother?.EducationLevel?.NameEn)),
            GuardianJob = FirstNonEmpty(provider?.Job, father?.Job, mother?.Job),
            GuardianHealthStatus = FirstNonEmpty(
                Label(provider?.HealthStatus?.NameAr, provider?.HealthStatus?.NameEn),
                Label(father?.HealthStatus?.NameAr, father?.HealthStatus?.NameEn),
                Label(mother?.HealthStatus?.NameAr, mother?.HealthStatus?.NameEn)),
            GuardianSocialStatus = Label(provider?.SocialStatus?.NameAr, provider?.SocialStatus?.NameEn),
            // مشروع تنموى للمعيل — no backing column; stays null by design

            // residence & income
            GovernorateName = Label(family?.Region?.NameAr, family?.Region?.NameEn),
            CenterName = Label(family?.Center?.NameAr, family?.Center?.NameEn),
            CityVillage = family?.CityVillage,
            DetailedAddress = family?.Address,
            MobileNumber = family?.PhoneNumber,
            // MobileNumber2 — no backing column; stays null by design
            HouseOwnershipName = Label(family?.HouseOwnership?.NameAr, family?.HouseOwnership?.NameEn),
            RentAmount = family?.RentAmount,
            HousingTypeName = Label(family?.HousingType?.NameAr, family?.HousingType?.NameEn),
            HouseStatusName = Label(family?.HouseStatus?.NameAr, family?.HouseStatus?.NameEn),
            MonthlyIncome = family?.MonthlyIncome,

            // status
            SocialStatusName = Label(source.SocialStatus?.NameAr, source.SocialStatus?.NameEn),
            HealthStatusName = Label(source.HealthStatus?.NameAr, source.HealthStatus?.NameEn),
            Profession = source.Profession,

            // education
            EducationLevelName = Label(source.EducationLevel?.NameAr, source.EducationLevel?.NameEn),
            GradeClass = source.GradeClass,
            SchoolName = source.SchoolName,
            FacultyName = source.FacultyName,
            DepartmentName = source.DepartmentName,
            // HasAcademicDegree — no backing column; stays null by design

            // father
            FatherDeathDate = father?.DeathDate,
            // FatherDeathCause — no backing column; stays null by design

            // exclusion — no backing columns; IsExcluded stays false, reason stays null by design
            IsExcluded = false,

            // audit / org
            Notes = source.Notes,
            CharityId = source.FK_CharityId,
            LastUpdatedDate = source.UpdatedOn
        };
    }

    /// <summary>Lookup label rule: NameAr ?? NameEn (null when the lookup itself is absent).</summary>
    private static string? Label(string? nameAr, string? nameEn) => nameAr ?? nameEn;

    private static string? FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));

    // Review P5 2026-08-26: completed birthday-aware years — the old DayOfYear compare
    // miscounted across leap-year boundaries (Mar 1 is day 60 or 61 by year), and a future
    // DOB printed a negative age; bad data renders blank instead. Same rule as
    // ReportService.CompletedAge (kept local: profiles take no dependencies).
    private static int? ComputeAge(DateTime? dateOfBirth, DateTime today)
    {
        if (dateOfBirth is not { } dob || dob > today)
        {
            return null;
        }

        var age = today.Year - dob.Year;
        return dob.AddYears(age) > today ? age - 1 : age;
    }
}
