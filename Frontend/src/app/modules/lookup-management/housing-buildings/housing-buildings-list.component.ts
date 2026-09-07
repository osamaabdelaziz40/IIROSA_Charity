import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { LookupManagementService } from '../services/lookup-management.service';
import {
  HousingBuildingDto,
  CreateHousingBuildingDto,
  UpdateHousingBuildingDto,
  LookupFilterDto,
  LookupPagedResult
} from '../models/lookup.model';
import { PaginationComponent } from '../../../shared/components';

@Component({
  selector: 'app-housing-buildings-list',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterModule, TranslateModule, PaginationComponent],
  templateUrl: './housing-buildings-list.component.html',
  styleUrls: ['./housing-buildings-list.component.scss']
})
export class HousingBuildingsListComponent implements OnInit {
  buildings: HousingBuildingDto[] = [];
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
  selectedBuilding?: HousingBuildingDto;
  buildingForm: FormGroup;

  constructor(
    private lookupService: LookupManagementService,
    private fb: FormBuilder,
    private translate: TranslateService
  ) {
    this.buildingForm = this.createBuildingForm();
  }

  ngOnInit(): void {
    this.loadBuildings();
  }

  loadBuildings(): void {
    this.loading = true;
    const filter: LookupFilterDto = {
      page: this.currentPage,
      pageSize: this.pageSize
    };

    if (this.searchText && this.searchText.trim()) {
      filter.searchText = this.searchText.trim();
    }
    if (this.filterIsActive !== undefined && this.filterIsActive !== null) {
      filter.isActive = this.filterIsActive;
    }

    this.lookupService.getHousingBuildingsItems(filter).subscribe({
      next: (result: LookupPagedResult<HousingBuildingDto>) => {
        this.buildings = result.items;
        this.totalCount = result.totalCount;
        this.totalPages = result.totalPages;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading housing buildings:', error);
        this.loading = false;
      }
    });
  }

  // Search and Filter
  onSearch(): void {
    this.currentPage = 1;
    this.loadBuildings();
  }

  onFilter(): void {
    this.currentPage = 1;
    this.loadBuildings();
  }

  clearFilters(): void {
    this.searchText = '';
    this.filterIsActive = undefined;
    this.loadBuildings();
  }

  // Pagination
  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadBuildings();
  }

  onPageSizeChange(size: number): void {
    this.pageSize = size;
    this.currentPage = 1;
    this.loadBuildings();
  }

  // Modal Operations
  openCreateModal(): void {
    this.modalMode = 'create';
    this.selectedBuilding = undefined;
    this.buildingForm = this.createBuildingForm();
    this.showModal = true;
  }

  openEditModal(building: HousingBuildingDto): void {
    this.modalMode = 'edit';
    this.selectedBuilding = building;
    this.buildingForm = this.createBuildingForm(building);
    this.showModal = true;
  }

  openViewModal(building: HousingBuildingDto): void {
    this.modalMode = 'view';
    this.selectedBuilding = building;
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.buildingForm.reset();
  }

  // CRUD Operations
  createBuilding(): void {
    if (this.buildingForm.invalid) {
      this.buildingForm.markAllAsTouched();
      return;
    }

    const formValue = this.buildingForm.value;
    // The entity's display name is computed from NameAr — send it as the Name too
    const dto: CreateHousingBuildingDto = { ...formValue, name: formValue.nameAr || '' };

    this.lookupService.createHousingBuilding(dto).subscribe({
      next: () => {
        this.closeModal();
        this.loadBuildings();
        this.showSuccessMessage('Housing building created successfully');
      },
      error: (error) => {
        console.error('Error creating housing building:', error);
        this.showErrorMessage('Failed to create housing building');
      }
    });
  }

  updateBuilding(): void {
    if (this.buildingForm.invalid) {
      this.buildingForm.markAllAsTouched();
      return;
    }

    if (!this.selectedBuilding) return;

    const dto: UpdateHousingBuildingDto = this.buildingForm.value;

    this.lookupService.updateHousingBuilding(this.selectedBuilding.id, dto).subscribe({
      next: () => {
        this.closeModal();
        this.loadBuildings();
        this.showSuccessMessage('Housing building updated successfully');
      },
      error: (error) => {
        console.error('Error updating housing building:', error);
        this.showErrorMessage('Failed to update housing building');
      }
    });
  }

  deleteBuilding(building: HousingBuildingDto): void {
    if (confirm(this.translate.instant('lookupManagement.confirmDelete', { name: building.name }))) {
      this.lookupService.deleteHousingBuilding(building.id).subscribe({
        next: () => {
          this.loadBuildings();
          this.showSuccessMessage('Housing building deleted successfully');
        },
        error: (error) => {
          console.error('Error deleting housing building:', error);
          this.showErrorMessage('Failed to delete housing building');
        }
      });
    }
  }

  toggleActiveStatus(building: HousingBuildingDto): void {
    if (building.isActive) {
      this.lookupService.deactivateHousingBuilding(building.id).subscribe({
        next: () => this.loadBuildings(),
        error: (error) => {
          console.error('Error deactivating housing building:', error);
          this.showErrorMessage('Failed to update status');
        }
      });
    } else {
      this.lookupService.activateHousingBuilding(building.id).subscribe({
        next: () => this.loadBuildings(),
        error: (error) => {
          console.error('Error activating housing building:', error);
          this.showErrorMessage('Failed to update status');
        }
      });
    }
  }

  // Export
  exportBuildings(): void {
    const exportDto = {
      format: 'Excel' as const,
      includeInactive: false,
      language: 'Both' as const
    };

    this.lookupService.exportLookupTable('HousingBuildings', exportDto).subscribe({
      next: (blob: Blob) => {
        this.downloadFile(blob, 'housing_buildings_export');
      },
      error: (error) => {
        console.error('Error exporting housing buildings:', error);
        this.showErrorMessage('Failed to export housing buildings');
      }
    });
  }

  trackById(index: number, item: HousingBuildingDto): number {
    return item.id;
  }

  // Form Helper
  private createBuildingForm(building?: HousingBuildingDto): FormGroup {
    return this.fb.group({
      nameAr: [building?.nameAr || '', [Validators.required]],
      nameEn: [building?.nameEn || ''],
      buildingNumber: [building?.buildingNumber ?? null],
      buildingAddress: [building?.buildingAddress || ''],
      description: [building?.description || ''],
      isActive: [building?.isActive ?? true],
      sortOrder: [building?.sortOrder ?? 0, [Validators.required]]
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
