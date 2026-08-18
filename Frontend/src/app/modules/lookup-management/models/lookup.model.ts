import { Injectable } from '@angular/core';

/// <summary>
/// Base DTO for lookup entities
/// </summary>
export interface LookupDto {
  id: number;
  name: string;
  nameAr?: string;
  nameEn?: string;
  description?: string;
  isActive: boolean;
  sortOrder: number;
  createdOn: Date;
  updatedOn?: Date;
}

/// <summary>
/// DTO for creating lookup entities
/// </summary>
export interface CreateLookupDto {
  name: string;
  nameAr?: string;
  nameEn?: string;
  description?: string;
  isActive?: boolean;
  sortOrder?: number;
}

/// <summary>
/// DTO for updating lookup entities
/// </summary>
export interface UpdateLookupDto {
  name?: string;
  nameAr?: string;
  nameEn?: string;
  description?: string;
  isActive?: boolean;
  sortOrder?: number;
}

/// <summary>
/// Country-specific DTOs (UC-14.8)
/// </summary>
export interface CountryDto extends LookupDto {
  isoCode?: string;
  dialingCode?: string;
  currency?: string;
  flagIcon?: string;
  regionCount?: number;
  centerCount?: number;
}

export interface CreateCountryDto {
  name: string;
  nameAr?: string;
  nameEn?: string;
  isoCode?: string;
  dialingCode?: string;
  currency?: string;
  flagIcon?: string;
  isActive?: boolean;
  sortOrder?: number;
}

export interface UpdateCountryDto {
  name?: string;
  nameAr?: string;
  nameEn?: string;
  isoCode?: string;
  dialingCode?: string;
  currency?: string;
  flagIcon?: string;
  isActive?: boolean;
  sortOrder?: number;
}

/// <summary>
/// Region-specific DTOs (UC-14.7)
/// </summary>
export interface RegionDto extends LookupDto {
  regionCode?: string;
  countryId?: number;
  countryName?: string;
  centerCount?: number;
}

export interface CreateRegionDto {
  name: string;
  nameAr?: string;
  nameEn?: string;
  regionCode?: string;
  countryId: number;
  isActive?: boolean;
  sortOrder?: number;
}

export interface UpdateRegionDto {
  name?: string;
  nameAr?: string;
  nameEn?: string;
  regionCode?: string;
  countryId?: number;
  isActive?: boolean;
  sortOrder?: number;
}

/// <summary>
/// Center-specific DTOs (UC-14.6)
/// </summary>
export interface CenterDto extends LookupDto {
  centerCode?: string;
  regionId?: number;
  countryId?: number;
  regionName?: string;
  countryName?: string;
}

export interface CreateCenterDto {
  name: string;
  nameAr?: string;
  nameEn?: string;
  centerCode?: string;
  regionId: number;
  isActive?: boolean;
  sortOrder?: number;
}

export interface UpdateCenterDto {
  name?: string;
  nameAr?: string;
  nameEn?: string;
  centerCode?: string;
  regionId?: number;
  isActive?: boolean;
  sortOrder?: number;
}

/// <summary>
/// Department-specific DTOs (UC-14.9)
/// </summary>
export interface DepartmentDto extends LookupDto {
  departmentCode?: string;
  description?: string;
}

export interface CreateDepartmentDto {
  name: string;
  nameAr?: string;
  nameEn?: string;
  departmentCode?: string;
  description?: string;
  isActive?: boolean;
  sortOrder?: number;
}

export interface UpdateDepartmentDto {
  name?: string;
  nameAr?: string;
  nameEn?: string;
  departmentCode?: string;
  description?: string;
  isActive?: boolean;
  sortOrder?: number;
}

/// <summary>
/// Bank-specific DTOs
/// </summary>
export interface BankDto extends LookupDto {
  bankCode?: string;
  swiftCode?: string;
  branchCount?: number;
}

export interface CreateBankDto {
  name: string;
  nameAr?: string;
  nameEn?: string;
  bankCode?: string;
  swiftCode?: string;
  isActive?: boolean;
  sortOrder?: number;
}

export interface UpdateBankDto {
  name?: string;
  nameAr?: string;
  nameEn?: string;
  bankCode?: string;
  swiftCode?: string;
  isActive?: boolean;
  sortOrder?: number;
}

/// <summary>
/// Filter and pagination DTOs
/// </summary>
export interface LookupFilterDto {
  searchText?: string;
  isActive?: boolean;
  page?: number;
  pageSize?: number;
  countryId?: number;  // For filtering regions by country
  regionId?: number;   // For filtering centers by region
}

export interface LookupPagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

/// <summary>
/// Lookup table summary for UC-14.5
/// </summary>
export interface LookupTableSummaryDto {
  tableName: string;
  tableDisplayName: string;
  itemCount: number;
  activeItems: number;
  inactiveItems: number;
  lastModified?: Date;
}

/// <summary>
/// Export/Import DTOs
/// </summary>
export interface BulkExportDto {
  format: 'Excel' | 'CSV' | 'JSON';
  includeInactive: boolean;
  language: 'Arabic' | 'English' | 'Both';
}

export interface BulkImportDto {
  items: CreateLookupDto[];
  skipDuplicates: boolean;
  updateExisting: boolean;
}

export interface BulkImportResultDto {
  totalProcessed: number;
  successful: number;
  failed: number;
  skipped: number;
  errors: string[];
}
