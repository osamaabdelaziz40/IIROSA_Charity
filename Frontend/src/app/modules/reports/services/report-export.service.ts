import { Injectable } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { saveAs } from 'file-saver';
import type * as ExcelJSNs from 'exceljs';

import { BeneficiaryFamilyRow, CharityPaymentTrackingRow, ExcludedOrphanRow, FamilyProjectRow, MezaCardsRow, MissedPaymentRow, NonRenewedOrphanRow, OrphanDataRow, OrphanFileManifestRow, OrphansMissingFilesRow, OrphansMissingReportsSummaryRow, OrphansMissingReportsDetailRow, OrphanStatusReportRow, ProviderChangeRow, RefusedReportsRow, ReportsAwaitingApprovalRow } from '../models/report.model';
import { BeneficiaryDistributionDetail, SeasonalAidCampaignReport } from '../../seasonal-aid/models/seasonal-aid.model';
import { ReportColumnDef, ReportColumnSet, ReportExportRequest, ReportExportResult, REPORT_EXPORT_REGISTRY, orphanStatusColumns } from '../models/report-columns';
import { AttachmentService } from '../../../core/services/attachment.service';
import { NotificationService } from '../../../core/services/notification.service';

/**
 * EP-18's Excel engine (18-1 landed the builder; 18-41 hardens it into the engine). ExcelJS
 * is imported HERE and nowhere else in the reports module — every report exports through
 * `exportRows` / `exportAll` with its registered column set
 * (`models/report-columns.ts`, the single shape declaration).
 *
 * Sheet contract (§23.U.41): RTL, styled + frozen header, engine-owned serial column, typed
 * cells (dates as dates, numbers as numbers, booleans through the set's labels), autofilter,
 * footer row with the total count, file named `<reportKey>-<yyyyMMdd>.xlsx` via file-saver.
 * `exportAll` walks every page under the endpoint cap — a truncation is DECLARED on a
 * manifest sheet, never silent.
 *
 * Two registered special cases keep their own builders: the 18-24/25 image manifests (images
 * are not grid columns) and the seasonal-aid campaign report (pre-epic precedent — noted,
 * left alone per the story's out-of-scope table).
 */
@Injectable({
  providedIn: 'root'
})
export class ReportExportService {
  constructor(
    private translate: TranslateService,
    private attachmentService: AttachmentService,
    // Review P27 2026-08-26: declares the image-manifest embed budget — see exportOrphanFiles.
    private notification: NotificationService
  ) {}

  // ==================== the engine (18-41) ====================

  /**
   * Exports already-collected rows through the report's registered column set. The legacy
   * per-report methods below are thin wrappers over this one — a single workbook builder
   * serves every report (AC 6).
   */
  async exportRows<T>(
    reportKey: string,
    rows: T[],
    totalCount: number = rows.length,
    options: { fileName?: string; columns?: ReportColumnDef<T>[]; sheetKey?: string } = {}
  ): Promise<void> {
    const columns = this.resolveColumns(reportKey, options.columns);
    const sheetKey = options.sheetKey ?? this.resolveSet(reportKey).sheetKey;
    const { default: ExcelJS } = await import('exceljs');

    const workbook = new ExcelJS.Workbook();
    this.buildDataSheet(workbook, sheetKey, columns, rows, totalCount);
    await this.downloadWorkbook(workbook, options.fileName ?? this.defaultFileName(reportKey));
  }

  /**
   * §23.U.41's paging contract — walks `page … totalPages` under the endpoint cap and
   * appends, so the workbook covers the WHOLE result, not the visible page. The walk stops
   * at `maxRows` (default 5000) or a short delivery and declares the truncation on a
   * manifest sheet (AC 4 — no silent caps).
   */
  async exportAll<T>(request: ReportExportRequest<T>): Promise<ReportExportResult> {
    const cap = request.pageSize ?? 100;
    const maxRows = request.maxRows ?? 5000;

    const collected: T[] = [];
    let totalCount = 0;
    let truncated = false;
    let page = 1;

    for (;;) {
      const result = await request.fetchPage(page, cap);
      totalCount = result.totalCount || 0;
      const items = result.items || [];
      // Review P21 2026-08-26: loop-append, never spread-push — a spread push over a large
      // page blows the argument stack.
      for (const item of items) {
        collected.push(item);
      }

      if (totalCount > 0 && collected.length >= totalCount) {
        break;
      }
      if (!items.length) {
        // Review P21 2026-08-26: an empty page is a STOP either way — declared short only
        // when the server had advertised a larger total; with no total advertised
        // (totalCount missing/0) the empty page IS the natural end, not a truncation.
        if (totalCount > 0) {
          truncated = true;
        }
        break;
      }
      if (collected.length >= maxRows || page >= 200) {
        truncated = true;
        break;
      }
      page++;
    }
    if (collected.length < totalCount) {
      truncated = true;
    }

    if (!collected.length) {
      return { exportedCount: 0, totalCount, truncated: false };
    }

    const columns = this.resolveColumns(request.reportKey, request.columns);
    const sheetKey = request.sheetKey ?? this.resolveSet(request.reportKey).sheetKey;
    const { default: ExcelJS } = await import('exceljs');

    const workbook = new ExcelJS.Workbook();
    // Review P23 2026-08-26: when the source advertises no total (walk-until-empty), the
    // footer prints the WALKED count — never a misleading 0.
    this.buildDataSheet(workbook, sheetKey, columns, collected, totalCount || collected.length);
    if (truncated) {
      this.buildManifestSheet(workbook, request.reportKey, collected.length, totalCount);
    }
    await this.downloadWorkbook(workbook, request.fileName ?? this.defaultFileName(request.reportKey));
    return { exportedCount: collected.length, totalCount, truncated };
  }

  /** The one sheet builder — header, serial, typed cells, autofilter, footer total. */
  private buildDataSheet<T>(
    workbook: ExcelJSNs.Workbook,
    sheetKey: string,
    columns: ReportColumnDef<T>[],
    rows: T[],
    totalCount: number
  ): ExcelJSNs.Worksheet {
    const sheet = workbook.addWorksheet(this.sheetName(sheetKey));
    // RTL + the header row frozen — §23.U.41's sheet contract.
    sheet.views = [{ rightToLeft: true, state: 'frozen', ySplit: 1 }];

    sheet.addRow(['#', ...columns.map(c => this.t(c.i18nLabel))]);
    const headerRow = sheet.getRow(1);
    headerRow.font = { bold: true };
    headerRow.fill = {
      type: 'pattern', pattern: 'solid',
      fgColor: { argb: 'FFF0F0F0' }
    };

    sheet.getColumn(1).width = 6;
    columns.forEach((c, i) => {
      sheet.getColumn(i + 2).width = c.width ?? (c.type === 'text' ? 18 : c.type === 'number' ? 12 : 14);
    });

    rows.forEach((row, index) => {
      const dataRow = sheet.addRow([]);
      dataRow.getCell(1).value = index + 1;
      columns.forEach((c, i) => this.writeCell(dataRow.getCell(i + 2), c, row));
    });

    // Autofilter spans the data rows only — the footer total stays out of the filter.
    sheet.autoFilter = {
      from: { row: 1, column: 1 },
      to: { row: Math.max(rows.length + 1, 1), column: columns.length + 1 }
    };

    const footer = sheet.addRow(['', this.t('reports.export.totalRows'), totalCount]);
    footer.font = { bold: true };
    return sheet;
  }

  /** The truncation declaration (AC 4) — rides the workbook as its own sheet. */
  private buildManifestSheet(
    workbook: ExcelJSNs.Workbook,
    reportKey: string,
    exportedCount: number,
    totalCount: number
  ): void {
    const sheet = workbook.addWorksheet(this.sheetName('reports.export.manifestTitle'));
    sheet.views = [{ rightToLeft: true }];
    sheet.addRow([this.t('reports.export.colReport'), reportKey]);
    sheet.addRow([this.t('reports.export.declaredTotal'), totalCount]);
    sheet.addRow([this.t('reports.export.exportedRows'), exportedCount]);
    sheet.addRow([this.t('reports.export.truncationNotice')]);
    sheet.getColumn(1).width = 26;
    sheet.getColumn(2).width = 30;
    sheet.getRow(1).font = { bold: true };
  }

  /** Typed cells (AC 2) — numbers as numbers, dates as dates, booleans through the set. */
  private writeCell(cell: ExcelJSNs.Cell, def: ReportColumnDef, row: any): void {
    if (def.constantLabel) {
      cell.value = this.t(def.constantLabel);
      return;
    }
    const raw = def.value(row);
    if (raw === null || raw === undefined || raw === '') {
      cell.value = '';
      return;
    }
    switch (def.type) {
      case 'number': {
        // Review P21 2026-08-26: whitespace-only strings must NOT become a numeric 0 —
        // Number(' ') is 0; trim first and let a blank render blank.
        const text = typeof raw === 'string' ? raw.trim() : raw;
        if (text === '') {
          cell.value = '';
          break;
        }
        const n = typeof text === 'number' ? text : Number(text);
        cell.value = Number.isNaN(n) ? String(raw) : n;
        break;
      }
      case 'date': {
        const d = raw instanceof Date ? raw : new Date(String(raw));
        if (isNaN(d.getTime())) {
          cell.value = '';
          break;
        }
        cell.value = d;
        cell.numFmt = def.format === 'datetime' ? 'dd/mm/yyyy hh:mm' : 'dd/mm/yyyy';
        break;
      }
      case 'boolean': {
        if (raw === true) {
          cell.value = def.booleanLabels ? this.t(def.booleanLabels.true) : '✓';
        } else if (raw === false) {
          cell.value = def.booleanLabels ? this.t(def.booleanLabels.false) : '';
        } else {
          cell.value = '';
        }
        break;
      }
      default:
        cell.value = String(raw);
    }
  }

  /**
   * Excel caps sheet names at 31 characters — the translated titles (Arabic especially)
   * can exceed that, which corrupts the workbook. Review P21 2026-08-26: every
   * addWorksheet goes through this slicer.
   */
  private sheetName(i18nKey: string): string {
    const name = this.t(i18nKey).slice(0, 31);
    return name || i18nKey.slice(0, 31);
  }

  private resolveSet(reportKey: string): ReportColumnSet {
    const set = REPORT_EXPORT_REGISTRY[reportKey];
    if (!set) {
      throw new Error(`No export column set registered for '${reportKey}'`);
    }
    return set;
  }

  private resolveColumns<T>(reportKey: string, override?: ReportColumnDef<T>[]): ReportColumnDef<T>[] {
    if (override && override.length) {
      return override;
    }
    const set = this.resolveSet(reportKey);
    if (!set.columns.length) {
      throw new Error(`Column set '${reportKey}' is a registered special case — it exports through its own builder`);
    }
    return set.columns;
  }

  /** §23.U.41's file-name contract: `<reportKey>-<yyyyMMdd>.xlsx`. */
  private defaultFileName(reportKey: string): string {
    const now = new Date();
    const pad = (n: number): string => String(n).padStart(2, '0');
    const stamp = `${now.getFullYear()}${pad(now.getMonth() + 1)}${pad(now.getDate())}`;
    return `${reportKey}-${stamp}.xlsx`;
  }

  /** Writes the workbook and triggers the browser save dialog (file-saver). */
  private async downloadWorkbook(workbook: ExcelJSNs.Workbook, fileName: string): Promise<void> {
    const buffer = await workbook.xlsx.writeBuffer();
    saveAs(new Blob([buffer], {
      type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
    }), fileName);
  }

  // ==================== the per-report wrappers (one code path each) ====================

  /** UC-RPT-01 — the full §23.S.3 workbook (identity, guardian, residence & income, status, education, father, exclusion, audit/org). */
  async exportOrphanData(rows: OrphanDataRow[]): Promise<void> {
    return this.exportRows('orphan-data', rows);
  }

  /** UC-RPT-03 — the §23.S.4 workbook (serial + 6 contract columns). */
  async exportExcludedOrphans(rows: ExcludedOrphanRow[]): Promise<void> {
    return this.exportRows('excluded-orphans', rows);
  }

  /**
   * UC-RPT-04 / UC-RPT-05 — the twin §23.S.5/§23.S.6 workbooks: one registered column set,
   * the wrapper swaps the i18n block (keyPrefix) and the file-name base (filePrefix).
   */
  async exportOrphanStatusReport(rows: OrphanStatusReportRow[], keyPrefix: string, filePrefix: string): Promise<void> {
    // Review P27 2026-08-26: the factory ships FULL i18n keys per twin — the set used to
    // carry bare colOrphanCode fragments that only resolved through this runtime prefixing.
    return this.exportRows('orphan-status', rows, rows.length, {
      columns: orphanStatusColumns(keyPrefix),
      sheetKey: `${keyPrefix}.title`,
      fileName: this.defaultFileName(filePrefix)
    });
  }

  /** UC-RPT-07/08 — the §23.S.8 workbook; fileName carries §23.U.8's bank-file name when given. */
  async exportMezaCards(rows: MezaCardsRow[], fileName?: string): Promise<void> {
    return this.exportRows('meza-cards', rows, rows.length, { fileName });
  }

  /** UC-RPT-09 — the §23.S.13 workbook (one row per distinct assisted family). */
  async exportBeneficiaryFamilies(rows: BeneficiaryFamilyRow[]): Promise<void> {
    return this.exportRows('beneficiary-families', rows);
  }

  /**
   * UC-RPT-11 — the §23.S.7 workbook; the file name carries the charity scope.
   */
  async exportFamilyProjects(rows: FamilyProjectRow[], fileName?: string): Promise<void> {
    return this.exportRows('family-projects', rows, rows.length, { fileName });
  }

  /** UC-RPT-12 — the §23.S.19 workbook; the file name carries the charity scope. */
  async exportProviderChanges(rows: ProviderChangeRow[], fileName?: string): Promise<void> {
    return this.exportRows('provider-changes', rows, rows.length, { fileName });
  }

  /** UC-RPT-15 — the §23.S.14 summary workbook (الجمعيه · العدد). */
  async exportOrphansMissingReportsSummary(rows: OrphansMissingReportsSummaryRow[], fileName?: string): Promise<void> {
    return this.exportRows('orphans-missing-reports-summary', rows, rows.length, { fileName });
  }

  /** UC-RPT-15 — the drill-down workbook for one charity. */
  async exportOrphansMissingReportsDetails(
    rows: OrphansMissingReportsDetailRow[],
    charityScope: string,
    fileName?: string): Promise<void> {
    return this.exportRows('orphans-missing-reports-details', rows, rows.length, {
      fileName: fileName ?? this.defaultFileName(`orphans-missing-reports-${charityScope}`)
    });
  }

  /** UC-RPT-16 — the §23.S.15 workbook (5 screen columns + missing-documents detail). */
  async exportOrphansMissingFiles(rows: OrphansMissingFilesRow[], fileName?: string): Promise<void> {
    return this.exportRows('orphans-missing-files', rows, rows.length, { fileName });
  }

  /** UC-RPT-17 (§23.S.16) — the review-queue workbook; سبب الرفض rides as the sixth column. */
  async exportReportsAwaitingApproval(rows: ReportsAwaitingApprovalRow[], fileName?: string): Promise<void> {
    return this.exportRows('reports-awaiting-approval', rows, rows.length, { fileName });
  }

  /** UC-RPT-18 (§23.S.17) — the refused-worklist workbook; the state column is a constant label. */
  async exportRefusedReports(rows: RefusedReportsRow[], fileName?: string): Promise<void> {
    return this.exportRows('refused-reports', rows, rows.length, { fileName });
  }

  /** UC-RPT-19 (§23.U.19) — the batch chase list. */
  async exportNonRenewedReports(rows: NonRenewedOrphanRow[], fileName?: string): Promise<void> {
    return this.exportRows('non-renewed-reports', rows, rows.length, { fileName });
  }

  /** UC-RPT-20 (§23.S.12) — the tracking sheet; تم رفع الدفعة carries نعم/لا labels. */
  async exportCharityPaymentTracking(rows: CharityPaymentTrackingRow[], fileName?: string): Promise<void> {
    return this.exportRows('charity-payment-tracking', rows, rows.length, { fileName });
  }

  /**
   * UC-RPT-22 (§23.S.11) — the arrears workbook: the registered base set + one boolean column
   * per batch present in the selection (dynamic tail, headers are the batch numbers verbatim).
   */
  async exportMissedPayments(rows: MissedPaymentRow[], batchColumns: string[], fileName?: string): Promise<void> {
    const tail: ReportColumnDef<MissedPaymentRow>[] = batchColumns.map(b => ({
      key: `batch-${b}`,
      i18nLabel: b,
      type: 'boolean',
      width: 14,
      value: row => row.batchStates?.[b],
      booleanLabels: { true: 'reports.missedPayments.received', false: 'reports.missedPayments.missed' }
    }));
    return this.exportRows('missed-payments', rows, rows.length, {
      columns: [...this.resolveSet('missed-payments').columns, ...tail],
      fileName
    });
  }

  // ==================== the registered special cases ====================

  /**
   * UC-RPT-24 / UC-RPT-25 (§23.S.18 صور الأيتام وصور الشهادات) — the shared image-manifest
   * workbook: اسم اليتيم · كود اليتيم · الصوره. Where feasible the الصوره cell EMBEDS the
   * image (bytes fetched through the authenticated attachment endpoint); rows past the embed
   * budget, with a non-image content type, or whose fetch failed fall back to a hyperlink
   * cell — the manifest listing itself stays authoritative, no silent row loss (recorded
   * decision). Images are not grid columns, so this builder stays behind its registry entry.
   */
  async exportOrphanFiles(rows: OrphanFileManifestRow[], fileName?: string, sheetKey?: string): Promise<void> {
    const { default: ExcelJS } = await import('exceljs');

    const workbook = new ExcelJS.Workbook();
    const sheet = workbook.addWorksheet(this.sheetName(sheetKey || 'reports.orphanFiles.title'));
    sheet.views = [{ rightToLeft: true }];

    sheet.addRow([
      '#',
      this.t('reports.orphanFiles.colOrphanName'),
      this.t('reports.orphanFiles.colOrphanCode'),
      this.t('reports.orphanFiles.colPhoto')
    ]);

    sheet.getRow(1).font = { bold: true };
    sheet.getRow(1).fill = {
      type: 'pattern', pattern: 'solid',
      fgColor: { argb: 'FFF0F0F0' }
    };

    sheet.getColumn(1).width = 6;
    sheet.getColumn(2).width = 30;
    sheet.getColumn(3).width = 16;
    sheet.getColumn(4).width = 34;

    // Feasibility budget (recorded): embed at most this many images per workbook — image
    // bytes are fetched serially through the auth'd endpoint and inflate the file.
    const maxEmbedded = 40;
    const candidates = rows
      .filter(r => (r.contentType || '').toLowerCase().startsWith('image/'))
      .slice(0, maxEmbedded);

    const embedded = new Map<string, { base64: string; extension: 'png' | 'jpeg' | 'gif' }>();
    for (const row of candidates) {
      if (embedded.has(row.attachmentId)) {
        continue;
      }
      try {
        const blob = await this.attachmentService.download(row.attachmentId).toPromise();
        if (!blob) {
          continue;
        }
        const base64 = await this.blobToBase64(blob);
        const extension = this.imageExtension(row.contentType!);
        if (base64 && extension) {
          embedded.set(row.attachmentId, { base64, extension });
        }
      } catch {
        // Failed fetch → hyperlink cell below; the row itself never drops.
      }
    }

    let hyperlinkCount = 0;
    rows.forEach((row, i) => {
      sheet.addRow([i + 1, row.orphanName ?? '', row.orphanCode ?? '', '']);
      const excelRow = i + 2;
      const photoCell = sheet.getCell(excelRow, 4);
      const image = embedded.get(row.attachmentId);

      if (image) {
        sheet.getRow(excelRow).height = 52;
        const imageId = workbook.addImage({
          base64: image.base64,
          extension: image.extension
        });
        sheet.addImage(imageId, {
          tl: { col: 3, row: excelRow - 1 },
          ext: { width: 60, height: 60 },
          editAs: 'oneCell'
        });
      } else {
        hyperlinkCount++;
        photoCell.value = {
          text: row.fileName || row.attachmentId,
          // Review P27 2026-08-26: only an absolute http(s) URL passes verbatim — anything
          // else is treated as a root-relative path (leading slash stripped once), so an
          // unexpected relative downloadUrl no longer produces a broken double-slash link.
          hyperlink: /^https?:/i.test(row.downloadUrl)
            ? row.downloadUrl
            : `${window.location.origin}/${String(row.downloadUrl).replace(/^\//, '')}`
        };
        photoCell.font = { color: { argb: 'FF0563C1' }, underline: true };
      }
    });

    // Review P27 2026-08-26: the embed budget is DECLARED, not silent — rows past the
    // 40-image cap (plus non-image types and failed fetches) ride as links; say so.
    if (hyperlinkCount > 0) {
      this.notification.warning(this.t('reports.orphanFiles.hyperlinkFallback'));
    }

    await this.downloadWorkbook(workbook, fileName ?? this.defaultFileName('orphan-files'));
  }

  /**
   * UC-RPT-10 (§23.S.2 campaign report) — the seasonal-aid module's own two-sheet workbook
   * (Summary + Distribution Details). Pre-epic precedent, NOT gridded: noted per 18-41's
   * out-of-scope table and left as-is (unification is maintenance backlog).
   */
  async exportCampaignReport(
    report: SeasonalAidCampaignReport,
    details: BeneficiaryDistributionDetail[],
    fileName?: string
  ): Promise<void> {
    const { default: ExcelJS } = await import('exceljs');

    const workbook = new ExcelJS.Workbook();
    const summary = workbook.addWorksheet(this.sheetName('seasonalAid.campaignReport'));
    summary.views = [{ rightToLeft: true }];

    summary.addRow([this.t('seasonalAid.campaignName'), report.campaignName]);
    summary.addRow([this.t('seasonalAid.campaignType'), report.campaignType]);
    summary.addRow([
      this.t('seasonalAid.period'),
      `${this.formatDate(report.startDate)} - ${this.formatDate(report.endDate)}`
    ]);
    summary.addRow([this.t('seasonalAid.totalBudget'), report.totalBudget, report.budgetCurrency]);
    summary.addRow([this.t('seasonalAid.allocatedAmount'), report.allocatedBudget, report.budgetCurrency]);
    summary.addRow([this.t('seasonalAid.distributedAmount'), report.distributedBudget, report.budgetCurrency]);
    summary.addRow([this.t('seasonalAid.remainingBudget'), report.remainingBudget, report.budgetCurrency]);
    summary.addRow([this.t('seasonalAid.totalBeneficiaries'), report.totalBeneficiaries]);
    summary.addRow([this.t('seasonalAid.distributedBeneficiaries'), report.distributedBeneficiaries]);
    summary.addRow([this.t('seasonalAid.pendingBeneficiaries'), report.pendingBeneficiaries]);
    summary.getRow(1).font = { bold: true };

    const detailsSheet = workbook.addWorksheet(this.sheetName('seasonalAid.distributionDetails'));
    detailsSheet.views = [{ rightToLeft: true }];
    detailsSheet.addRow([
      '#',
      this.t('seasonalAid.familyCode'),
      this.t('seasonalAid.address'),
      this.t('seasonalAid.charity'),
      this.t('seasonalAid.region'),
      this.t('seasonalAid.allocatedAmount'),
      this.t('seasonalAid.distributedAmount'),
      this.t('seasonalAid.currency'),
      this.t('seasonalAid.distributedOn'),
      this.t('seasonalAid.receivedBy')
    ]);
    detailsSheet.getRow(1).font = { bold: true };
    detailsSheet.getRow(1).fill = {
      type: 'pattern', pattern: 'solid',
      fgColor: { argb: 'FFF0F0F0' }
    };

    details.forEach((d, i) => detailsSheet.addRow([
      i + 1,
      d.familyCode ?? '',
      d.familyAddress ?? '',
      d.charityName ?? '',
      d.regionName ?? '',
      d.allocationAmount,
      d.distributedAmount,
      report.budgetCurrency,
      this.formatDate(d.distributionDate),
      d.receivedBy ?? ''
    ]));

    const safeName = (report.campaignName || 'campaign').replace(/[\\/:*?"<>|]/g, '-');
    await this.downloadWorkbook(workbook, fileName ?? `campaign_report_${safeName}.xlsx`);
  }

  // ==================== helpers ====================

  /** The workbook needs the raw base64 (no data-URL prefix) plus an explicit extension. */
  private blobToBase64(blob: Blob): Promise<string> {
    return new Promise(resolve => {
      const reader = new FileReader();
      reader.onloadend = () => {
        const result = String(reader.result || '');
        const marker = 'base64,';
        resolve(result.includes(marker) ? result.substring(result.indexOf(marker) + marker.length) : '');
      };
      reader.onerror = () => resolve('');
      reader.readAsDataURL(blob);
    });
  }

  private imageExtension(contentType: string): 'png' | 'jpeg' | 'gif' | null {
    const type = contentType.toLowerCase();
    if (type === 'image/png') return 'png';
    if (type === 'image/jpeg' || type === 'image/jpg') return 'jpeg';
    if (type === 'image/gif') return 'gif';
    return null;
  }

  private formatDate(value: string | null | undefined): string {
    if (!value) return '';
    const date = new Date(value);
    return isNaN(date.getTime()) ? '' : date.toLocaleDateString('en-GB');
  }

  private t(key: string): string {
    return this.translate.instant(key);
  }
}
