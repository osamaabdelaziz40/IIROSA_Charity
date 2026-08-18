import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Observable, Subject, takeUntil } from 'rxjs';
import {
  OrphanPaymentDto,
  OrphanForSelectionDto,
  OrphanSelectionFilter,
  SPONSORSHIP_STATUS_OPTIONS,
  GENDER_OPTIONS
} from '../models/orphan-payment.model';
import { OrphanPaymentService } from '../services/orphan-payment.service';

@Component({
  selector: 'app-add-orphans-to-group',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule, RouterModule],
  templateUrl: './add-orphans-to-group.component.html',
  styleUrls: ['./add-orphans-to-group.component.scss']
})
export class AddOrphansToGroupComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // Data
  paymentGroupId: string | null = null;
  paymentGroup: OrphanPaymentDto | null = null;
  availableOrphans: OrphanForSelectionDto[] = [];
  selectedOrphans = new Set<string>();
  loading = false;
  loadingOrphans = false;
  adding = false;

  // Filters
  filter: OrphanSelectionFilter = {
    searchTerm: '',
    charityId: undefined,
    regionId: undefined,
    centerId: undefined,
    sponsorshipStatus: 'All',
    ageFrom: undefined,
    ageTo: undefined,
    gender: 'All'
  };

  // Options
  sponsorshipStatusOptions = SPONSORSHIP_STATUS_OPTIONS;
  genderOptions = GENDER_OPTIONS;
  availableCharities: { id: number; name: string }[] = [];
  availableRegions: { id: number; name: string }[] = [];
  availableCenters: { id: number; name: string }[] = [];

  // UI State
  selectAll = false;

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private orphanPaymentService: OrphanPaymentService,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.paymentGroupId = this.route.snapshot.paramMap.get('id');
    if (this.paymentGroupId) {
      this.loadPaymentGroup();
      this.loadFilterOptions();
      this.loadAvailableOrphans();
    } else {
      this.router.navigate(['/orphan-payments']);
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // ==================== LOADING DATA ====================

  loadPaymentGroup(): void {
    if (!this.paymentGroupId) return;

    this.loading = true;
    this.orphanPaymentService.getOrphanPayment(this.paymentGroupId).subscribe({
      next: (data) => {
        this.paymentGroup = data;

        // Pre-fill filters from group settings
        if (data.charityId) {
          this.filter.charityId = data.charityId;
        }
        if (data.regionId) {
          this.filter.regionId = data.regionId;
        }
        if (data.centerId) {
          this.filter.centerId = data.centerId;
        }

        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.router.navigate(['/orphan-payments']);
      }
    });
  }

  loadAvailableOrphans(): void {
    if (!this.paymentGroupId) return;

    this.loadingOrphans = true;
    this.orphanPaymentService.getAvailableOrphans(this.paymentGroupId, this.filter).subscribe({
      next: (data) => {
        this.availableOrphans = data;
        this.loadingOrphans = false;
      },
      error: () => {
        this.loadingOrphans = false;
      }
    });
  }

  private loadFilterOptions(): void {
    this.orphanPaymentService.getAvailableCharities().subscribe(data => {
      this.availableCharities = data;
    });

    this.orphanPaymentService.getAvailableRegions().subscribe(data => {
      this.availableRegions = data;
    });

    this.orphanPaymentService.getAvailableCenters().subscribe(data => {
      this.availableCenters = data;
    });
  }

  // ==================== FILTERS ====================

  onSearch(): void {
    this.loadAvailableOrphans();
  }

  onResetFilters(): void {
    this.filter = {
      searchTerm: '',
      charityId: this.paymentGroup?.charityId,
      regionId: this.paymentGroup?.regionId,
      centerId: this.paymentGroup?.centerId,
      sponsorshipStatus: 'All',
      ageFrom: undefined,
      ageTo: undefined,
      gender: 'All'
    };
    this.loadAvailableOrphans();
  }

  // ==================== SELECTION ====================

  onToggleSelectAll(): void {
    if (this.selectAll) {
      this.availableOrphans.forEach(orphan => {
        if (!orphan.isAlreadyInGroup) {
          this.selectedOrphans.add(orphan.orphanId);
        }
      });
    } else {
      this.selectedOrphans.clear();
    }
  }

  onToggleOrphan(orphanId: string): void {
    if (this.selectedOrphans.has(orphanId)) {
      this.selectedOrphans.delete(orphanId);
    } else {
      this.selectedOrphans.add(orphanId);
    }

    // Update select all state
    this.updateSelectAllState();
  }

  private updateSelectAllState(): void {
    const availableCount = this.availableOrphans.filter(o => !o.isAlreadyInGroup).length;
    this.selectAll = this.selectedOrphans.size === availableCount && availableCount > 0;
  }

  isOrphanSelected(orphanId: string): boolean {
    return this.selectedOrphans.has(orphanId);
  }

  // ==================== ACTIONS ====================

  onAddSelectedOrphans(): void {
    if (this.selectedOrphans.size === 0) {
      alert(this.translate.instant('orphanPayments.selectAtLeastOneOrphan'));
      return;
    }

    if (!this.paymentGroupId) return;

    this.adding = true;
    this.orphanPaymentService.addOrphansToGroup(
      this.paymentGroupId,
      { orphanIds: Array.from(this.selectedOrphans) }
    ).subscribe({
      next: () => {
        this.adding = false;
        this.selectedOrphans.clear();
        this.selectAll = false;
        this.loadAvailableOrphans();
      },
      error: () => {
        this.adding = false;
      }
    });
  }

  onCancel(): void {
    if (this.paymentGroupId) {
      this.router.navigate(['/orphan-payments', this.paymentGroupId]);
    } else {
      this.router.navigate(['/orphan-payments']);
    }
  }

  // ==================== HELPERS ====================

  getSelectedCount(): number {
    return this.selectedOrphans.size;
  }

  getAvailableCount(): number {
    return this.availableOrphans.filter(o => !o.isAlreadyInGroup).length;
  }

  getAlreadyInGroupCount(): number {
    return this.availableOrphans.filter(o => o.isAlreadyInGroup).length;
  }

  isOrphanDisabled(orphan: OrphanForSelectionDto): boolean {
    return orphan.isAlreadyInGroup;
  }

  getDisabledReason(orphan: OrphanForSelectionDto): string {
    if (orphan.isAlreadyInGroup) {
      return this.translate.instant('orphanPayments.alreadyInGroup');
    }
    return '';
  }

  trackOrphan(index: number, orphan: OrphanForSelectionDto): string {
    return orphan.orphanId;
  }

  // Get charity/region/center names
  getCharityName(id: number): string {
    const charity = this.availableCharities.find(c => c.id === id);
    return charity ? charity.name : '-';
  }

  getRegionName(id: number): string {
    const region = this.availableRegions.find(r => r.id === id);
    return region ? region.name : '-';
  }

  getCenterName(id: number): string {
    const center = this.availableCenters.find(c => c.id === id);
    return center ? center.name : '-';
  }

  getSponsorshipBadgeClass(status: string): string {
    return status === 'Sponsored' ? 'badge-success' : 'badge-warning';
  }

  getGenderBadgeClass(gender: string): string {
    return gender === 'Male' ? 'badge-primary' : 'badge-info';
  }
}
