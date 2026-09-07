import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { firstValueFrom } from 'rxjs';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { ReportViewerComponent } from '../report-viewer/report-viewer.component';
import { DropDownComponent } from '../../../shared/components/drop-down/drop-down.component';
import { ReportService } from '../services/report.service';
import { CharityService } from '../../charities/services/charity.service';
import { OutgoingService } from '../../incoming-outgoing/services/outgoing.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import {
  MissingOutgoingAttachmentsFilter,
  MissingOutgoingAttachmentsRow,
  ReportPagedResult
} from '../models/report.model';
import { ReportExportRequest } from '../models/report-columns';

/**
 * UC-RPT-38 (§23.U.38 مرفقات الصادر الناقصة) — the back-office audit grid: outgoing letters
 * inside the window that carry ZERO orphan-report attachment rows (the v1 rule; the legacy
 * expected-list table is absent — the constant-0 count column states the finding).
 *
 * Query/grid by story design — no print payload (18-40 keeps عرض disabled); the Excel export
 * rides 18-41's generic engine (the story's own deferral). Thin: the shared report-viewer
 * shell owns the chrome (بحث / استخراج / Pagination / empty states); this component owns
 * the filter bar (charity for HQ, letter-date window, category) and the bespoke grid.
 *
 * The charity scope is enforced SERVER-SIDE (pin-never-widen) — the charity filter narrows HQ
 * queries only. The endpoint is HQ-only regardless (the route's role set), so the grid is a
 * back-office concern end to end.
 */
@Component({
  selector: 'app-missing-outgoing-attachments',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    ReportViewerComponent,
    DropDownComponent
  ],
  templateUrl: './missing-outgoing-attachments.component.html',
  styleUrls: ['./missing-outgoing-attachments.component.scss']
})
export class MissingOutgoingAttachmentsComponent implements OnInit, OnDestroy {
  rows: MissingOutgoingAttachmentsRow[] = [];
  loading = false;
  hasRun = false;

  currentPage = 1;
  pageSize = 20;
  totalCount = 0;

  isHQ = false;
  charityOptions: Array<{ id: string; name: string }> = [];
  categoryOptions: Array<{ id: string; name: string }> = [];

  filterForm: FormGroup;

  private readonly destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private reportService: ReportService,
    private charityService: CharityService,
    private outgoingService: OutgoingService,
    private authService: AuthService,
    private notification: NotificationService,
    private translate: TranslateService
  ) {
    this.filterForm = this.fb.group({
      charityId: [''],
      dateFrom: [null],
      dateTo: [null],
      outgoingCategoryId: ['']
    });
  }

  ngOnInit(): void {
    this.isHQ = this.authService.hasAnyRole(['SuperAdmin', 'Admin']);
    if (this.isHQ) {
      this.loadCharities();
    }
    this.loadCategories();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // ==================== lookups ====================

  /** الجمعية — HQ only; a charity caller is scoped server-side regardless. */
  private loadCharities(): void {
    this.charityService.getCharities({ pageNumber: 1, pageSize: 500 })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: response => {
          this.charityOptions = [
            { id: '', name: this.translate.instant('reports.missingOutgoingAttachments.allCharities') },
            ...(response.items || []).map(c => ({ id: c.id, name: c.name }))
          ];
          // Review P26 2026-08-26: a capped lookup says so — a silently truncated dropdown hides choices.
          if ((response.totalCount || 0) > (response.items || []).length) {
            this.notification.warning(this.translate.instant('reports.lookup.truncated'));
          }
        },
        error: () => console.error('Error loading charities')
      });
  }

  /** التصنيف — the §21.S.5 category options (the outgoing register's own list). */
  private loadCategories(): void {
    this.outgoingService.getAvailableCategories()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: categories => {
          this.categoryOptions = [
            { id: '', name: this.translate.instant('reports.missingOutgoingAttachments.allCategories') },
            ...(categories || []).map(c => ({ id: String(c.id), name: c.nameAr || c.nameEn || String(c.id) }))
          ];
        },
        error: () => console.error('Error loading outgoing categories')
      });
  }

  // ==================== search ====================

  /** بحث — one paged read (page 1). Nothing stored changes (AC 1). */
  search(): void {
    if (this.loading) {
      return;
    }
    if (!this.validateFilter()) {
      return;
    }

    this.currentPage = 1;
    this.runSearch();
  }

  onPageChange(page: number): void {
    if (page < 1 || this.loading) {
      return;
    }
    this.currentPage = page;
    this.runSearch();
  }

  private runSearch(): void {
    this.loading = true;
    this.reportService.getMissingOutgoingAttachments(this.buildFilter())
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (report: ReportPagedResult<MissingOutgoingAttachmentsRow>) => {
          this.rows = report.items || [];
          this.totalCount = report.totalCount || 0;
          this.loading = false;
          this.hasRun = true;
        },
        error: (httpError: any) => this.handleFilterError(httpError)
      });
  }

  // ==================== builders ====================

  private buildFilter(): MissingOutgoingAttachmentsFilter {
    const v = this.filterForm.getRawValue();
    const filter: MissingOutgoingAttachmentsFilter = {
      page: this.currentPage,
      pageSize: this.pageSize
    };
    if (this.isHQ && v.charityId) {
      filter.charityId = String(v.charityId);
    }
    if (v.dateFrom) {
      filter.dateFrom = v.dateFrom;
    }
    if (v.dateTo) {
      filter.dateTo = v.dateTo;
    }
    if (v.outgoingCategoryId) {
      filter.outgoingCategoryId = Number(v.outgoingCategoryId);
    }
    return filter;
  }

  /**
   * §23.U.41 استخراج البيانات — the audit workbook through the engine (18-41): the screen's
   * own column set (the register's serial included), every page under the endpoint's 100 cap.
   */
  buildExportRequest = (): ReportExportRequest<MissingOutgoingAttachmentsRow> | null  => {
    if (!this.hasRun || !this.totalCount) {
      return null;
    }
    const filter = this.buildFilter();
    return {
      reportKey: 'missing-outgoing-attachments',
      fetchPage: (page, pageSize) => firstValueFrom(
        this.reportService.getMissingOutgoingAttachments({ ...filter, page, pageSize })
      )
    };
  }

  // ==================== helpers ====================

  /** AC mirror — date order client-side; the server refuses again regardless. */
  private validateFilter(): boolean {
    const v = this.filterForm.getRawValue();
    if (v.dateFrom && v.dateTo && new Date(v.dateTo) < new Date(v.dateFrom)) {
      this.notification.error(this.translate.instant('reports.missingOutgoingAttachments.dateOrderInvalid'));
      return false;
    }
    return true;
  }

  /** Server 400s carry { message, errors } — field errors land on their controls (P-pattern). */
  private handleFilterError(httpError: any): void {
    this.loading = false;
    const errors = httpError?.details;
    if (errors && typeof errors === 'object') {
      for (const [field, messages] of Object.entries<any>(errors)) {
        const controlName = field.charAt(0).toLowerCase() + field.slice(1);
        const control = this.filterForm.get(controlName);
        const message = Array.isArray(messages) ? messages.join(' · ') : String(messages);
        if (control) {
          control.setErrors({ server: message });
          control.markAsTouched();
        }
      }
    }
    this.notification.error(
      httpError?.message || this.translate.instant('reports.missingOutgoingAttachments.searchFailed')
    );
  }

  hasServerError(controlName: string): boolean {
    const control = this.filterForm.get(controlName);
    return !!(control && control.errors && control.errors['server'] && control.touched);
  }

  serverError(controlName: string): string {
    const control = this.filterForm.get(controlName);
    return control?.errors?.['server'] ?? '';
  }

  trackByRow(_index: number, row: MissingOutgoingAttachmentsRow): string {
    // Rows carry no id — the register's own composite (serial / outgoing number + subject).
    return `${row.serial ?? ''}|${row.outGoingNumber ?? ''}|${row.subject ?? ''}`;
  }
}
