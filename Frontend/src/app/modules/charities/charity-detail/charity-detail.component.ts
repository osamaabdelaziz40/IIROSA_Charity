import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { CharityDto } from '../models/charity.model';
import { CharityService } from '../services/charity.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../shared/components';
import { FileViewerComponent } from '../../../shared/components/file-viewer';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-charity-detail',
  standalone: true,
  imports: [CommonModule, PageHeaderComponent, BreadcrumbComponent, TranslateModule, FileViewerComponent],
  templateUrl: './charity-detail.component.html',
  styleUrls: ['./charity-detail.component.scss']
})
export class CharityDetailComponent implements OnInit {
  charity: CharityDto | null = null;
  loading = false;
  isMyProfile = false;
  canEdit = false;

  // User roles
  userRole: string | null = null;

  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'charities.title', url: '/charities' },
    { label: 'charities.charityDetails' }
  ];

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private charityService: CharityService,
    private notification: NotificationService,
    private translate: TranslateService
  ) {}

  private getTranslation(key: string, params?: any): string {
    return this.translate.instant(key, params);
  }

  ngOnInit(): void {
    this.userRole = localStorage.getItem('user_role');
    this.canEdit = this.userRole === 'SuperAdmin' || this.userRole === 'Admin';

    this.checkProfileMode();
  }

  private checkProfileMode(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id === 'my-profile' || id === 'profile') {
      this.isMyProfile = true;
      this.loadMyProfile();
    } else if (id) {
      this.loadCharity(id);
    } else {
      this.router.navigate(['/charities']);
    }
  }

  private loadMyProfile(): void {
    this.loading = true;
    this.charityService.getMyCharityProfile().subscribe({
      next: (charity: CharityDto) => {
        this.charity = charity;
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error loading profile:', error);
        this.notification.error(this.getTranslation('charities.loadProfileFailed'));
        this.loading = false;
      }
    });
  }

  private loadCharity(id: string): void {
    this.loading = true;
    this.charityService.getCharity(id).subscribe({
      next: (charity: CharityDto) => {
        this.charity = charity;
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error loading charity:', error);
        this.notification.error(this.getTranslation('charities.loadCharityFailed'));
        this.loading = false;
      }
    });
  }

  editCharity(): void {
    if (this.charity) {
      this.router.navigate(['/charities', this.charity.id, 'edit']);
    }
  }

  async activateCharity(): Promise<void> {
    if (!this.charity) return;

    const confirmed = await this.notification.confirm(this.getTranslation('charities.confirmActivate'));
    if (confirmed) {
      this.charityService.activateCharity(this.charity.id).subscribe({
        next: () => {
          this.notification.success(this.getTranslation('charities.activateSuccess'));
          if (this.charity) {
            this.charity.isActive = true;
          }
        },
        error: (error: any) => {
          console.error('Error activating charity:', error);
          this.notification.error(this.getTranslation('charities.activateFailed'));
        }
      });
    }
  }

  async deactivateCharity(): Promise<void> {
    if (!this.charity) return;

    const confirmed = await this.notification.confirm(this.getTranslation('charities.confirmDeactivate'));
    if (confirmed) {
      this.charityService.deactivateCharity(this.charity.id).subscribe({
        next: () => {
          this.notification.success(this.getTranslation('charities.deactivateSuccess'));
          if (this.charity) {
            this.charity.isActive = false;
          }
        },
        error: (error: any) => {
          console.error('Error deactivating charity:', error);
          this.notification.error(this.getTranslation('charities.deactivateFailed'));
        }
      });
    }
  }

  async lockCharity(): Promise<void> {
    if (!this.charity) return;

    const confirmed = await this.notification.confirm(this.getTranslation('charities.confirmLock'));
    if (confirmed) {
      this.charityService.lockCharity(this.charity.id).subscribe({
        next: () => {
          this.notification.success(this.getTranslation('charities.lockSuccess'));
          if (this.charity) {
            this.charity.isLocked = true;
          }
        },
        error: (error: any) => {
          console.error('Error locking charity:', error);
          this.notification.error(this.getTranslation('charities.lockFailed'));
        }
      });
    }
  }

  async unlockCharity(): Promise<void> {
    if (!this.charity) return;

    const confirmed = await this.notification.confirm(this.getTranslation('charities.confirmUnlock'));
    if (confirmed) {
      this.charityService.unlockCharity(this.charity.id).subscribe({
        next: () => {
          this.notification.success(this.getTranslation('charities.unlockSuccess'));
          if (this.charity) {
            this.charity.isLocked = false;
          }
        },
        error: (error: any) => {
          console.error('Error unlocking charity:', error);
          this.notification.error(this.getTranslation('charities.unlockFailed'));
        }
      });
    }
  }

  async resetPassword(): Promise<void> {
    if (!this.charity) return;

    const confirmed = await this.notification.confirm(this.getTranslation('charities.confirmResetPassword'));
    if (confirmed) {
      this.charityService.resetPassword({
        charityId: this.charity.id,
        sendEmail: true
      }).subscribe({
        next: () => {
          this.notification.success(this.getTranslation('charities.resetPasswordSuccess'));
        },
        error: (error: any) => {
          console.error('Error resetting password:', error);
          this.notification.error(this.getTranslation('charities.resetPasswordFailed'));
        }
      });
    }
  }

  async toggleAddRights(): Promise<void> {
    if (!this.charity) return;

    const action = this.charity.isAddEnabled ? 'disable' : 'enable';
    const confirmed = await this.notification.confirm(
      this.getTranslation(`charities.confirm${action.charAt(0).toUpperCase() + action.slice(1)}AddRights`)
    );

    if (confirmed) {
      const action$ = this.charity.isAddEnabled
        ? this.charityService.disableAddRights(this.charity.id)
        : this.charityService.enableAddRights(this.charity.id);

      action$.subscribe({
        next: () => {
          this.notification.success(this.getTranslation(`charities.addRights${action.charAt(0).toUpperCase() + action.slice(1)}Success`));
          if (this.charity) {
            this.charity.isAddEnabled = !this.charity.isAddEnabled;
          }
        },
        error: (error: any) => {
          console.error(`Error ${action}ing add rights:`, error);
          this.notification.error(this.getTranslation(`charities.addRights${action.charAt(0).toUpperCase() + action.slice(1)}Failed`));
        }
      });
    }
  }

  async toggleUpdateRights(): Promise<void> {
    if (!this.charity) return;

    const action = this.charity.isUpdateEnabled ? 'disable' : 'enable';
    const confirmed = await this.notification.confirm(
      this.getTranslation(`charities.confirm${action.charAt(0).toUpperCase() + action.slice(1)}UpdateRights`)
    );

    if (confirmed) {
      const action$ = this.charity.isUpdateEnabled
        ? this.charityService.disableUpdateRights(this.charity.id)
        : this.charityService.enableUpdateRights(this.charity.id);

      action$.subscribe({
        next: () => {
          this.notification.success(this.getTranslation(`charities.updateRights${action.charAt(0).toUpperCase() + action.slice(1)}Success`));
          if (this.charity) {
            this.charity.isUpdateEnabled = !this.charity.isUpdateEnabled;
          }
        },
        error: (error: any) => {
          console.error(`Error ${action}ing update rights:`, error);
          this.notification.error(this.getTranslation(`charities.updateRights${action.charAt(0).toUpperCase() + action.slice(1)}Failed`));
        }
      });
    }
  }

  goBack(): void {
    if (this.isMyProfile) {
      this.router.navigate(['/dashboard']);
    } else {
      this.router.navigate(['/charities']);
    }
  }

  getStatusBadgeClass(): string {
    if (!this.charity) return '';
    if (this.charity.isLocked) return 'badge-danger';
    if (!this.charity.isActive) return 'badge-warning';
    return 'badge-success';
  }

  getStatusText(): string {
    if (!this.charity) return '';
    if (this.charity.isLocked) return this.getTranslation('charities.locked');
    if (!this.charity.isActive) return this.getTranslation('common.inactive');
    return this.getTranslation('common.active');
  }

  /**
   * Get the first icon attachment for display
   */
  getIconAttachment(): any {
    if (this.charity?.icon_Attach && this.charity.icon_Attach.length > 0) {
      return this.charity.icon_Attach[0];
    }
    return null;
  }
}
