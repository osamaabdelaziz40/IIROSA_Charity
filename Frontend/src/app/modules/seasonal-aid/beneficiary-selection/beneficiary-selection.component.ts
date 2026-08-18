import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

import { SeasonalAidService } from '../services/seasonal-aid.service';
import {
  SeasonalCampaign,
  BeneficiarySelection,
  CampaignBeneficiary,
  FamilyType,
  BeneficiaryFilter
} from '../models/seasonal-aid.model';

@Component({
  selector: 'app-beneficiary-selection',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule],
  templateUrl: './beneficiary-selection.component.html',
  styleUrls: ['./beneficiary-selection.component.scss']
})
export class BeneficiarySelectionComponent implements OnInit {
  campaign: SeasonalCampaign | null = null;
  availableBeneficiaries: BeneficiarySelection[] = [];
  registeredBeneficiaries: CampaignBeneficiary[] = [];
  loading: boolean = false;
  saving: boolean = false;

  filter: BeneficiaryFilter = {};
  searchTerm: string = '';
  selectAll: boolean = false;

  familyTypes = Object.values(FamilyType);
  regions: string[] = [];
  centers: string[] = [];
  charities: { id: string; name: string }[] = [];

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private seasonalAidService: SeasonalAidService
  ) {}

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.loadCampaign(params['id']);
      this.loadAvailableBeneficiaries(params['id']);
      this.loadRegisteredBeneficiaries(params['id']);
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

  loadAvailableBeneficiaries(id: string): void {
    this.loading = true;
    this.seasonalAidService.getAvailableBeneficiaries(id, {
      ...this.filter,
      searchTerm: this.searchTerm
    }).subscribe({
      next: (data) => {
        this.availableBeneficiaries = data;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  loadRegisteredBeneficiaries(id: string): void {
    this.seasonalAidService.getCampaignBeneficiaries(id).subscribe({
      next: (data) => {
        this.registeredBeneficiaries = data;
      }
    });
  }

  onFilterChange(): void {
    if (this.campaign) {
      this.loadAvailableBeneficiaries(this.campaign.id);
    }
  }

  onSearchChange(): void {
    if (this.campaign) {
      this.loadAvailableBeneficiaries(this.campaign.id);
    }
  }

  toggleSelectAll(): void {
    this.availableBeneficiaries.forEach(beneficiary => {
      beneficiary.isSelected = this.selectAll;
    });
  }

  onBeneficiaryToggle(beneficiary: BeneficiarySelection): void {
    beneficiary.isSelected = !beneficiary.isSelected;
    this.selectAll = this.availableBeneficiaries.every(b => b.isSelected);
  }

  getSelectedBeneficiaries(): BeneficiarySelection[] {
    return this.availableBeneficiaries.filter(b => b.isSelected);
  }

  onAddBeneficiaries(): void {
    const selected = this.getSelectedBeneficiaries();
    if (selected.length === 0) {
      alert('Please select at least one beneficiary');
      return;
    }

    if (!this.campaign) return;

    this.saving = true;
    const beneficiaryIds = selected.map(b => b.familyId);

    this.seasonalAidService.registerBeneficiaries(this.campaign.id, beneficiaryIds).subscribe({
      next: () => {
        this.saving = false;
        this.loadAvailableBeneficiaries(this.campaign!.id);
        this.loadRegisteredBeneficiaries(this.campaign!.id);
        this.selectAll = false;
      },
      error: () => {
        this.saving = false;
      }
    });
  }

  onRemoveBeneficiary(beneficiary: CampaignBeneficiary): void {
    if (!this.campaign) return;

    if (confirm(`Are you sure you want to remove ${beneficiary.familyCode}?`)) {
      this.seasonalAidService.removeBeneficiary(this.campaign.id, beneficiary.id).subscribe({
        next: () => {
          this.loadRegisteredBeneficiaries(this.campaign!.id);
          this.loadAvailableBeneficiaries(this.campaign!.id);
        }
      });
    }
  }

  onCancel(): void {
    if (this.campaign) {
      this.router.navigate(['/seasonal-aid', this.campaign.id]);
    }
  }

  onContinueToDistribution(): void {
    if (this.campaign) {
      this.router.navigate(['/seasonal-aid', this.campaign.id, 'distribution']);
    }
  }

  getTotalAllocation(): number {
    return this.getSelectedBeneficiaries().length * (this.campaign?.perFamilyAllocation || 0);
  }

  getRemainingBudget(): number {
    if (!this.campaign) return 0;
    return this.campaign.totalBudget - this.registeredBeneficiaries.length * this.campaign.perFamilyAllocation;
  }

  canAddMoreBeneficiaries(): boolean {
    const totalAllocation = this.getTotalAllocation();
    const remainingBudget = this.getRemainingBudget();
    return totalAllocation <= remainingBudget;
  }
}
