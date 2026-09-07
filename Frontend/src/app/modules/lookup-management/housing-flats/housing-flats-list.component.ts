import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { LookupManagementService } from '../services/lookup-management.service';
import {
  LookupDto,
  HousingFlatDto,
  CreateHousingFlatDto,
  UpdateHousingFlatDto
} from '../models/lookup.model';

@Component({
  selector: 'app-housing-flats-list',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterModule, TranslateModule],
  templateUrl: './housing-flats-list.component.html',
  styleUrls: ['./housing-flats-list.component.scss']
})
export class HousingFlatsListComponent implements OnInit {
  flats: HousingFlatDto[] = [];
  buildings: LookupDto[] = [];
  selectedBuildingId?: number;
  loading = false;
  buildingsLoading = false;

  // Modal
  showModal = false;
  modalMode: 'create' | 'edit' | 'view' = 'view';
  selectedFlat?: HousingFlatDto;
  flatForm: FormGroup;

  constructor(
    private lookupService: LookupManagementService,
    private fb: FormBuilder,
    private translate: TranslateService
  ) {
    this.flatForm = this.createFlatForm();
  }

  ngOnInit(): void {
    this.loadBuildings();
  }

  /** Active buildings — the cascade source (شقق تُعرض لكل عمارة) */
  loadBuildings(): void {
    this.buildingsLoading = true;
    this.lookupService.getHousingBuildings().subscribe({
      next: (buildings) => {
        this.buildings = buildings;
        this.buildingsLoading = false;
      },
      error: (error) => {
        console.error('Error loading housing buildings:', error);
        this.buildingsLoading = false;
      }
    });
  }

  onBuildingChange(): void {
    this.flats = [];
    if (this.selectedBuildingId) {
      this.loadFlats();
    }
  }

  loadFlats(): void {
    if (!this.selectedBuildingId) return;

    this.loading = true;
    this.lookupService.getHousingFlatItems(this.selectedBuildingId).subscribe({
      next: (flats) => {
        this.flats = flats;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading housing flats:', error);
        this.loading = false;
      }
    });
  }

  get selectedBuildingName(): string | undefined {
    return this.buildings.find(b => b.id === this.selectedBuildingId)?.name;
  }

  // Modal Operations
  openCreateModal(): void {
    if (!this.selectedBuildingId) return;

    this.modalMode = 'create';
    this.selectedFlat = undefined;
    this.flatForm = this.createFlatForm();
    this.showModal = true;
  }

  openEditModal(flat: HousingFlatDto): void {
    this.modalMode = 'edit';
    this.selectedFlat = flat;
    this.flatForm = this.createFlatForm(flat);
    this.showModal = true;
  }

  openViewModal(flat: HousingFlatDto): void {
    this.modalMode = 'view';
    this.selectedFlat = flat;
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.flatForm.reset();
  }

  // CRUD Operations
  createFlat(): void {
    if (this.flatForm.invalid) {
      this.flatForm.markAllAsTouched();
      return;
    }

    const formValue = this.flatForm.value;
    // The entity's display name is computed from NameAr — send it as the Name too
    const dto: CreateHousingFlatDto = {
      ...formValue,
      name: formValue.nameAr || '',
      buildingId: formValue.buildingId ?? this.selectedBuildingId!
    };

    this.lookupService.createHousingFlat(dto).subscribe({
      next: () => {
        this.closeModal();
        this.loadFlats();
        console.log('Housing flat created successfully');
      },
      error: (error) => {
        console.error('Error creating housing flat:', error);
      }
    });
  }

  updateFlat(): void {
    if (this.flatForm.invalid) {
      this.flatForm.markAllAsTouched();
      return;
    }

    if (!this.selectedFlat) return;

    const dto: UpdateHousingFlatDto = this.flatForm.value;

    this.lookupService.updateHousingFlat(this.selectedFlat.id, dto).subscribe({
      next: () => {
        this.closeModal();
        this.loadFlats();
        console.log('Housing flat updated successfully');
      },
      error: (error) => {
        console.error('Error updating housing flat:', error);
      }
    });
  }

  deleteFlat(flat: HousingFlatDto): void {
    if (confirm(this.translate.instant('lookupManagement.confirmDelete', { name: flat.name }))) {
      this.lookupService.deleteHousingFlat(flat.id).subscribe({
        next: () => {
          this.loadFlats();
          console.log('Housing flat deleted successfully');
        },
        error: (error) => {
          console.error('Error deleting housing flat:', error);
        }
      });
    }
  }

  toggleActiveStatus(flat: HousingFlatDto): void {
    if (flat.isActive) {
      this.lookupService.deactivateHousingFlat(flat.id).subscribe({
        next: () => this.loadFlats(),
        error: (error) => console.error('Error deactivating housing flat:', error)
      });
    } else {
      this.lookupService.activateHousingFlat(flat.id).subscribe({
        next: () => this.loadFlats(),
        error: (error) => console.error('Error activating housing flat:', error)
      });
    }
  }

  // Export
  exportFlats(): void {
    const exportDto = {
      format: 'Excel' as const,
      includeInactive: false,
      language: 'Both' as const
    };

    this.lookupService.exportLookupTable('HousingFlats', exportDto).subscribe({
      next: (blob: Blob) => {
        this.downloadFile(blob, 'housing_flats_export');
      },
      error: (error) => {
        console.error('Error exporting housing flats:', error);
      }
    });
  }

  trackById(index: number, item: HousingFlatDto): number {
    return item.id;
  }

  trackByBuildingId(index: number, item: LookupDto): number {
    return item.id;
  }

  // Form Helper
  private createFlatForm(flat?: HousingFlatDto): FormGroup {
    return this.fb.group({
      nameAr: [flat?.nameAr || '', [Validators.required]],
      nameEn: [flat?.nameEn || ''],
      buildingId: [flat?.buildingId ?? this.selectedBuildingId ?? null, [Validators.required, Validators.min(1)]],
      number: [flat?.number ?? null],
      sizeInMtr: [flat?.sizeInMtr ?? null],
      description: [flat?.description || ''],
      isActive: [flat?.isActive ?? true],
      sortOrder: [flat?.sortOrder ?? 0, [Validators.required]]
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
}
