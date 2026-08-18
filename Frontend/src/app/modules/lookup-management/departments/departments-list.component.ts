import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { LookupManagementService } from '../services/lookup-management.service';
import {
  DepartmentDto,
  CreateDepartmentDto,
  UpdateDepartmentDto,
  LookupFilterDto,
  LookupPagedResult
} from '../models/lookup.model';
import { PaginationComponent } from '../../../shared/components';

@Component({
  selector: 'app-departments-list',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterModule, TranslateModule, PaginationComponent],
  templateUrl: './departments-list.component.html',
  styleUrls: ['./departments-list.component.scss']
})
export class DepartmentsListComponent implements OnInit {
  departments: DepartmentDto[] = [];
  filteredDepartments: DepartmentDto[] = [];
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
  selectedDepartment?: DepartmentDto;
  departmentForm: FormGroup;

  constructor(
    private lookupService: LookupManagementService,
    private fb: FormBuilder,
    private translate: TranslateService
  ) {
    this.departmentForm = this.createDepartmentForm();
  }

  ngOnInit(): void {
    this.loadDepartments();
  }

  loadDepartments(): void {
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

    this.lookupService.getDepartments(filter).subscribe({
      next: (result: LookupPagedResult<DepartmentDto>) => {
        this.departments = result.items;
        this.filteredDepartments = result.items;
        this.totalCount = result.totalCount;
        this.totalPages = result.totalPages;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading departments:', error);
        this.loading = false;
      }
    });
  }

  // Search and Filter
  onSearch(): void {
    this.currentPage = 1;
    this.loadDepartments();
  }

  onFilter(): void {
    this.currentPage = 1;
    this.loadDepartments();
  }

  clearFilters(): void {
    this.searchText = '';
    this.filterIsActive = undefined;
    this.loadDepartments();
  }

  // Pagination
  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadDepartments();
  }

  onPageSizeChange(size: number): void {
    this.pageSize = size;
    this.currentPage = 1;
    this.loadDepartments();
  }

  // Modal Operations
  openCreateModal(): void {
    this.modalMode = 'create';
    this.selectedDepartment = undefined;
    this.departmentForm = this.createDepartmentForm();
    this.showModal = true;
  }

  openEditModal(department: DepartmentDto): void {
    this.modalMode = 'edit';
    this.selectedDepartment = department;
    this.departmentForm = this.createDepartmentForm(department);
    this.showModal = true;
  }

  openViewModal(department: DepartmentDto): void {
    this.modalMode = 'view';
    this.selectedDepartment = department;
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.departmentForm.reset();
  }

  // CRUD Operations
  createDepartment(): void {
    if (this.departmentForm.invalid) {
      this.departmentForm.markAllAsTouched();
      return;
    }

    const department: CreateDepartmentDto = this.departmentForm.value;

    this.lookupService.createDepartment(department).subscribe({
      next: (createdDepartment) => {
        this.closeModal();
        this.loadDepartments();
        this.showSuccessMessage('Department created successfully');
      },
      error: (error) => {
        console.error('Error creating department:', error);
        this.showErrorMessage('Failed to create department');
      }
    });
  }

  updateDepartment(): void {
    if (this.departmentForm.invalid) {
      this.departmentForm.markAllAsTouched();
      return;
    }

    if (!this.selectedDepartment) return;

    const department: UpdateDepartmentDto = this.departmentForm.value;

    this.lookupService.updateDepartment(this.selectedDepartment.id, department).subscribe({
      next: () => {
        this.closeModal();
        this.loadDepartments();
        this.showSuccessMessage('Department updated successfully');
      },
      error: (error) => {
        console.error('Error updating department:', error);
        this.showErrorMessage('Failed to update department');
      }
    });
  }

  deleteDepartment(department: DepartmentDto): void {
    if (confirm(this.translate.instant('lookupManagement.confirmDelete', { name: department.name }))) {
      this.lookupService.deleteDepartment(department.id).subscribe({
        next: () => {
          this.loadDepartments();
          this.showSuccessMessage('Department deleted successfully');
        },
        error: (error) => {
          console.error('Error deleting department:', error);
          this.showErrorMessage('Failed to delete department');
        }
      });
    }
  }

  toggleActiveStatus(department: DepartmentDto): void {
    const newStatus = !department.isActive;

    this.lookupService.updateDepartment(department.id, { ...department, isActive: newStatus }).subscribe({
      next: () => {
        this.loadDepartments();
        const message = newStatus ?
          'Department activated successfully' :
          'Department deactivated successfully';
        this.showSuccessMessage(message);
      },
      error: (error) => {
        console.error('Error toggling department status:', error);
        this.showErrorMessage('Failed to update department status');
      }
    });
  }

  // Export
  exportDepartments(): void {
    const exportDto = {
      format: 'Excel' as const,
      includeInactive: false,
      language: 'Both' as const
    };

    this.lookupService.exportLookupTable('Departments', exportDto).subscribe({
      next: (blob: Blob) => {
        this.downloadFile(blob, 'departments_export');
      },
      error: (error) => {
        console.error('Error exporting departments:', error);
        this.showErrorMessage('Failed to export departments');
      }
    });
  }

  // Form Helper
  private createDepartmentForm(department?: DepartmentDto): FormGroup {
    return this.fb.group({
      name: [department?.name || '', [Validators.required]],
      nameAr: [department?.nameAr || ''],
      departmentCode: [department?.departmentCode || ''],
      isActive: [department?.isActive ?? true],
      sortOrder: [department?.sortOrder ?? 0, [Validators.required]]
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

import { TranslateService } from '@ngx-translate/core';
