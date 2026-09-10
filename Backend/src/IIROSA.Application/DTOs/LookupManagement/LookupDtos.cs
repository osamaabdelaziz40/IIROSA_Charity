namespace IIROSA.Application.DTOs.LookupManagement;

/// <summary>
/// Base DTO for lookup entities
/// </summary>
public class LookupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? UpdatedOn { get; set; }
}

/// <summary>
/// DTO for creating lookup entities
/// </summary>
public class CreateLookupDto
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
}

/// <summary>
/// DTO for updating lookup entities
/// </summary>
public class UpdateLookupDto
{
    public string? Name { get; set; }
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
    public int? SortOrder { get; set; }
}

/// <summary>
/// Country-specific DTOs
/// </summary>
public class CountryDto : LookupDto
{
    public string? IsoCode { get; set; }
    public string? DialingCode { get; set; }
    public string? Currency { get; set; }
    public string? FlagIcon { get; set; }
    public int RegionCount { get; set; }
    public int CenterCount { get; set; }

    /// <summary>
    /// HQ transfer ceiling for this country (UC-TRF-06) — NULL = unlimited. Read-only carry;
    /// the write lives in 17-7's max-amount endpoint, not the country CRUD.
    /// </summary>
    public decimal? MaxTransferAmount { get; set; }

    /// <summary>National-id regex rule (UC-SYS-11) — NULL = no rule, input stays free-form.</summary>
    public string? NationalIdPattern { get; set; }

    /// <summary>National-id exact length rule (UC-SYS-11) — NULL = no rule.</summary>
    public int? NationalIdLength { get; set; }
}

public class CreateCountryDto
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? IsoCode { get; set; }
    public string? DialingCode { get; set; }
    public string? Currency { get; set; }
    public string? FlagIcon { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
    public string? NationalIdPattern { get; set; }
    public int? NationalIdLength { get; set; }
}

public class UpdateCountryDto
{
    public string? Name { get; set; }
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? IsoCode { get; set; }
    public string? DialingCode { get; set; }
    public string? Currency { get; set; }
    public string? FlagIcon { get; set; }
    public bool? IsActive { get; set; }
    public int? SortOrder { get; set; }
    public string? NationalIdPattern { get; set; }
    public int? NationalIdLength { get; set; }
}

/// <summary>
/// Region-specific DTOs
/// </summary>
public class RegionDto : LookupDto
{
    public string? RegionCode { get; set; }
    public int? CountryId { get; set; }
    public string? CountryName { get; set; }
    public int CenterCount { get; set; }
}

public class CreateRegionDto
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? RegionCode { get; set; }
    public int CountryId { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
}

public class UpdateRegionDto
{
    public string? Name { get; set; }
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? RegionCode { get; set; }
    public int? CountryId { get; set; }
    public bool? IsActive { get; set; }
    public int? SortOrder { get; set; }
}

/// <summary>
/// Center-specific DTOs
/// </summary>
public class CenterDto : LookupDto
{
    public string? CenterCode { get; set; }
    public int? RegionId { get; set; }
    public int? CountryId { get; set; }
    public string? RegionName { get; set; }
    public string? CountryName { get; set; }
}

public class CreateCenterDto
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? CenterCode { get; set; }
    public int RegionId { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
}

public class UpdateCenterDto
{
    public string? Name { get; set; }
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? CenterCode { get; set; }
    public int? RegionId { get; set; }
    public bool? IsActive { get; set; }
    public int? SortOrder { get; set; }
}

/// <summary>
/// Department-specific DTOs
/// </summary>
public class DepartmentDto : LookupDto
{
    public string? Description { get; set; }
}

public class CreateDepartmentDto
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
}

public class UpdateDepartmentDto
{
    public string? Name { get; set; }
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
    public int? SortOrder { get; set; }
}

/// <summary>
/// MissionType-specific DTOs
/// </summary>
public class MissionTypeDto : LookupDto
{
    public string? TypeCode { get; set; }
    public string? TypeDescription { get; set; }
}

public class CreateMissionTypeDto
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? TypeCode { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
}

public class UpdateMissionTypeDto
{
    public string? Name { get; set; }
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? TypeCode { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
    public int? SortOrder { get; set; }
}

/// <summary>
/// MissionTimeType-specific DTOs
/// </summary>
public class MissionTimeTypeDto : LookupDto
{
    public string? TimeTypeCode { get; set; }
    public string? TypeDescription { get; set; }
}

public class CreateMissionTimeTypeDto
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? TimeTypeCode { get; set; }
    public string? TypeDescription { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
}

public class UpdateMissionTimeTypeDto
{
    public string? Name { get; set; }
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? TimeTypeCode { get; set; }
    public string? TypeDescription { get; set; }
    public bool? IsActive { get; set; }
    public int? SortOrder { get; set; }
}

/// <summary>
/// MissionInterviewType-specific DTOs (UC-MSN-04)
/// </summary>
public class MissionInterviewTypeDto : LookupDto
{
    public string? TypeCode { get; set; }
}

public class CreateMissionInterviewTypeDto
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? TypeCode { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
}

public class UpdateMissionInterviewTypeDto
{
    public string? Name { get; set; }
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? TypeCode { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
    public int? SortOrder { get; set; }
}

/// <summary>
/// ProjectType-specific DTOs
/// </summary>
public class ProjectTypeDto : LookupDto
{
    public string? TypeCode { get; set; }
    public string? TypeDescription { get; set; }
}

public class CreateProjectTypeDto
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? TypeCode { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
}

public class UpdateProjectTypeDto
{
    public string? Name { get; set; }
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? TypeCode { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
    public int? SortOrder { get; set; }
}

/// <summary>
/// Bank-specific DTOs
/// </summary>
public class BankDto : LookupDto
{
    public string? BankCode { get; set; }
    public string? SwiftCode { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
}

public class CreateBankDto
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? BankCode { get; set; }
    public string? SwiftCode { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
}

public class UpdateBankDto
{
    public string? Name { get; set; }
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? BankCode { get; set; }
    public string? SwiftCode { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public bool? IsActive { get; set; }
    public int? SortOrder { get; set; }
}

/// <summary>
/// NGOType-specific DTOs
/// </summary>
public class NGOTypeDto : LookupDto
{
    public string? TypeCode { get; set; }
    public string? TypeDescription { get; set; }
}

public class CreateNGOTypeDto
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? TypeCode { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
}

public class UpdateNGOTypeDto
{
    public string? Name { get; set; }
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? TypeCode { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
    public int? SortOrder { get; set; }
}

/// <summary>
/// Education-level-specific DTOs (UC-ORP-11 — orphan reference data)
/// </summary>
public class EducationLevelDto : LookupDto
{
}

public class CreateEducationLevelDto
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
}

public class UpdateEducationLevelDto
{
    public string? Name { get; set; }
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
    public int? SortOrder { get; set; }
}

/// <summary>
/// Health-status-specific DTOs (UC-ORP-11 — orphan reference data)
/// </summary>
public class HealthStatusDto : LookupDto
{
}

public class CreateHealthStatusDto
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
}

public class UpdateHealthStatusDto
{
    public string? Name { get; set; }
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
    public int? SortOrder { get; set; }
}

/// <summary>
/// Death-reason DTOs (سبب الوفاة — father/mother death details)
/// </summary>
public class DeathReasonDto : LookupDto
{
}

public class CreateDeathReasonDto
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
}

public class UpdateDeathReasonDto
{
    public string? Name { get; set; }
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
    public int? SortOrder { get; set; }
}

/// <summary>
/// Refuse-reason DTOs (epic 9, UC-ORR-08 — periodic report refusal catalogue)
/// </summary>
public class RefuseReasonDto : LookupDto
{
}

public class CreateRefuseReasonDto
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
}

public class UpdateRefuseReasonDto
{
    public string? Name { get; set; }
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
    public int? SortOrder { get; set; }
}

// ==================== Refugee register lookups (epic 7, UC-REF-03) ====================

public class HouseOwnershipDto : LookupDto
{
}

public class CreateHouseOwnershipDto
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
}

public class UpdateHouseOwnershipDto
{
    public string? Name { get; set; }
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
    public int? SortOrder { get; set; }
}

public class HouseStatusDto : LookupDto
{
}

public class CreateHouseStatusDto
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
}

public class UpdateHouseStatusDto
{
    public string? Name { get; set; }
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
    public int? SortOrder { get; set; }
}

public class IncomeTypeDto : LookupDto
{
}

public class CreateIncomeTypeDto
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
}

public class UpdateIncomeTypeDto
{
    public string? Name { get; set; }
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
    public int? SortOrder { get; set; }
}

/// <summary>حالة المشروع — family data extension catalogue (يوجد مشروع قائم / مشروع جديد)</summary>
public class FamilyProjectStatusDto : LookupDto
{
}

public class CreateFamilyProjectStatusDto
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
}

public class UpdateFamilyProjectStatusDto
{
    public string? Name { get; set; }
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
    public int? SortOrder { get; set; }
}

public class SocialStatusDto : LookupDto
{
}

public class CreateSocialStatusDto
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
}

public class UpdateSocialStatusDto
{
    public string? Name { get; set; }
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
    public int? SortOrder { get; set; }
}

public class RelationDto : LookupDto
{
}

public class CreateRelationDto
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
}

public class UpdateRelationDto
{
    public string? Name { get; set; }
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
    public int? SortOrder { get; set; }
}

public class ReasonOfRelDto : LookupDto
{
}

public class CreateReasonOfRelDto
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
}

public class UpdateReasonOfRelDto
{
    public string? Name { get; set; }
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
    public int? SortOrder { get; set; }
}

// نوع السكن — shared catalogue (§12.S.2 refugee form)
public class HousingTypeDto : LookupDto
{
}

public class CreateHousingTypeDto
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
}

public class UpdateHousingTypeDto
{
    public string? Name { get; set; }
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
    public int? SortOrder { get; set; }
}

/// <summary>
/// Paged result for lookup queries
/// </summary>
public class LookupPagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

/// <summary>
/// Filter DTO for lookup queries
/// </summary>
public class LookupFilterDto
{
    /// <summary>
    /// Platform ceiling for a single lookup page (19-4). Requests above it are clamped;
    /// totalCount always carries the true filtered count, so a catalogue that outgrows a
    /// page is detectable (totalPages > 1) rather than silently truncated.
    /// </summary>
    public const int MaxPageSize = 5000;

    public string? SearchText { get; set; }
    public bool? IsActive { get; set; }

    private int _page = 1;
    private int _pageSize = 20;

    public int Page
    {
        get => _page;
        set => _page = Math.Max(1, value);
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = Math.Clamp(value, 1, MaxPageSize);
    }

    public int? CountryId { get; set; }  // For filtering regions by country
    public int? RegionId { get; set; }   // For filtering centers by region
}

/// <summary>
/// Lookup table summary for UC-14.5
/// </summary>
public class LookupTableSummaryDto
{
    public string TableName { get; set; } = string.Empty;
    public string TableDisplayName { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public int ActiveItems { get; set; }
    public int InactiveItems { get; set; }
    public DateTime? LastModified { get; set; }
}

/// <summary>
/// Bulk operations DTOs for import/export
/// </summary>
public class BulkImportDto
{
    public List<CreateLookupDto> Items { get; set; } = new();
    public bool SkipDuplicates { get; set; } = true;
    public bool UpdateExisting { get; set; } = false;
}

public class BulkImportResultDto
{
    public int TotalProcessed { get; set; }
    public int Successful { get; set; }
    public int Failed { get; set; }
    public int Skipped { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class BulkExportDto
{
    public string Format { get; set; } = "Excel"; // Excel, CSV, JSON
    public bool IncludeInactive { get; set; } = false;
    public string Language { get; set; } = "Both"; // Arabic, English, Both
}

/// <summary>
/// Marital-status-specific DTOs (UC-SYS-05 — guardian reference data)
/// </summary>
public class MaritalStatusDto : LookupDto
{
}

public class CreateMaritalStatusDto
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
}

public class UpdateMaritalStatusDto
{
    public string? Name { get; set; }
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
    public int? SortOrder { get; set; }
}
/// <summary>Guardian job / profession — المهنة (UC-SYS-09, epic 19)</summary>
public class JobDto : LookupDto
{
}

public class CreateJobDto
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
}

public class UpdateJobDto
{
    public string? Name { get; set; }
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
    public int? SortOrder { get; set; }
}

/// <summary>
/// Housing building DTOs (UC-HOU-05 · §11.S.2 رقم العماره) — the building title lives in
/// NameAr/NameEn; Description carries BuildingDescription.
/// </summary>
public class HousingBuildingDto : LookupDto
{
    public int? BuildingNumber { get; set; }
    public string? BuildingAddress { get; set; }
}

public class CreateHousingBuildingDto
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public int? BuildingNumber { get; set; }
    public string? BuildingAddress { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
}

public class UpdateHousingBuildingDto
{
    public string? Name { get; set; }
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public int? BuildingNumber { get; set; }
    public string? BuildingAddress { get; set; }
    public bool? IsActive { get; set; }
    public int? SortOrder { get; set; }
}

/// <summary>
/// Housing flat DTOs (UC-HOU-05 · §11.S.2 رقم الشقه) — the flat's display name (its
/// number as text) lives in NameAr/NameEn; Number/SizeInMtr carry it numerically.
/// </summary>
public class HousingFlatDto : LookupDto
{
    public int? Number { get; set; }
    public int? SizeInMtr { get; set; }
    public int BuildingId { get; set; }
    public string? BuildingName { get; set; }
}

public class CreateHousingFlatDto
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public int? Number { get; set; }
    public int? SizeInMtr { get; set; }
    public int BuildingId { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
}

public class UpdateHousingFlatDto
{
    public string? Name { get; set; }
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public int? Number { get; set; }
    public int? SizeInMtr { get; set; }
    public int? BuildingId { get; set; }
    public bool? IsActive { get; set; }
    public int? SortOrder { get; set; }
}
