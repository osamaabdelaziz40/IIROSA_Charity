import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { TranslateService, TranslateModule } from '@ngx-translate/core';
import { IncomingListDto, IncomingFilterDto, CorrespondenceStatusOption } from '../models/incoming.model';
import { IncomingService } from '../services/incoming.service';
import { CharityService } from '../../charities/services/charity.service';
import { EmployeeService } from '../../employees/services/employee.service';
import { LookupManagementService } from '../../lookup-management/services/lookup-management.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { AttachmentService } from '../../../core/services/attachment.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { RouterModule } from '@angular/router';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../shared/components';
import { DropDownComponent } from '../../../shared/components/drop-down/drop-down.component';
import { Employee } from '../../../core/models/employee.model';

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
    DropDownComponent
  ],
  templateUrl: './incoming-letters-list.component.html',
  styleUrls: ['./incoming-letters-list.component.scss']
})
export class IncomingLettersListComponent implements OnInit, OnDestroy {
  Math = Math;
  letters: IncomingListDto[] = [];  loading = false;
  exporting = false;
  totalCount = 0;
  currentPage = 1;
  pageSize = 20;

  // HQ callers can narrow the register to one charity (§21.S.1 الجمعية); deletes are
  // the General Director's alone — SuperAdmin only (UC-COR-07).
  isSuperAdmin = false;
  canDelete = false;

  // Breadcrumb items
  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'incomingOutgoing.incomingLetters' }
  ];

  // Filter form with shared components (§21.S.1 criteria)
  filterForm: FormGroup;

  // The spec's tri-state (معلق / تم الرد / تم عمل اللازم) — served by the endpoint,
  // never hard-coded here. The stored value is the Arabic term.
  statusOptions: Array<{ id: string | null; name: string }> = [];
  private statusColors: { [key: string]: string } = {};

  // Departments come from the live catalogue (UC-COR-08)
  departmentOptions: Array<{ id: number | null; name: string }> = [];

  // الموظف — the responsible-employee lookup (§21.S.1)
  employeeOptions: Array<{ id: string | null; name: string }> = [];

  // HQ-only charity filter (§21.S.1 الجمعية + كافة الجهات)
  charityOptions: Array<{ id: string | null; name: string }> = [];

  // Server rows behind the option arrays — names are data (nameAr/fullName), so
  // only the "All" head option needs re-translation on language switch.
  private statusRows: Array<{ id: string; name: string }> = [];
  private departmentRows: Array<{ id: number; name: string }> = [];
  private employeeRows: Array<{ id: string; name: string }> = [];
  private charityRows: Array<{ id: string; name: string }> = [];

  /** Rebuilds translated option labels on language switch; torn down in ngOnDestroy. */
  private langChangeSubscription?: Subscription;

  pageActions = [
    {
      label: 'incomingOutgoing.addIncomingLetter',
      type: 'primary',
      icon: 'fe-plus',
      click: () => this.createLetter()
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
    private incomingService: IncomingService,
    private charityService: CharityService,
    private employeeService: EmployeeService,
    private lookupService: LookupManagementService,
    private authService: AuthService,
    private notification: NotificationService,
    private translate: TranslateService,
    private router: Router,
    private attachmentService: AttachmentService
  ) {
    // Initialize filter form — every §21.S.1 criterion has its own key; the server
    // never folds them into one search.
    this.filterForm = this.fb.group({
      searchValue: [''],        // الموضوع
      serial: [null],           // رقم الوارد
      letterNumber: [''],       // رقم الخطاب
      selectedEmployee: [null], // الموظف
      selectedCharity: [null],  // الجمعية (HQ only)
      selectedDepartment: [null],
      selectedStatus: [null],   // الحاله
      selectedYear: [null],
      startDate: [null],        // من تاريخ
      endDate: [null]           // الي تاريخ
    });
  }

  ngOnInit(): void {
    this.isSuperAdmin = this.authService.hasRole('SuperAdmin');
    this.canDelete = this.authService.hasRole('SuperAdmin');
    this.rebuildFilterOptions();
    this.loadStatuses();
    this.loadDepartments();
    this.loadEmployees();
    if (this.isSuperAdmin) {
      this.loadCharities();
    }
    this.loadLetters();

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
   * Rebuilds the select2 option arrays from the raw rows plus the translated
   * "All" head option — called on init, after each lookup load, and on every
   * language switch.
   */
  private rebuildFilterOptions(): void {
    const allLabel = this.translate.instant('common.all');
    this.statusOptions = [{ id: null, name: allLabel }, ...this.statusRows];
    this.departmentOptions = [{ id: null, name: allLabel }, ...this.departmentRows];
    this.employeeOptions = [{ id: null, name: allLabel }, ...this.employeeRows];
    this.charityOptions = [
      { id: null, name: this.translate.instant('incomingOutgoing.allCharities') },
      ...this.charityRows
    ];
  }

  loadStatuses(): void {
    this.incomingService.getAvailableStatuses().subscribe({
      next: (statuses: CorrespondenceStatusOption[]) => {
        this.statusRows = statuses.map(s => ({ id: s.id, name: s.nameAr }));
        this.statusColors = statuses.reduce((map, s) => ({ ...map, [s.id]: `badge-${s.color}` }), {});
        this.rebuildFilterOptions();
      },
      error: (error: any) => console.error('Error loading statuses:', error)
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

  loadEmployees(): void {
    this.employeeService.getEmployees({ page: 1, pageSize: 1000, isActive: true }).subscribe({
      next: result => {
        this.employeeRows = (result.items || []).map((e: Employee) => ({ id: e.id, name: e.fullName }));
        this.rebuildFilterOptions();
      },
      error: (error: any) => console.error('Error loading employees:', error)
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
    const filter = this.buildFilter();

    this.incomingService.getIncomingLetters(filter).subscribe({
      next: (response) => {
        this.letters = response.items || [];
        this.totalCount = response.totalCount || 0;
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error loading incoming letters:', error);
        this.notification.error(`${error.message || 'Failed to load letters'}`);
        this.loading = false;
      }
    });
  }

  /** The §21.S.1 criteria as the API filter — only non-empty keys ride the query. */
  private buildFilter(pageNumber = this.currentPage, pageSize = this.pageSize): IncomingFilterDto {
    const formValues = this.filterForm.value;

    const filter: IncomingFilterDto = {
      pageNumber,
      pageSize
    };

    if (formValues.searchValue && formValues.searchValue.trim()) {
      filter.searchTerm = formValues.searchValue.trim();
    }
    if (formValues.serial) {
      filter.serial = Number(formValues.serial);
    }
    if (formValues.letterNumber && formValues.letterNumber.trim()) {
      filter.letterNumber = formValues.letterNumber.trim();
    }
    if (formValues.selectedEmployee) {
      filter.assignedUserId = formValues.selectedEmployee;
    }
    if (formValues.selectedCharity) {
      filter.charityId = formValues.selectedCharity;
    }
    if (formValues.selectedDepartment) {
      filter.departmentId = Number(formValues.selectedDepartment);
    }
    if (formValues.selectedStatus) {
      filter.status = formValues.selectedStatus;
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
      letterNumber: '',
      selectedEmployee: null,
      selectedCharity: null,
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
      formValues.serial ||
      formValues.letterNumber ||
      formValues.selectedEmployee ||
      formValues.selectedCharity ||
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

  /**
   * UC-SYS-02: الملف column — the file link downloads through the authenticated
   * blob path (a plain href cannot carry the Bearer header).
   */
  downloadLetterFile(letter: IncomingListDto): void {
    if (!letter.uploadedFileId) {
      return;
    }
    this.attachmentService.downloadAndSave(
      letter.uploadedFileId,
      letter.uploadedFileName || 'attachment',
      () => this.notification.error(this.translate.instant('common.downloadFailed'))
    );
  }

  editLetter(id: string): void {
    this.router.navigate(['/incoming-outgoing/incoming', id, 'edit']);
  }

  async deleteLetter(letter: IncomingListDto): Promise<void> {
    const message = this.translate.instant('incomingOutgoing.deleteConfirm', { subject: letter.subject });
    const confirmed = await this.notification.confirm(message, this.translate.instant('common.delete'));

    if (confirmed) {
      this.incomingService.deleteIncomingLetter(letter.id).subscribe({
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

  trackById(_index: number, letter: IncomingListDto): string {
    return letter.id;
  }

  getStatusBadgeClass(status?: string): string {
    return this.statusColors[status || ''] || 'badge-secondary';
  }

  formatDate(date: string | undefined): string {
    if (!date) return '-';
    const d = new Date(date);
    return d.toLocaleDateString('en-GB');
  }

  // ========== §21.S.1 Extract commands — client-side ExcelJS ==========

  /** Extract the current page (ExtractIncomingData). */
  extractCurrentPage(): void {
    if (this.letters.length === 0) {
      this.notification.info(this.translate.instant('incomingOutgoing.nothingToExport'));
      return;
    }
    this.exportRows(this.letters);
  }

  /** Extract the whole filtered register (ExtractAllIncomingData) — one paged read. */
  extractAll(): void {
    if (this.exporting) return;
    this.exporting = true;

    this.incomingService.getIncomingLetters(this.buildFilter(1, Math.max(this.totalCount, 1))).subscribe({
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

  private exportRows(rows: IncomingListDto[]): void {
    import('exceljs').then(({ default: ExcelJS }) => {
      const workbook = new ExcelJS.Workbook();
      const sheet = workbook.addWorksheet(this.translate.instant('incomingOutgoing.incomingLetters'));
      // §21.S.1 grid columns, spec order
      sheet.addRow([
        this.translate.instant('incomingOutgoing.serial'),
        this.translate.instant('incomingOutgoing.year'),
        this.translate.instant('incomingOutgoing.department'),
        this.translate.instant('incomingOutgoing.date'),
        this.translate.instant('incomingOutgoing.letterNumber'),
        this.translate.instant('incomingOutgoing.subject'),
        this.translate.instant('incomingOutgoing.attachedFile'),
        this.translate.instant('common.status')
      ]);
      rows.forEach(l => sheet.addRow([
        l.serialTxt || l.serial || '',
        l.year || '',
        l.departmentName || '',
        this.formatDate(l.date),
        l.letterNumber || '',
        l.subject,
        l.uploadedFileName || '',
        l.status || ''
      ]));

      workbook.xlsx.writeBuffer().then(buffer => {
        const blob = new Blob([buffer], {
          type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
        });
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `incoming_letters_${new Date().toISOString().slice(0, 10)}.xlsx`;
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
