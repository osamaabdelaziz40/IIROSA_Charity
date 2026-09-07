import { Component, OnInit, OnDestroy, ChangeDetectionStrategy, ChangeDetectorRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { firstValueFrom } from 'rxjs';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { FamilyService } from '../../families/services/family.service';
import { CharityService } from '../../charities/services/charity.service';
import { NotificationService } from '../../../core/services/notification.service';
import { AuthService } from '../../../core/services/auth.service';
import { FamilyFollowUpRow } from '../../families/models/family.model';
import { ReportService } from '../services/report.service';
import { ReportExportService } from '../services/report-export.service';
import { ReportPdfService, ReportPreviewSource, ReportSheetConfig } from '../services/report-pdf.service';
import { ReportPreviewComponent } from '../report-viewer/report-preview/report-preview.component';
import {
  GuardianIdentificationRow,
  GuardianIdentificationSheet,
  GuardianIdentificationVariant,
  ReportSheetPayload
} from '../models/report.model';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';

/**
 * Family follow-up report (UC-FAM-11 متابعة إدخالات الأسر).
 *
 * One calendar day of register activity: family files created or updated on the date, who made
 * the change and when — a read-only projection over the audit columns. HQ roles (SuperAdmin/
 * Admin) can narrow to one charity; a Charity-role caller is scoped server-side to its own
 * register. Runs on demand (Run button); the empty state renders only after a run.
 *
 * UC-FAM-14 طباعة كشوف المتابعة: the print action beside the Excel export produces the
 * tracking sheet. UC-RPT-37 (§23.S.10's deferred command طباعة الاستبانة من تاريخ محدد):
 * the identification-sheet action prints تعريف العائل / الأرامل / أسرة محددة through the
 * variant-keyed POST /api/Reports/guardian-identification-sheets — superseding the legacy
 * per-kind /export/pdf pair (recorded). All sheets are PRINTED through the browser
 * (client-side print ruling — no server PDF pipeline).
 */
@Component({
  selector: 'app-family-follow-up-report',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, FormsModule, TranslateModule, PageHeaderComponent, PaginationComponent, ReportPreviewComponent],
  templateUrl: './family-follow-up-report.component.html',
  styleUrls: ['./family-follow-up-report.component.scss']
})
export class FamilyFollowUpReportComponent implements OnInit, OnDestroy {
  rows: FamilyFollowUpRow[] = [];
  loading = false;
  exporting = false;
  hasRun = false;

  // Filter bar state — the date defaults to today; the report runs on demand.
  reportDate = new Date().toISOString().slice(0, 10);
  charityFilter = 'all';
  charityOptions: { id: string; name: string }[] = [];
  isHQ = false;

  // Print state (UC-FAM-14 + UC-RPT-37): one in-flight flag for the sheet actions.
  printing = false;

  // UC-RPT-37 identification-sheet state — variant + the single-family dropdown.
  idVariant: GuardianIdentificationVariant = 'AllGuardians';
  familyFilter = '';
  familyOptions: { id: string; name: string }[] = [];
  private familiesLoaded = false;

  // Paging.
  currentPage = 1;
  pageSize = 10;
  totalCount = 0;

  // Review P20 2026-08-26: OnPush + subscribe callbacks that never mark for check left the
  // grid stale until some other interaction; and the screen had no destroy$ — subscriptions
  // outlived navigation. Both live here now.
  private readonly destroy$ = new Subject<void>();

  constructor(
    private familyService: FamilyService,
    private charityService: CharityService,
    private reportService: ReportService,
    private reportExportService: ReportExportService,
    private reportPdfService: ReportPdfService,
    private notification: NotificationService,
    private auth: AuthService,
    private translate: TranslateService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.isHQ = this.auth.hasAnyRole(['SuperAdmin', 'Admin']);
    if (this.isHQ) {
      this.loadCharityOptions();
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  run(): void {
    if (this.loading) {
      return;
    }
    if (!this.reportDate) {
      this.notification.error(this.getTranslation('families.followUp.dateRequired'));
      return;
    }

    this.loading = true;
    this.familyService
      .getFollowUp({
        date: this.reportDate,
        charityId: this.isHQ && this.charityFilter !== 'all' ? this.charityFilter : undefined,
        pageNumber: this.currentPage,
        pageSize: this.pageSize
      })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => {
          this.rows = result.items || [];
          this.totalCount = result.totalCount || 0;
          this.loading = false;
          this.hasRun = true;
          this.cdr.markForCheck();
        },
        error: (error: any) => {
          this.loading = false;
          console.error('Error loading follow-up activity:', error);
          this.notification.error(
            error?.error?.message || error?.message || this.getTranslation('families.followUp.loadFailed')
          );
          this.cdr.markForCheck();
        }
      });
  }

  onCharityChange(): void {
    // A new scope starts from the first page; the report re-runs on demand.
    this.currentPage = 1;
    if (this.hasRun) {
      this.run();
    }
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.run();
  }

  isCreated(row: FamilyFollowUpRow): boolean {
    return row.changeKind === 'Created';
  }

  /**
   * §23.U.41 استخراج البيانات — the day's activity through 18-41's generic engine (this screen
   * predates the 18-1 shell, so the button stays here while the workbook itself is engine-built
   * from the registered 'family-follow-up' column set): every page under the endpoint's cap
   * walked server-side, not just the loaded grid page. No server export endpoint in this story.
   */
  exportToExcel(): void {
    if (this.exporting || this.rows.length === 0) {
      return;
    }
    this.exporting = true;

    // Review P21 2026-08-26: the filter is snapshotted at press time — the walk is async and
    // re-reading live bar state mid-export would stitch pages of different scopes together.
    const date = this.reportDate;
    const charityId = this.isHQ && this.charityFilter !== 'all' ? this.charityFilter : undefined;

    this.reportExportService
      .exportAll<FamilyFollowUpRow>({
        reportKey: 'family-follow-up',
        fetchPage: (page, pageSize) => firstValueFrom(
          this.familyService.getFollowUp({
            date,
            charityId,
            pageNumber: page,
            pageSize
          })
        )
      })
      .then(result => {
        if (result.exportedCount === 0) {
          this.notification.info(this.getTranslation('reports.nothingToExport'));
        }
      })
      .catch((error: any) => {
        console.error('Excel export failed:', error);
        this.notification.error(this.getTranslation('reports.export.failed'));
      })
      .finally(() => {
        this.exporting = false;
      });
  }

  private loadCharityOptions(): void {
    this.charityService.getCharities({ pageNumber: 1, pageSize: 500 })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: any) => {
          this.charityOptions = [
            { id: 'all', name: this.getTranslation('families.followUp.allCharities') },
            ...(response.items || []).map((c: any) => ({ id: c.id, name: c.name }))
          ];
          // Review P26 2026-08-26: a capped lookup says so — a silently truncated dropdown hides choices.
          if ((response.totalCount || 0) > (response.items || []).length) {
            this.notification.warning(this.getTranslation('reports.lookup.truncated'));
          }
          this.cdr.markForCheck();
        },
        error: (error: any) => console.error('Error loading charities:', error)
      });
  }

  // ==================== UC-FAM-14: printable sheets (طباعة كشوف المتابعة) ====================

  /** The tracking sheet for the chosen date (whole selection — never just the loaded page). */
  printTrackingSheet(): void {
    if (this.printing) {
      return;
    }
    if (!this.reportDate) {
      this.notification.error(this.getTranslation('families.followUp.dateRequired'));
      return;
    }

    this.printing = true;
    this.reportService
      .exportSheet<FamilyFollowUpRow>('family-update-tracking', {
        date: this.reportDate,
        charityId: this.isHQ && this.charityFilter !== 'all' ? this.charityFilter : undefined
      })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: payload => {
          this.printing = false;
          this.cdr.markForCheck();
          this.printSheet(payload);
        },
        error: (error: any) => {
          this.printing = false;
          this.cdr.markForCheck();
          console.error('Error printing tracking sheet:', error);
          this.notification.error(
            error?.error?.message || error?.message || this.getTranslation('reports.print.failed')
          );
        }
      });
  }

  /**
   * UC-RPT-37 (§23.S.10's deferred command) — طباعة الاستبانة من تاريخ محدد: the
   * identification sheet for the chosen variant (تعريف العائل / الأرامل / أسرة محددة) over
   * the screen's charity scope and date, rendered through 18-21's print pipeline. AC 4: an
   * empty selection refuses with the nothing-to-produce message; AC 7: أسرة محددة without a
   * family is refused client-side (the server 400s again regardless).
   */
  printIdentificationSheet(): void {
    if (this.printing) {
      return;
    }
    if (this.idVariant === 'SingleFamily' && !this.familyFilter) {
      this.notification.error(this.getTranslation('reports.guardianIdentification.missingFamily'));
      return;
    }

    this.printing = true;
    this.reportService
      .getGuardianIdentificationSheets({
        variant: this.idVariant,
        charityId: this.isHQ && this.charityFilter !== 'all' ? this.charityFilter : undefined,
        date: this.idVariant === 'SingleFamily' ? undefined : this.reportDate || undefined,
        familyId: this.idVariant === 'SingleFamily' ? this.familyFilter : undefined
      })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: sheet => {
          this.printing = false;
          this.cdr.markForCheck();
          if (!sheet.rows?.length) {
            this.notification.info(
              sheet.message || this.getTranslation('reports.guardianIdentification.nothingToPrint'));
            return;
          }
          // Review P22 2026-08-26: the server caps ID sheets at its row ceiling — a partial
          // sheet warns (same contract as the tracking sheet), never silently truncates.
          if (sheet.truncated) {
            this.notification.warning(this.getTranslation('reports.print.truncated'));
          }
          this.renderIdentificationSheet(sheet);
        },
        error: (error: any) => {
          this.printing = false;
          this.cdr.markForCheck();
          console.error('Error printing identification sheets:', error);
          this.notification.error(
            error?.message || this.getTranslation('reports.guardianIdentification.printFailed')
          );
        }
      });
  }

  /** The shared §23.U.40 preview modal, hosted directly (this screen predates the 18-1 shell). */
  @ViewChild('identificationPreview') identificationPreview!: ReportPreviewComponent;

  /**
   * §23.U.40 عرض — the identification sheet for the current variant + date, fetched at press
   * time (the sheet is not grid state) and opened in the SHARED preview modal, hosted here
   * directly because this screen predates the 18-1 shell. Guards mirror
   * printIdentificationSheet (أسرة محددة needs a family; an empty selection refuses).
   */
  previewIdentificationSheet(): void {
    if (this.printing) {
      return;
    }
    if (this.idVariant === 'SingleFamily' && !this.familyFilter) {
      this.notification.error(this.getTranslation('reports.guardianIdentification.missingFamily'));
      return;
    }

    firstValueFrom(this.reportService.getGuardianIdentificationSheets({
      variant: this.idVariant,
      charityId: this.isHQ && this.charityFilter !== 'all' ? this.charityFilter : undefined,
      date: this.idVariant === 'SingleFamily' ? undefined : this.reportDate || undefined,
      familyId: this.idVariant === 'SingleFamily' ? this.familyFilter : undefined
    }))
      .then(sheet => {
        if (!sheet.rows?.length) {
          this.notification.info(
            sheet.message || this.getTranslation('reports.guardianIdentification.nothingToPrint'));
          return;
        }
        // Review P22 2026-08-26: preview mirrors print — a capped sheet warns here too.
        if (sheet.truncated) {
          this.notification.warning(this.getTranslation('reports.print.truncated'));
        }
        this.identificationPreview.open(
          this.reportPdfService.sheetSource(this.buildIdentificationSheetConfig(sheet)));
      })
      .catch(() => this.notification.error(
        this.getTranslation('reports.guardianIdentification.printFailed')));
  }

  /** The identification sheet — the variant's column set, A4 portrait RTL via printSheet. */
  private renderIdentificationSheet(sheet: GuardianIdentificationSheet): void {
    this.reportPdfService.printSheet(this.buildIdentificationSheetConfig(sheet));
  }

  /** The identification sheet's config — shared by the print command and the §23.U.40 preview. */
  private buildIdentificationSheetConfig(sheet: GuardianIdentificationSheet): ReportSheetConfig<GuardianIdentificationRow> {
    const t = (key: string): string => this.getTranslation(key);
    const variantLabel = this.idVariant === 'AllGuardians'
      ? t('reports.guardianIdentification.variantAllGuardians')
      : this.idVariant === 'WidowsOnly'
        ? t('reports.guardianIdentification.variantWidowsOnly')
        : t('reports.guardianIdentification.variantSingleFamily');
    const charityLabel = this.isHQ && this.charityFilter !== 'all'
      ? (this.charityOptions.find(c => c.id === this.charityFilter)?.name
        || t('families.followUp.allCharities'))
      : t('families.followUp.allCharities');

    const serial = {
      header: t('reports.guardianIdentification.colSerial'),
      render: (_r: GuardianIdentificationRow, i: number) => String(i + 1)
    };
    const guardianName = {
      header: t('reports.guardianIdentification.colGuardianName'),
      render: (r: GuardianIdentificationRow) => r.guardianName ?? r.widowName ?? '-'
    };
    const nationalId = {
      header: t('reports.guardianIdentification.colNationalId'),
      render: (r: GuardianIdentificationRow) => r.nationalId ?? '-'
    };
    const phone = {
      header: t('reports.guardianIdentification.colPhone'),
      render: (r: GuardianIdentificationRow) => r.phone ?? '-'
    };
    const relationship = {
      header: t('reports.guardianIdentification.colRelationship'),
      render: (r: GuardianIdentificationRow) => r.relationshipToFamily ?? '-'
    };
    const job = {
      header: t('reports.guardianIdentification.colJob'),
      render: (r: GuardianIdentificationRow) => r.job ?? '-'
    };
    const birthDate = {
      header: t('reports.guardianIdentification.colBirthDate'),
      render: (r: GuardianIdentificationRow) => this.formatSheetDate(r.birthDate)
    };
    const familyCode = {
      header: t('reports.guardianIdentification.colFamilyCode'),
      render: (r: GuardianIdentificationRow) => r.familyCode ?? '-'
    };
    const charityName = {
      header: t('reports.guardianIdentification.colCharity'),
      render: (r: GuardianIdentificationRow) => r.charityName ?? '-'
    };

    // The variant's column contract (§23.U.37): guardians carry relationship + job; widows
    // carry birth date; أسرة محددة is the union grouped under the family code.
    const columns = this.idVariant === 'WidowsOnly'
      ? [serial, guardianName, nationalId, phone, birthDate, familyCode, charityName]
      : [serial, guardianName, nationalId, phone, relationship, job, familyCode, charityName];

    const sheetConfig: ReportSheetConfig<GuardianIdentificationRow> = {
      documentTitle: `${t('reports.guardianIdentification.title')} - ${variantLabel}`,
      title: t('reports.guardianIdentification.title'),
      subtitle: variantLabel,
      meta: [
        { label: t('families.followUp.colCharity'), value: charityLabel },
        {
          label: t('common.date'),
          value: this.idVariant === 'SingleFamily' ? '-' : this.reportDate
        },
        {
          label: t('reports.guardianIdentification.metaCount'),
          value: String(sheet.totalCount)
        },
        {
          label: t('reports.guardianIdentification.metaProducedBy'),
          value: sheet.producedBy || '-'
        }
      ],
      columns,
      rows: sheet.rows
    };

    return sheetConfig;
  }

  /** أسرة محددة needs the family register — loaded lazily on first switch, labelled by code. */
  onIdVariantChange(): void {
    if (this.idVariant === 'SingleFamily' && !this.familiesLoaded) {
      this.loadFamilyOptions();
    }
  }

  private loadFamilyOptions(): void {
    this.familyService
      .getFamilies({ pageNumber: 1, pageSize: 500 })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => {
          this.familiesLoaded = true;
          this.familyOptions = (result.items || []).map(f => ({ id: f.id, name: f.code }));
          // Review P26 2026-08-26: a capped lookup says so — a silently truncated dropdown hides choices.
          if ((result.totalCount || 0) > (result.items || []).length) {
            this.notification.warning(this.getTranslation('reports.lookup.truncated'));
          }
          this.cdr.markForCheck();
        },
        error: (error: any) => console.error('Error loading families:', error)
      });
  }

  /** Short yyyy-MM-dd, dash for empty. */
  private formatSheetDate(value?: string | null): string {
    if (!value) {
      return '-';
    }
    const date = new Date(value);
    return isNaN(date.getTime()) ? '-' : date.toISOString().slice(0, 10);
  }

  /**
   * Render the sheet in a print window and hand it to the browser (print-to-PDF) — the
   * client-side print ruling recorded across epics 9/10/18 (the browser shapes Arabic
   * natively; 18-21 owns any future jsPDF path). Row values are escaped — user data never
   * enters the document as markup.
   */
  private printSheet(payload: ReportSheetPayload<any>): void {
    const win = window.open('', '_blank', 'width=1000,height=700');
    if (!win) {
      this.notification.error(this.getTranslation('reports.print.popupBlocked'));
      return;
    }

    const esc = (value: unknown): string =>
      String(value ?? '')
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;');

    const titleKey = 'reports.print.titleTracking';

    const headers = [
      this.getTranslation('families.followUp.colCode'),
      this.getTranslation('families.followUp.colHead'),
      this.getTranslation('families.followUp.colCharity'),
      this.getTranslation('families.followUp.colKind'),
      this.getTranslation('families.followUp.colChangedBy'),
      this.getTranslation('families.followUp.colChangedOn')
    ];

    const bodyRows = payload.rows.map((row: any) => {
      const kindKey = row.changeKind === 'Created'
        ? 'families.followUp.kindCreated'
        : 'families.followUp.kindUpdated';
      return `<tr><td>${esc(row.code)}</td><td>${esc(row.headOfFamily)}</td><td>${esc(row.charityName)}</td>` +
        `<td>${esc(this.getTranslation(kindKey))}</td><td>${esc(row.changedBy)}</td>` +
        `<td>${esc(new Date(row.changedOn).toLocaleString())}</td></tr>`;
    }).join('');

    const charityLabel = payload.charityName || this.getTranslation('families.followUp.allCharities');
    const dateLabel = this.reportDate;

    // The server caps a sheet at its row ceiling; a truncated document is partial — the
    // warning rides on the paper itself (and the toast), never silently.
    const truncatedNotice = payload.truncated
      ? `<div class="truncated-notice">${esc(this.getTranslation('reports.print.truncated'))}</div>`
      : '';
    if (payload.truncated) {
      this.notification.warning(this.getTranslation('reports.print.truncated'));
    }

    win.document.write(`<!DOCTYPE html>
<html dir="rtl" lang="ar">
<head>
<meta charset="utf-8">
<title>${esc(this.getTranslation(titleKey))}</title>
<style>
  body { font-family: 'Segoe UI', Tahoma, Arial, sans-serif; direction: rtl; margin: 24px; color: #222; }
  h2 { margin: 0 0 4px; font-size: 20px; }
  .meta { color: #555; margin-bottom: 16px; font-size: 13px; }
  .truncated-notice { background: #fff3cd; border: 1px solid #d4a017; color: #7a5c00; padding: 8px 12px; margin-bottom: 12px; font-size: 13px; font-weight: 600; }
  table { width: 100%; border-collapse: collapse; }
  th, td { border: 1px solid #999; padding: 6px 8px; text-align: right; font-size: 13px; }
  th { background: #f0f0f0; }
  thead { display: table-header-group; }
  tr { page-break-inside: avoid; }
  @page { size: A4; margin: 12mm; }
</style>
</head>
<body>
<h2>${esc(this.getTranslation(titleKey))}</h2>
${truncatedNotice}
<div class="meta">${esc(charityLabel)} &middot; ${esc(this.getTranslation('common.date'))}: ${esc(dateLabel)} &middot; ${esc(this.getTranslation('reports.print.generatedOn'))}: ${esc(new Date(payload.generatedOn).toLocaleString())}</div>
<table>
<thead><tr>${headers.map(h => `<th>${esc(h)}</th>`).join('')}</tr></thead>
<tbody>${bodyRows}</tbody>
</table>
</body>
</html>`);
    win.document.close();
    win.focus();
    win.print();
  }

  trackByRow(index: number, item: FamilyFollowUpRow): string {
    return item.familyId;
  }

  trackByOption(index: number, item: { id: string; name: string }): string {
    return item.id;
  }

  private getTranslation(key: string, params?: any): string {
    return this.translate.instant(key, params);
  }
}
