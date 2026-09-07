import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Subject, firstValueFrom } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { ReportViewerComponent } from '../report-viewer/report-viewer.component';
import { DropDownComponent } from '../../../shared/components/drop-down/drop-down.component';
import { ReportPdfService, ReportPreviewSource, ReportSheetConfig } from '../services/report-pdf.service';
import { ReportExportRequest } from '../models/report-columns';
import { GeneralChecksService } from '../../general-checks/services/general-checks.service';
import { CheckListItem, CheckStatement } from '../../general-checks/models/check.model';
import { LookupManagementService } from '../../lookup-management/services/lookup-management.service';
import { NotificationService } from '../../../core/services/notification.service';

/**
 * UC-RPT-33 (§23.U.33 بيان الشيكات) — the reports-module surfacing of the bank
 * reconciliation statement.
 *
 * Data comes from the EXISTING UC-CHQ-09 endpoint (GET /api/CheckManagement/report) via the
 * general-checks module's own service — no forked HTTP client, no new Reports endpoint (the
 * board's POST /api/Reports/cheque-statement/export/pdf is superseded; recorded). The
 * charity/country scope of the cheque read is whatever EP-11 enforces — not re-cut here.
 *
 * Filters are the story's four §23.U.33 params mapped onto the audited CheckFilterDto keys:
 * bankId / dateFrom / dateTo / chequeType. Task 1's audit found NO server-required parameter
 * (every CheckFilterDto field is nullable) — the screen guards date order client-side only.
 */
@Component({
  selector: 'app-cheque-statement',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    ReportViewerComponent,
    DropDownComponent
  ],
  templateUrl: './cheque-statement.component.html',
  styleUrls: ['./cheque-statement.component.scss']
})
export class ChequeStatementComponent implements OnInit, OnDestroy {
  statement: CheckStatement | null = null;
  rows: CheckListItem[] = [];
  loading = false;
  printing = false;
  hasRun = false;
  totalCount = 0;

  bankOptions: Array<{ id: string; name: string }> = [];

  /**
   * The audited cheque-type source — CheckFilterDto.ChequeType's documented values
   * ("Orphans" = شيكات إيتام / "Individuals" = شيكات أفراد; null = both). Not a hardcoded
   * option array: the values ARE the live wire contract.
   */
  typeOptions: Array<{ id: string; name: string }> = [];

  filterForm: FormGroup;

  private readonly destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private generalChecksService: GeneralChecksService,
    private lookupService: LookupManagementService,
    private reportPdfService: ReportPdfService,
    private notification: NotificationService,
    private translate: TranslateService
  ) {
    this.filterForm = this.fb.group({
      bankId: [''],
      dateFrom: [null],
      dateTo: [null],
      chequeType: ['']
    });
  }

  ngOnInit(): void {
    this.loadBanks();
    this.typeOptions = [
      { id: '', name: this.translate.instant('reports.chequeStatement.typeBoth') },
      { id: 'Orphans', name: this.translate.instant('reports.chequeStatement.typeOrphans') },
      { id: 'Individuals', name: this.translate.instant('reports.chequeStatement.typeIndividuals') }
    ];
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /** البنك — GET /api/LookupManagement/banks (the same lookup the cheque register uses). */
  private loadBanks(): void {
    this.lookupService.getBanks({ pageNumber: 1, pageSize: 1000, isActive: true } as any)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => {
          this.bankOptions = [
            { id: '', name: this.translate.instant('reports.chequeStatement.allBanks') },
            ...(result.items || []).map(b => ({ id: String(b.id), name: b.nameAr || b.nameEn || b.name }))
          ];
          // Review P26 2026-08-26: a capped lookup says so — a silently truncated dropdown hides choices.
          if ((result.totalCount || 0) > (result.items || []).length) {
            this.notification.warning(this.translate.instant('reports.lookup.truncated'));
          }
        },
        error: () => console.error('Error loading banks')
      });
  }

  // ==================== search / print ====================

  /** بحث — one statement read (page 1, the 200 cap the register's own statement uses). */
  search(): void {
    if (this.loading || this.printing) {
      return;
    }
    if (!this.validateDates()) {
      return;
    }

    this.loading = true;
    const v = this.filterForm.getRawValue();
    this.generalChecksService.getStatement({
      bankId: v.bankId ? Number(v.bankId) : undefined,
      dateFrom: v.dateFrom || undefined,
      dateTo: v.dateTo || undefined,
      chequeType: v.chequeType || undefined,
      page: 1,
      pageSize: 200
    })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: statement => {
          this.statement = statement;
          this.rows = statement.items || [];
          this.totalCount = statement.totalCount || 0;
          this.loading = false;
          this.hasRun = true;
        },
        error: (httpError: any) => {
          this.loading = false;
          this.notification.error(
            httpError?.message || this.translate.instant('reports.chequeStatement.searchFailed'));
        }
      });
  }

  /**
   * طباعة — the statement document via the epic's browser-print pipeline. An empty result
   * refuses with the nothing-to-produce message instead of an empty sheet (AC 3).
   */
  printStatement(): void {
    if (this.loading || this.printing) {
      return;
    }
    if (!this.rows.length) {
      this.notification.info(this.translate.instant('reports.chequeStatement.nothingToPrint'));
      return;
    }

    // Review P22 2026-08-26: the statement prints the LOADED page (the 200-cap fetch) while
    // its meta band shows the full totalCount — a partial document says so before it prints.
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

  /** The printed sheet — totals ride the meta band (printSheet renders no totals row). */
  /**
   * §23.U.40 عرض — the statement sheet over the loaded page: the SAME buildSheet() the print
   * command composes, previewed in the shell's shared modal instead of printed directly.
   */
  previewDocument = (): ReportPreviewSource | null  => {
    return this.rows.length ? this.reportPdfService.sheetSource(this.buildSheet()) : null;
  }

  /**
   * §23.U.41 استخراج البيانات — the statement workbook through the engine (18-41): the
   * cheque-statement column set, amount as a NUMBER with its own currency column, pages
   * walked under the register's own 200 cap.
   */
  buildExportRequest = (): ReportExportRequest<CheckListItem> | null  => {
    if (!this.hasRun || !this.totalCount) {
      return null;
    }
    const v = this.filterForm.getRawValue();
    return {
      reportKey: 'cheque-statement',
      pageSize: 200,
      fetchPage: (page, pageSize) => firstValueFrom(this.generalChecksService.getStatement({
        bankId: v.bankId ? Number(v.bankId) : undefined,
        dateFrom: v.dateFrom || undefined,
        dateTo: v.dateTo || undefined,
        chequeType: v.chequeType || undefined,
        page,
        pageSize
      }))
    };
  }

  private buildSheet(): ReportSheetConfig<CheckListItem> {
    const t = (key: string): string => this.translate.instant(key);
    const v = this.filterForm.getRawValue();
    const bankName = this.bankOptions.find(b => b.id === String(v.bankId || ''))?.name
      || this.translate.instant('reports.chequeStatement.allBanks');
    const typeLabel = this.typeOptions.find(o => o.id === String(v.chequeType || ''))?.name
      || t('reports.chequeStatement.typeBoth');
    const totals = Object.entries(this.statement?.totalByCurrency || {})
      .map(([currency, total]) => `${currency}: ${total}`)
      .join(' · ');

    return {
      documentTitle: `${t('reports.chequeStatement.title')} - ${bankName}`,
      title: t('reports.chequeStatement.title'),
      subtitle: `${t('reports.chequeStatement.filterBank')}: ${bankName} — ${typeLabel}`,
      meta: [
        {
          label: t('reports.chequeStatement.metaRange'),
          value: `${this.formatDate(v.dateFrom)} — ${this.formatDate(v.dateTo)}`
        },
        {
          label: t('reports.chequeStatement.metaTotal'),
          value: `${t('reports.chequeStatement.metaCount')}: ${this.totalCount}`
            + (totals ? ` — ${totals}` : '')
        }
      ],
      columns: [
        { header: t('reports.chequeStatement.colSerial'), render: (_row, index) => String(index + 1) },
        { header: t('reports.chequeStatement.colNumber'), render: row => row.checkNumber },
        { header: t('reports.chequeStatement.colDate'), render: row => this.formatDate(row.checkDate) },
        { header: t('reports.chequeStatement.colBeneficiary'), render: row => row.beneficiaryName },
        { header: t('reports.chequeStatement.colAmount'), render: row => `${row.amount} ${row.currency}` },
        {
          header: t('reports.chequeStatement.colType'),
          render: row => row.chequeType === 'Orphans'
            ? t('reports.chequeStatement.typeOrphans')
            : t('reports.chequeStatement.typeIndividuals')
        },
        { header: t('reports.chequeStatement.colBank'), render: row => row.bankName || '-' }
      ],
      rows: this.rows
    };
  }

  // ==================== helpers ====================

  /** Date-order guard (AC 4's client-side half — no CheckFilterDto field is server-required). */
  private validateDates(): boolean {
    const v = this.filterForm.getRawValue();
    if (v.dateFrom && v.dateTo && new Date(v.dateTo) < new Date(v.dateFrom)) {
      this.notification.error(this.translate.instant('reports.chequeStatement.dateOrderInvalid'));
      return false;
    }
    return true;
  }

  /** Short yyyy-MM-dd, dash for empty — the meta band's range format. */
  private formatDate(value?: string | null): string {
    if (!value) {
      return '-';
    }
    const date = new Date(value);
    return isNaN(date.getTime()) ? '-' : date.toISOString().slice(0, 10);
  }

  trackByRow(_index: number, row: CheckListItem): string {
    return row.id;
  }

  trackTotal(_index: number, entry: { key: string; value: number }): string {
    return entry.key;
  }
}
