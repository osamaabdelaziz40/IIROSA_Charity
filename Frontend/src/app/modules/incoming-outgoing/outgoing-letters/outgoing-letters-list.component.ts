import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { Router } from '@angular/router';
import { OutgoingDto, OutgoingSearchRequest } from '../models/outgoing.model';
import { OutgoingService } from '../services/outgoing.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { TranslateModule } from '@ngx-translate/core';
import { RouterModule } from '@angular/router';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../shared/components';
import { SharedModule } from '../../../shared/shared.module';
import { DropDownComponent } from '../../../shared/components/drop-down/drop-down.component';

@Component({
  selector: 'app-outgoing-letters-list',
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
  templateUrl: './outgoing-letters-list.component.html',
  styleUrls: ['./outgoing-letters-list.component.scss']
})
export class OutgoingLettersListComponent implements OnInit {
  Math = Math;
  letters: OutgoingDto[] = [];
  loading = false;
  totalCount = 0;
  currentPage = 1;
  pageSize = 20;

  // Breadcrumb items
  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'incomingOutgoing.outgoingLetters' }
  ];

  // Filter form with shared components
  filterForm: FormGroup;

  // Has reply options for dropdown
  hasReplyOptions = [
    { id: null, name: 'common.all' },
    { id: true, name: 'common.yes' },
    { id: false, name: 'common.no' }
  ];

  // Category options for dropdown
  categoryOptions = [
    { id: null, name: 'common.all' },
    { id: 'official', name: 'incomingOutgoing.official' },
    { id: 'internal', name: 'incomingOutgoing.internal' },
    { id: 'external', name: 'incomingOutgoing.external' }
  ];

  pageActions = [
    {
      label: 'incomingOutgoing.addOutgoingLetter',
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
    private outgoingService: OutgoingService,
    private notification: NotificationService,
    private router: Router
  ) {
    // Initialize filter form
    this.filterForm = this.fb.group({
      searchValue: [''],
      selectedDepartment: [null],
      selectedCategory: [null],
      selectedYear: [null],
      startDate: [null],
      endDate: [null],
      hasReply: [null]
    });
  }

  ngOnInit(): void {
    this.loadLetters();
  }

  loadLetters(): void {
    this.loading = true;
    const formValues = this.filterForm.value;

    const searchRequest: OutgoingSearchRequest = {
      pageNumber: this.currentPage,
      pageSize: this.pageSize
    };

    if (formValues.searchValue && formValues.searchValue.trim()) {
      searchRequest.searchTerm = formValues.searchValue.trim();
    }
    if (formValues.selectedDepartment) {
      searchRequest.departmentId = formValues.selectedDepartment;
    }
    if (formValues.selectedCategory) {
      searchRequest.categoryId = formValues.selectedCategory;
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
    if (formValues.hasReply !== null) {
      searchRequest.hasReply = formValues.hasReply;
    }

    this.outgoingService.getOutgoingLetters(searchRequest).subscribe({
      next: (response) => {
        this.letters = response.items || [];
        this.totalCount = response.totalCount || 0;
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error loading outgoing letters:', error);
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
      selectedCategory: null,
      selectedYear: null,
      startDate: null,
      endDate: null,
      hasReply: null
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
      formValues.selectedCategory ||
      formValues.selectedYear ||
      formValues.startDate ||
      formValues.endDate ||
      formValues.hasReply !== null
    );
  }

  onFilterChange(): void {
    this.currentPage = 1;
    this.loadLetters();
  }

  createLetter(): void {
    this.router.navigate(['/incoming-outgoing/outgoing/create']);
  }

  viewLetter(id: string): void {
    this.router.navigate(['/incoming-outgoing/outgoing', id]);
  }

  editLetter(id: string): void {
    this.router.navigate(['/incoming-outgoing/outgoing', id, 'edit']);
  }

  deleteLetter(letter: OutgoingDto): void {
    const confirmed = confirm(`Are you sure you want to delete letter "${letter.subject}"?`);

    if (confirmed) {
      this.outgoingService.deleteOutgoingLetter(letter.id).subscribe({
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
    this.router.navigate(['/incoming-outgoing/import/outgoing']);
  }

  navigateToExport(): void {
    this.router.navigate(['/incoming-outgoing/export/outgoing']);
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

  formatDate(date: Date | string | undefined): string {
    if (!date) return '-';
    const d = new Date(date);
    return d.toLocaleDateString('en-GB');
  }
}
