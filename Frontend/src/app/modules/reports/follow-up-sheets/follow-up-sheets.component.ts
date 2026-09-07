import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { firstValueFrom } from 'rxjs';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { ReportViewerComponent } from '../report-viewer/report-viewer.component';
import { DropDownComponent } from '../../../shared/components/drop-down/drop-down.component';
import { ReportService } from '../services/report.service';
import { ReportPdfService, ReportPreviewSource, ReportSheetConfig } from '../services/report-pdf.service';
import { ReportExportRequest } from '../models/report-columns';
import { CharityService } from '../../charities/services/charity.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import {
  FollowUpSheetFilter,
  FollowUpSheetRow,
  FollowUpSheetVariant,
  ReportPagedResult
} from '../models/report.model';

/**
 * UC-RPT-36 (§23.U.36 كشوف المتابعة والتسليم) — the legacy Crystal trio collapsed to ONE
 * variant-keyed GET (GET /api/Reports/follow-up-sheets) and ONE screen with a three-command
 * variant group: متابعة (one row per orphan with its latest accepted report) · متابعة الأسر
 * (one row per family) · تسليم (the handover sheet with the guardian).
 *
 * Thin by design: the shared report-viewer shell owns the chrome (بحث / Pagination / empty
 * states); this component owns the filter bar (variant selector, charity for HQ, date range)
 * and the per-variant bespoke grid. The sheet is composed client-side by 18-21's
 * report-pdf.service (browser print — the recorded epic-wide path; the Crystal trio and a
 * server /export/pdf are superseded).
 *
 * The charity scope is enforced SERVER-SIDE (pin-never-widen) — the charity filter narrows HQ
 * queries only; hiding it for charity users is convenience, not the control. The optional date
 * window scopes the row anchor's registration date (CreatedOn server-side — the 18-35 proxy
 * ruling, no dedicated registration column exists).
 */
@Component({
  selector: 'app-follow-up-sheets',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    ReportViewerComponent,
    DropDownComponent
  ],
  templateUrl: './follow-up-sheets.component.html',
  styleUrls: ['./follow-up-sheets.component.scss']
})
export class FollowUpSheetsComponent implements OnInit, OnDestroy {
  rows: FollowUpSheetRow[] = [];
  loading = false;
  printing = false;
  hasRun = false;

  currentPage = 1;
  pageSize = 20;
  totalCount = 0;

  isHQ = false;
  charityOptions: Array<{ id: string; name: string }> = [];

  /** The selected variant — drives both the grid's column set and the sheet's. */
  variant: FollowUpSheetVariant = 'FollowUp';

  /** The three commands — the variant registry (the Crystal trio, collapsed). */
  readonly variantButtons: Array<{ id: FollowUpSheetVariant; labelKey: string }> = [
    { id: 'FollowUp', labelKey: 'reports.followUpSheets.variantFollowUp' },
    { id: 'FollowUpFamily', labelKey: 'reports.followUpSheets.variantFollowUpFamily' },
    { id: 'Tasleem', labelKey: 'reports.followUpSheets.variantTasleem' }
  ];

  /** §23.U.41 — each variant IS its own export shape: its own registered column set. */
  private readonly exportKeys: Record<FollowUpSheetVariant, string> = {
    FollowUp: 'follow-up-orphans',
    FollowUpFamily: 'follow-up-families',
    Tasleem: 'follow-up-tasleem'
  };

  filterForm: FormGroup;

  private readonly destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private reportService: ReportService,
    private reportPdfService: ReportPdfService,
    private charityService: CharityService,
    private authService: AuthService,
    private notification: NotificationService,
    private translate: TranslateService
  ) {
    this.filterForm = this.fb.group({
      charityId: [''],
      dateFrom: [null],
      dateTo: [null]
    });
  }

  ngOnInit(): void {
    this.isHQ = this.authService.hasAnyRole(['SuperAdmin', 'Admin']);
    if (this.isHQ) {
      this.loadCharities();
    }
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
            { id: '', name: this.translate.instant('reports.followUpSheets.allCharities') },
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

  // ==================== variant group ====================

  /**
   * Variant switch — the three legacy printouts. متابعة and تسليم share the orphan row
   * source (only the column set differs); متابعة الأسر is the FAMILY projection. Review
   * P19 2026-08-26: crossing to/from متابعة الأسر renders the other projection's rows as
   * dashes — the stale rows, count and hasRun reset so the screen reads initial until
   * بحث re-runs.
   */
  selectVariant(variant: FollowUpSheetVariant): void {
    if (this.loading || this.printing || variant === this.variant) {
      return;
    }
    const crossesProjection =
      (this.variant === 'FollowUpFamily') !== (variant === 'FollowUpFamily');
    this.variant = variant;
    if (crossesProjection) {
      this.rows = [];
      this.totalCount = 0;
      this.hasRun = false;
      this.currentPage = 1;
    }
  }

  // ==================== search / print ====================

  /** بحث — one variant-keyed read (page 1). Nothing stored changes (AC 1). */
  search(): void {
    if (this.loading || this.printing) {
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
    this.reportService.getFollowUpSheets(this.buildFilter())
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (report: ReportPagedResult<FollowUpSheetRow>) => {
          this.rows = report.items || [];
          this.totalCount = report.totalCount || 0;
          this.loading = false;
          this.hasRun = true;
        },
        error: (httpError: any) => this.handleFilterError(httpError)
      });
  }

  /**
   * طباعة — the variant's sheet via the epic's browser-print pipeline. An empty grid refuses
   * with the nothing-to-produce message instead of an empty sheet (AC 7).
   */
  printSheetCommand(): void {
    if (this.loading || this.printing) {
      return;
    }
    if (!this.rows.length) {
      this.notification.info(this.translate.instant('reports.followUpSheets.nothingToPrint'));
      return;
    }

    // Review P22 2026-08-26: the sheet prints the LOADED page while its meta band shows the
    // full totalCount — a partial document says so before it prints, never silently.
    if (this.totalCount > this.rows.length) {
      this.notification.warning(this.translate.instant('reports.print.truncated'));
    }

    this.printing = true;
    // Review P27 2026-08-26: window.print() blocks the main thread — deferring one tick lets
    // the [disabled]="printing" guard actually paint (set+clear in one tick never rendered).
    setTimeout(() => {
      try {
        this.reportPdfService.printSheet(this.buildSheet());
      } finally {
        this.printing = false;
      }
    }, 50);
  }

  /**
   * §23.U.40 عرض — the variant's sheet over the loaded page: the SAME buildSheet() the print
   * command composes, previewed in the shell's shared modal instead of printed directly.
   */
  previewDocument = (): ReportPreviewSource | null  => {
    return this.rows.length ? this.reportPdfService.sheetSource(this.buildSheet()) : null;
  }

  /**
   * §23.U.41 استخراج البيانات — the variant's workbook through the engine (18-41): the
   * current variant's registered column set, every page walked under the endpoint's 100 cap.
   */
  buildExportRequest = (): ReportExportRequest<FollowUpSheetRow> | null  => {
    if (!this.hasRun || !this.totalCount) {
      return null;
    }
    const filter = this.buildFilter();
    return {
      reportKey: this.exportKeys[this.variant],
      fetchPage: (page, pageSize) => firstValueFrom(
        this.reportService.getFollowUpSheets({ ...filter, page, pageSize })
      )
    };
  }

  // ==================== builders ====================

  private buildFilter(): FollowUpSheetFilter {
    const v = this.filterForm.getRawValue();
    const filter: FollowUpSheetFilter = {
      variant: this.variant,
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
    return filter;
  }

  /** The printed sheet — the variant's column set, A4 portrait RTL; totals ride the meta band. */
  private buildSheet(): ReportSheetConfig<FollowUpSheetRow> {
    const t = (key: string): string => this.translate.instant(key);
    const v = this.filterForm.getRawValue();
    const charityName = this.charityOptions.find(c => c.id === String(v.charityId || ''))?.name
      || t('reports.followUpSheets.allCharities');
    const variantLabel = t(this.variantButtons.find(b => b.id === this.variant)!.labelKey);

    return {
      documentTitle: `${t('reports.followUpSheets.title')} - ${variantLabel}`,
      title: t('reports.followUpSheets.title'),
      subtitle: variantLabel,
      meta: [
        { label: t('reports.followUpSheets.filterCharity'), value: charityName },
        {
          label: t('reports.followUpSheets.metaRange'),
          value: `${this.formatDate(v.dateFrom)} — ${this.formatDate(v.dateTo)}`
        },
        {
          label: t('reports.followUpSheets.metaTotal'),
          value: `${t('reports.followUpSheets.metaCount')}: ${this.totalCount}`
        }
      ],
      columns: this.sheetColumns(t),
      rows: this.rows
    };
  }

  /** The variant's printed column set (§23.U.36 projection design, verbatim column lists). */
  private sheetColumns(t: (key: string) => string): ReportSheetConfig<FollowUpSheetRow>['columns'] {
    const serial = { header: t('reports.followUpSheets.colSerial'), render: (_r: FollowUpSheetRow, i: number) => String(i + 1) };
    const orphanCode = { header: t('reports.followUpSheets.colOrphanCode'), render: (r: FollowUpSheetRow) => r.orphanCode ?? '-' };
    const orphanName = { header: t('reports.followUpSheets.colOrphanName'), render: (r: FollowUpSheetRow) => r.orphanName ?? '-' };
    const guardianName = { header: t('reports.followUpSheets.colGuardian'), render: (r: FollowUpSheetRow) => r.guardianName ?? '-' };
    const familyCode = { header: t('reports.followUpSheets.colFamilyCode'), render: (r: FollowUpSheetRow) => r.familyCode ?? '-' };
    const charityName = { header: t('reports.followUpSheets.colCharity'), render: (r: FollowUpSheetRow) => r.charityName ?? '-' };
    const sponsorshipStatus = { header: t('reports.followUpSheets.colSponsorshipStatus'), render: (r: FollowUpSheetRow) => r.sponsorshipStatus ?? '-' };
    const lastReportDate = { header: t('reports.followUpSheets.colLastReportDate'), render: (r: FollowUpSheetRow) => this.formatDate(r.lastReportDate) };
    const monthlyAmount = { header: t('reports.followUpSheets.colMonthlyAmount'), render: (r: FollowUpSheetRow) => r.monthlyAmount != null ? String(r.monthlyAmount) : '-' };
    const headOfFamily = { header: t('reports.followUpSheets.colHeadOfFamily'), render: (r: FollowUpSheetRow) => r.headOfFamily ?? '-' };
    const orphansCount = { header: t('reports.followUpSheets.colOrphansCount'), render: (r: FollowUpSheetRow) => String(r.orphansCount ?? 0) };
    const familyStatus = { header: t('reports.followUpSheets.colFamilyStatus'), render: (r: FollowUpSheetRow) => r.familyStatus ?? '-' };
    const registrationDate = { header: t('reports.followUpSheets.colRegistrationDate'), render: (r: FollowUpSheetRow) => this.formatDate(r.registrationDate) };
    const lastUpdate = { header: t('reports.followUpSheets.colLastUpdate'), render: (r: FollowUpSheetRow) => this.formatDate(r.lastUpdate) };

    switch (this.variant) {
      case 'FollowUp':
        // rptFollowUp — one row per orphan: code, name, family, charity, status, latest
        // accepted report, monthly amount.
        return [serial, orphanCode, orphanName, familyCode, charityName, sponsorshipStatus, lastReportDate, monthlyAmount];
      case 'FollowUpFamily':
        // rptFollowUpFamily — one row per family.
        return [serial, familyCode, headOfFamily, charityName, orphansCount, familyStatus, registrationDate, lastUpdate];
      case 'Tasleem':
        // rptFollowUpTasleem — the handover sheet: orphan + guardian + family + amount.
        return [serial, orphanCode, orphanName, guardianName, familyCode, charityName, monthlyAmount, sponsorshipStatus];
    }
  }

  // ==================== helpers ====================

  /** AC mirror — date order client-side; the server refuses again regardless. */
  private validateFilter(): boolean {
    const v = this.filterForm.getRawValue();
    if (v.dateFrom && v.dateTo && new Date(v.dateTo) < new Date(v.dateFrom)) {
      this.notification.error(this.translate.instant('reports.followUpSheets.dateOrderInvalid'));
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
      httpError?.message || this.translate.instant('reports.followUpSheets.searchFailed')
    );
  }

  /** Short yyyy-MM-dd, dash for empty — the sheet's date format. */
  private formatDate(value?: string | null): string {
    if (!value) {
      return '-';
    }
    const date = new Date(value);
    return isNaN(date.getTime()) ? '-' : date.toISOString().slice(0, 10);
  }

  /** Serial continuous across pages. */
  serial(index: number): number {
    return (this.currentPage - 1) * this.pageSize + index + 1;
  }

  hasServerError(controlName: string): boolean {
    const control = this.filterForm.get(controlName);
    return !!(control && control.errors && control.errors['server'] && control.touched);
  }

  serverError(controlName: string): string {
    const control = this.filterForm.get(controlName);
    return control?.errors?.['server'] ?? '';
  }

  trackByRow(_index: number, row: FollowUpSheetRow): string {
    // Rows carry no id — the stable composite per variant (orphan code / family code + names).
    return `${row.orphanCode ?? row.familyCode ?? ''}|${row.orphanName ?? row.headOfFamily ?? ''}|${row.lastReportDate ?? row.lastUpdate ?? ''}`;
  }

  trackByVariant(_index: number, button: { id: FollowUpSheetVariant }): string {
    return button.id;
  }
}
