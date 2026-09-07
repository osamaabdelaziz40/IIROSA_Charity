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
  selector: 'app-outgoing-categories-list',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterModule, TranslateModule, PaginationComponent],
  templateUrl: './outgoing-categories-list.component.html',
  styleUrls: ['./outgoing-categories-list.component.scss']
})
export class OutgoingCategoriesListComponent implements OnInit {
  outgoingCategories: LookupDto[] = [];
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
  selectedCategory?: LookupDto;
  categoryForm: FormGroup;

  constructor(
    private lookupService: LookupManagementService,
    private fb: FormBuilder,
    private translate: TranslateService
  ) {
    this.categoryForm = this.createCategoryForm();
  }

  ngOnInit(): void {
    this.loadOutgoingCategories();
  }

  loadOutgoingCategories(): void {
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

    this.lookupService.getOutgoingCategoriesItems(filter).subscribe({
      next: (result: LookupPagedResult<LookupDto>) => {
        this.outgoingCategories = result.items;
        this.totalCount = result.totalCount;
        this.totalPages = result.totalPages;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading outgoing categories:', error);
        this.loading = false;
      }
    });
  }

  // Search and Filter
  onSearch(): void {
    this.currentPage = 1;
    this.loadOutgoingCategories();
  }

  onFilter(): void {
    this.currentPage = 1;
    this.loadOutgoingCategories();
  }

  clearFilters(): void {
    this.searchText = '';
    this.filterIsActive = undefined;
    this.loadOutgoingCategories();
  }

  // Pagination
  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadOutgoingCategories();
  }

  onPageSizeChange(size: number): void {
    this.pageSize = size;
    this.currentPage = 1;
    this.loadOutgoingCategories();
  }

  // Modal Operations
  openCreateModal(): void {
    this.modalMode = 'create';
    this.selectedCategory = undefined;
    this.categoryForm = this.createCategoryForm();
    this.showModal = true;
  }

  openEditModal(category: LookupDto): void {
    this.modalMode = 'edit';
    this.selectedCategory = category;
    this.categoryForm = this.createCategoryForm(category);
    this.showModal = true;
  }

  openViewModal(category: LookupDto): void {
    this.modalMode = 'view';
    this.selectedCategory = category;
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.categoryForm.reset();
  }

  // CRUD Operations
  createOutgoingCategory(): void {
    if (this.categoryForm.invalid) {
      this.categoryForm.markAllAsTouched();
      return;
    }

    // The entity's display name is computed from NameAr — send it as the Name too
    const formValue = this.categoryForm.value;
    const dto: CreateLookupDto = { ...formValue, name: formValue.nameAr || '' };

    this.lookupService.createOutgoingCategory(dto).subscribe({
      next: () => {
        this.closeModal();
        this.loadOutgoingCategories();
        this.showSuccessMessage('Outgoing category created successfully');
      },
      error: (error) => {
        console.error('Error creating outgoing category:', error);
        this.showErrorMessage('Failed to create outgoing category');
      }
    });
  }

  updateOutgoingCategory(): void {
    if (this.categoryForm.invalid) {
      this.categoryForm.markAllAsTouched();
      return;
    }

    if (!this.selectedCategory) return;

    const dto: UpdateLookupDto = this.categoryForm.value;

    this.lookupService.updateOutgoingCategory(this.selectedCategory.id, dto).subscribe({
      next: () => {
        this.closeModal();
        this.loadOutgoingCategories();
        this.showSuccessMessage('Outgoing category updated successfully');
      },
      error: (error) => {
        console.error('Error updating outgoing category:', error);
        this.showErrorMessage('Failed to update outgoing category');
      }
    });
  }

  deleteOutgoingCategory(category: LookupDto): void {
    if (confirm(this.translate.instant('lookupManagement.confirmDelete', { name: category.name }))) {
      this.lookupService.deleteOutgoingCategory(category.id).subscribe({
        next: () => {
          this.loadOutgoingCategories();
          this.showSuccessMessage('Outgoing category deleted successfully');
        },
        error: (error) => {
          console.error('Error deleting outgoing category:', error);
          this.showErrorMessage('Failed to delete outgoing category');
        }
      });
    }
  }

  toggleActiveStatus(category: LookupDto): void {
    const newStatus = !category.isActive;

    if (newStatus) {
      this.lookupService.activateOutgoingCategory(category.id).subscribe({
        next: () => this.loadOutgoingCategories(),
        error: (error) => {
          console.error('Error activating outgoing category:', error);
          this.showErrorMessage('Failed to update status');
        }
      });
    } else {
      this.lookupService.deactivateOutgoingCategory(category.id).subscribe({
        next: () => this.loadOutgoingCategories(),
        error: (error) => {
          console.error('Error deactivating outgoing category:', error);
          this.showErrorMessage('Failed to update status');
        }
      });
    }
  }

  // Export
  exportOutgoingCategories(): void {
    const exportDto = {
      format: 'Excel' as const,
      includeInactive: false,
      language: 'Both' as const
    };

    this.lookupService.exportLookupTable('OutgoingCategories', exportDto).subscribe({
      next: (blob: Blob) => {
        this.downloadFile(blob, 'outgoing_categories_export');
      },
      error: (error) => {
        console.error('Error exporting outgoing categories:', error);
        this.showErrorMessage('Failed to export outgoing categories');
      }
    });
  }

  // Form Helper
  trackById(index: number, item: LookupDto): number {
    return item.id;
  }

  private createCategoryForm(category?: LookupDto): FormGroup {
    return this.fb.group({
      nameAr: [category?.nameAr || '', [Validators.required]],
      nameEn: [category?.nameEn || ''],
      description: [category?.description || ''],
      isActive: [category?.isActive ?? true],
      sortOrder: [category?.sortOrder ?? 0, [Validators.required]]
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
