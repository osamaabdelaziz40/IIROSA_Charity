import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { LookupManagementService } from '../services/lookup-management.service';
import {
  CountryDto,
  CreateCountryDto,
  UpdateCountryDto,
  LookupFilterDto,
  LookupPagedResult
} from '../models/lookup.model';
import { PaginationComponent, BreadcrumbComponent, BreadcrumbItem } from '../../../shared/components';

@Component({
  selector: 'app-countries-list',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterModule, TranslateModule, PaginationComponent, BreadcrumbComponent],
  templateUrl: './countries-list.component.html',
  styleUrls: ['./countries-list.component.scss']
})
export class CountriesListComponent implements OnInit {
  // Breadcrumb items
  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'lookupManagement.title', url: '/lookup-management' },
    { label: 'lookupManagement.countries.title' }
  ];
  countries: CountryDto[] = [];
  filteredCountries: CountryDto[] = [];
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
  selectedCountry?: CountryDto;
  countryForm: FormGroup;

  constructor(
    private lookupService: LookupManagementService,
    private fb: FormBuilder,
    private translate: TranslateService
  ) {
    this.countryForm = this.createCountryForm();
  }

  ngOnInit(): void {
    this.loadCountries();
  }

  loadCountries(): void {
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

    this.lookupService.getCountries(filter).subscribe({
      next: (result: LookupPagedResult<CountryDto>) => {
        this.countries = result.items;
        this.filteredCountries = result.items;
        this.totalCount = result.totalCount;
        this.totalPages = result.totalPages;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading countries:', error);
        this.loading = false;
      }
    });
  }

  // Search and Filter
  onSearch(): void {
    this.currentPage = 1;
    this.loadCountries();
  }

  onFilter(): void {
    this.currentPage = 1;
    this.loadCountries();
  }

  clearFilters(): void {
    this.searchText = '';
    this.filterIsActive = undefined;
    this.loadCountries();
  }

  // Pagination
  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadCountries();
  }

  onPageSizeChange(size: number): void {
    this.pageSize = size;
    this.currentPage = 1;
    this.loadCountries();
  }

  // Modal Operations
  openCreateModal(): void {
    this.modalMode = 'create';
    this.selectedCountry = undefined;
    this.countryForm = this.createCountryForm();
    this.showModal = true;
  }

  openEditModal(country: CountryDto): void {
    this.modalMode = 'edit';
    this.selectedCountry = country;
    this.countryForm = this.createCountryForm(country);
    this.showModal = true;
  }

  openViewModal(country: CountryDto): void {
    this.modalMode = 'view';
    this.selectedCountry = country;
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.countryForm.reset();
  }

  // CRUD Operations
  createCountry(): void {
    if (this.countryForm.invalid) {
      this.countryForm.markAllAsTouched();
      return;
    }

    const country: CreateCountryDto = this.countryForm.value;

    this.lookupService.createCountry(country).subscribe({
      next: (createdCountry) => {
        this.closeModal();
        this.loadCountries();
        this.showSuccessMessage('Country created successfully');
      },
      error: (error) => {
        console.error('Error creating country:', error);
        this.showErrorMessage('Failed to create country');
      }
    });
  }

  updateCountry(): void {
    if (this.countryForm.invalid) {
      this.countryForm.markAllAsTouched();
      return;
    }

    if (!this.selectedCountry) return;

    const country: UpdateCountryDto = this.countryForm.value;

    this.lookupService.updateCountry(this.selectedCountry.id, country).subscribe({
      next: () => {
        this.closeModal();
        this.loadCountries();
        this.showSuccessMessage('Country updated successfully');
      },
      error: (error) => {
        console.error('Error updating country:', error);
        this.showErrorMessage('Failed to update country');
      }
    });
  }

  deleteCountry(country: CountryDto): void {
    if (confirm(this.translate.instant('lookupManagement.confirmDelete', { name: country.name }))) {
      this.lookupService.deleteCountry(country.id).subscribe({
        next: () => {
          this.loadCountries();
          this.showSuccessMessage('Country deleted successfully');
        },
        error: (error) => {
          console.error('Error deleting country:', error);
          this.showErrorMessage('Failed to delete country');
        }
      });
    }
  }

  toggleActiveStatus(country: CountryDto): void {
    const action = country.isActive ?
      this.lookupService.deactivateCountry(country.id) :
      this.lookupService.activateCountry(country.id);

    action.subscribe({
      next: () => {
        this.loadCountries();
        const message = country.isActive ?
          'Country deactivated successfully' :
          'Country activated successfully';
        this.showSuccessMessage(message);
      },
      error: (error) => {
        console.error('Error toggling country status:', error);
        this.showErrorMessage('Failed to update country status');
      }
    });
  }

  // Export
  exportCountries(): void {
    const exportDto = {
      format: 'Excel' as const,
      includeInactive: false,
      language: 'Both' as const
    };

    this.lookupService.exportLookupTable('Countries', exportDto).subscribe({
      next: (blob: Blob) => {
        this.downloadFile(blob, 'countries_export');
      },
      error: (error) => {
        console.error('Error exporting countries:', error);
        this.showErrorMessage('Failed to export countries');
      }
    });
  }

  // Form Helper
  private createCountryForm(country?: CountryDto): FormGroup {
    return this.fb.group({
      name: [country?.name || '', [Validators.required]],
      nameAr: [country?.nameAr || ''],
      nameEn: [country?.nameEn || ''],
      isoCode: [country?.isoCode || ''],
      dialingCode: [country?.dialingCode || ''],
      currency: [country?.currency || ''],
      flagIcon: [country?.flagIcon || ''],
      isActive: [country?.isActive ?? true],
      sortOrder: [country?.sortOrder ?? 0, [Validators.required]]
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