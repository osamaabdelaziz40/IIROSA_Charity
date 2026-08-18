import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { PaginationComponent } from '../../../shared/components';
import { LookupManagementService } from '../services/lookup-management.service';
import {
  RegionDto,
  CreateRegionDto,
  UpdateRegionDto,
  LookupFilterDto,
  LookupPagedResult,
  CountryDto
} from '../models/lookup.model';

@Component({
  selector: 'app-regions-list',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterModule, TranslateModule, PaginationComponent],
  templateUrl: './regions-list.component.html',
  styleUrls: ['./regions-list.component.scss']
})
export class RegionsListComponent implements OnInit {
  regions: RegionDto[] = [];
  filteredRegions: RegionDto[] = [];
  countries: CountryDto[] = [];
  loading = false;

  // Pagination
  currentPage = 1;
  pageSize = 20;
  totalCount = 0;
  totalPages = 0;

  // Filter
  searchText = '';
  filterIsActive?: boolean;
  filterCountryId?: number;

  // Modal
  showModal = false;
  modalMode: 'create' | 'edit' | 'view' = 'view';
  selectedRegion?: RegionDto;
  regionForm: FormGroup;

  constructor(
    private lookupService: LookupManagementService,
    private fb: FormBuilder,
    private translate: TranslateService
  ) {
    this.regionForm = this.createRegionForm();
  }

  ngOnInit(): void {
    this.loadRegions();
    this.loadCountries();
  }

  loadRegions(): void {
    this.loading = true;
    const filter: LookupFilterDto = {
      page: this.currentPage,
      pageSize: this.pageSize
    };

    // Only add optional filters if they have values
    if (this.searchText && this.searchText.trim()) {
      filter.searchText = this.searchText.trim();
    }
    if (this.filterIsActive !== undefined) {
      filter.isActive = this.filterIsActive;
    }

    this.lookupService.getRegions(filter).subscribe({
      next: (result: LookupPagedResult<RegionDto>) => {
        this.regions = result.items;
        this.filteredRegions = result.items;
        this.totalCount = result.totalCount;
        this.totalPages = result.totalPages;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading regions:', error);
        this.loading = false;
      }
    });
  }

  loadCountries(): void {
    this.lookupService.getCountries({ page: 1, pageSize: 100 }).subscribe({
      next: (result) => {
        this.countries = result.items.filter(c => c.isActive);
      },
      error: (error) => {
        console.error('Error loading countries:', error);
      }
    });
  }

  // Search and Filter
  onSearch(): void {
    this.currentPage = 1;
    this.loadRegions();
  }

  onFilter(): void {
    this.currentPage = 1;
    this.loadRegions();
  }

  clearFilters(): void {
    this.searchText = '';
    this.filterIsActive = undefined;
    this.filterCountryId = undefined;
    this.loadRegions();
  }

  // Pagination
  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadRegions();
  }

  onPageSizeChange(size: number): void {
    this.pageSize = size;
    this.currentPage = 1;
    this.loadRegions();
  }

  // Modal Operations
  openCreateModal(): void {
    this.modalMode = 'create';
    this.selectedRegion = undefined;
    this.regionForm = this.createRegionForm();
    this.showModal = true;
  }

  openEditModal(region: RegionDto): void {
    this.modalMode = 'edit';
    this.selectedRegion = region;
    this.regionForm = this.createRegionForm(region);
    this.showModal = true;
  }

  openViewModal(region: RegionDto): void {
    this.modalMode = 'view';
    this.selectedRegion = region;
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.regionForm.reset();
  }

  // CRUD Operations
  createRegion(): void {
    if (this.regionForm.invalid) {
      this.regionForm.markAllAsTouched();
      return;
    }

    const region: CreateRegionDto = this.regionForm.value;

    this.lookupService.createRegion(region).subscribe({
      next: (createdRegion) => {
        this.closeModal();
        this.loadRegions();
        this.showSuccessMessage('Region created successfully');
      },
      error: (error) => {
        console.error('Error creating region:', error);
        this.showErrorMessage('Failed to create region');
      }
    });
  }

  updateRegion(): void {
    if (this.regionForm.invalid) {
      this.regionForm.markAllAsTouched();
      return;
    }

    if (!this.selectedRegion) return;

    const region: UpdateRegionDto = this.regionForm.value;

    this.lookupService.updateRegion(this.selectedRegion.id, region).subscribe({
      next: () => {
        this.closeModal();
        this.loadRegions();
        this.showSuccessMessage('Region updated successfully');
      },
      error: (error) => {
        console.error('Error updating region:', error);
        this.showErrorMessage('Failed to update region');
      }
    });
  }

  deleteRegion(region: RegionDto): void {
    if (confirm(this.translate.instant('lookupManagement.confirmDelete', { name: region.name }))) {
      this.lookupService.deleteRegion(region.id).subscribe({
        next: () => {
          this.loadRegions();
          this.showSuccessMessage('Region deleted successfully');
        },
        error: (error) => {
          console.error('Error deleting region:', error);
          this.showErrorMessage('Failed to delete region');
        }
      });
    }
  }

  toggleActiveStatus(region: RegionDto): void {
    const newStatus = !region.isActive;

    this.lookupService.updateRegion(region.id, { ...region, isActive: newStatus }).subscribe({
      next: () => {
        this.loadRegions();
        const message = newStatus ?
          'Region activated successfully' :
          'Region deactivated successfully';
        this.showSuccessMessage(message);
      },
      error: (error) => {
        console.error('Error toggling region status:', error);
        this.showErrorMessage('Failed to update region status');
      }
    });
  }

  // Export
  exportRegions(): void {
    const exportDto = {
      format: 'Excel' as const,
      includeInactive: false,
      language: 'Both' as const
    };

    this.lookupService.exportLookupTable('Regions', exportDto).subscribe({
      next: (blob: Blob) => {
        this.downloadFile(blob, 'regions_export');
      },
      error: (error) => {
        console.error('Error exporting regions:', error);
        this.showErrorMessage('Failed to export regions');
      }
    });
  }

  // Form Helper
  private createRegionForm(region?: RegionDto): FormGroup {
    return this.fb.group({
      name: [region?.name || '', [Validators.required]],
      nameAr: [region?.nameAr || ''],
      regionCode: [region?.regionCode || ''],
      countryId: [region?.countryId || null, [Validators.required]],
      isActive: [region?.isActive ?? true],
      sortOrder: [region?.sortOrder ?? 0, [Validators.required]]
    });
  }

  private downloadFile(blob: Blob, filename: string): void {
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `${filename}_${new Date().toISOString().slice(0, 10)}.csv`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    window.URL.revokeObjectURL(url);
  }

  private showSuccessMessage(message: string): void {
    // TODO: Implement toast notification
    console.log('Success:', message);
  }

  private showErrorMessage(message: string): void {
    // TODO: Implement toast notification
    console.error('Error:', message);
  }

  getPageRange(): number[] {
    const pages: number[] = [];
    const maxPagesToShow = 5;
    let startPage = Math.max(1, this.currentPage - Math.floor(maxPagesToShow / 2));
    let endPage = Math.min(this.totalPages, startPage + maxPagesToShow - 1);

    if (endPage - startPage + 1 < maxPagesToShow) {
      startPage = Math.max(1, endPage - maxPagesToShow + 1);
    }

    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }
    return pages;
  }

  getMinValue(a: number, b: number): number {
    return Math.min(a, b);
  }
}

import { TranslateService } from '@ngx-translate/core';
