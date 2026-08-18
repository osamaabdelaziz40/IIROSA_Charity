import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

import { SeasonalAidService } from '../services/seasonal-aid.service';
import {
  SeasonalCampaign,
  CampaignStatistics,
  CampaignBeneficiary,
  DistributionStatus,
  CampaignType,
  FamilyType
} from '../models/seasonal-aid.model';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../shared/components';
import { SharedModule } from '../../../shared/shared.module';
import { DropDownComponent } from '../../../shared/components/drop-down/drop-down.component';

@Component({
  selector: 'app-campaign-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterModule, TranslateModule, BreadcrumbComponent, SharedModule, DropDownComponent],
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
  campaign: SeasonalCampaign | null = null;
  statistics: CampaignStatistics | null = null;
  beneficiaries: CampaignBeneficiary[] = [];
  loading: boolean = false;
  closingCampaign: boolean = false;

  // Beneficiary filter form
  beneficiaryFilterForm: FormGroup;

  // Distribution status options
  distributionStatusOptions = [
    { id: '', name: 'seasonalAid.allStatuses' },
    { id: 'Pending', name: 'seasonalAid.pending' },
    { id: 'Distributed', name: 'seasonalAid.distributed' },
    { id: 'Cancelled', name: 'seasonalAid.cancelled' }
  ];

  constructor(
    public router: Router,
    private route: ActivatedRoute,
    private seasonalAidService: SeasonalAidService,
    private fb: FormBuilder
  ) {
    // Initialize beneficiary filter form
    this.beneficiaryFilterForm = this.fb.group({
      searchTerm: [''],
      distributionStatus: ['']
    });
  }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.loadCampaign(params['id']);
      this.loadStatistics(params['id']);
      this.loadBeneficiaries(params['id']);
    });
  }

  loadCampaign(id: string): void {
    this.loading = true;
    this.seasonalAidService.getCampaignById(id).subscribe({
      next: (data) => {
        this.campaign = data;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  loadStatistics(id: string): void {
    this.seasonalAidService.getCampaignStatistics(id).subscribe({
      next: (data) => {
        this.statistics = data;
      }
    });
  }

  loadBeneficiaries(id: string): void {
    const formValues = this.beneficiaryFilterForm.value;

    // Build filter object from form values
    const filter: {
      distributionStatus?: DistributionStatus;
      searchTerm?: string;
    } = {};

    if (formValues.searchTerm) {
      filter.searchTerm = formValues.searchTerm;
    }
    if (formValues.distributionStatus) {
      filter.distributionStatus = formValues.distributionStatus as DistributionStatus;
    }

    this.seasonalAidService.getCampaignBeneficiaries(id, filter).subscribe({
      next: (data) => {
        this.beneficiaries = data;
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

    const notes = prompt('Please enter closure notes:');
    if (notes) {
      this.closingCampaign = true;
      this.seasonalAidService.closeCampaign(this.campaign.id, notes).subscribe({
        next: () => {
          this.closingCampaign = false;
          this.loadCampaign(this.campaign!.id);
        },
        error: () => {
          this.closingCampaign = false;
        }
      });
    }
  }

  onGenerateReport(): void {
    if (this.campaign) {
      this.router.navigate(['/seasonal-aid', this.campaign.id, 'report']);
    }
  }

  getBudgetUtilization(): number {
    if (!this.statistics || !this.statistics.totalBudget) return 0;
    return (this.statistics.distributedAmount / this.statistics.totalBudget) * 100;
  }

  getBudgetUtilizationClass(): string {
    const utilization = this.getBudgetUtilization();
    if (utilization >= 90) return 'text-danger';
    if (utilization >= 70) return 'text-warning';
    return 'text-success';
  }

  getCampaignTypeClass(type: CampaignType): string {
    const classes: { [key in CampaignType]: string } = {
      [CampaignType.Ramadan]: 'badge-primary',
      [CampaignType.EidAlFitr]: 'badge-info',
      [CampaignType.EidAlAdha]: 'badge-info',
      [CampaignType.Winter]: 'badge-info',
      [CampaignType.SchoolSupplies]: 'badge-warning',
      [CampaignType.Other]: 'badge-light'
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
    if (this.campaign.isClosed) return 'Closed';
    if (this.campaign.isActive) return 'Active';
    return 'Inactive';
  }

  getDistributionStatusClass(status: DistributionStatus): string {
    const classes: { [key in DistributionStatus]: string } = {
      [DistributionStatus.Pending]: 'badge-warning',
      [DistributionStatus.Distributed]: 'badge-success',
      [DistributionStatus.Cancelled]: 'badge-danger'
    };
    return classes[status] || 'badge-light';
  }

  onBeneficiaryFilterChange(): void {
    if (this.campaign) {
      this.loadBeneficiaries(this.campaign.id);
    }
  }

  // Check if beneficiary filters are active
  hasActiveBeneficiaryFilters(): boolean {
    const formValues = this.beneficiaryFilterForm.value;
    return !!(formValues.searchTerm || formValues.distributionStatus);
  }

  // Clear beneficiary filters
  clearBeneficiaryFilters(): void {
    this.beneficiaryFilterForm.reset({
      searchTerm: '',
      distributionStatus: ''
    });
    if (this.campaign) {
      this.loadBeneficiaries(this.campaign.id);
    }
  }
}
