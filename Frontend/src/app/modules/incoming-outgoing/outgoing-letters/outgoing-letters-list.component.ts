import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { TranslateService, TranslateModule } from '@ngx-translate/core';
import { OutgoingListDto, OutgoingFilterDto, OutgoingStatistics, OutgoingCategoryOptionDto } from '../models/outgoing.model';
import { OutgoingService } from '../services/outgoing.service';
import { CharityService } from '../../charities/services/charity.service';
import { LookupManagementService } from '../../lookup-management/services/lookup-management.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { RouterModule } from '@angular/router';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../shared/components';
import { DropDownComponent } from '../../../shared/components/drop-down/drop-down.component';
import { SharedModule } from '../../../shared/shared.module';

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
    DropDownComponent,
    SharedModule
  ],
  templateUrl: './outgoing-letters-list.component.html',
  styleUrls: ['./outgoing-letters-list.component.scss']
})
export class OutgoingLettersListComponent implements OnInit, OnDestroy {
  Math = Math;
  letters: OutgoingListDto[] = [];
  loading = false;
  exporting = false;
  totalCount = 0;
  currentPage = 1;
  pageSize = 20;

  // Register statistics band — caller-scoped server-side, not tied to the filters below
  statistics: OutgoingStatistics | null = null;

  // HQ callers can narrow the register to one charity (§21.S.4 الجمعية); deletes are
  // the General Director's alone — SuperAdmin only (UC-COR-16).
  isSuperAdmin = false;
  canDelete = false;

  // Breadcrumb items
  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'incomingOutgoing.outgoingLetters' }
  ];

  // Filter form with shared components
  filterForm: FormGroup;

  // Categories come from the live catalogue (UC-COR-17)
  categoryOptions: Array<{ id: number | null; name: string }> = [];

  // Departments come from the live catalogue (UC-COR-08)
  departmentOptions: Array<{ id: number | null; name: string }> = [];

  // الحاله (§21.S.4): an outgoing letter's state is its reply state — the entity has
  // no status column (§21.S.5 defines none), so the spec's tri-state resolves to
  // تم الرد / لم يتم الرد over the hasReply criterion.
  replyStateOptions: Array<{ id: string | null; name: string }> = [];

  // HQ-only charity filter (§21.S.4 الجمعية + كافة الجهات)
  charityOptions: Array<{ id: string | null; name: string }> = [];

  // Server rows behind the option arrays — names are data (nameAr), so only the
  // "All" head option needs re-translation on language switch.
  private categoryRows: Array<{ id: number; name: string }> = [];
  private departmentRows: Array<{ id: number; name: string }> = [];
  private charityRows: Array<{ id: string; name: string }> = [];

  /** Rebuilds translated option labels on language switch; torn down in ngOnDestroy. */
  private langChangeSubscription?: Subscription;

  pageActions = [
    {
      label: 'incomingOutgoing.addOutgoingLetter',
      type: 'primary',
      icon: 'fe-plus',
      click: () => this.createLetter()
    },
    {
      // UC-COR-18 entry point — the §21.S.6 orphan-report attachment screen
      label: 'incomingOutgoing.attachOrphansTitle',
      type: 'secondary',
      icon: 'fe-users',
      click: () => this.router.navigate(['/incoming-outgoing/export/outgoing'])
    },
    {
      // UC-COR-19 entry point — the §21.S.7 orphans-by-letter report
      label: 'incomingOutgoing.orphansReportTitle',
      type: 'secondary',
      icon: 'fe-bar-chart',
      click: () => this.router.navigate(['/incoming-outgoing/export/outgoing-orphans'])
    },
    {
      label: 'incomingOutgoing.extractPage',
      type: 'secondary',
      icon: 'fe-download',
      click: () => this.extractCurrentPage()
    },
    {
      label: 'incomingOutgoing.extractAll',
      type: 'secondary',
      icon: 'fe-file-text',
      click: () => this.extractAll()
    }
  ];

  constructor(
    private fb: FormBuilder,
    private outgoingService: OutgoingService,
    private charityService: CharityService,
    private lookupService: LookupManagementService,
    private authService: AuthService,
    private notification: NotificationService,
    private translate: TranslateService,
    private router: Router
  ) {
    // Initialize filter form
    this.filterForm = this.fb.group({
      searchValue: [''],
      serial: [null],
      selectedCharity: [null],
      selectedReplyState: [null],
      selectedDepartment: [null],
      selectedCategory: [null],
      selectedYear: [null],
      startDate: [null],
      endDate: [null]
    });
  }

  ngOnInit(): void {
    this.isSuperAdmin = this.authService.hasRole('SuperAdmin');
    this.canDelete = this.authService.hasRole('SuperAdmin');
    this.rebuildFilterOptions();
    this.loadCategories();
    this.loadDepartments();
    if (this.isSuperAdmin) {
      this.loadCharities();
    }
    this.loadLetters();
    this.loadStatistics();

    // The option labels are pre-translated (app-drop-down renders raw text), so a
    // language switch needs a rebuild — the translate pipe can't refresh them.
    this.langChangeSubscription = this.translate.onLangChange.subscribe(() => {
      this.rebuildFilterOptions();
    });
  }

  ngOnDestroy(): void {
    this.langChangeSubscription?.unsubscribe();
  }

  /**
   * Rebuilds the select2 option arrays — the reply-state labels are pure i18n
   * keys, the rest are raw rows plus the translated "All" head option. Called on
   * init, after each lookup load, and on every language switch.
   */
  private rebuildFilterOptions(): void {
    const allLabel = this.translate.instant('common.all');
    this.replyStateOptions = [
      { id: null, name: allLabel },
      { id: 'replied', name: this.translate.instant('incomingOutgoing.replied') },
      { id: 'notReplied', name: this.translate.instant('incomingOutgoing.notReplied') }
    ];
    this.categoryOptions = [{ id: null, name: allLabel }, ...this.categoryRows];
    this.departmentOptions = [{ id: null, name: allLabel }, ...this.departmentRows];
    this.charityOptions = [
      { id: null, name: this.translate.instant('incomingOutgoing.allCharities') },
      ...this.charityRows
    ];
  }

  loadCategories(): void {
    this.outgoingService.getAvailableCategories().subscribe({
      next: (categories: OutgoingCategoryOptionDto[]) => {
        this.categoryRows = categories.map(c => ({ id: c.id, name: c.nameAr || c.nameEn }));
        this.rebuildFilterOptions();
      },
      error: (error: any) => console.error('Error loading categories:', error)
    });
  }

  loadDepartments(): void {
    this.lookupService.getDepartments({ page: 1, pageSize: 1000, isActive: true }).subscribe({
      next: result => {
        this.departmentRows = (result.items || []).map(d => ({ id: d.id, name: d.nameAr || d.name }));
        this.rebuildFilterOptions();
      },
      error: (error: any) => console.error('Error loading departments:', error)
    });
  }

  loadCharities(): void {
    this.charityService.getCharities({ pageNumber: 1, pageSize: 1000, isActive: true }).subscribe({
      next: result => {
        this.charityRows = (result.items || []).map(c => ({ id: c.id, name: c.name }));
        this.rebuildFilterOptions();
      },
      error: (error: any) => console.error('Error loading charities:', error)
    });
  }

  loadLetters(): void {
    this.loading = true;

    this.outgoingService.getOutgoingLetters(this.buildFilter()).subscribe({
      next: (response) => {
        this.letters = response.items || [];
        this.totalCount = response.totalCount || 0;
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error loading outgoing letters:', error);
        this.notification.error(error.message || 'Failed to load letters');
        this.loading = false;
      }
    });
  }

  /**
   * Register statistics band — describes the caller's whole scope, not the active
   * filters. Silent-fail so the register still renders without it.
   */
  loadStatistics(): void {
    this.outgoingService.getStatistics().subscribe({
      next: statistics => this.statistics = statistics,
      error: (error: any) => console.error('Error loading outgoing statistics:', error)
    });
  }

  /** The §21.S.4 criteria as the API filter — only non-empty keys ride the query. */
  private buildFilter(pageNumber = this.currentPage, pageSize = this.pageSize): OutgoingFilterDto {
    const formValues = this.filterForm.value;

    const filter: OutgoingFilterDto = {
      pageNumber,
      pageSize
    };

    if (formValues.searchValue && formValues.searchValue.trim()) {
      filter.searchTerm = formValues.searchValue.trim();
    }
    if (formValues.serial) {
      filter.serial = Number(formValues.serial);
    }
    if (formValues.selectedCharity) {
      filter.charityId = formValues.selectedCharity;
    }
    if (formValues.selectedReplyState === 'replied') {
      filter.hasReply = true;
    } else if (formValues.selectedReplyState === 'notReplied') {
      filter.hasReply = false;
    }
    if (formValues.selectedDepartment) {
      filter.departmentId = Number(formValues.selectedDepartment);
    }
    if (formValues.selectedCategory) {
      filter.categoryId = Number(formValues.selectedCategory);
    }
    if (formValues.selectedYear) {
      filter.year = Number(formValues.selectedYear);
    }
    if (formValues.startDate) {
      filter.startDate = formValues.startDate;
    }
    if (formValues.endDate) {
      filter.endDate = formValues.endDate;
    }

    return filter;
  }

  onSearch(): void {
    this.currentPage = 1;
    this.loadLetters();
  }

  clearFilters(): void {
    this.filterForm.reset({
      searchValue: '',
      serial: null,
      selectedCharity: null,
      selectedReplyState: null,
      selectedDepartment: null,
      selectedCategory: null,
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
      formValues.serial ||
      formValues.selectedCharity ||
      formValues.selectedReplyState ||
      formValues.selectedDepartment ||
      formValues.selectedCategory ||
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
    this.router.navigate(['/incoming-outgoing/outgoing/create']);
  }

  viewLetter(id: string): void {
    this.router.navigate(['/incoming-outgoing/outgoing', id]);
  }

  editLetter(id: string): void {
    this.router.navigate(['/incoming-outgoing/outgoing', id, 'edit']);
  }

  async deleteLetter(letter: OutgoingListDto): Promise<void> {
    const message = this.translate.instant('incomingOutgoing.deleteConfirm', { subject: letter.subject });
    const confirmed = await this.notification.confirm(message, this.translate.instant('common.delete'));

    if (confirmed) {
      this.outgoingService.deleteOutgoingLetter(letter.id).subscribe({
        next: () => {
          this.notification.success(this.translate.instant('incomingOutgoing.letterDeleted'));
          this.loadLetters();
        },
        error: (error: any) => {
          console.error('Error deleting letter:', error);
          this.notification.error(error.message || 'Failed to delete letter');
        }
      });
    }
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

  trackById(_index: number, letter: OutgoingListDto): string {
    return letter.id;
  }

  /** Zero-padded display serial, matching the register's رقم الصادر column */
  padSerial(serial?: number): string {
    return serial != null ? String(serial).padStart(4, '0') : '-';
  }

  formatDate(date: string | undefined): string {
    if (!date) return '-';
    const d = new Date(date);
    return d.toLocaleDateString('en-GB');
  }

  // ========== §21.S.4 Extract commands — client-side ExcelJS ==========

  /** Extract the current page (ExtractOutgoingData). */
  extractCurrentPage(): void {
    if (this.letters.length === 0) {
      this.notification.info(this.translate.instant('incomingOutgoing.nothingToExport'));
      return;
    }
    this.exportRows(this.letters);
  }

  /** Extract the whole filtered register (ExtractAllOutgoingData) — one paged read. */
  extractAll(): void {
    if (this.exporting) return;
    this.exporting = true;

    this.outgoingService.getOutgoingLetters({
      ...this.buildFilter(),
      pageNumber: 1,
      pageSize: Math.max(this.totalCount, 1)
    }).subscribe({
      next: response => {
        this.exporting = false;
        const rows = response.items || [];
        if (rows.length === 0) {
          this.notification.info(this.translate.instant('incomingOutgoing.nothingToExport'));
          return;
        }
        this.exportRows(rows);
      },
      error: (error: any) => {
        this.exporting = false;
        this.notification.error(error.message || 'Failed to export letters');
      }
    });
  }

  private exportRows(rows: OutgoingListDto[]): void {
    import('exceljs').then(({ default: ExcelJS }) => {
      const workbook = new ExcelJS.Workbook();
      const sheet = workbook.addWorksheet(this.translate.instant('incomingOutgoing.outgoingLetters'));
      // §21.S.4 grid columns, spec order
      sheet.addRow([
        this.translate.instant('incomingOutgoing.serial'),
        this.translate.instant('incomingOutgoing.year'),
        this.translate.instant('incomingOutgoing.department'),
        this.translate.instant('incomingOutgoing.date'),
        this.translate.instant('incomingOutgoing.subject'),
        this.translate.instant('incomingOutgoing.attachedFile')
      ]);
      rows.forEach(l => sheet.addRow([
        this.padSerial(l.serial),
        l.year || '',
        l.departmentName || '',
        this.formatDate(l.date),
        l.subject,
        l.uploadedFileName || ''
      ]));

      workbook.xlsx.writeBuffer().then(buffer => {
        const blob = new Blob([buffer], {
          type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
        });
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `outgoing_letters_${new Date().toISOString().slice(0, 10)}.xlsx`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
      }).catch(() => {
        // Review P11: a failed write is a user-facing failure, not a silent non-event
        this.notification.error(this.translate.instant('incomingOutgoing.exportFailed'));
      });
    }).catch(() => {
      // Review P11: the ExcelJS chunk itself can fail to load (offline first hit)
      this.notification.error(this.translate.instant('incomingOutgoing.exportFailed'));
    });
  }
}
