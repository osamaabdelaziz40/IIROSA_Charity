import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { forkJoin } from 'rxjs';

import { SeasonalAidService } from '../services/seasonal-aid.service';
import { NotificationService } from '../../../core/services/notification.service';
import {
  SeasonalAidCampaign,
  SeasonalAidBeneficiary,
  SeasonalAidBeneficiaryFilter
} from '../models/seasonal-aid.model';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../shared/components';
import { SharedModule } from '../../../shared/shared.module';
import { DropDownComponent } from '../../../shared/components/drop-down/drop-down.component';

@Component({
  selector: 'app-campaign-detail',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, TranslateModule, BreadcrumbComponent, SharedModule, DropDownComponent],
  templateUrl: './campaign-detail.component.html',
  styleUrls: ['./campaign-detail.component.scss']
})
export class CampaignDetailComponent implements OnInit {
  // Breadcrumb items
  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'seasonalAid.title', url: '/seasonal-aid' },
    { label: 'seasonalAid.campaignDetails' }
  ];

  campaign: SeasonalAidCampaign | null = null;
  // Main and pending registers are shown as two separate preview tables.
  mainBeneficiaries: SeasonalAidBeneficiary[] = [];
  pendingBeneficiaries: SeasonalAidBeneficiary[] = [];
  loading = false;
  closingCampaign = false;

  // Live counts from the beneficiaries endpoint — the counter fields on the campaign
  // DTO are unreliable, so the detail page computes its own split.
  mainCount = 0;
  pendingCount = 0;
  distributedCount = 0;

  beneficiaryFilterForm: FormGroup;

  // Server-side distribution filter — the backend exposes only isDistributed (bool).
  distributionStatusOptions = [
    { id: '', name: 'seasonalAid.allStatuses' },
    { id: 'pending', name: 'seasonalAid.pending' },
    { id: 'distributed', name: 'seasonalAid.distributed' }
  ];

  constructor(
    public router: Router,
    private route: ActivatedRoute,
    private seasonalAidService: SeasonalAidService,
    private notification: NotificationService,
    private translate: TranslateService,
    private fb: FormBuilder
  ) {
    this.beneficiaryFilterForm = this.fb.group({
      searchTerm: [''],
      distributionStatus: ['']
    });
  }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.loadCampaign(params['id']);
    });
  }

  loadCampaign(id: string): void {
    this.loading = true;
    this.seasonalAidService.getCampaignById(id).subscribe({
      next: data => {
        this.campaign = data;
        this.loading = false;
        this.loadBeneficiaries(id);
        this.loadBeneficiaryStats(id);
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  /** Main / pending / distributed counts — cheap paged queries, only totalCount is read. */
  loadBeneficiaryStats(id: string): void {
    const page = { pageNumber: 1, pageSize: 1 };
    forkJoin({
      main: this.seasonalAidService.getCampaignBeneficiaries(id, { ...page, isMain: true }),
      pending: this.seasonalAidService.getCampaignBeneficiaries(id, { ...page, isMain: false }),
      distributed: this.seasonalAidService.getCampaignBeneficiaries(id, { ...page, isDistributed: true })
    }).subscribe({
      next: ({ main, pending, distributed }) => {
        this.mainCount = main.totalCount || 0;
        this.pendingCount = pending.totalCount || 0;
        this.distributedCount = distributed.totalCount || 0;
      }
    });
  }

  /**
   * Beneficiary previews (first page of each register) — the full management
   * screen lives at :id/beneficiaries. Search and distribution filters apply
   * to both tables; the main/pending split itself is the two tables.
   */
  loadBeneficiaries(id: string): void {
    const formValues = this.beneficiaryFilterForm.value;
    const base: Partial<SeasonalAidBeneficiaryFilter> = {
      pageNumber: 1,
      pageSize: 10,
      sortDescending: false
    };

    if (formValues.searchTerm) {
      base.searchTerm = formValues.searchTerm;
    }
    if (formValues.distributionStatus === 'pending') {
      base.isDistributed = false;
    } else if (formValues.distributionStatus === 'distributed') {
      base.isDistributed = true;
    }

    forkJoin({
      main: this.seasonalAidService.getCampaignBeneficiaries(id, { ...base, isMain: true }),
      pending: this.seasonalAidService.getCampaignBeneficiaries(id, { ...base, isMain: false })
    }).subscribe({
      next: ({ main, pending }) => {
        this.mainBeneficiaries = main.items || [];
        this.pendingBeneficiaries = pending.items || [];
      }
    });
  }

  onEdit(): void {
    if (this.campaign) {
      this.router.navigate(['/seasonal-aid', this.campaign.id, 'edit']);
    }
  }

  onCloseCampaign(): void {
    if (!this.campaign) return;

    const notes = prompt(this.translate.instant('seasonalAid.closureNotesPrompt') || '');
    if (notes === null) return;

    this.closingCampaign = true;
    this.seasonalAidService.closeCampaign(this.campaign.id, notes || undefined).subscribe({
      next: () => {
        this.closingCampaign = false;
        this.notification.success(this.translate.instant('seasonalAid.campaignClosed'));
        this.loadCampaign(this.campaign!.id);
      },
      error: () => {
        this.closingCampaign = false;
        this.notification.error(this.translate.instant('seasonalAid.campaignCloseFailed'));
      }
    });
  }

  onGenerateReport(): void {
    if (this.campaign) {
      this.router.navigate(['/seasonal-aid', this.campaign.id, 'report']);
    }
  }

  /** Total families on the register (main + pending). */
  get registeredCount(): number {
    return this.mainCount + this.pendingCount;
  }

  getBudgetUtilization(): number {
    if (!this.campaign || !this.campaign.totalBudget) return 0;
    return (this.campaign.distributedBudget / this.campaign.totalBudget) * 100;
  }

  getBudgetUtilizationClass(): string {
    const utilization = this.getBudgetUtilization();
    if (utilization >= 90) return 'text-danger';
    if (utilization >= 70) return 'text-warning';
    return 'text-success';
  }

  getBudgetUtilizationBarClass(): string {
    const utilization = this.getBudgetUtilization();
    if (utilization >= 90) return 'bg-danger';
    if (utilization >= 70) return 'bg-warning';
    return 'bg-success';
  }

  /** Share of the budget committed to registered families. */
  getAllocationUtilization(): number {
    if (!this.campaign || !this.campaign.totalBudget) return 0;
    return (this.campaign.allocatedBudget / this.campaign.totalBudget) * 100;
  }

  getCampaignTypeClass(type: string): string {
    const classes: Record<string, string> = {
      Ramadan: 'badge-primary',
      EidAlFitr: 'badge-info',
      EidAlAdha: 'badge-info',
      Winter: 'badge-info',
      SchoolSupplies: 'badge-warning',
      Other: 'badge-light'
    };
    return classes[type] || 'badge-light';
  }

  getStatusBadgeClass(): string {
    if (!this.campaign) return '';
    if (this.campaign.isClosed) return 'badge-secondary';
    if (this.campaign.isActive) return 'badge-success';
    return 'badge-warning';
  }

  getStatusText(): string {
    if (!this.campaign) return '';
    if (this.campaign.isClosed) return 'seasonalAid.closed';
    if (this.campaign.isActive) return 'seasonalAid.active';
    return 'seasonalAid.inactive';
  }

  getDistributionStatusClass(beneficiary: SeasonalAidBeneficiary): string {
    return beneficiary.isDistributed ? 'badge-success' : 'badge-warning';
  }

  /** Wire values carry spaces ("Orphan Families"); i18n keys do not ("OrphanFamilies"). */
  familyTypeKey(familyType: string | null | undefined): string {
    return (familyType || 'All').replace(/\s+/g, '');
  }

  getDistributionStatusKey(beneficiary: SeasonalAidBeneficiary): string {
    return beneficiary.isDistributed ? 'seasonalAid.distributed' : 'seasonalAid.pending';
  }

  trackByBeneficiary(index: number, beneficiary: SeasonalAidBeneficiary): string {
    return beneficiary.id;
  }

  onBeneficiaryFilterChange(): void {
    if (this.campaign) {
      this.loadBeneficiaries(this.campaign.id);
    }
  }

  hasActiveBeneficiaryFilters(): boolean {
    const formValues = this.beneficiaryFilterForm.value;
    return !!(formValues.searchTerm || formValues.distributionStatus);
  }

  clearBeneficiaryFilters(): void {
    this.beneficiaryFilterForm.reset({ searchTerm: '', distributionStatus: '' });
    if (this.campaign) {
      this.loadBeneficiaries(this.campaign.id);
    }
  }
}
