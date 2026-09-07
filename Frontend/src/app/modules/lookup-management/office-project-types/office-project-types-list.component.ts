import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { LookupManagementService } from '../services/lookup-management.service';
import {
  LookupDto,
  CreateLookupDto,
  UpdateLookupDto,
  LookupFilterDto,
  LookupPagedResult
} from '../models/lookup.model';
import { PaginationComponent } from '../../../shared/components';

@Component({
  selector: 'app-office-project-types-list',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterModule, TranslateModule, PaginationComponent],
  templateUrl: './office-project-types-list.component.html',
  styleUrls: ['./office-project-types-list.component.scss']
})
export class OfficeProjectTypesListComponent implements OnInit {
  officeProjectTypes: LookupDto[] = [];
  loading = false;

  // Pagination
  currentPage = 1;
  pageSize = 20;
  totalCount = 0;
  totalPages = 0;

  // Filter
  searchText = '';
  filterIsActive?: boolean;

  // Modal
  showModal = false;
  modalMode: 'create' | 'edit' | 'view' = 'view';
  selectedType?: LookupDto;
  typeForm: FormGroup;

  constructor(
    private lookupService: LookupManagementService,
    private fb: FormBuilder,
    private translate: TranslateService
  ) {
    this.typeForm = this.createTypeForm();
  }

  ngOnInit(): void {
    this.loadOfficeProjectTypes();
  }

  loadOfficeProjectTypes(): void {
    this.loading = true;
    const filter: LookupFilterDto = {
      page: this.currentPage,
      pageSize: this.pageSize
    };

    // Only add optional filters if they have values
    if (this.searchText && this.searchText.trim()) {
      filter.searchText = this.searchText.trim();
    }
    if (this.filterIsActive !== undefined && this.filterIsActive !== null) {
      filter.isActive = this.filterIsActive;
    }

    this.lookupService.getOfficeProjectTypesItems(filter).subscribe({
      next: (result: LookupPagedResult<LookupDto>) => {
        this.officeProjectTypes = result.items;
        this.totalCount = result.totalCount;
        this.totalPages = result.totalPages;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading office project types:', error);
        this.loading = false;
      }
    });
  }

  // Search and Filter
  onSearch(): void {
    this.currentPage = 1;
    this.loadOfficeProjectTypes();
  }

  onFilter(): void {
    this.currentPage = 1;
    this.loadOfficeProjectTypes();
  }

  clearFilters(): void {
    this.searchText = '';
    this.filterIsActive = undefined;
    this.loadOfficeProjectTypes();
  }

  // Pagination
  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadOfficeProjectTypes();
  }

  onPageSizeChange(size: number): void {
    this.pageSize = size;
    this.currentPage = 1;
    this.loadOfficeProjectTypes();
  }

  // Modal Operations
  openCreateModal(): void {
    this.modalMode = 'create';
    this.selectedType = undefined;
    this.typeForm = this.createTypeForm();
    this.showModal = true;
  }

  openEditModal(type: LookupDto): void {
    this.modalMode = 'edit';
    this.selectedType = type;
    this.typeForm = this.createTypeForm(type);
    this.showModal = true;
  }

  openViewModal(type: LookupDto): void {
    this.modalMode = 'view';
    this.selectedType = type;
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.typeForm.reset();
  }

  // CRUD Operations
  createOfficeProjectType(): void {
    if (this.typeForm.invalid) {
      this.typeForm.markAllAsTouched();
      return;
    }

    // The entity's display name is computed from NameAr — send it as the Name too
    const formValue = this.typeForm.value;
    const dto: CreateLookupDto = { ...formValue, name: formValue.nameAr || '' };

    this.lookupService.createOfficeProjectType(dto).subscribe({
      next: () => {
        this.closeModal();
        this.loadOfficeProjectTypes();
        this.showSuccessMessage('Office project type created successfully');
      },
      error: (error) => {
        console.error('Error creating office project type:', error);
        this.showErrorMessage('Failed to create office project type');
      }
    });
  }

  updateOfficeProjectType(): void {
    if (this.typeForm.invalid) {
      this.typeForm.markAllAsTouched();
      return;
    }

    if (!this.selectedType) return;

    const dto: UpdateLookupDto = this.typeForm.value;

    this.lookupService.updateOfficeProjectType(this.selectedType.id, dto).subscribe({
      next: () => {
        this.closeModal();
        this.loadOfficeProjectTypes();
        this.showSuccessMessage('Office project type updated successfully');
      },
      error: (error) => {
        console.error('Error updating office project type:', error);
        this.showErrorMessage('Failed to update office project type');
      }
    });
  }

  deleteOfficeProjectType(type: LookupDto): void {
    if (confirm(this.translate.instant('lookupManagement.confirmDelete', { name: type.name }))) {
      this.lookupService.deleteOfficeProjectType(type.id).subscribe({
        next: () => {
          this.loadOfficeProjectTypes();
          this.showSuccessMessage('Office project type deleted successfully');
        },
        error: (error) => {
          console.error('Error deleting office project type:', error);
          this.showErrorMessage('Failed to delete office project type');
        }
      });
    }
  }

  toggleActiveStatus(type: LookupDto): void {
    const newStatus = !type.isActive;

    if (newStatus) {
      this.lookupService.activateOfficeProjectType(type.id).subscribe({
        next: () => this.loadOfficeProjectTypes(),
        error: (error) => {
          console.error('Error activating office project type:', error);
          this.showErrorMessage('Failed to update status');
        }
      });
    } else {
      this.lookupService.deactivateOfficeProjectType(type.id).subscribe({
        next: () => this.loadOfficeProjectTypes(),
        error: (error) => {
          console.error('Error deactivating office project type:', error);
          this.showErrorMessage('Failed to update status');
        }
      });
    }
  }

  // Export
  exportOfficeProjectTypes(): void {
    const exportDto = {
      format: 'Excel' as const,
      includeInactive: false,
      language: 'Both' as const
    };

    this.lookupService.exportLookupTable('OfficeProjectTypes', exportDto).subscribe({
      next: (blob: Blob) => {
        this.downloadFile(blob, 'office_project_types_export');
      },
      error: (error) => {
        console.error('Error exporting office project types:', error);
        this.showErrorMessage('Failed to export office project types');
      }
    });
  }

  // Form Helper
  trackById(index: number, item: LookupDto): number {
    return item.id;
  }

  private createTypeForm(type?: LookupDto): FormGroup {
    return this.fb.group({
      nameAr: [type?.nameAr || '', [Validators.required]],
      nameEn: [type?.nameEn || ''],
      description: [type?.description || ''],
      isActive: [type?.isActive ?? true],
      sortOrder: [type?.sortOrder ?? 0, [Validators.required]]
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
    console.log('Success:', message);
  }

  private showErrorMessage(message: string): void {
    console.error('Error:', message);
  }
}
