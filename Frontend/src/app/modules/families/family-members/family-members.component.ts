import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Subscription } from 'rxjs';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { FamilyService } from '../services/family.service';
import { NotificationService } from '../../../core/services/notification.service';
import { AuthService } from '../../../core/services/auth.service';
import { FamilyDto, OrphanDto } from '../models/family.model';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';

/**
 * Family members screen (UC-FAM-07 نقل يتيم بين الأسر + UC-FAM-08 نقل عائل بين الأسر,
 * module spec §10.S.3).
 *
 * Lists the family's members with the specified columns (الاسم · الرقم القومى · الصفة · الكود ·
 * عمليات) and hosts the move modal «هل انت متاكد من نقل ؟»: entering a target family code
 * attaches the member to that family (action 1); leaving it empty detaches the member to a new
 * holding family, for which a justification is required (action 0). Orphans move as memberType 1,
 * the guardian/provider as memberType 2; father and mother rows render read-only (the spec moves
 * orphans and guardians only). BR-06 ruling 2026-08-24: the acting guardian (providerType
 * "Other") MAY move — the server vacates the seat and the confirm warns that the family is
 * left without a guardian of record until HQ approves one (5-9/5-10).
 *
 * UC-FAM-13 حذف كفالة العائل: HQ roles also get the delete-link action on the guardian row —
 * the «هل انت متاكد من حذف ؟» modal with the optional التعليق removes the guardian's
 * sponsorship link (soft delete; refused server-side while a live sponsorship exists).
 */
@Component({
  selector: 'app-family-members',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, TranslateModule, PageHeaderComponent],
  templateUrl: './family-members.component.html',
  styleUrls: ['./family-members.component.scss']
})
export class FamilyMembersComponent implements OnInit, OnDestroy {
  familyId = '';
  family?: FamilyDto;
  orphans: OrphanDto[] = [];
  loading = false;
  /** True when the members load failed — the grid then shows an error row, not the empty state
   * (an error and a genuinely member-less family must not look the same). */
  loadError = false;

  // Move modal state (one modal, both actions — the legacy contract). The target is the generic
  // member shape: orphans (memberType 1, UC-FAM-07) and the guardian/provider row (memberType 2,
  // UC-FAM-08) share the modal; father/mother rows render read-only.
  showMoveModal = false;
  moveTarget?: { id: string; fullName: string };
  moveTargetMemberType = 1;
  moving = false;
  moveTargetCode = '';
  moveJustification = '';

  // Delete-link modal state (UC-FAM-13, HQ roles only — the endpoint authorises too).
  canRemoveLink = false;
  showDeleteModal = false;
  removing = false;
  deleteComment = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private familyService: FamilyService,
    private notification: NotificationService,
    private translate: TranslateService,
    private auth: AuthService
  ) {
    this.canRemoveLink = this.auth.hasAnyRole(['SuperAdmin', 'Admin']);
  }

  private paramMapSub?: Subscription;

  ngOnInit(): void {
    this.paramMapSub = this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (id) {
        this.familyId = id;
        this.load();
      }
    });
  }

  ngOnDestroy(): void {
    this.paramMapSub?.unsubscribe();
  }

  load(): void {
    this.loading = true;
    this.loadError = false;
    this.familyService.getFamily(this.familyId).subscribe({
      next: family => {
        this.family = family;
        this.loading = false;
      },
      error: (error: any) => {
        this.loading = false;
        this.loadError = true;
        console.error('Error loading family:', error);
        this.notification.error(error?.error?.message || error?.message || this.getTranslation('families.members.loadFailed'));
      }
    });

    this.familyService.getFamilyOrphans(this.familyId).subscribe({
      next: orphans => (this.orphans = orphans || []),
      error: (error: any) => {
        this.loadError = true;
        console.error('Error loading family orphans:', error);
        this.notification.error(error?.error?.message || error?.message || this.getTranslation('families.members.loadFailed'));
      }
    });
  }

  openMoveModal(member: { id: string; fullName: string }, memberType: number): void {
    this.moveTarget = member;
    this.moveTargetMemberType = memberType;
    this.moveTargetCode = '';
    this.moveJustification = '';
    this.showMoveModal = true;
  }

  closeMoveModal(): void {
    this.showMoveModal = false;
    this.moveTarget = undefined;
    this.moveTargetMemberType = 1;
    this.moveTargetCode = '';
    this.moveJustification = '';
  }

  async submitMove(): Promise<void> {
    const target = this.moveTarget;
    if (!target || this.moving) {
      return;
    }

    const code = this.moveTargetCode.trim();
    const justification = this.moveJustification.trim();
    if (!code && !justification) {
      this.notification.error(this.getTranslation('families.members.codeOrJustificationRequired'));
      return;
    }

    // BR-06 ruling 2026-08-24: moving the family's ACTING guardian (providerType "Other") is
    // allowed — the server vacates the seat, so the family is left without a guardian of record
    // until HQ approves a replacement through the 5-9/5-10 raise-and-approve flow. The confirm
    // swaps to the stranded-family warning so the operator decides with the consequence stated.
    const vacatesGuardianSeat =
      this.moveTargetMemberType === 2 && (this.family?.providerType ?? '').toLowerCase() === 'other';

    // Set BEFORE the awaited confirm: the guard at the top must hold across the SweetAlert2
    // promise too, or a double-click opens two confirms and fires two POSTs. `target` is
    // captured because closing the modal clears moveTarget while the dialog is open.
    this.moving = true;
    const confirmed = await this.notification.confirm(
      this.getTranslation(
        vacatesGuardianSeat ? 'families.members.moveGuardianConfirm' : 'families.members.moveConfirm',
        {
          name: target.fullName || '-',
          target: code || this.getTranslation('families.members.newHoldingFamily')
        }
      )
    );
    if (!confirmed) {
      this.moving = false;
      return;
    }

    // Legacy action contract: a code attaches to that family (1); no code detaches to a new
    // holding family under the same charity (0) with the justification recorded on the member.
    this.familyService
      .controlMember(this.familyId, target.id, {
        memberType: this.moveTargetMemberType,
        action: code ? 1 : 0,
        targetFamilyCode: code || undefined,
        justification: justification || undefined
      })
      .subscribe({
        next: () => {
          this.moving = false;
          this.closeMoveModal();
          this.notification.success(this.getTranslation('families.members.moveSuccess', { name: target.fullName || '-' }));
          this.load();
        },
        error: (error: any) => {
          this.moving = false;
          console.error('Error moving member:', error);
          this.notification.error(error?.error?.message || error?.message || this.getTranslation('families.members.moveFailed'));
        }
      });
  }

  goToFamily(): void {
    this.router.navigate(['/families', this.familyId]);
  }

  openDeleteModal(): void {
    this.deleteComment = '';
    this.showDeleteModal = true;
  }

  closeDeleteModal(): void {
    this.showDeleteModal = false;
    this.deleteComment = '';
  }

  async submitRemoveLink(): Promise<void> {
    if (this.removing) {
      return;
    }

    const guardianName = this.family?.provider?.fullName || '-';
    // Same pre-confirm guard as submitMove — the double-confirm window must not slip through.
    this.removing = true;
    const confirmed = await this.notification.confirm(
      this.getTranslation('families.members.removeLinkConfirm', { name: guardianName })
    );
    if (!confirmed) {
      this.removing = false;
      return;
    }

    this.familyService
      .removeProviderSponsorLink(this.familyId, this.deleteComment.trim() || undefined)
      .subscribe({
        next: () => {
          this.removing = false;
          this.closeDeleteModal();
          this.notification.success(this.getTranslation('families.members.removeLinkSuccess', { name: guardianName }));
          this.load();
        },
        error: (error: any) => {
          this.removing = false;
          console.error('Error removing guardian sponsorship link:', error);
          this.notification.error(error?.error?.message || error?.message || this.getTranslation('families.members.removeLinkFailed'));
        }
      });
  }

  trackByOrphan(index: number, item: OrphanDto): string {
    return item.id;
  }

  private getTranslation(key: string, params?: any): string {
    return this.translate.instant(key, params);
  }
}
