import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { FamilyService } from '../services/family.service';
import { CharityService } from '../../charities/services/charity.service';
import { NotificationService } from '../../../core/services/notification.service';
import { AuthService } from '../../../core/services/auth.service';
import {
  GuardianChangeRequestRow,
  GUARDIAN_REQUEST_STATUS
} from '../models/family.model';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';

/**
 * Guardian-change review queue (UC-FAM-09 طلبات تعديل العائل, module spec §10.S.4) with the
 * head-office decision (UC-FAM-10 اعتماد تعديل العائل).
 *
 * Lists the requests with old and new guardian snapshots side by side — the reviewer compares
 * without opening the family file. Pending by default; HQ (SuperAdmin/Admin) additionally filters
 * by charity, while a Charity-role caller is scoped to its own rows server-side. The موافقه
 * column opens the decision modal (approve / refuse + reason) for the General Director
 * (SuperAdmin); decided rows show their state and, when refused, the reason.
 */
@Component({
  selector: 'app-provider-request-list',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, FormsModule, RouterModule, TranslateModule, PageHeaderComponent, PaginationComponent],
  templateUrl: './provider-request-list.component.html',
  styleUrls: ['./provider-request-list.component.scss']
})
export class ProviderRequestListComponent implements OnInit {
  requests: GuardianChangeRequestRow[] = [];
  loading = false;

  // Filter bar state — status defaults to the pending queue view.
  statusFilter = GUARDIAN_REQUEST_STATUS.Pending;
  charityFilter = 'all';
  charityOptions: { id: string; name: string }[] = [];
  isHQ = false;

  // UC-FAM-10 decision modal state — the General Director (SuperAdmin) decides from the queue.
  canDecide = false;
  decisionModalOpen = false;
  deciding = false;
  decisionTarget: GuardianChangeRequestRow | null = null;
  decisionIsApproved = true;
  decisionReason = '';

  // Paging.
  currentPage = 1;
  pageSize = 10;
  totalCount = 0;

  statusOptions: { value: number; labelKey: string }[] = [
    { value: GUARDIAN_REQUEST_STATUS.Pending, labelKey: 'families.providerRequests.statusPending' },
    { value: GUARDIAN_REQUEST_STATUS.Approved, labelKey: 'families.providerRequests.statusApproved' },
    { value: GUARDIAN_REQUEST_STATUS.Rejected, labelKey: 'families.providerRequests.statusRejected' }
  ];

  constructor(
    private familyService: FamilyService,
    private charityService: CharityService,
    private notification: NotificationService,
    private auth: AuthService,
    private translate: TranslateService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.isHQ = this.auth.hasAnyRole(['SuperAdmin', 'Admin']);
    this.canDecide = this.auth.hasAnyRole(['SuperAdmin']);
    if (this.isHQ) {
      this.loadCharityOptions();
    }
    this.load();
  }

  load(): void {
    this.loading = true;
    this.familyService
      .getProviderRequests({
        status: this.statusFilter,
        charityId: this.isHQ && this.charityFilter !== 'all' ? this.charityFilter : undefined,
        pageNumber: this.currentPage,
        pageSize: this.pageSize
      })
      .subscribe({
        next: result => {
          this.requests = result.items || [];
          this.totalCount = result.totalCount || 0;
          this.loading = false;
          // OnPush: nothing inside this callback originates from a template event, so the
          // view is never marked — without this the queue stays on the spinner.
          this.cdr.markForCheck();
        },
        error: (error: any) => {
          this.loading = false;
          console.error('Error loading guardian-change requests:', error);
          this.notification.error(error?.error?.message || error?.message || this.getTranslation('families.providerRequests.loadFailed'));
          this.cdr.markForCheck();
        }
      });
  }

  onStatusChange(): void {
    this.currentPage = 1;
    this.load();
  }

  onCharityChange(): void {
    this.currentPage = 1;
    this.load();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.load();
  }

  isPending(request: GuardianChangeRequestRow): boolean {
    return request.status === GUARDIAN_REQUEST_STATUS.Pending;
  }

  isApproved(request: GuardianChangeRequestRow): boolean {
    return request.status === GUARDIAN_REQUEST_STATUS.Approved;
  }

  isRejected(request: GuardianChangeRequestRow): boolean {
    return request.status === GUARDIAN_REQUEST_STATUS.Rejected;
  }

  // ==================== UC-FAM-10: head-office decision ====================

  /** Open the decision modal on a pending request — approve is the pre-selected choice. */
  openDecision(request: GuardianChangeRequestRow): void {
    if (!this.isPending(request) || !this.canDecide) {
      return;
    }
    this.decisionTarget = request;
    this.decisionIsApproved = true;
    this.decisionReason = '';
    this.decisionModalOpen = true;
  }

  closeDecision(): void {
    this.decisionModalOpen = false;
    this.decisionTarget = null;
    this.decisionReason = '';
  }

  /** Record the decision: approve applies the guardian to the family; a refusal needs a reason. */
  async submitDecision(): Promise<void> {
    const target = this.decisionTarget;
    if (!target || this.deciding) {
      return;
    }

    // Client mirror of the server validator (AC 3) — a refusal is not accepted without a reason.
    if (!this.decisionIsApproved && !this.decisionReason.trim()) {
      this.notification.error(this.getTranslation('families.providerRequests.reasonRequired'));
      return;
    }

    // Set BEFORE the awaited confirm (double-click = two confirms = two POSTs otherwise) and
    // BEFORE the Swal promise runs outside Angular's template (OnPush view needs re-marking).
    this.deciding = true;
    this.cdr.markForCheck();
    const approve = this.decisionIsApproved;
    const reason = this.decisionReason.trim();
    const confirmed = await this.notification.confirm(
      this.getTranslation(
        approve
          ? 'families.providerRequests.confirmApprove'
          : 'families.providerRequests.confirmReject',
        { guardian: target.newGuardianName, family: target.familyCode || '-' }
      )
    );
    if (!confirmed) {
      this.deciding = false;
      this.cdr.markForCheck();
      return;
    }

    this.familyService
      .decideProviderRequest(target.id, {
        isApproved: approve,
        rejectionReason: approve ? undefined : reason
      })
      .subscribe({
        next: () => {
          this.deciding = false;
          this.notification.success(
            this.getTranslation(
              approve
                ? 'families.providerRequests.approveSuccess'
                : 'families.providerRequests.rejectSuccess'
            )
          );
          this.closeDecision();
          // The decided row leaves the pending view — pull the page back in range first.
          if (this.requests.length === 1 && this.currentPage > 1) {
            this.currentPage--;
          }
          this.load();
          this.cdr.markForCheck();
        },
        error: (error: any) => {
          this.deciding = false;
          console.error('Error deciding guardian-change request:', error);
          // Already-decided refusals and duplicate-guardian blocks surface here (AC 5).
          this.notification.error(
            error?.error?.message || error?.message || this.getTranslation('families.providerRequests.decideFailed')
          );
          this.cdr.markForCheck();
        }
      });
  }

  private loadCharityOptions(): void {
    this.charityService.getCharities({ pageNumber: 1, pageSize: 500 }).subscribe({
      next: (response: any) => {
        this.charityOptions = [
          { id: 'all', name: this.getTranslation('families.providerRequests.allCharities') },
          ...(response.items || []).map((c: any) => ({ id: c.id, name: c.name }))
        ];
        this.cdr.markForCheck();
      },
      error: (error: any) => console.error('Error loading charities:', error)
    });
  }

  trackByRequestId(index: number, item: GuardianChangeRequestRow): string {
    return item.id;
  }

  trackByStatus(index: number, item: { value: number; labelKey: string }): number {
    return item.value;
  }

  trackByOption(index: number, item: { id: string; name: string }): string {
    return item.id;
  }

  private getTranslation(key: string, params?: any): string {
    return this.translate.instant(key, params);
  }
}
