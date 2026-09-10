import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';
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

/** Per-table labels + export key — the §4 family-data catalogues share one screen. */
interface SimpleLookupConfig {
  /** API route segment (house-ownerships / income-types / family-project-statuses) */
  endpoint: string;
  /** LookupManagement export switch key */
  exportTable: string;
  keys: {
    title: string;
    add: string;
    edit: string;
    details: string;
    noneFound: string;
  };
}

const TABLE_CONFIGS: Record<string, SimpleLookupConfig> = {
  'house-ownerships': {
    endpoint: 'house-ownerships',
    exportTable: 'HouseOwnerships',
    keys: {
      title: 'lookupManagement.houseOwnerships.title',
      add: 'lookupManagement.houseOwnerships.add',
      edit: 'lookupManagement.houseOwnerships.edit',
      details: 'lookupManagement.houseOwnerships.details',
      noneFound: 'lookupManagement.houseOwnerships.noneFound'
    }
  },
  'income-types': {
    endpoint: 'income-types',
    exportTable: 'IncomeTypes',
    keys: {
      title: 'lookupManagement.incomeTypes.title',
      add: 'lookupManagement.incomeTypes.add',
      edit: 'lookupManagement.incomeTypes.edit',
      details: 'lookupManagement.incomeTypes.details',
      noneFound: 'lookupManagement.incomeTypes.noneFound'
    }
  },
  'family-project-statuses': {
    endpoint: 'family-project-statuses',
    exportTable: 'FamilyProjectStatuses',
    keys: {
      title: 'lookupManagement.familyProjectStatuses.title',
      add: 'lookupManagement.familyProjectStatuses.add',
      edit: 'lookupManagement.familyProjectStatuses.edit',
      details: 'lookupManagement.familyProjectStatuses.details',
      noneFound: 'lookupManagement.familyProjectStatuses.noneFound'
    }
  }
};

@Component({
  selector: 'app-simple-lookup-list',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterModule, TranslateModule, PaginationComponent],
  templateUrl: './simple-lookup-list.component.html',
  styleUrls: ['./simple-lookup-list.component.scss']
})
export class SimpleLookupListComponent implements OnInit {
  cfg!: SimpleLookupConfig;

  items: LookupDto[] = [];
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
  selectedItem?: LookupDto;
  itemForm: FormGroup;

  constructor(
    private lookupService: LookupManagementService,
    private fb: FormBuilder,
    private translate: TranslateService,
    private route: ActivatedRoute
  ) {
    this.itemForm = this.createItemForm();
  }

  ngOnInit(): void {
    const table = this.route.snapshot.data['table'] as string;
    this.cfg = TABLE_CONFIGS[table];
    if (!this.cfg) {
      console.error(`Unknown simple lookup table: ${table}`);
      return;
    }
    this.loadItems();
  }

  loadItems(): void {
    if (!this.cfg) return;
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

    this.lookupService.getSimpleLookupItems(this.cfg.endpoint, filter).subscribe({
      next: (result: LookupPagedResult<LookupDto>) => {
        this.items = result.items;
        this.totalCount = result.totalCount;
        this.totalPages = result.totalPages;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading lookup items:', error);
        this.loading = false;
      }
    });
  }

  // Search and Filter
  onSearch(): void {
    this.currentPage = 1;
    this.loadItems();
  }

  onFilter(): void {
    this.currentPage = 1;
    this.loadItems();
  }

  clearFilters(): void {
    this.searchText = '';
    this.filterIsActive = undefined;
    this.loadItems();
  }

  // Pagination
  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadItems();
  }

  onPageSizeChange(size: number): void {
    this.pageSize = size;
    this.currentPage = 1;
    this.loadItems();
  }

  // Modal Operations
  openCreateModal(): void {
    this.modalMode = 'create';
    this.selectedItem = undefined;
    this.itemForm = this.createItemForm();
    this.showModal = true;
  }

  openEditModal(item: LookupDto): void {
    this.modalMode = 'edit';
    this.selectedItem = item;
    this.itemForm = this.createItemForm(item);
    this.showModal = true;
  }

  openViewModal(item: LookupDto): void {
    this.modalMode = 'view';
    this.selectedItem = item;
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.itemForm.reset();
  }

  // CRUD Operations
  createItem(): void {
    if (this.itemForm.invalid) {
      this.itemForm.markAllAsTouched();
      return;
    }

    // The entity's display name is computed from NameAr — send it as the Name too
    const formValue = this.itemForm.value;
    const dto: CreateLookupDto = { ...formValue, name: formValue.nameAr || '' };

    this.lookupService.createSimpleLookup(this.cfg.endpoint, dto).subscribe({
      next: () => {
        this.closeModal();
        this.loadItems();
        this.showSuccessMessage('Lookup item created successfully');
      },
      error: (error) => {
        console.error('Error creating lookup item:', error);
        this.showErrorMessage('Failed to create lookup item');
      }
    });
  }

  updateItem(): void {
    if (this.itemForm.invalid) {
      this.itemForm.markAllAsTouched();
      return;
    }

    if (!this.selectedItem) return;

    const dto: UpdateLookupDto = this.itemForm.value;

    this.lookupService.updateSimpleLookup(this.cfg.endpoint, this.selectedItem.id, dto).subscribe({
      next: () => {
        this.closeModal();
        this.loadItems();
        this.showSuccessMessage('Lookup item updated successfully');
      },
      error: (error) => {
        console.error('Error updating lookup item:', error);
        this.showErrorMessage('Failed to update lookup item');
      }
    });
  }

  deleteItem(item: LookupDto): void {
    if (confirm(this.translate.instant('lookupManagement.confirmDelete', { name: item.name }))) {
      this.lookupService.deleteSimpleLookup(this.cfg.endpoint, item.id).subscribe({
        next: () => {
          this.loadItems();
          this.showSuccessMessage('Lookup item deleted successfully');
        },
        error: (error) => {
          console.error('Error deleting lookup item:', error);
          this.showErrorMessage('Failed to delete lookup item');
        }
      });
    }
  }

  toggleActiveStatus(item: LookupDto): void {
    const newStatus = !item.isActive;

    if (newStatus) {
      this.lookupService.activateSimpleLookup(this.cfg.endpoint, item.id).subscribe({
        next: () => this.loadItems(),
        error: (error) => {
          console.error('Error activating lookup item:', error);
          this.showErrorMessage('Failed to update status');
        }
      });
    } else {
      this.lookupService.deactivateSimpleLookup(this.cfg.endpoint, item.id).subscribe({
        next: () => this.loadItems(),
        error: (error) => {
          console.error('Error deactivating lookup item:', error);
          this.showErrorMessage('Failed to update status');
        }
      });
    }
  }

  // Export
  exportItems(): void {
    const exportDto = {
      format: 'Excel' as const,
      includeInactive: false,
      language: 'Both' as const
    };

    this.lookupService.exportLookupTable(this.cfg.exportTable, exportDto).subscribe({
      next: (blob: Blob) => {
        this.downloadFile(blob, `${this.cfg.endpoint}_export`);
      },
      error: (error) => {
        console.error('Error exporting lookup items:', error);
        this.showErrorMessage('Failed to export lookup items');
      }
    });
  }

  // Form Helper
  trackById(index: number, item: LookupDto): number {
    return item.id;
  }

  private createItemForm(item?: LookupDto): FormGroup {
    return this.fb.group({
      nameAr: [item?.nameAr || '', [Validators.required]],
      nameEn: [item?.nameEn || ''],
      description: [item?.description || ''],
      isActive: [item?.isActive ?? true],
      sortOrder: [item?.sortOrder ?? 0, [Validators.required]]
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
