import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { SeasonalAidService } from '../services/seasonal-aid.service';
import { NotificationService } from '../../../core/services/notification.service';
import { SharedModule } from '../../../shared/shared.module';
import {
  SeasonalAidCampaign,
  SeasonalAidBeneficiary
} from '../models/seasonal-aid.model';

/**
 * Beneficiary selection screen (UC-PRJ-06 اختيار الأسر للمشروع) — three tables:
 *
 *  1. all families of the caller's charity that are not yet on the project
 *     (server excludes already-registered rows) with row selection, select-all
 *     and two add commands — add as main family or add to the pending list;
 *  2. main families registered on the project (isMain = true);
 *  3. the pending list awaiting confirmation as main (isMain = false).
 *
 * One search field above the tables drives all three reads. Every mutation goes
 * through the register endpoint (with isMain) or the main-status move endpoint;
 * quota, budget and charity ownership are enforced server-side.
 */
@Component({
  selector: 'app-beneficiary-selection',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule, SharedModule],
  templateUrl: './beneficiary-selection.component.html',
  styleUrls: ['./beneficiary-selection.component.scss']
})
export class BeneficiarySelectionComponent implements OnInit {
  campaign: SeasonalAidCampaign | null = null;
  loading = false;

  // Table 1 — pool: every charity family not yet on the project
  poolFamilies: SeasonalAidBeneficiary[] = [];
  poolTotalCount = 0;
  loadingPool = false;

  // Tables 2 & 3 — the project register split by main/pending
  mainFamilies: SeasonalAidBeneficiary[] = [];
  pendingFamilies: SeasonalAidBeneficiary[] = [];
  loadingMain = false;
  loadingPending = false;

  saving = false;

  /** Family search (code/address) — one field above the tables, applied to all three. */
  searchTerm = '';
  private searchDebounce: ReturnType<typeof setTimeout> | null = null;

  /** Families ticked on the pool table, pending an add-as-main / add-as-pending. */
  selectedFamilyIds = new Set<string>();

  /** Pool page size — the pool can be much larger than the two register tables. */
  readonly poolPageSize = 100;

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private seasonalAidService: SeasonalAidService,
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
        this.reloadTables();
      },
      error: () => {
        this.loading = false;
        this.notification.error(this.translate.instant('seasonalAid.campaignLoadFailed'));
        this.onCancel();
      }
    });
  }

  /** Refresh the three tables with the current search term. */
  reloadTables(): void {
    if (!this.campaign) {
      return;
    }
    this.loadPool();
    this.loadMain();
    this.loadPending();
  }

  // ========== Table 1: charity family pool (UC-PRJ-10 read) ==========

  loadPool(): void {
    if (!this.campaign) return;
    this.loadingPool = true;
    this.seasonalAidService.getEligibleFamilies(this.campaign.id, {
      pageNumber: 1,
      pageSize: this.poolPageSize,
      sortDescending: false,
      searchTerm: this.searchTerm || undefined
    }).subscribe({
      next: result => {
        this.poolFamilies = result.items || [];
        this.poolTotalCount = result.totalCount || this.poolFamilies.length;
        this.loadingPool = false;
        // Selections belong to the rows on screen — a reloaded pool starts clean.
        this.selectedFamilyIds.clear();
      },
      error: () => (this.loadingPool = false)
    });
  }

  // ========== Tables 2 & 3: the register split ==========

  loadMain(): void {
    if (!this.campaign) return;
    this.loadingMain = true;
    this.seasonalAidService.getCampaignBeneficiaries(this.campaign.id, {
      pageNumber: 1,
      pageSize: 200,
      sortDescending: false,
      isMain: true,
      searchTerm: this.searchTerm || undefined
    }).subscribe({
      next: result => {
        this.mainFamilies = result.items || [];
        this.loadingMain = false;
      },
      error: () => (this.loadingMain = false)
    });
  }

  loadPending(): void {
    if (!this.campaign) return;
    this.loadingPending = true;
    this.seasonalAidService.getCampaignBeneficiaries(this.campaign.id, {
      pageNumber: 1,
      pageSize: 200,
      sortDescending: false,
      isMain: false,
      searchTerm: this.searchTerm || undefined
    }).subscribe({
      next: result => {
        this.pendingFamilies = result.items || [];
        this.loadingPending = false;
      },
      error: () => (this.loadingPending = false)
    });
  }

  // ========== Search ==========

  /** Debounced typing search; Enter bypasses the wait. */
  onSearchInput(): void {
    if (this.searchDebounce !== null) {
      clearTimeout(this.searchDebounce);
    }
    this.searchDebounce = setTimeout(() => {
      this.searchDebounce = null;
      this.reloadTables();
    }, 400);
  }

  onSearch(): void {
    if (this.searchDebounce !== null) {
      clearTimeout(this.searchDebounce);
      this.searchDebounce = null;
    }
    this.reloadTables();
  }

  clearSearch(): void {
    this.searchTerm = '';
    this.onSearch();
  }

  // ========== Selection on the pool table ==========

  toggleSelectAll(selectAll: boolean): void {
    this.poolFamilies.forEach(f => {
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

  // ========== Campaign context strip ==========

  /** Share of the total budget committed to the registered families. */
  get allocationUtilization(): number {
    if (!this.campaign || !this.campaign.totalBudget) return 0;
    return (this.campaign.allocatedBudget / this.campaign.totalBudget) * 100;
  }

  get budgetUtilization(): number {
    if (!this.campaign || !this.campaign.totalBudget) return 0;
    return (this.campaign.distributedBudget / this.campaign.totalBudget) * 100;
  }

  get utilizationBarClass(): string {
    const utilization = this.budgetUtilization;
    if (utilization >= 90) return 'bg-danger';
    if (utilization >= 70) return 'bg-warning';
    return 'bg-success';
  }

  isPoolSelected(family: SeasonalAidBeneficiary): boolean {
    return this.selectedFamilyIds.has(family.familyId);
  }

  togglePoolSelected(family: SeasonalAidBeneficiary, checked: boolean): void {
    if (checked) {
      this.selectedFamilyIds.add(family.familyId);
    } else {
      this.selectedFamilyIds.delete(family.familyId);
    }
  }

  get allPoolSelected(): boolean {
    return this.poolFamilies.length > 0 &&
      this.poolFamilies.every(f => this.selectedFamilyIds.has(f.familyId));
  }

  // ========== Add to the project (UC-PRJ-07 quick add) ==========

  /** Add the ticked pool families to the main list (true) or the pending list (false). */
  addSelectedAs(isMain: boolean): void {
    if (!this.campaign || this.selectedFamilyIds.size === 0) return;
    this.registerFamilies([...this.selectedFamilyIds], isMain);
  }

  /** Row-level add of a single pool family. */
  addFamilyAs(family: SeasonalAidBeneficiary, isMain: boolean): void {
    this.registerFamilies([family.familyId], isMain);
  }

  private registerFamilies(familyIds: string[], isMain: boolean): void {
    if (!this.campaign) return;
    this.saving = true;
    this.seasonalAidService.registerBeneficiaries(this.campaign.id, {
      familyIds,
      isMain
    }).subscribe({
      next: result => {
        this.saving = false;
        this.notification.success(
          this.translate.instant(isMain ? 'seasonalAid.selection.addedAsMain' : 'seasonalAid.selection.addedAsPending')
            .replace('{count}', String(result.registeredCount))
        );
        this.reloadTables();
      },
      error: () => {
        this.saving = false;
        this.notification.error(
          this.translate.instant(isMain ? 'seasonalAid.selection.addFailed' : 'seasonalAid.selection.addFailed'));
      }
    });
  }

  // ========== Move between the two lists (UC-PRJ-06) ==========

  moveToList(beneficiary: SeasonalAidBeneficiary, isMain: boolean): void {
    if (!this.campaign) return;
    this.saving = true;
    this.seasonalAidService.setBeneficiaryMainStatus(beneficiary.id, isMain).subscribe({
      next: () => {
        this.saving = false;
        this.notification.success(
          this.translate.instant(isMain ? 'seasonalAid.selection.movedToMain' : 'seasonalAid.selection.movedToPending'));
        this.reloadTables();
      },
      error: () => {
        this.saving = false;
        this.notification.error(this.translate.instant('seasonalAid.selection.moveFailed'));
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
        this.reloadTables();
      },
      error: () => {
        this.saving = false;
        this.notification.error(this.translate.instant('seasonalAid.beneficiaryRemoveFailed'));
      }
    });
  }

  onCancel(): void {
    if (this.campaign) {
      this.router.navigate(['/seasonal-aid', this.campaign.id]);
    } else {
      this.router.navigate(['/seasonal-aid']);
    }
  }

  get totalOnProject(): number {
    return this.mainFamilies.length + this.pendingFamilies.length;
  }

  trackByFamilyId(index: number, family: SeasonalAidBeneficiary): string {
    return family.familyId;
  }

  trackByBeneficiaryId(index: number, beneficiary: SeasonalAidBeneficiary): string {
    return beneficiary.id;
  }
}
