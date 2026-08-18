import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { Router } from '@angular/router';
import { IncomingDto, IncomingSearchRequest } from '../models/incoming.model';
import { IncomingService } from '../services/incoming.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { TranslateModule } from '@ngx-translate/core';
import { RouterModule } from '@angular/router';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../shared/components';
import { SharedModule } from '../../../shared/shared.module';
import { DropDownComponent } from '../../../shared/components/drop-down/drop-down.component';

@Component({
  selector: 'app-incoming-letters-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    PageHeaderComponent,
    PaginationComponent,
    TranslateModule,
    RouterModule,
    BreadcrumbComponent,
    SharedModule,
    DropDownComponent
  ],
  templateUrl: './incoming-letters-list.component.html',
  styleUrls: ['./incoming-letters-list.component.scss']
})
export class IncomingLettersListComponent implements OnInit {
  Math = Math;
  letters: IncomingDto[] = [];
  loading = false;
  totalCount = 0;
  currentPage = 1;
  pageSize = 20;

  // Breadcrumb items
  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'incomingOutgoing.incomingLetters' }
  ];

  // Filter form with shared components
  filterForm: FormGroup;

  // Status options for dropdown
  statusOptions = [
    { id: null, name: 'common.all' },
    { id: 'Received', name: 'incomingOutgoing.statusReceived' },
    { id: 'Processing', name: 'incomingOutgoing.statusProcessing' },
    { id: 'Completed', name: 'incomingOutgoing.statusCompleted' },
    { id: 'Closed', name: 'incomingOutgoing.statusClosed' },
    { id: 'Pending', name: 'incomingOutgoing.statusPending' }
  ];

  pageActions = [
    {
      label: 'incomingOutgoing.addIncomingLetter',
      type: 'primary',
      icon: 'fe-plus',
      click: () => this.createLetter()
    },
    {
      label: 'incomingOutgoing.importLetters',
      type: 'info',
      icon: 'fe-upload',
      click: () => this.navigateToImport()
    },
    {
      label: 'incomingOutgoing.exportLetters',
      type: 'success',
      icon: 'fe-file-plus',
      click: () => this.navigateToExport()
    }
  ];

  constructor(
    private fb: FormBuilder,
    private incomingService: IncomingService,
    private notification: NotificationService,
    private router: Router
  ) {
    // Initialize filter form
    this.filterForm = this.fb.group({
      searchValue: [''],
      selectedDepartment: [null],
      selectedStatus: [null],
      selectedYear: [null],
      startDate: [null],
      endDate: [null]
    });
  }

  ngOnInit(): void {
    this.loadLetters();
  }

  loadLetters(): void {
    this.loading = true;
    const formValues = this.filterForm.value;

    const searchRequest: IncomingSearchRequest = {
      pageNumber: this.currentPage,
      pageSize: this.pageSize
    };

    if (formValues.searchValue && formValues.searchValue.trim()) {
      searchRequest.searchTerm = formValues.searchValue.trim();
    }
    if (formValues.selectedDepartment) {
      searchRequest.departmentId = formValues.selectedDepartment;
    }
    if (formValues.selectedStatus) {
      searchRequest.status = formValues.selectedStatus;
    }
    if (formValues.selectedYear) {
      searchRequest.year = formValues.selectedYear;
    }
    if (formValues.startDate) {
      searchRequest.startDate = formValues.startDate;
    }
    if (formValues.endDate) {
      searchRequest.endDate = formValues.endDate;
    }

    this.incomingService.getIncomingLetters(searchRequest).subscribe({
      next: (response) => {
        this.letters = response.items || [];
        this.totalCount = response.totalCount || 0;
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error loading incoming letters:', error);
        this.notification.error(`Failed to load letters: ${error.message || 'Unknown error'}`);
        this.loading = false;
      }
    });
  }

  onSearch(): void {
    this.currentPage = 1;
    this.loadLetters();
  }

  clearFilters(): void {
    this.filterForm.reset({
      searchValue: '',
      selectedDepartment: null,
      selectedStatus: null,
      selectedYear: null,
      startDate: null,
      endDate: null
    });
    this.currentPage = 1;
    this.loadLetters();
  }

  // Check if any filters are active
  hasActiveFilters(): boolean {
    const formValues = this.filterForm.value;
    return !!(
      formValues.searchValue ||
      formValues.selectedDepartment ||
      formValues.selectedStatus ||
      formValues.selectedYear ||
      formValues.startDate ||
      formValues.endDate
    );
  }

  onFilterChange(): void {
    this.currentPage = 1;
    this.loadLetters();
  }

  createLetter(): void {
    this.router.navigate(['/incoming-outgoing/incoming/create']);
  }

  viewLetter(id: string): void {
    this.router.navigate(['/incoming-outgoing/incoming', id]);
  }

  editLetter(id: string): void {
    this.router.navigate(['/incoming-outgoing/incoming', id, 'edit']);
  }

  deleteLetter(letter: IncomingDto): void {
    const confirmed = confirm(`Are you sure you want to delete letter "${letter.subject}"?`);

    if (confirmed) {
      this.incomingService.deleteIncomingLetter(letter.id).subscribe({
        next: () => {
          this.notification.success('Letter deleted successfully');
          this.loadLetters();
        },
        error: (error: any) => {
          console.error('Error deleting letter:', error);
          this.notification.error('Failed to delete letter');
        }
      });
    }
  }

  navigateToImport(): void {
    this.router.navigate(['/incoming-outgoing/import/incoming']);
  }

  navigateToExport(): void {
    this.router.navigate(['/incoming-outgoing/export/incoming']);
  }

  changePage(page: number): void {
    if (page >= 1 && page <= this.getTotalPages()) {
      this.currentPage = page;
      this.loadLetters();
    }
  }

  getTotalPages(): number {
    return Math.ceil(this.totalCount / this.pageSize);
  }

  getPages(): number[] {
    const totalPages = this.getTotalPages();
    const pages: number[] = [];
    const maxPagesToShow = 5;

    if (totalPages <= maxPagesToShow) {
      for (let i = 1; i <= totalPages; i++) {
        pages.push(i);
      }
    } else {
      const startPage = Math.max(1, this.currentPage - 2);
      const endPage = Math.min(totalPages, this.currentPage + 2);

      if (startPage > 1) {
        pages.push(1);
        if (startPage > 2) {
          pages.push(-1);
        }
      }

      for (let i = startPage; i <= endPage; i++) {
        pages.push(i);
      }

      if (endPage < totalPages) {
        if (endPage < totalPages - 1) {
          pages.push(-1);
        }
        pages.push(totalPages);
      }
    }

    return pages;
  }

  getStatusBadgeClass(status: string): string {
    const statusMap: { [key: string]: string } = {
      'Received': 'badge-success',
      'Processing': 'badge-info',
      'Completed': 'badge-primary',
      'Closed': 'badge-secondary',
      'Pending': 'badge-warning'
    };
    return statusMap[status] || 'badge-secondary';
  }

  formatDate(date: Date | string | undefined): string {
    if (!date) return '-';
    const d = new Date(date);
    return d.toLocaleDateString('en-GB');
  }
}
