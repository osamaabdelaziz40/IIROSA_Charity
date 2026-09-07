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
import { ReportExportRequest, FamilyOrphansFlatRow } from '../models/report-columns';
import { CharityService } from '../../charities/services/charity.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import {
  FamilyOrphansByDateFilter,
  FamilyOrphanRow,
  FamilyWithOrphans,
  ReportPagedResult
} from '../models/report.model';

/** The flattened print row — family cells render on the group's first orphan row only. */
interface FamilyOrphanSheetRow {
  familyCode: string;
  headOfFamily: string;
  regionCenter: string;
  orphansCount: number;
  orphanCode: string;
  orphanName: string;
  dateOfBirth: string | null;
  age: number | null;
  isFirstOfGroup: boolean;
}

/**
 * UC-RPT-39 (§23.U.39 أيتام الأسر بتاريخ) — one charity's families as at a date, its orphans
 * grouped under each family header, printed through the epic's browser-print pipeline.
 *
 * Thin by design: the shared report-viewer shell owns the chrome (بحث / Pagination / empty
 * states); this component owns the filter bar (charity for HQ + the mandatory as-at date), the
 * collapsible grouped grid, and the طباعة command. The as-at predicate runs server-side on
 * Family.RegistrationDate (the dedicated column — recorded; not a CreatedOn proxy).
 *
 * Distinct route from 18-13's #/reports/family-orphans (follow-up tracking) by story ruling —
 * do NOT rename that one. The charity scope is enforced SERVER-SIDE (pin-never-widen); the HQ
 * dropdown is convenience, not the control.
 */
@Component({
  selector: 'app-family-orphans-by-date-report',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    ReportViewerComponent,
    DropDownComponent
  ],
  templateUrl: './family-orphans-by-date-report.component.html',
  styleUrls: ['./family-orphans-by-date-report.component.scss']
})
export class FamilyOrphansByDateReportComponent implements OnInit, OnDestroy {
  families: FamilyWithOrphans[] = [];
  loading = false;
  printing = false;
  hasRun = false;

  currentPage = 1;
  pageSize = 20;
  totalCount = 0;

  isHQ = false;
  charityOptions: Array<{ id: string; name: string }> = [];

  /** Collapsed group keys — groups render expanded by default. */
  private readonly collapsed = new Set<string>();

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
      date: [null]
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

  /** الجمعية — HQ only; the all-option runs across charities (country claim narrows server-side). */
  private loadCharities(): void {
    this.charityService.getCharities({ pageNumber: 1, pageSize: 500 })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: response => {
          this.charityOptions = [
            { id: '', name: this.translate.instant('reports.familyOrphansByDate.allCharities') },
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

  // ==================== search / print ====================

  /** بحث — one grouped read (page 1). Nothing stored changes (AC 1). */
  search(): void {
    if (this.loading || this.printing) {
      return;
    }
    const v = this.filterForm.getRawValue();
    if (!v.date) {
      this.notification.error(this.translate.instant('reports.familyOrphansByDate.dateRequired'));
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
    this.reportService.getFamilyOrphansByDate(this.buildFilter())
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (report: ReportPagedResult<FamilyWithOrphans>) => {
          this.families = report.items || [];
          this.totalCount = report.totalCount || 0;
          this.collapsed.clear();
          this.loading = false;
          this.hasRun = true;
        },
        error: (httpError: any) => this.handleFilterError(httpError)
      });
  }

  /**
   * طباعة — the grouped list via the epic's browser-print pipeline. The group headers flatten
   * into the table: family cells render on the group's first orphan row, the per-family count
   * rides a column (the contract's footer as data), and the final totals ride the meta band.
   * An empty selection refuses with the nothing-to-produce message instead of an empty file (AC 3).
   */
  printSheetCommand(): void {
    if (this.loading || this.printing) {
      return;
    }
    const flat = this.flatten();
    if (!flat.length) {
      this.notification.info(this.translate.instant('reports.familyOrphansByDate.nothingToPrint'));
      return;
    }

    // Review P22 2026-08-26: the sheet prints the LOADED page's families while its meta band
    // shows the full family totalCount — a partial document says so before it prints.
    if (this.totalCount > this.families.length) {
      this.notification.warning(this.translate.instant('reports.print.truncated'));
    }

    this.printing = true;
    // Review P27 2026-08-26: window.print() blocks the main thread — deferring one tick lets
    // the [disabled]="printing" guard actually paint (set+clear in one tick never rendered).
    setTimeout(() => {
      try {
        this.reportPdfService.printSheet(this.buildSheet(flat));
      } finally {
        this.printing = false;
      }
    }, 50);
  }

  /**
   * §23.U.40 عرض — the grouped list over the loaded page: the SAME buildSheet(flatten()) the
   * print command composes, previewed in the shell's shared modal instead of printed directly.
   */
  previewDocument = (): ReportPreviewSource | null  => {
    const flat = this.flatten();
    return flat.length ? this.reportPdfService.sheetSource(this.buildSheet(flat)) : null;
  }

  /**
   * §23.U.41 استخراج البيانات — the grouped workbook through the engine (18-41), the
   * 18-39 answer for "all pages": every family page walked under the endpoint's 100 cap and
   * flattened — family cells REPEAT on every orphan row (Excel has no rowspan, and the
   * autofilter demands consistent cells; the print sheet's group-first banding stays print-only).
   */
  buildExportRequest = (): ReportExportRequest<FamilyOrphansFlatRow> | null  => {
    if (!this.hasRun || !this.totalCount) {
      return null;
    }
    const filter = this.buildFilter();
    return {
      reportKey: 'family-orphans-by-date',
      fetchPage: async (page, pageSize) => {
        const result = await firstValueFrom(
          this.reportService.getFamilyOrphansByDate({ ...filter, page, pageSize })
        );
        const items: FamilyOrphansFlatRow[] = [];
        for (const family of result.items || []) {
          if (!family.orphans?.length) {
            // Review P23 2026-08-26: an orphan-less family keeps its row (family cells only) —
            // the grid shows the group header; the export must not drop it.
            items.push({
              familyCode: family.familyCode,
              headOfFamily: family.headOfFamily,
              regionCenter: this.regionCenter(family),
              orphansCount: family.orphansCount,
              orphanCode: '',
              orphanName: '',
              dateOfBirth: null,
              age: null
            });
            continue;
          }
          for (const orphan of family.orphans) {
            items.push({
              familyCode: family.familyCode,
              headOfFamily: family.headOfFamily,
              regionCenter: this.regionCenter(family),
              orphansCount: family.orphansCount,
              orphanCode: orphan.code,
              orphanName: orphan.fullName,
              dateOfBirth: orphan.dateOfBirth,
              age: orphan.age
            });
          }
        }
        // Review P23 2026-08-26: the server's total counts FAMILIES while these rows are
        // ORPHANS — advertising it here would stop the walk early (collected orphans vs the
        // family total) and read as complete. No advertised total: the engine walks until the
        // server's last page, which is exact for this endpoint.
        return { items, totalCount: 0 };
      }
    };
  }

  // ==================== builders ====================

  private buildFilter(): FamilyOrphansByDateFilter {
    const v = this.filterForm.getRawValue();
    const filter: FamilyOrphansByDateFilter = {
      date: v.date,
      page: this.currentPage,
      pageSize: this.pageSize
    };
    if (this.isHQ && v.charityId) {
      filter.charityId = String(v.charityId);
    }
    return filter;
  }

  /** Flatten the page's families into print rows — group-first rows carry the family band. */
  private flatten(): FamilyOrphanSheetRow[] {
    const flat: FamilyOrphanSheetRow[] = [];
    for (const family of this.families) {
      if (!family.orphans?.length) {
        // Review P23 2026-08-26: family-only rows print too — family band, empty orphan cells.
        flat.push({
          familyCode: family.familyCode,
          headOfFamily: family.headOfFamily,
          regionCenter: this.regionCenter(family),
          orphansCount: family.orphansCount,
          orphanCode: '',
          orphanName: '',
          dateOfBirth: null,
          age: null,
          isFirstOfGroup: true
        });
        continue;
      }
      family.orphans.forEach((orphan: FamilyOrphanRow, index: number) => {
        flat.push({
          familyCode: index === 0 ? family.familyCode : '',
          headOfFamily: index === 0 ? family.headOfFamily : '',
          regionCenter: index === 0 ? this.regionCenter(family) : '',
          orphansCount: family.orphansCount,
          orphanCode: orphan.code,
          orphanName: orphan.fullName,
          dateOfBirth: orphan.dateOfBirth,
          age: orphan.age,
          isFirstOfGroup: index === 0
        });
      });
    }
    return flat;
  }

  /** The printed sheet — A4 portrait RTL; totals ride the meta band (epic convention). */
  private buildSheet(flat: FamilyOrphanSheetRow[]): ReportSheetConfig<FamilyOrphanSheetRow> {
    const t = (key: string): string => this.translate.instant(key);
    const v = this.filterForm.getRawValue();
    const charityName = this.charityOptions.find(c => c.id === String(v.charityId || ''))?.name
      || t('reports.familyOrphansByDate.allCharities');
    const orphansOnPage = this.families.reduce((sum, f) => sum + (f.orphansCount || 0), 0);

    return {
      documentTitle: t('reports.familyOrphansByDate.title'),
      title: t('reports.familyOrphansByDate.title'),
      subtitle: t('reports.familyOrphansByDate.subtitle'),
      meta: [
        { label: t('reports.familyOrphansByDate.filterCharity'), value: charityName },
        { label: t('reports.familyOrphansByDate.metaAsAt'), value: this.formatDate(v.date) },
        {
          label: t('reports.familyOrphansByDate.metaTotalFamilies'),
          value: `${t('reports.familyOrphansByDate.metaTotalFamilies')}: ${this.totalCount}`
        },
        {
          label: t('reports.familyOrphansByDate.metaOrphansPage'),
          value: `${t('reports.familyOrphansByDate.metaOrphansPage')}: ${orphansOnPage}`
        }
      ],
      columns: [
        {
          header: t('reports.familyOrphansByDate.colFamilyCode'),
          render: (r: FamilyOrphanSheetRow) => r.isFirstOfGroup ? r.familyCode : ''
        },
        {
          header: t('reports.familyOrphansByDate.colHeadOfFamily'),
          render: (r: FamilyOrphanSheetRow) => r.isFirstOfGroup ? r.headOfFamily : ''
        },
        {
          header: t('reports.familyOrphansByDate.colRegionCenter'),
          render: (r: FamilyOrphanSheetRow) => r.isFirstOfGroup ? r.regionCenter : ''
        },
        {
          header: t('reports.familyOrphansByDate.colOrphansCount'),
          render: (r: FamilyOrphanSheetRow) => r.isFirstOfGroup
            ? (r.orphansCount != null ? String(r.orphansCount) : '') : ''
        },
        { header: t('reports.familyOrphansByDate.colOrphanCode'), render: (r: FamilyOrphanSheetRow) => r.orphanCode },
        { header: t('reports.familyOrphansByDate.colOrphanName'), render: (r: FamilyOrphanSheetRow) => r.orphanName },
        { header: t('reports.familyOrphansByDate.colBirthDate'), render: (r: FamilyOrphanSheetRow) => this.formatDate(r.dateOfBirth) },
        { header: t('reports.familyOrphansByDate.colAge'), render: (r: FamilyOrphanSheetRow) => r.age != null ? String(r.age) : '-' }
      ],
      rows: flat
    };
  }

  // ==================== grouped-grid helpers ====================

  groupKey(index: number, family: FamilyWithOrphans): string {
    return `${index}|${family.familyCode}`;
  }

  isCollapsed(index: number, family: FamilyWithOrphans): boolean {
    return this.collapsed.has(this.groupKey(index, family));
  }

  toggleGroup(index: number, family: FamilyWithOrphans): void {
    const key = this.groupKey(index, family);
    if (this.collapsed.has(key)) {
      this.collapsed.delete(key);
    } else {
      this.collapsed.add(key);
    }
  }

  /** المحافظة/المركز — composed with a dash, either side optional. */
  regionCenter(family: FamilyWithOrphans): string {
    return [family.regionName, family.centerName].filter(Boolean).join(' — ');
  }

  // ==================== helpers ====================

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
      httpError?.message || this.translate.instant('reports.familyOrphansByDate.searchFailed')
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

  hasServerError(controlName: string): boolean {
    const control = this.filterForm.get(controlName);
    return !!(control && control.errors && control.errors['server'] && control.touched);
  }

  serverError(controlName: string): string {
    const control = this.filterForm.get(controlName);
    return control?.errors?.['server'] ?? '';
  }

  trackByFamily(_index: number, family: FamilyWithOrphans): string {
    return `${family.familyCode}|${family.headOfFamily}`;
  }

  trackByOrphan(_index: number, orphan: FamilyOrphanRow): string {
    return `${orphan.code}|${orphan.fullName}`;
  }
}
