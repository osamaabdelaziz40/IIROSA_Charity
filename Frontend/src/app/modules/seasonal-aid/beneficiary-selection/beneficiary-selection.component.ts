import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { SeasonalAidService } from '../services/seasonal-aid.service';
import { NotificationService } from '../../../core/services/notification.service';
import { LookupManagementService } from '../../lookup-management/services/lookup-management.service';
import {
  SeasonalAidCampaign,
  SeasonalAidBeneficiary,
  EligibleFamiliesFilter
} from '../models/seasonal-aid.model';
import { LookupBase } from '../../../shared/models/lookup.base.model';

@Component({
  selector: 'app-beneficiary-selection',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule],
  templateUrl: './beneficiary-selection.component.html',
  styleUrls: ['./beneficiary-selection.component.scss']
})
export class BeneficiarySelectionComponent implements OnInit {
  campaign: SeasonalAidCampaign | null = null;
  eligibleFamilies: SeasonalAidBeneficiary[] = [];
  registeredBeneficiaries: SeasonalAidBeneficiary[] = [];
  loading = false;
  saving = false;
  togglingFamilyId: string | null = null;

  searchTerm = '';
  charityFilter = '';
  regionFilter: number | null = null;
  familyTypeFilter = '';

  /** Families ticked on the eligible panel, pending an "Add selected". */
  selectedFamilyIds = new Set<string>();

  regionOptions: LookupBase[] = [];

  familyTypeOptions = [
    { id: '', name: 'seasonalAid.allTypes' },
    { id: 'All', name: 'seasonalAid.familyTypes.All' },
    { id: 'Orphan Families', name: 'seasonalAid.familyTypes.OrphanFamilies' },
    { id: 'Needy Families', name: 'seasonalAid.familyTypes.NeedyFamilies' }
  ];

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private seasonalAidService: SeasonalAidService,
    private lookupManagementService: LookupManagementService,
    private notification: NotificationService,
    private translate: TranslateService
  ) {}

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
        this.loadRegistered(id);
        this.loadEligible(id);
        if (data.countryId) {
          this.loadRegions(data.countryId);
        }
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  private loadRegions(countryId: number): void {
    this.lookupManagementService.getRegionsByCountry(countryId).subscribe({
      next: regions => {
        this.regionOptions = (regions || []).map(r => ({ id: r.id, name: r.nameAr || r.name }));
      },
      error: () => (this.regionOptions = [])
    });
  }

  /** UC-PRJ-10 — families matching the campaign scope that are not yet registered. */
  loadEligible(id: string): void {
    const filter: Partial<EligibleFamiliesFilter> = {
      pageNumber: 1,
      pageSize: 50,
      sortDescending: false
    };
    if (this.searchTerm) {
      filter.searchTerm = this.searchTerm;
    }
    if (this.regionFilter) {
      filter.regionId = this.regionFilter;
    }
    if (this.familyTypeFilter) {
      filter.familyType = this.familyTypeFilter;
    }

    this.seasonalAidService.getEligibleFamilies(id, filter).subscribe({
      next: result => {
        this.eligibleFamilies = result.items || [];
        this.selectedFamilyIds.clear();
      }
    });
  }

  loadRegistered(id: string): void {
    this.seasonalAidService.getCampaignBeneficiaries(id, { pageNumber: 1, pageSize: 200, sortDescending: false }).subscribe({
      next: result => {
        this.registeredBeneficiaries = result.items || [];
      }
    });
  }

  onFilterChange(): void {
    if (this.campaign) {
      this.loadEligible(this.campaign.id);
    }
  }

  onSearchChange(): void {
    if (this.campaign) {
      this.loadEligible(this.campaign.id);
    }
  }

  toggleSelectAll(selectAll: boolean): void {
    this.eligibleFamilies.forEach(f => {
      if (selectAll) {
        this.selectedFamilyIds.add(f.familyId);
      } else {
        this.selectedFamilyIds.delete(f.familyId);
      }
    });
  }

  get selectedCount(): number {
    return this.selectedFamilyIds.size;
  }

  isSelected(family: SeasonalAidBeneficiary): boolean {
    return this.selectedFamilyIds.has(family.familyId);
  }

  toggleSelected(family: SeasonalAidBeneficiary, checked: boolean): void {
    if (checked) {
      this.selectedFamilyIds.add(family.familyId);
    } else {
      this.selectedFamilyIds.delete(family.familyId);
    }
  }

  /**
   * UC-PRJ-07 quick add: sends the desired final set (existing + newly selected) through the
   * PUT sync endpoint — one mutation path for every change on this screen. The quota
   * (current + adds ≤ MaximumFamilies) and charity ownership are enforced server-side.
   */
  onAddBeneficiaries(): void {
    if (!this.campaign || this.selectedFamilyIds.size === 0) return;

    this.saving = true;
    const desiredFamilyIds = [
      ...this.registeredBeneficiaries.map(b => b.familyId),
      ...this.selectedFamilyIds
    ];

    this.seasonalAidService.updateCampaignBeneficiaries(this.campaign.id, {
      familyIds: desiredFamilyIds
    }).subscribe({
      next: result => {
        this.saving = false;
        this.notification.success(
          this.translate.instant('seasonalAid.beneficiariesUpdated')
            .replace('{added}', String(result.addedCount))
            .replace('{total}', String(result.totalRegistered))
        );
        this.refresh();
      },
      error: () => {
        this.saving = false;
        this.notification.error(this.translate.instant('seasonalAid.beneficiariesUpdateFailed'));
      }
    });
  }

  onRemoveBeneficiary(beneficiary: SeasonalAidBeneficiary): void {
    if (!this.campaign) return;
    if (!confirm(this.translate.instant('seasonalAid.removeBeneficiaryConfirm').replace('{code}', beneficiary.familyCode))) {
      return;
    }

    this.saving = true;
    this.seasonalAidService.removeBeneficiary(beneficiary.id).subscribe({
      next: () => {
        this.saving = false;
        this.notification.success(this.translate.instant('seasonalAid.beneficiaryRemoved'));
        this.refresh();
      },
      error: () => {
        this.saving = false;
        this.notification.error(this.translate.instant('seasonalAid.beneficiaryRemoveFailed'));
      }
    });
  }

  /** UC-PRJ-08 — mark a family as having received the assistance (toggle). */
  onToggleReceived(beneficiary: SeasonalAidBeneficiary): void {
    if (!this.campaign) return;

    this.togglingFamilyId = beneficiary.familyId;
    this.seasonalAidService.setFamilyReceivedFlag(beneficiary.familyId, {
      campaignId: this.campaign.id,
      isReceived: !beneficiary.isDistributed
    }).subscribe({
      next: () => {
        this.togglingFamilyId = null;
        this.notification.success(
          this.translate.instant(
            beneficiary.isDistributed ? 'seasonalAid.receivedFlagWithdrawn' : 'seasonalAid.receivedFlagConfirmed'
          )
        );
        this.refresh();
      },
      error: () => {
        this.togglingFamilyId = null;
        this.notification.error(this.translate.instant('seasonalAid.receivedFlagFailed'));
      }
    });
  }

  private refresh(): void {
    if (!this.campaign) return;
    this.loadCampaign(this.campaign.id);
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

  getRegisteredAllocation(): number {
    return this.registeredBeneficiaries.reduce((sum, b) => sum + (b.allocationAmount || 0), 0);
  }

  getRemainingBudget(): number {
    if (!this.campaign) return 0;
    return this.campaign.totalBudget - this.campaign.allocatedBudget;
  }

  getSelectedAllocation(): number {
    return this.selectedFamilyIds.size * (this.campaign?.perFamilyAllocation || 0);
  }

  canAddMoreBeneficiaries(): boolean {
    if (!this.campaign) return false;
    if (this.campaign.maximumFamilies) {
      return this.registeredBeneficiaries.length + this.selectedFamilyIds.size <= this.campaign.maximumFamilies;
    }
    return this.getSelectedAllocation() <= this.getRemainingBudget();
  }

  trackByFamily(index: number, family: SeasonalAidBeneficiary): string {
    return family.familyId;
  }

  trackByBeneficiary(index: number, beneficiary: SeasonalAidBeneficiary): string {
    return beneficiary.id;
  }

  trackByRegion(index: number, region: LookupBase): number {
    return region.id;
  }

  trackFamilyType(index: number, type: { id: string; name: string }): string {
    return type.id;
  }
}
