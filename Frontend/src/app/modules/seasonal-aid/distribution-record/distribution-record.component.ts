import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { SeasonalAidService } from '../services/seasonal-aid.service';
import { NotificationService } from '../../../core/services/notification.service';
import {
  SeasonalAidCampaign,
  SeasonalAidBeneficiary,
  SeasonalAidDistribution
} from '../models/seasonal-aid.model';
import { SharedModule } from '../../../shared/shared.module';
import { ReportPdfService, ReportSheetConfig } from '../../reports/services/report-pdf.service';

@Component({
  selector: 'app-distribution-record',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule, SharedModule],
  templateUrl: './distribution-record.component.html',
  styleUrls: ['./distribution-record.component.scss']
})
export class DistributionRecordComponent implements OnInit {
  campaign: SeasonalAidCampaign | null = null;
  beneficiaries: SeasonalAidBeneficiary[] = [];
  distributions: SeasonalAidDistribution[] = [];
  loading = false;
  saving = false;
  printing = false;

  // 18-34 (UC-RPT-34): which distribution sheet is in flight ('cards' | 'primary' |
  // 'secondary'; null = idle) — drives the three commands' spinners.
  sheetPrinting: 'cards' | 'primary' | 'secondary' | null = null;

  distributionForm: FormGroup;
  currentBeneficiary: SeasonalAidBeneficiary | null = null;
  showDistributionForm = false;

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private route: ActivatedRoute,
    private seasonalAidService: SeasonalAidService,
    private reportPdfService: ReportPdfService,
    private notification: NotificationService,
    private translate: TranslateService
  ) {
    this.distributionForm = this.createDistributionForm();
  }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.loadCampaign(params['id']);
    });
  }

  createDistributionForm(): FormGroup {
    return this.fb.group({
      distributionDate: [new Date().toISOString().slice(0, 10), Validators.required],
      amountDistributed: [null, [Validators.required, Validators.min(0.01)]],
      receivedBy: ['', [Validators.required, Validators.maxLength(100)]],
      recipientRelationship: [''],
      distributionMethod: ['Cash'],
      notes: ['']
    });
  }

  loadCampaign(id: string): void {
    this.loading = true;
    this.seasonalAidService.getCampaignById(id).subscribe({
      next: data => {
        this.campaign = data;
        this.loading = false;
        this.loadBeneficiaries(id);
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  loadBeneficiaries(id: string): void {
    this.seasonalAidService.getCampaignBeneficiaries(id, { pageNumber: 1, pageSize: 500, sortDescending: false }).subscribe({
      next: result => {
        this.beneficiaries = result.items || [];
        this.rebuildDistributions();
      }
    });
  }

  /**
   * There is no campaign-wide distributions endpoint — the list is derived from the
   * beneficiaries' own distribution fields, so it stays in sync with what the server holds.
   */
  private rebuildDistributions(): void {
    this.distributions = this.beneficiaries
      .filter(b => b.isDistributed && b.distributionDate)
      .map(b => ({
        id: b.id,
        beneficiaryId: b.id,
        familyCode: b.familyCode,
        campaignName: this.campaign?.name || '',
        isDistributed: true,
        distributionDate: b.distributionDate!,
        amountDistributed: b.distributedAmount,
        currency: b.currency,
        receivedBy: b.receivedBy,
        recipientRelationship: null,
        notes: b.notes,
        signatureImageUrl: null,
        attachmentId: null,
        distributionMethod: null,
        distributorName: null,
        distributorRole: null,
        createdOn: b.distributionDate!,
        createdBy: null
      }));
  }

  onRecordDistribution(beneficiary: SeasonalAidBeneficiary): void {
    this.currentBeneficiary = beneficiary;
    this.showDistributionForm = true;

    this.distributionForm.patchValue({
      distributionDate: new Date().toISOString().slice(0, 10),
      amountDistributed: beneficiary.allocationAmount,
      receivedBy: beneficiary.receivedBy || '',
      recipientRelationship: '',
      distributionMethod: 'Cash',
      notes: ''
    });
  }

  onSubmitDistribution(): void {
    if (this.distributionForm.invalid || !this.campaign || !this.currentBeneficiary) {
      this.distributionForm.markAllAsTouched();
      return;
    }

    this.saving = true;
    const formValue = this.distributionForm.value;

    this.seasonalAidService.recordDistribution(this.currentBeneficiary.id, {
      beneficiaryId: this.currentBeneficiary.id,
      distributionDate: formValue.distributionDate,
      amountDistributed: formValue.amountDistributed,
      currency: this.campaign.budgetCurrency,
      receivedBy: formValue.receivedBy,
      recipientRelationship: formValue.recipientRelationship || null,
      distributionMethod: formValue.distributionMethod || null,
      notes: formValue.notes || null
    }).subscribe({
      next: () => {
        this.saving = false;
        this.showDistributionForm = false;
        this.notification.success(this.translate.instant('seasonalAid.distributionRecorded'));
        this.loadBeneficiaries(this.campaign!.id);
      },
      error: () => {
        this.saving = false;
        this.notification.error(this.translate.instant('seasonalAid.distributionRecordFailed'));
      }
    });
  }

  onCancelDistribution(): void {
    this.showDistributionForm = false;
    this.currentBeneficiary = null;
    this.distributionForm.reset({
      distributionDate: new Date().toISOString().slice(0, 10),
      amountDistributed: null,
      receivedBy: '',
      recipientRelationship: '',
      distributionMethod: 'Cash',
      notes: ''
    });
  }

  /**
   * UC-PRJ-13 — print a delivery receipt (إيذون استلام) for one distribution.
   * jsPDF is not a dependency, so the receipt is a print-optimized HTML window.
   */
  printDistribution(distribution: SeasonalAidDistribution): void {
    const t = this.translate;
    const rows: Array<[string, string]> = [
      [t.instant('seasonalAid.campaignName'), distribution.campaignName],
      [t.instant('seasonalAid.familyCode'), distribution.familyCode],
      [t.instant('seasonalAid.distributionDate'), new Date(distribution.distributionDate).toLocaleDateString()],
      [t.instant('seasonalAid.amountDistributed'), `${Number(distribution.amountDistributed).toLocaleString()} ${distribution.currency}`],
      [t.instant('seasonalAid.receivedBy'), distribution.receivedBy || '—']
    ];

    const html = `
      <!DOCTYPE html>
      <html dir="rtl" lang="ar">
        <head>
          <meta charset="utf-8">
          <title>${t.instant('seasonalAid.distributionReceipt')}</title>
          <style>
            body { font-family: 'Segoe UI', Tahoma, sans-serif; direction: rtl; padding: 24px; color: #222; }
            h2 { text-align: center; margin-bottom: 4px; }
            .sub { text-align: center; color: #666; margin-bottom: 24px; }
            table { width: 100%; border-collapse: collapse; }
            td { border: 1px solid #ccc; padding: 8px 12px; }
            td.label { background: #f5f5f5; width: 35%; font-weight: 600; }
            .signature { margin-top: 48px; display: flex; justify-content: space-between; }
            .signature div { border-top: 1px solid #333; padding-top: 6px; width: 40%; text-align: center; }
          </style>
        </head>
        <body>
          <h2>${t.instant('seasonalAid.distributionReceipt')}</h2>
          <div class="sub">${t.instant('seasonalAid.distributionReceiptSub')}</div>
          <table>
            ${rows.map(([label, value]) => `<tr><td class="label">${label}</td><td>${value ?? ''}</td></tr>`).join('')}
          </table>
          <div class="signature">
            <div>${t.instant('seasonalAid.recipientSignature')}</div>
            <div>${t.instant('seasonalAid.distributorSignature')}</div>
          </div>
        </body>
      </html>`;

    const printWindow = window.open('', '_blank', 'width=800,height=600');
    if (!printWindow) {
      this.notification.error(this.translate.instant('seasonalAid.printBlocked'));
      return;
    }
    printWindow.document.write(html);
    printWindow.document.close();
    printWindow.focus();
    printWindow.print();
  }

  // ==================== 18-34 DISTRIBUTION SHEETS (UC-RPT-34) ====================

  /**
   * كروت الأسر — the campaign's family cards, one per beneficiary, cut-line card grid
   * (18-31's pattern). Data rides the screen's own beneficiaries load (the EXISTING
   * GET /campaigns/{id}/beneficiaries read — no new endpoint, no refetch). Empty campaign ⇒
   * nothing-to-produce message, no document (AC 3).
   */
  printFamilyCards(): void {
    if (!this.guardSheet('cards')) {
      return;
    }
    const t = (key: string) => this.translate.instant(key);
    const campaign = this.campaign!;

    this.reportPdfService.printCardSheet({
      documentTitle: `${t('seasonalAid.sheets.cardsTitle')} - ${campaign.name}`,
      title: t('seasonalAid.sheets.cardsTitle'),
      subtitle: campaign.name,
      meta: this.sheetMeta(),
      fields: [
        { header: t('seasonalAid.sheets.colFamilyCode'), render: row => row.familyCode },
        {
          header: t('seasonalAid.sheets.colRegionCenter'),
          render: row => `${row.regionName || '-'} / ${row.centerName || '-'}`
        },
        { header: t('seasonalAid.sheets.colOrphans'), render: row => String(row.orphansCount) },
        {
          header: t('seasonalAid.sheets.colValue'),
          render: row => `${row.allocationAmount} ${row.currency}`
        },
        { header: t('seasonalAid.sheets.colCharity'), render: row => row.charityName || '-' }
      ],
      signatureLabel: t('seasonalAid.sheets.recipientSignature'),
      rows: this.beneficiaries
    });
    this.sheetPrinting = null;
  }

  /** كشف التوزيع الأساسي — one row per beneficiary, the sheet officers carry on distribution day. */
  printPrimaryList(): void {
    if (!this.guardSheet('primary')) {
      return;
    }
    this.reportPdfService.printSheet(this.buildListSheet(
      this.translate.instant('seasonalAid.sheets.primaryTitle'),
      [...this.beneficiaries]
    ));
    this.sheetPrinting = null;
  }

  /**
   * كشف التوزيع الثانوي — the same rows grouped by the geography the campaign's data
   * expresses (region → center; the read carries no distribution-batch column — recorded
   * in the audit). Sorted, not aggregated: one row per beneficiary, grouped visually by
   * the sort order.
   */
  printSecondaryList(): void {
    if (!this.guardSheet('secondary')) {
      return;
    }
    const grouped = [...this.beneficiaries].sort((a, b) =>
      (a.regionName || '').localeCompare(b.regionName || '', 'ar')
      || (a.centerName || '').localeCompare(b.centerName || '', 'ar')
      || a.familyCode.localeCompare(b.familyCode, 'ar'));
    this.reportPdfService.printSheet(this.buildListSheet(
      this.translate.instant('seasonalAid.sheets.secondaryTitle'),
      grouped
    ));
    this.sheetPrinting = null;
  }

  /** Shared guard — refuses while another sheet is in flight or the campaign has no rows. */
  private guardSheet(which: 'cards' | 'primary' | 'secondary'): boolean {
    if (this.sheetPrinting || !this.campaign) {
      return false;
    }
    if (!this.beneficiaries.length) {
      this.notification.info(this.translate.instant('seasonalAid.sheets.nothingToPrint'));
      return false;
    }
    this.sheetPrinting = which;
    return true;
  }

  /** The meta band every sheet shares — campaign, aid type, period, count + total. */
  private sheetMeta(): Array<{ label: string; value: string }> {
    const t = (key: string) => this.translate.instant(key);
    const campaign = this.campaign!;
    const total = this.beneficiaries.reduce((sum, b) => sum + (b.allocationAmount || 0), 0);
    const currency = this.beneficiaries[0]?.currency || campaign.budgetCurrency;
    return [
      { label: t('seasonalAid.sheets.metaAidType'), value: campaign.campaignType || '-' },
      {
        label: t('seasonalAid.sheets.metaPeriod'),
        value: `${this.formatSheetDate(campaign.startDate)} — ${this.formatSheetDate(campaign.endDate)}`
      },
      {
        label: t('seasonalAid.sheets.metaTotal'),
        value: `${t('seasonalAid.sheets.metaCount')}: ${this.beneficiaries.length} — `
          + `${t('seasonalAid.sheets.metaAmount')}: ${total} ${currency}`
      }
    ];
  }

  /** The two list sheets differ only in title and row order — one builder serves both. */
  private buildListSheet(title: string, rows: SeasonalAidBeneficiary[]): ReportSheetConfig<SeasonalAidBeneficiary> {
    const t = (key: string) => this.translate.instant(key);
    return {
      documentTitle: `${title} - ${this.campaign?.name}`,
      title,
      subtitle: this.campaign?.name || '',
      meta: this.sheetMeta(),
      columns: [
        { header: t('seasonalAid.sheets.colSerial'), render: (_row, index) => String(index + 1) },
        { header: t('seasonalAid.sheets.colFamilyCode'), render: row => row.familyCode },
        { header: t('seasonalAid.sheets.colRegion'), render: row => row.regionName || '-' },
        { header: t('seasonalAid.sheets.colCenter'), render: row => row.centerName || '-' },
        {
          header: t('seasonalAid.sheets.colValue'),
          render: row => `${row.allocationAmount} ${row.currency}`
        },
        // Blank by design — the officers sign the paper on distribution day.
        { header: t('seasonalAid.sheets.colSignature'), render: () => '' }
      ],
      rows
    };
  }

  /** Short yyyy-MM-dd for the meta band; dash for empty. */
  private formatSheetDate(value?: string | null): string {
    if (!value) {
      return '-';
    }
    const date = new Date(value);
    return isNaN(date.getTime()) ? '-' : date.toISOString().slice(0, 10);
  }

  getPendingBeneficiaries(): SeasonalAidBeneficiary[] {
    return this.beneficiaries.filter(b => !b.isDistributed);
  }

  getDistributedBeneficiaries(): SeasonalAidBeneficiary[] {
    return this.beneficiaries.filter(b => b.isDistributed);
  }

  onBackToCampaign(): void {
    if (this.campaign) {
      this.router.navigate(['/seasonal-aid', this.campaign.id]);
    }
  }

  getTotalDistributed(): number {
    return this.distributions.reduce((sum, d) => sum + (d.amountDistributed || 0), 0);
  }

  getDistributionProgress(): number {
    if (this.beneficiaries.length === 0) return 0;
    return (this.getDistributedBeneficiaries().length / this.beneficiaries.length) * 100;
  }

  trackByBeneficiary(index: number, beneficiary: SeasonalAidBeneficiary): string {
    return beneficiary.id;
  }

  trackByDistribution(index: number, distribution: SeasonalAidDistribution): string {
    return distribution.id;
  }
}
