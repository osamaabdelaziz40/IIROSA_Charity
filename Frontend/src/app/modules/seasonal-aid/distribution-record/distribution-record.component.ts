import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

import { SeasonalAidService } from '../services/seasonal-aid.service';
import {
  SeasonalCampaign,
  CampaignBeneficiary,
  AidDistribution,
  DistributionStatus
} from '../models/seasonal-aid.model';
import { SharedModule } from '../../../shared/shared.module';

@Component({
  selector: 'app-distribution-record',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, TranslateModule, SharedModule],
  templateUrl: './distribution-record.component.html',
  styleUrls: ['./distribution-record.component.scss']
})
export class DistributionRecordComponent implements OnInit {
  campaign: SeasonalCampaign | null = null;
  beneficiaries: CampaignBeneficiary[] = [];
  distributions: AidDistribution[] = [];
  loading: boolean = false;
  saving: boolean = false;

  distributionForm: FormGroup;
  currentBeneficiary: CampaignBeneficiary | null = null;
  showDistributionForm: boolean = false;

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private route: ActivatedRoute,
    private seasonalAidService: SeasonalAidService
  ) {
    this.distributionForm = this.createDistributionForm();
  }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.loadCampaign(params['id']);
      this.loadBeneficiaries(params['id']);
      this.loadDistributions(params['id']);
    });
  }

  createDistributionForm(): FormGroup {
    return this.fb.group({
      campaignBeneficiaryId: ['', Validators.required],
      distributionDate: [new Date(), Validators.required],
      amountDistributed: [0, [Validators.required, Validators.min(0)]],
      receivedBy: ['', Validators.required],
      notes: [''],
      signatureUrl: ['']
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

  loadBeneficiaries(id: string): void {
    this.seasonalAidService.getCampaignBeneficiaries(id).subscribe({
      next: (data) => {
        this.beneficiaries = data.filter(b => b.distributionStatus === DistributionStatus.Pending);
      }
    });
  }

  loadDistributions(id: string): void {
    this.seasonalAidService.getCampaignDistributions(id).subscribe({
      next: (data) => {
        this.distributions = data;
      }
    });
  }

  onRecordDistribution(beneficiary: CampaignBeneficiary): void {
    this.currentBeneficiary = beneficiary;
    this.showDistributionForm = true;

    this.distributionForm.patchValue({
      campaignBeneficiaryId: beneficiary.id,
      distributionDate: new Date(),
      amountDistributed: beneficiary.allocatedAmount,
      receivedBy: '',
      notes: '',
      signatureUrl: ''
    });
  }

  onSubmitDistribution(): void {
    if (this.distributionForm.invalid || !this.campaign) {
      return;
    }

    this.saving = true;
    const formValue = this.distributionForm.value;

    const distribution: AidDistribution = {
      ...formValue,
      id: '',
      campaignId: this.campaign.id,
      familyId: this.currentBeneficiary?.familyId || '',
      familyCode: this.currentBeneficiary?.familyCode || '',
      distributedBy: 'Current User',
      createdAt: new Date()
    };

    this.seasonalAidService.recordDistribution(distribution).subscribe({
      next: () => {
        this.saving = false;
        this.showDistributionForm = false;
        this.loadBeneficiaries(this.campaign!.id);
        this.loadDistributions(this.campaign!.id);
      },
      error: () => {
        this.saving = false;
      }
    });
  }

  onCancelDistribution(): void {
    this.showDistributionForm = false;
    this.currentBeneficiary = null;
    this.distributionForm.reset();
  }

  getPendingBeneficiaries(): CampaignBeneficiary[] {
    return this.beneficiaries.filter(b => b.distributionStatus === DistributionStatus.Pending);
  }

  getDistributedBeneficiaries(): CampaignBeneficiary[] {
    return this.beneficiaries.filter(b => b.distributionStatus === DistributionStatus.Distributed);
  }

  onBackToCampaign(): void {
    if (this.campaign) {
      this.router.navigate(['/seasonal-aid', this.campaign.id]);
    }
  }

  getTotalDistributed(): number {
    return this.distributions.reduce((sum, d) => sum + d.amountDistributed, 0);
  }

  getDistributionProgress(): number {
    if (!this.campaign) return 0;
    const total = this.beneficiaries.length;
    const distributed = this.getDistributedBeneficiaries().length;
    return total > 0 ? (distributed / total) * 100 : 0;
  }
}
