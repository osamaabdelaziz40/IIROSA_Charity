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
  NewBeneficiariesFilter,
  NewBeneficiariesReport,
  NewBeneficiariesVariant,
  NewBeneficiaryRow
} from '../models/report.model';

/**
 * UC-RPT-35 (§23.U.35 الأيتام والأرامل الجدد) — the sponsorship-offer lists: orphans and
 * widows registered in a window, the four legacy .rpt variants collapsed to ONE variant-keyed
 * read (POST /api/Reports/new-beneficiaries) and ONE screen with four commands.
 *
 * Thin by design: the shared report-viewer shell owns the chrome (بحث / Pagination / empty
 * states); this component owns the charity + registration-window filter panel, the four-button
 * variant group, and the per-variant bespoke grid. The document is composed client-side by
 * 18-21's report-pdf.service (browser print — the recorded epic-wide path).
 *
 * The charity scope is enforced SERVER-SIDE (pin-never-widen) — the charity filter narrows HQ
 * queries only; hiding it for charity users is convenience, not the control. The registration
 * date is CreatedOn server-side (the recorded proxy — no dedicated column exists).
 */
@Component({
  selector: 'app-new-beneficiaries-report',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    ReportViewerComponent,
    DropDownComponent
  ],
  templateUrl: './new-beneficiaries-report.component.html',
  styleUrls: ['./new-beneficiaries-report.component.scss']
})
export class NewBeneficiariesReportComponent implements OnInit, OnDestroy {
  rows: NewBeneficiaryRow[] = [];
  loading = false;
  printing = false;
  hasRun = false;

  currentPage = 1;
  pageSize = 20;
  totalCount = 0;

  isHQ = false;
  charityOptions: Array<{ id: string; name: string }> = [];

  /** The selected variant — drives both the grid's column set and the document's. */
  variant: NewBeneficiariesVariant = 'orphans';

  /** The four commands — the variant registry (a fifth layout joins here, not a new endpoint). */
  readonly variantButtons: Array<{ id: NewBeneficiariesVariant; labelKey: string }> = [
    { id: 'orphans', labelKey: 'reports.newBeneficiaries.variantOrphans' },
    { id: 'orphansv2', labelKey: 'reports.newBeneficiaries.variantOrphansV2' },
    { id: 'widows', labelKey: 'reports.newBeneficiaries.variantWidows' },
    { id: 'widowsbyfamily', labelKey: 'reports.newBeneficiaries.variantWidowsByFamily' }
  ];

  /** §23.U.41 — each variant IS its own export shape: its own registered column set. */
  private readonly exportKeys: Record<NewBeneficiariesVariant, string> = {
    orphans: 'new-beneficiaries-orphans',
    orphansv2: 'new-beneficiaries-orphansv2',
    widows: 'new-beneficiaries-widows',
    widowsbyfamily: 'new-beneficiaries-widowsbyfamily'
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
            { id: '', name: this.translate.instant('reports.newBeneficiaries.allCharities') },
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
   * Variant switch — the four legacy .rpt commands. WITHIN a pair (orphans/orphansv2,
   * widows/widowsbyfamily) the rows share their source and stay loaded — the grid
   * re-renders its column set only. ACROSS pairs the loaded rows belong to the other
   * projection (an orphans row under the widows grid renders all dashes). Review
   * P19 2026-08-26: a cross-projection switch resets rows, count and hasRun so the
   * screen reads initial until بحث re-runs for the chosen variant.
   */
  selectVariant(variant: NewBeneficiariesVariant): void {
    if (this.loading || this.printing || variant === this.variant) {
      return;
    }
    const crossesProjection =
      (this.variant === 'orphans' || this.variant === 'orphansv2') !==
      (variant === 'orphans' || variant === 'orphansv2');
    this.variant = variant;
    if (crossesProjection) {
      this.rows = [];
      this.totalCount = 0;
      this.hasRun = false;
      this.currentPage = 1;
    }
  }

  // ==================== search / print ====================

  /** بحث — one variant-keyed read (page 1). Nothing stored changes. */
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
    this.reportService.getNewBeneficiaries(this.buildFilter())
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (report: NewBeneficiariesReport) => {
          this.rows = report.items || [];
          this.totalCount = report.totalCount || 0;
          this.loading = false;
          this.hasRun = true;
        },
        error: (httpError: any) => this.handleFilterError(httpError)
      });
  }

  /**
   * طباعة — the variant's list document via the epic's browser-print pipeline. An empty
   * selection refuses with the nothing-to-produce message instead of an empty sheet (AC 3).
   */
  printReport(): void {
    if (this.loading || this.printing) {
      return;
    }
    if (!this.rows.length) {
      this.notification.info(this.translate.instant('reports.newBeneficiaries.nothingToPrint'));
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

  // ==================== builders ====================

  private buildFilter(): NewBeneficiariesFilter {
    const v = this.filterForm.getRawValue();
    const filter: NewBeneficiariesFilter = {
      dateFrom: v.dateFrom,
      variant: this.variant,
      page: this.currentPage,
      pageSize: this.pageSize
    };
    if (this.isHQ && v.charityId) {
      filter.charityId = String(v.charityId);
    }
    if (v.dateTo) {
      filter.dateTo = v.dateTo;
    }
    return filter;
  }

  /**
   * The printed list — the variant-driven column set, A4 portrait RTL through printSheet
   * (totals ride the meta band; the footer carries the count row). widowsByFamily groups its
   * rows by family: the widow row IS the family's mother, so a familyCode ordering is the
   * grouping — the sheet sorts its copy; the grid keeps the server's registration order.
   */
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
  buildExportRequest = (): ReportExportRequest<NewBeneficiaryRow> | null  => {
    if (!this.hasRun || !this.totalCount) {
      return null;
    }
    const filter = this.buildFilter();
    return {
      reportKey: this.exportKeys[this.variant],
      fetchPage: (page, pageSize) => firstValueFrom(
        this.reportService.getNewBeneficiaries({ ...filter, page, pageSize })
      )
    };
  }

  private buildSheet(): ReportSheetConfig<NewBeneficiaryRow> {
    const t = (key: string): string => this.translate.instant(key);
    const v = this.filterForm.getRawValue();
    const charityName = this.charityOptions.find(c => c.id === String(v.charityId || ''))?.name
      || t('reports.newBeneficiaries.allCharities');
    const variantLabel = t(this.variantButtons.find(b => b.id === this.variant)!.labelKey);
    const rows = this.variant === 'widowsbyfamily'
      ? [...this.rows].sort((a, b) =>
          (a.familyCode || '').localeCompare(b.familyCode || '', 'ar'))
      : this.rows;

    return {
      documentTitle: `${t('reports.newBeneficiaries.title')} - ${variantLabel}`,
      title: t('reports.newBeneficiaries.title'),
      subtitle: variantLabel,
      meta: [
        { label: t('reports.newBeneficiaries.filterCharity'), value: charityName },
        {
          label: t('reports.newBeneficiaries.metaRange'),
          value: `${this.formatDate(v.dateFrom)} — ${this.formatDate(v.dateTo)}`
        },
        {
          label: t('reports.newBeneficiaries.metaTotal'),
          value: `${t('reports.newBeneficiaries.metaCount')}: ${this.totalCount}`
        }
      ],
      columns: this.sheetColumns(t),
      rows
    };
  }

  /** The variant's printed column set (§23.U.35 variant contract, verbatim column lists). */
  private sheetColumns(t: (key: string) => string): ReportSheetConfig<NewBeneficiaryRow>['columns'] {
    const serial = { header: t('reports.newBeneficiaries.colSerial'), render: (_r: NewBeneficiaryRow, i: number) => String(i + 1) };
    const orphanCode = { header: t('reports.newBeneficiaries.colOrphanCode'), render: (r: NewBeneficiaryRow) => r.orphanCode ?? '-' };
    const orphanName = { header: t('reports.newBeneficiaries.colOrphanName'), render: (r: NewBeneficiaryRow) => r.orphanName ?? '-' };
    const birthDate = { header: t('reports.newBeneficiaries.colBirthDate'), render: (r: NewBeneficiaryRow) => this.formatDate(r.birthDate) };
    const familyCode = { header: t('reports.newBeneficiaries.colFamilyCode'), render: (r: NewBeneficiaryRow) => r.familyCode ?? '-' };
    const guardianName = { header: t('reports.newBeneficiaries.colGuardian'), render: (r: NewBeneficiaryRow) => r.guardianName ?? '-' };
    const widowName = { header: t('reports.newBeneficiaries.colWidowName'), render: (r: NewBeneficiaryRow) => r.widowName ?? '-' };
    const nationalId = { header: t('reports.newBeneficiaries.colNationalId'), render: (r: NewBeneficiaryRow) => r.nationalId ?? '-' };
    const husbandDeathDate = { header: t('reports.newBeneficiaries.colHusbandDeathDate'), render: (r: NewBeneficiaryRow) => this.formatDate(r.husbandDeathDate) };
    const childrenCount = { header: t('reports.newBeneficiaries.colChildrenCount'), render: (r: NewBeneficiaryRow) => String(r.childrenCount ?? 0) };
    const charityName = { header: t('reports.newBeneficiaries.colCharity'), render: (r: NewBeneficiaryRow) => r.charityName ?? '-' };
    const registrationDate = { header: t('reports.newBeneficiaries.colRegistrationDate'), render: (r: NewBeneficiaryRow) => this.formatDate(r.registrationDate) };

    switch (this.variant) {
      case 'orphans':
        return [serial, orphanCode, orphanName, birthDate, charityName, registrationDate];
      case 'orphansv2':
        // rptNewOrphansV2 — the same rows + كود العائلة · اسم المعيل (the wider layout).
        return [serial, orphanCode, orphanName, birthDate, familyCode, guardianName, charityName, registrationDate];
      case 'widows':
        return [serial, widowName, nationalId, husbandDeathDate, charityName, registrationDate];
      case 'widowsbyfamily':
        return [serial, familyCode, widowName, childrenCount, charityName];
    }
  }

  // ==================== helpers ====================

  /** AC 2/4 client mirror — dateFrom mandatory, dateTo ≥ dateFrom; the server refuses again. */
  private validateFilter(): boolean {
    const v = this.filterForm.getRawValue();
    if (!v.dateFrom) {
      this.notification.error(this.translate.instant('reports.newBeneficiaries.dateFromRequired'));
      return false;
    }
    if (v.dateFrom && v.dateTo && new Date(v.dateTo) < new Date(v.dateFrom)) {
      this.notification.error(this.translate.instant('reports.newBeneficiaries.dateOrderInvalid'));
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
      httpError?.message || this.translate.instant('reports.newBeneficiaries.searchFailed')
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

  trackByRow(_index: number, row: NewBeneficiaryRow): string {
    // Rows carry no id — the stable composite per branch (orphan code / family code + names).
    return `${row.orphanCode ?? row.familyCode ?? ''}|${row.orphanName ?? row.widowName ?? ''}|${row.nationalId ?? ''}`;
  }

  trackByVariant(_index: number, button: { id: NewBeneficiariesVariant }): string {
    return button.id;
  }
}
