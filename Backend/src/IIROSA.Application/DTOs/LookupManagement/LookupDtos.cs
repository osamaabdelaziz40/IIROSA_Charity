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
    public string? SearchText { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
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