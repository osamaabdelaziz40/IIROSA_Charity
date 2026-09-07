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
import { CharityService } from '../../charities/services/charity.service';
import { LookupManagementService } from '../../lookup-management/services/lookup-management.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PaginationComponent } from '../../../shared/components';

@Component({
  selector: 'app-add-orphans-to-group',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule, RouterModule, PaginationComponent],
  templateUrl: './add-orphans-to-group.component.html',
  styleUrls: ['./add-orphans-to-group.component.scss']
})
export class AddOrphansToGroupComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // Data
  paymentGroupId: string | null = null;
  paymentGroup: OrphanPaymentDto | null = null;
  availableOrphans: OrphanForSelectionDto[] = [];
  totalAvailable = 0;
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
  availableCharities: { id: string; name: string }[] = [];
  availableRegions: { id: number; name: string }[] = [];
  availableCenters: { id: number; name: string }[] = [];

  // UI State
  selectAll = false;

  // Review P18: server-side paging of the available-orphans read — the previous hidden
  // 200-row cap silently made orphans 201+ unenrollable.
  availablePage = 1;
  availablePageSize = 10;

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private orphanPaymentService: OrphanPaymentService,
    private charityService: CharityService,
    private lookupService: LookupManagementService,
    private notificationService: NotificationService,
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
    this.orphanPaymentService.getOrphanPayment(this.paymentGroupId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
      next: (data) => {
        this.paymentGroup = data;
        // 10-2 trim: no group-level filter pre-fill — the server never persisted the
        // create form's charity/region/center pre-selection, so it could never echo back.
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
    this.orphanPaymentService.getAvailableOrphans(this.paymentGroupId, {
      ...this.filter,
      pageNumber: this.availablePage,
      pageSize: this.availablePageSize
    })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.availableOrphans = data.items || [];
          this.totalAvailable = data.totalCount || 0;
          this.loadingOrphans = false;
        },
        error: () => {
          this.loadingOrphans = false;
        }
      });
  }

  /**
   * Filter dropdowns load from the live verticals — charities from /api/Charities (the
   * families-screen idiom), regions/centers from /api/LookupManagement — never from the
   * removed /api/OrphanPayments/filter-options routes (they never existed server-side).
   */
  private loadFilterOptions(): void {
    this.charityService.getCharities({ pageNumber: 1, pageSize: 500 })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: response => {
          this.availableCharities = (response.items || []).map(c => ({ id: c.id, name: c.name }));
        }
      });

    this.lookupService.getRegions()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => {
          this.availableRegions = (result.items || []).map(r => ({ id: r.id, name: r.nameAr || r.name }));
        }
      });

    this.lookupService.getCenters()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => {
          this.availableCenters = (result.items || []).map(c => ({ id: c.id, name: c.nameAr || c.name }));
        }
      });
  }

  // ==================== FILTERS ====================

  onSearch(): void {
    this.availablePage = 1;
    this.loadAvailableOrphans();
  }

  onResetFilters(): void {
    this.filter = {
      searchTerm: '',
      charityId: undefined,
      regionId: undefined,
      centerId: undefined,
      sponsorshipStatus: 'All',
      ageFrom: undefined,
      ageTo: undefined,
      gender: 'All'
    };
    this.availablePage = 1;
    this.loadAvailableOrphans();
  }

  /** Review P18: the shared pager drives the server-side available-orphans read. */
  onAvailablePageChange(page: number): void {
    this.availablePage = page;
    this.loadAvailableOrphans();
  }

  // ==================== SELECTION ====================

  onToggleSelectAll(): void {
    if (this.selectAll) {
      this.availableOrphans.forEach(orphan => {
        if (!orphan.isInGroup) {
          this.selectedOrphans.add(orphan.id);
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
    const availableCount = this.availableOrphans.filter(o => !o.isInGroup).length;
    this.selectAll = this.selectedOrphans.size === availableCount && availableCount > 0;
  }

  isOrphanSelected(orphanId: string): boolean {
    return this.selectedOrphans.has(orphanId);
  }

  // ==================== ACTIONS ====================

  onAddSelectedOrphans(): void {
    if (this.selectedOrphans.size === 0) {
      // Review P22: module standard is NotificationService, not window.alert
      this.notificationService.warning(this.translate.instant('orphanPayments.selectAtLeastOneOrphan'));
      return;
    }

    if (!this.paymentGroupId) return;

    this.adding = true;
    this.orphanPaymentService.addOrphansToGroup(
      this.paymentGroupId,
      { orphanIds: Array.from(this.selectedOrphans) }
    )
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          this.adding = false;
          this.selectedOrphans.clear();
          this.selectAll = false;
          this.availablePage = 1;
          this.loadAvailableOrphans();
          // UC-5.3 alternative flow: already-in-group orphans are reported as skipped
          // (review P22: NotificationService, not window.alert)
          this.notificationService.success(this.translate.instant('orphanPayments.orphansAddedResult', {
            added: result.addedCount,
            skipped: result.skippedCount
          }));
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
    return this.availableOrphans.filter(o => !o.isInGroup).length;
  }

  getAlreadyInGroupCount(): number {
    return this.availableOrphans.filter(o => o.isInGroup).length;
  }

  isOrphanDisabled(orphan: OrphanForSelectionDto): boolean {
    return orphan.isInGroup;
  }

  getDisabledReason(orphan: OrphanForSelectionDto): string {
    if (orphan.isInGroup) {
      return this.translate.instant('orphanPayments.alreadyInGroup');
    }
    return '';
  }

  trackOrphan(index: number, orphan: OrphanForSelectionDto): string {
    return orphan.id;
  }

  trackById(index: number, item: { id: string | number }): string | number {
    return item.id;
  }

  // Get charity/region/center names
  getCharityName(id: string): string {
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
