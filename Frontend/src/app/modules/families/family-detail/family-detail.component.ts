import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { FamilyDto, FatherDto, MotherDto, ProviderDto, OrphanDto, FamilyAttachmentDto } from '../models/family.model';
import { FamilyService } from '../services/family.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../shared/components';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { RouterModule } from '@angular/router';
import { SharedModule } from '../../../shared/shared.module';
import { Subject, Subscription } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

@Component({
  selector: 'app-family-detail',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    PageHeaderComponent,
    BreadcrumbComponent,
    TranslateModule,
    RouterModule,
    SharedModule
  ],
  templateUrl: './family-detail.component.html',
  styleUrls: ['./family-detail.component.scss']
})
export class FamilyDetailComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  family: FamilyDto | null = null;
  father: FatherDto | null = null;
  mother: MotherDto | null = null;
  provider: ProviderDto | null = null;
  orphans: OrphanDto[] = [];
  attachments: FamilyAttachmentDto[] = [];

  loading = false;
  loadingOrphans = false;
  loadingAttachments = false;

  familyId: string | null = null;

  pageActions = [
    {
      label: 'common.edit',
      type: 'primary',
      icon: 'fe-edit',
      click: () => this.editFamily()
    },
    {
      label: 'families.manageOrphans',
      type: 'info',
      icon: 'fe-user',
      click: () => this.manageOrphans()
    }
  ];

  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'families.title', url: '/families' },
    { label: 'families.familyDetails' }
  ];

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private familyService: FamilyService,
    private auth: AuthService,
    private notification: NotificationService,
    private translate: TranslateService
  ) {}

  // UC-FAM-07 — «تعديل اعضاء الاسرة» (members screen) is an HQ correction, permission-gated.
  canManageMembers = this.auth.hasPermission('Families.Members');

  // UC-FAM-09 — «طلب تعديل العائل»: the charity-side raise hook. The queue itself lives at
  // #/families/provider-requests; this button proposes a new guardian for THIS family.
  canRaiseProviderRequest = this.auth.hasPermission('Families.ProviderRequests');

  // Raise-modal state (house idiom: *ngIf-toggled modal-backdrop + modal-dialog divs).
  showProviderRequestModal = false;
  raisingProviderRequest = false;
  requestNewGuardianName = '';
  requestNewGuardianNationalId = '';
  requestRelationship = '';
  requestReason = '';

  openProviderRequestModal(): void {
    this.requestNewGuardianName = '';
    this.requestNewGuardianNationalId = '';
    this.requestRelationship = '';
    this.requestReason = '';
    this.showProviderRequestModal = true;
  }

  closeProviderRequestModal(): void {
    this.showProviderRequestModal = false;
  }

  submitProviderRequest(): void {
    if (this.raisingProviderRequest || !this.familyId) {
      return;
    }
    const name = this.requestNewGuardianName.trim();
    const nationalId = this.requestNewGuardianNationalId.trim();
    const relationship = this.requestRelationship.trim();
    const reason = this.requestReason.trim();
    if (!name || !nationalId || !relationship || !reason) {
      this.notification.error(this.translate.instant('families.providerRequests.allFieldsRequired'));
      return;
    }

    this.raisingProviderRequest = true;
    this.familyService
      .raiseGuardianChangeRequest(this.familyId, {
        newGuardianName: name,
        newGuardianNationalId: nationalId,
        relationship,
        reason
      })
      .subscribe({
        next: () => {
          this.raisingProviderRequest = false;
          this.showProviderRequestModal = false;
          this.notification.success(this.translate.instant('families.providerRequests.raiseSuccess'));
        },
        error: (error: any) => {
          this.raisingProviderRequest = false;
          console.error('Error raising guardian-change request:', error);
          // The pending-duplicate guard returns the literal legacy message — surface it verbatim.
          this.notification.error(error?.error?.message || error?.message ||
            this.translate.instant('families.providerRequests.raiseFailed'));
        }
      });
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.familyId = id;
      this.loadFamily(id);
      this.loadOrphans(id);
      this.loadAttachments(id);
    } else {
      this.router.navigate(['/families']);
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private getTranslation(key: string, params?: any): string {
    return this.translate.instant(key, params);
  }

  loadFamily(id: string): void {
    this.loading = true;
    this.familyService.getFamily(id).subscribe({
      next: (family: FamilyDto) => {
        this.family = family;
        this.father = family.father || null;
        this.mother = family.mother || null;
        this.provider = family.provider || null;
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error loading family:', error);
        this.notification.error(`Failed to load family: ${error.message || 'Unknown error'}`);
        this.loading = false;
      }
    });
  }

  loadOrphans(familyId: string): void {
    this.loadingOrphans = true;
    this.familyService.getFamilyOrphans(familyId).subscribe({
      next: (orphans: OrphanDto[]) => {
        this.orphans = orphans || [];
        this.loadingOrphans = false;
      },
      error: (error: any) => {
        console.error('Error loading orphans:', error);
        this.loadingOrphans = false;
      }
    });
  }

  loadAttachments(familyId: string): void {
    this.loadingAttachments = true;
    this.familyService.getFamilyAttachments(familyId).subscribe({
      next: (attachments: FamilyAttachmentDto[]) => {
        this.attachments = attachments || [];
        this.loadingAttachments = false;
      },
      error: (error: any) => {
        console.error('Error loading attachments:', error);
        this.loadingAttachments = false;
      }
    });
  }

  editFamily(): void {
    if (this.familyId) {
      this.router.navigate(['/families', this.familyId, 'edit']);
    }
  }

  manageOrphans(): void {
    if (this.familyId) {
      this.router.navigate(['/families', this.familyId, 'orphans']);
    }
  }

  viewOrphan(orphanId: string): void {
    if (this.familyId) {
      this.router.navigate(['/orphans', orphanId]);
    }
  }

  async deleteOrphan(orphan: OrphanDto): Promise<void> {
    const confirmed = await this.notification.confirm(
      this.getTranslation('orphans.confirmDelete', { name: orphan.fullName })
    );

    if (confirmed) {
      this.familyService.deleteOrphan(orphan.id).subscribe({
        next: () => {
          this.notification.success(this.getTranslation('orphans.deleteSuccess'));
          if (this.familyId) {
            this.loadOrphans(this.familyId);
          }
        },
        error: (error: any) => {
          console.error('Error deleting orphan:', error);
          this.notification.error(this.getTranslation('orphans.deleteFailed'));
        }
      });
    }
  }

  async toggleFamilyStatus(): Promise<void> {
    if (!this.family) return;

    const action = this.family.isActive ? 'deactivate' : 'activate';
    const confirmed = await this.notification.confirm(
      this.getTranslation(`families.confirm${action.charAt(0).toUpperCase() + action.slice(1)}`)
    );

    if (confirmed && this.familyId) {
      const action$ = this.family.isActive
        ? this.familyService.deactivateFamily(this.familyId)
        : this.familyService.activateFamily(this.familyId);

      action$.subscribe({
        next: () => {
          this.notification.success(this.getTranslation(`families.${action}Success`));
          if (this.familyId) {
            this.loadFamily(this.familyId);
          }
        },
        error: (error: any) => {
          console.error(`Error ${action}ing family:`, error);
          this.notification.error(this.getTranslation(`families.${action}Failed`));
        }
      });
    }
  }

  exportToPDF(): void {
    if (this.familyId) {
      this.familyService.exportFamilyToPDF(this.familyId).subscribe({
        next: (blob: Blob) => {
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = `family_${this.family?.code || 'details'}.pdf`;
          document.body.appendChild(a);
          a.click();
          document.body.removeChild(a);
          window.URL.revokeObjectURL(url);

          this.notification.success('Family exported successfully');
        },
        error: (error: any) => {
          console.error('Export failed:', error);
          this.notification.error('Failed to export family');
        }
      });
    }
  }

  addOrphan(): void {
    if (this.familyId) {
      this.router.navigate(['/orphans/create'], { queryParams: { familyId: this.familyId } });
    }
  }

  addFather(): void {
    if (this.familyId) {
      this.router.navigate(['/families', this.familyId, 'edit'], { fragment: 'father' });
    }
  }

  addMother(): void {
    if (this.familyId) {
      this.router.navigate(['/families', this.familyId, 'edit'], { fragment: 'mother' });
    }
  }

  addProvider(): void {
    if (this.familyId) {
      this.router.navigate(['/families', this.familyId, 'edit'], { fragment: 'provider' });
    }
  }

  addRelative(): void {
    if (this.familyId) {
      this.router.navigate(['/families', this.familyId, 'edit'], { fragment: 'relatives' });
    }
  }

  async deleteRelative(relative: any): Promise<void> {
    const confirmed = await this.notification.confirm(
      this.getTranslation('families.confirmDeleteRelative', { name: relative.fullName })
    );

    if (confirmed && relative.id && this.familyId) {
      const familyId = this.familyId;
      this.familyService.deleteRelative(familyId, relative.id).subscribe({
        next: () => {
          this.notification.success(this.getTranslation('families.relativeDeleteSuccess'));
          this.loadFamily(familyId);
        },
        error: (error: any) => {
          console.error('Error deleting relative:', error);
          this.notification.error(this.getTranslation('families.relativeDeleteFailed'));
        }
      });
    }
  }

  editRelative(relative: any): void {
    if (this.familyId && relative.id) {
      this.router.navigate(['/families', this.familyId, 'edit'], { fragment: `relatives/${relative.id}` });
    } else if (this.familyId) {
      this.router.navigate(['/families', this.familyId, 'edit'], { fragment: 'relatives' });
    }
  }

  getProviderTypeText(providerType?: string): string {
    switch (providerType) {
      case 'father': return this.getTranslation('families.providerTypeFather');
      case 'mother': return this.getTranslation('families.providerTypeMother');
      case 'other': return this.getTranslation('families.providerTypeOther');
      default: return '-';
    }
  }

  getLivingConditionText(livingCondition?: string): string {
    if (!livingCondition) return '-';
    switch (livingCondition) {
      case 'good': return this.getTranslation('families.livingConditionGood');
      case 'fair': return this.getTranslation('families.livingConditionFair');
      case 'poor': return this.getTranslation('families.livingConditionPoor');
      default: return livingCondition;
    }
  }

  getHousingTypeText(housingType?: string): string {
    if (!housingType) return '-';
    switch (housingType) {
      case 'owned': return this.getTranslation('families.housingTypeOwned');
      case 'rented': return this.getTranslation('families.housingTypeRented');
      case 'shared': return this.getTranslation('families.housingTypeShared');
      case 'other': return this.getTranslation('families.housingTypeOther');
      default: return housingType;
    }
  }

  calculateAge(dateOfBirth: string): number {
    const today = new Date();
    const birthDate = new Date(dateOfBirth);
    let age = today.getFullYear() - birthDate.getFullYear();
    const monthDiff = today.getMonth() - birthDate.getMonth();
    if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birthDate.getDate())) {
      age--;
    }
    return age;
  }

  getOrphanTypeBadgeClass(orphanType?: string): string {
    switch (orphanType) {
      case 'father-deceased': return 'badge-warning';
      case 'mother-deceased': return 'badge-info';
      case 'both-deceased': return 'badge-danger';
      default: return 'badge-secondary';
    }
  }

  getSponsorshipStatusBadgeClass(status?: string): string {
    switch (status) {
      case 'sponsored': return 'badge-success';
      case 'unsponsored': return 'badge-warning';
      case 'pending': return 'badge-info';
      default: return 'badge-secondary';
    }
  }
}
