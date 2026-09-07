import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

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
  beneficiaries: SeasonalAidBeneficiary[] = [];
  loading = false;
  closingCampaign = false;

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
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  /** Beneficiary preview (first page) — the full management screen lives at :id/beneficiaries. */
  loadBeneficiaries(id: string): void {
    const formValues = this.beneficiaryFilterForm.value;
    const filter: Partial<SeasonalAidBeneficiaryFilter> = {
      pageNumber: 1,
      pageSize: 10,
      sortDescending: false
    };

    if (formValues.searchTerm) {
      filter.searchTerm = formValues.searchTerm;
    }
    if (formValues.distributionStatus === 'pending') {
      filter.isDistributed = false;
    } else if (formValues.distributionStatus === 'distributed') {
      filter.isDistributed = true;
    }

    this.seasonalAidService.getCampaignBeneficiaries(id, filter).subscribe({
      next: result => {
        this.beneficiaries = result.items || [];
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
