import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { OutgoingService } from '../services/outgoing.service';
import { CharityService } from '../../charities/services/charity.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../shared/components';
import { DropDownComponent } from '../../../shared/components/drop-down/drop-down.component';
import { SharedModule } from '../../../shared/shared.module';
import { OutgoingOrphanReportFilterDto, OutgoingOrphanReportRowDto } from '../models/outgoing.model';

/**
 * UC-COR-19 / §21.S.7 — تقرير الأيتام حسب خطاب الصادر.
 *
 * Read-only projection over Outgoing ⋈ OutgoingOrphanReport ⋈ Orphan. ChildCode
 * (كود اليتيم) is mandatory: an empty value refuses client-side and server-side
 * («Operation Faild») rather than running an unbounded report.
 */
@Component({
  selector: 'app-outgoing-orphans-report',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    PageHeaderComponent,
    PaginationComponent,
    BreadcrumbComponent,
    TranslateModule,
    SharedModule,
    DropDownComponent
  ],
  templateUrl: './outgoing-orphans-report.component.html',
  styleUrls: ['./outgoing-orphans-report.component.scss']
})
export class OutgoingOrphansReportComponent implements OnInit {
  rows: OutgoingOrphanReportRowDto[] = [];
  loading = false;
  totalCount = 0;
  currentPage = 1;
  pageSize = 20;

  // Whether بحث has run at least once (drives the empty state)
  searched = false;

  isSuperAdmin = false;

  filterForm: FormGroup;

  // HQ-only charity filter (§21.S.7 الجمعية + كافة الجهات)
  charityOptions: Array<{ id: string | null; name: string }> = [{ id: null, name: 'incomingOutgoing.allCharities' }];

  pageActions = [
    {
      label: 'common.print',
      type: 'secondary',
      icon: 'fe-printer',
      click: () => this.print()
    },
    {
      // §21.S.7 Extract — client-side ExcelJS over the whole result set
      label: 'incomingOutgoing.extractAll',
      type: 'secondary',
      icon: 'fe-download',
      click: () => this.extractAll()
    }
  ];

  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'incomingOutgoing.outgoingLetters', url: '/incoming-outgoing/outgoing' },
    { label: 'incomingOutgoing.orphansReportTitle' }
  ];

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private outgoingService: OutgoingService,
    private charityService: CharityService,
    private authService: AuthService,
    private notification: NotificationService,
    private translate: TranslateService
  ) {
    this.filterForm = this.fb.group({
      childCode: [''],
      selectedCharity: [null],
      serial: [null],
      year: [null],
      dateFrom: [null],
      dateTo: [null]
    });
  }

  ngOnInit(): void {
    this.isSuperAdmin = this.authService.hasRole('SuperAdmin');
    if (this.isSuperAdmin) {
      this.loadCharities();
    }
  }

  private loadCharities(): void {
    this.charityService.getCharities({ pageNumber: 1, pageSize: 1000, isActive: true }).subscribe({
      next: result => {
        this.charityOptions = [
          { id: null, name: 'incomingOutgoing.allCharities' },
          ...(result.items || []).map(c => ({ id: c.id, name: c.name }))
        ];
      },
      error: () => console.error('Error loading charities')
    });
  }

  search(): void {
    const formValues = this.filterForm.value;
    const childCode = (formValues.childCode || '').trim();

    // ChildCode is mandatory per §21.S.7 — refuse locally; the server refuses too
    if (!childCode) {
      this.notification.error(this.translate.instant('incomingOutgoing.childCodeRequired'));
      return;
    }

    this.loading = true;

    const filter: OutgoingOrphanReportFilterDto = {
      pageNumber: this.currentPage,
      pageSize: this.pageSize,
      childCode
    };

    if (formValues.selectedCharity) {
      filter.charityId = formValues.selectedCharity;
    }
    if (formValues.serial) {
      filter.serial = Number(formValues.serial);
    }
    if (formValues.year) {
      filter.year = Number(formValues.year);
    }
    if (formValues.dateFrom) {
      filter.dateFrom = formValues.dateFrom;
    }
    if (formValues.dateTo) {
      filter.dateTo = formValues.dateTo;
    }

    this.outgoingService.getOrphanReport(filter).subscribe({
      next: result => {
        this.rows = result.items || [];
        this.totalCount = result.totalCount || 0;
        this.searched = true;
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error running orphans report:', error);
        this.notification.error(error.message || this.translate.instant('incomingOutgoing.reportFailed'));
        this.loading = false;
      }
    });
  }

  resetFilters(): void {
    this.filterForm.reset({
      childCode: '',
      selectedCharity: null,
      serial: null,
      year: null,
      dateFrom: null,
      dateTo: null
    });
    this.currentPage = 1;
    this.rows = [];
    this.totalCount = 0;
    this.searched = false;
  }

  changePage(page: number): void {
    if (page >= 1 && page <= this.getTotalPages()) {
      this.currentPage = page;
      this.search();
    }
  }

  getTotalPages(): number {
    return Math.ceil(this.totalCount / this.pageSize);
  }

  print(): void {
    window.print();
  }

  /** §21.S.7 Extract — re-run the report with one page holding every row, then ExcelJS. */
  extractAll(): void {
    const formValues = this.filterForm.value;
    const childCode = (formValues.childCode || '').trim();

    if (!childCode) {
      this.notification.error(this.translate.instant('incomingOutgoing.childCodeRequired'));
      return;
    }
    if (this.rows.length === 0) {
      this.notification.info(this.translate.instant('incomingOutgoing.nothingToExport'));
      return;
    }

    const filter: OutgoingOrphanReportFilterDto = {
      pageNumber: 1,
      pageSize: Math.max(this.totalCount, 1),
      childCode
    };
    if (formValues.selectedCharity) {
      filter.charityId = formValues.selectedCharity;
    }
    if (formValues.serial) {
      filter.serial = Number(formValues.serial);
    }
    if (formValues.year) {
      filter.year = Number(formValues.year);
    }
    if (formValues.dateFrom) {
      filter.dateFrom = formValues.dateFrom;
    }
    if (formValues.dateTo) {
      filter.dateTo = formValues.dateTo;
    }

    this.outgoingService.getOrphanReport(filter).subscribe({
      next: result => {
        const rows = result.items || [];
        if (rows.length === 0) {
          this.notification.info(this.translate.instant('incomingOutgoing.nothingToExport'));
          return;
        }

        import('exceljs').then(({ default: ExcelJS }) => {
          const workbook = new ExcelJS.Workbook();
          const sheet = workbook.addWorksheet(this.translate.instant('incomingOutgoing.orphansReportTitle'));
          sheet.addRow([
            this.translate.instant('incomingOutgoing.serial'),
            this.translate.instant('incomingOutgoing.year'),
            this.translate.instant('incomingOutgoing.date'),
            this.translate.instant('incomingOutgoing.charity'),
            this.translate.instant('incomingOutgoing.orphansCount'),
            this.translate.instant('incomingOutgoing.orphanAttached')
          ]);
          rows.forEach(r => sheet.addRow([
            r.serial != null ? String(r.serial).padStart(4, '0') : '',
            r.year || '',
            this.formatDate(r.letterDate),
            r.charityName || '',
            r.orphanCount || 0,
            r.orphanAttached ? '✓' : ''
          ]));

          workbook.xlsx.writeBuffer().then(buffer => {
            const blob = new Blob([buffer], {
              type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
            });
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = `orphans_by_outgoing_letter_${new Date().toISOString().slice(0, 10)}.xlsx`;
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
      },
      error: (error: any) => {
        this.notification.error(error.message || this.translate.instant('incomingOutgoing.reportFailed'));
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/incoming-outgoing/outgoing']);
  }

  trackById(_index: number, row: OutgoingOrphanReportRowDto): string {
    return row.outgoingId;
  }

  formatDate(date: string | undefined): string {
    if (!date) return '-';
    const d = new Date(date);
    return d.toLocaleDateString('en-GB');
  }
}
