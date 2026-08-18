import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { LookupManagementService } from '../services/lookup-management.service';
import {
  CenterDto,
  CreateCenterDto,
  UpdateCenterDto,
  LookupFilterDto,
  LookupPagedResult,
  RegionDto
} from '../models/lookup.model';
import { PaginationComponent } from '../../../shared/components';

@Component({
  selector: 'app-centers-list',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterModule, TranslateModule, PaginationComponent],
  templateUrl: './centers-list.component.html',
  styleUrls: ['./centers-list.component.scss']
})
export class CentersListComponent implements OnInit {
  centers: CenterDto[] = [];
  filteredCenters: CenterDto[] = [];
  regions: RegionDto[] = [];
  loading = false;

  // Pagination
  currentPage = 1;
  pageSize = 20;
  totalCount = 0;
  totalPages = 0;

  // Filter
  searchText = '';
  filterIsActive?: boolean;
  filterRegionId?: number;

  // Modal
  showModal = false;
  modalMode: 'create' | 'edit' | 'view' = 'view';
  selectedCenter?: CenterDto;
  centerForm: FormGroup;

  constructor(
    private lookupService: LookupManagementService,
    private fb: FormBuilder,
    private translate: TranslateService
  ) {
    this.centerForm = this.createCenterForm();
  }

  ngOnInit(): void {
    this.loadCenters();
    this.loadRegions();
  }

  loadCenters(): void {
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

    this.lookupService.getCenters(filter).subscribe({
      next: (result: LookupPagedResult<CenterDto>) => {
        this.centers = result.items;
        this.filteredCenters = result.items;
        this.totalCount = result.totalCount;
        this.totalPages = result.totalPages;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading centers:', error);
        this.loading = false;
      }
    });
  }

  loadRegions(): void {
    this.lookupService.getRegions({ page: 1, pageSize: 100 }).subscribe({
      next: (result) => {
        this.regions = result.items.filter(r => r.isActive);
      },
      error: (error) => {
        console.error('Error loading regions:', error);
      }
    });
  }

  // Search and Filter
  onSearch(): void {
    this.currentPage = 1;
    this.loadCenters();
  }

  onFilter(): void {
    this.currentPage = 1;
    this.loadCenters();
  }

  clearFilters(): void {
    this.searchText = '';
    this.filterIsActive = undefined;
    this.filterRegionId = undefined;
    this.loadCenters();
  }

  // Pagination
  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadCenters();
  }

  onPageSizeChange(size: number): void {
    this.pageSize = size;
    this.currentPage = 1;
    this.loadCenters();
  }

  // Modal Operations
  openCreateModal(): void {
    this.modalMode = 'create';
    this.selectedCenter = undefined;
    this.centerForm = this.createCenterForm();
    this.showModal = true;
  }

  openEditModal(center: CenterDto): void {
    this.modalMode = 'edit';
    this.selectedCenter = center;
    this.centerForm = this.createCenterForm(center);
    this.showModal = true;
  }

  openViewModal(center: CenterDto): void {
    this.modalMode = 'view';
    this.selectedCenter = center;
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.centerForm.reset();
  }

  // CRUD Operations
  createCenter(): void {
    if (this.centerForm.invalid) {
      this.centerForm.markAllAsTouched();
      return;
    }

    const center: CreateCenterDto = this.centerForm.value;

    this.lookupService.createCenter(center).subscribe({
      next: (createdCenter) => {
        this.closeModal();
        this.loadCenters();
        this.showSuccessMessage('Center created successfully');
      },
      error: (error) => {
        console.error('Error creating center:', error);
        this.showErrorMessage('Failed to create center');
      }
    });
  }

  updateCenter(): void {
    if (this.centerForm.invalid) {
      this.centerForm.markAllAsTouched();
      return;
    }

    if (!this.selectedCenter) return;

    const center: UpdateCenterDto = this.centerForm.value;

    this.lookupService.updateCenter(this.selectedCenter.id, center).subscribe({
      next: () => {
        this.closeModal();
        this.loadCenters();
        this.showSuccessMessage('Center updated successfully');
      },
      error: (error) => {
        console.error('Error updating center:', error);
        this.showErrorMessage('Failed to update center');
      }
    });
  }

  deleteCenter(center: CenterDto): void {
    if (confirm(this.translate.instant('lookupManagement.confirmDelete', { name: center.name }))) {
      this.lookupService.deleteCenter(center.id).subscribe({
        next: () => {
          this.loadCenters();
          this.showSuccessMessage('Center deleted successfully');
        },
        error: (error) => {
          console.error('Error deleting center:', error);
          this.showErrorMessage('Failed to delete center');
        }
      });
    }
  }

  toggleActiveStatus(center: CenterDto): void {
    const newStatus = !center.isActive;

    this.lookupService.updateCenter(center.id, { ...center, isActive: newStatus }).subscribe({
      next: () => {
        this.loadCenters();
        const message = newStatus ?
          'Center activated successfully' :
          'Center deactivated successfully';
        this.showSuccessMessage(message);
      },
      error: (error) => {
        console.error('Error toggling center status:', error);
        this.showErrorMessage('Failed to update center status');
      }
    });
  }

  // Export
  exportCenters(): void {
    const exportDto = {
      format: 'Excel' as const,
      includeInactive: false,
      language: 'Both' as const
    };

    this.lookupService.exportLookupTable('Centers', exportDto).subscribe({
      next: (blob: Blob) => {
        this.downloadFile(blob, 'centers_export');
      },
      error: (error) => {
        console.error('Error exporting centers:', error);
        this.showErrorMessage('Failed to export centers');
      }
    });
  }

  // Form Helper
  private createCenterForm(center?: CenterDto): FormGroup {
    return this.fb.group({
      name: [center?.name || '', [Validators.required]],
      nameAr: [center?.nameAr || ''],
      centerCode: [center?.centerCode || ''],
      regionId: [center?.regionId || null, [Validators.required]],
      isActive: [center?.isActive ?? true],
      sortOrder: [center?.sortOrder ?? 0, [Validators.required]]
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
}
