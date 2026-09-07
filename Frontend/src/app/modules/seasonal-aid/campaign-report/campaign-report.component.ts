import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { SeasonalAidService } from '../services/seasonal-aid.service';
import {
  BeneficiaryDistributionDetail,
  SeasonalAidCampaignListItem,
  SeasonalAidCampaignReport
} from '../models/seasonal-aid.model';
import { ReportExportService } from '../../reports/services/report-export.service';
import { CharityService } from '../../charities/services/charity.service';
import { NotificationService } from '../../../core/services/notification.service';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../shared/components';
import { SharedModule } from '../../../shared/shared.module';

/**
 * UC-RPT-10 (§23.S.2) completes the shipped campaign-report surface: the two §23.S.2
 * drop-downs (الجمعية filtering the breakdowns client-side; the campaign selector standing in
 * for the nonexistent Projects lookup — recorded adaptation), next/prev campaign navigation,
 * and the shared report exporter replacing the inline ExcelJS block. «حفظ → DeleteProject()»
 * is a spec artefact and is not built — this screen writes nothing.
 */
@Component({
  selector: 'app-campaign-report',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule, BreadcrumbComponent, SharedModule],
  templateUrl: './campaign-report.component.html',
  styleUrls: ['./campaign-report.component.scss']
})
export class CampaignReportComponent implements OnInit {
  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'seasonalAid.title', url: '/seasonal-aid' },
    { label: 'seasonalAid.campaignReport' }
  ];

  report: SeasonalAidCampaignReport | null = null;
  loading = false;
  exporting = false;

  /**
   * §23.S.2 الجمعية — bound to the charity ID. Review P27 2026-08-26: the options used to
   * carry the NAME as the id, so two same-named charities collapsed into one filter value;
   * the wire rows key on charityName, so the filter resolves id → name once.
   */
  charityOptions: Array<{ id: string; name: string }> = [];
  selectedCharityId = '';

  /** The selected charity's NAME — what the details/breakdown wire rows key on. */
  private get selectedCharityName(): string {
    if (!this.selectedCharityId) {
      return '';
    }
    return this.charityOptions.find(o => o.id === this.selectedCharityId)?.name ?? '';
  }

  /** §23.S.2 project drop-down — the programme's campaigns (recorded adaptation). */
  campaigns: SeasonalAidCampaignListItem[] = [];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private seasonalAidService: SeasonalAidService,
    private reportExportService: ReportExportService,
    private charityService: CharityService,
    private notification: NotificationService,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.loadCharities();
    this.loadCampaigns();
    this.route.params.subscribe(params => {
      this.selectedCharityId = '';
      this.loadReport(params['id']);
    });
  }

  /** الجمعية — real charities endpoint only; كافة الجهات all-option. */
  private loadCharities(): void {
    this.charityService.getCharities({ pageNumber: 1, pageSize: 500 })
      .subscribe({
        next: response => {
          this.charityOptions = [
            { id: '', name: this.translate.instant('seasonalAid.allCharities') },
            // Review P27 2026-08-26: option ids are the charity Guids — the name stays the label.
            ...(response.items || []).map(c => ({ id: c.id, name: c.name }))
          ];
        },
        error: () => console.error('Error loading charities')
      });
  }

  /** The campaign selector + next/prev both walk the same loaded campaigns page. */
  private loadCampaigns(): void {
    this.seasonalAidService.getCampaigns({ pageNumber: 1, pageSize: 100 })
      .subscribe({
        next: result => (this.campaigns = result.items || []),
        error: () => console.error('Error loading campaigns')
      });
  }

  // ==================== §23.S.2 filters ====================

  onCharityFilterChange(id: string): void {
    // Review P27 2026-08-26: the select now emits the charity ID (see selectedCharityId).
    this.selectedCharityId = id;
  }

  /** The distribution-details rows after the charity filter (all rows when no filter). */
  get filteredDetails(): BeneficiaryDistributionDetail[] {
    const details = this.report?.distributionDetails ?? [];
    if (!this.selectedCharityName) {
      return details;
    }
    return details.filter(d => d.charityName === this.selectedCharityName);
  }

  /** The charity-breakdown rows after the same filter. */
  get filteredCharityEntries(): Array<{ key: string; value: number }> {
    return this.toEntries(this.report?.beneficiariesByCharity)
      .filter(entry => !this.selectedCharityName || entry.key === this.selectedCharityName);
  }

  /**
   * Review P27 2026-08-26: the REGION breakdown follows the charity filter too — recomputed
   * from the filtered details when a filter is active (the dictionary has no charity split);
   * the server dictionary stays authoritative for the unfiltered view. The family-type
   * breakdown CANNOT be narrowed the same way (the detail rows carry no family type) and is
   * left campaign-wide.
   */
  get filteredRegionEntries(): Array<{ key: string; value: number }> {
    if (!this.selectedCharityName) {
      return this.toEntries(this.report?.beneficiariesByRegion);
    }
    const counts = new Map<string, number>();
    for (const detail of this.filteredDetails) {
      const key = (detail.regionName || '').trim();
      if (!key) {
        continue;
      }
      counts.set(key, (counts.get(key) || 0) + 1);
    }
    return Array.from(counts)
      .map(([key, value]) => ({ key, value }))
      .sort((a, b) => b.value - a.value);
  }

  /** Campaign selection navigates to that campaign's report (the Projects-lookup adaptation). */
  onCampaignSelect(id: string): void {
    if (id) {
      this.router.navigate(['/seasonal-aid', id, 'report']);
    }
  }

  // ==================== next / prev ====================

  private get campaignIndex(): number {
    const id = this.report?.campaignId ?? this.route.snapshot.paramMap.get('id');
    return this.campaigns.findIndex(c => c.id === id);
  }

  get canGoNext(): boolean {
    const i = this.campaignIndex;
    return i >= 0 && i < this.campaigns.length - 1;
  }

  get canGoPrev(): boolean {
    return this.campaignIndex > 0;
  }

  getNext(): void {
    const i = this.campaignIndex;
    if (i >= 0 && i < this.campaigns.length - 1) {
      this.router.navigate(['/seasonal-aid', this.campaigns[i + 1].id, 'report']);
    }
  }

  getPrev(): void {
    const i = this.campaignIndex;
    if (i > 0) {
      this.router.navigate(['/seasonal-aid', this.campaigns[i - 1].id, 'report']);
    }
  }

  loadReport(id: string): void {
    this.loading = true;
    this.seasonalAidService.getCampaignReport(id).subscribe({
      next: report => {
        this.report = report;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.router.navigate(['/seasonal-aid', id]);
      }
    });
  }

  /** Dictionary<string, int> → sortable rows for the breakdown tables. */
  toEntries(dict: Record<string, number> | undefined): Array<{ key: string; value: number }> {
    return Object.entries(dict || {}).sort((a, b) => b[1] - a[1]).map(([key, value]) => ({ key, value }));
  }

  familyTypeKey(familyType: string): string {
    return (familyType || 'All').replace(/\s+/g, '');
  }

  /** UC-PRJ-12 — print view of the report (jsPDF is not a dependency; window.print is). */
  printReport(): void {
    window.print();
  }

  /**
   * UC-RPT-10 استخراج — the shared 18-1 exporter (two sheets, RTL). The details sheet carries
   * the FILTERED rows; zero rows after filtering → on-screen message, no file. The summary
   * sheet stays campaign-wide (it is labelled the campaign summary).
   */
  exportReportData(): void {
    if (!this.report || this.exporting) {
      return;
    }
    const details = this.filteredDetails;
    if (details.length === 0) {
      this.notification.info(this.translate.instant('seasonalAid.noFilteredRows'));
      return;
    }

    const report = this.report;
    this.exporting = true;
    this.reportExportService.exportCampaignReport(report, details)
      .catch(() => this.notification.error(this.translate.instant('seasonalAid.exportFailed')))
      .finally(() => (this.exporting = false));
  }

  onBack(): void {
    if (this.report) {
      this.router.navigate(['/seasonal-aid', this.report.campaignId]);
    }
  }

  trackByDetail(index: number, detail: { beneficiaryId: string }): string {
    return detail.beneficiaryId;
  }

  trackByEntry(index: number, entry: { key: string; value: number }): string {
    return entry.key;
  }

  trackByOption(_index: number, option: { id: string; name: string }): string {
    return option.id;
  }

  trackByCampaign(_index: number, campaign: SeasonalAidCampaignListItem): string {
    return campaign.id;
  }
}
