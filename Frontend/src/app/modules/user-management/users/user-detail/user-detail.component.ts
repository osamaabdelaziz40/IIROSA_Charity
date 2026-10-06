import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { UserManagementService } from '../../services/user-management.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { User } from '../../../../core/models/user.model';
import { Role } from '../../../../core/models/role.model';
import { Observable } from 'rxjs';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header.component';
import { LoadingComponent } from '../../../../shared/components/loading/loading.component';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { AppDatePipe } from '../../../../shared/pipes/date.pipe';
import { SharedPipesModule } from '../../../../shared/pipes/shared-pipes.module';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../../shared/components';

@Component({
  selector: 'app-user-detail',
  standalone: true,
  imports: [CommonModule, PageHeaderComponent, LoadingComponent, TranslateModule, AppDatePipe, SharedPipesModule, BreadcrumbComponent],
  templateUrl: './user-detail.component.html',
  styleUrls: ['./user-detail.component.scss']
})
export class UserDetailComponent implements OnInit {
  // Breadcrumb items
  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'userManagement.title', url: '/user-management/users' },
    { label: 'userManagement.userDetails' }
  ];
  user?: User;
  userRoles: Role[] = [];
  loading = false;
  userId?: string;
  showActivityLog = false;
  showClaims = false;
  userActivities: any[] = [];
  userClaims: any[] = [];
  loadingActivities = false;
  loadingClaims = false;

  pageActions = [
    {
      label: 'common.back',
      type: 'secondary',
      icon: 'fe-arrow-left',
      click: () => this.goBack()
    },
    {
      label: 'userManagement.manageRoles',
      type: 'primary',
      icon: 'fe-shield',
      click: () => this.manageRoles()
    },
    {
      label: 'userManagement.editUser',
      type: 'warning',
      icon: 'fe-edit',
      click: () => this.editUser()
    }
  ];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private userManagementService: UserManagementService,
    private notification: NotificationService,
    private translate: TranslateService
  ) {}

  ngOnInit() {
    // Page actions are now directly bound

    this.userId = this.route.snapshot.params['id'];
    this.loadUser();
    this.loadUserRoles();
  }

  loadUser() {
    this.loading = true;
    this.userManagementService.getUserById(this.userId!).subscribe({
      next: (user: User) => {
        this.user = user;
        this.loading = false;
      },
      error: (err) => {
        console.log('Full error:', err);
        console.log('Status:', err.status);
        console.log('Message:', err.message);
        console.log('Error body:', err.error);

        this.notification.error(err?.error?.message || 'Failed to load user');
        this.loading = false;
      }
    });
  }

  loadUserRoles() {
    // Since getUserRoles doesn't exist in the service, we'll use user.roles array
    if (this.user && this.user.roles) {
      // Convert role names to Role objects
      this.userRoles = this.user.roles.map(roleName => ({
        id: roleName,
        name: roleName,
        isSystemRole: false,
        userCount: 0,
        permissions: []
      }));
    }
  }

  goBack() {
    this.router.navigate(['/user-management/users']);
  }

  editUser() {
    if (this.userId) {
      this.router.navigate(['/user-management/users', this.userId, 'edit']);
    }
  }

  manageRoles() {
    if (this.userId) {
      this.router.navigate(['/user-management/users', this.userId, 'roles']);
    }
  }

  toggleUserStatus() {
    if (!this.user) return;

    const action = this.user.isActive ? 'deactivate' : 'activate';
    if (confirm(`Are you sure you want to ${action} this user?`)) {
      const action$ = this.user.isActive
        ? this.userManagementService.deactivateUser(this.userId!)
        : this.userManagementService.activateUser(this.userId!);

      action$.subscribe({
        next: () => {
          this.notification.success(`User ${action}d successfully`);
          this.loadUser();
        },
        error: () => {
          this.notification.error(`Failed to ${action} user`);
        }
      });
    }
  }

  /**
   * UC-1.5 — resets to a server-generated password shown once in a modal, mirroring the users
   * grid. (The previous version set a hard-coded password and claimed it had been e-mailed —
   * no mailer exists, and a fixed password handed nothing to the operator.)
   */
  async resetPassword() {
    if (!this.user) return;

    const confirmed = await this.notification.confirm(
      this.translate.instant('userManagement.confirmResetPassword', {
        name: this.user.fullName || this.user.email
      })
    );
    if (!confirmed) return;

    this.userManagementService.resetPassword(this.userId!).subscribe({
      next: (result) => {
        this.notification.showGeneratedPassword({
          title: this.translate.instant('userManagement.passwordReset'),
          loginLabel: this.translate.instant('userManagement.email'),
          login: this.user?.email,
          passwordLabel: this.translate.instant('userManagement.newPassword'),
          password: result.newPassword,
          copyLabel: this.translate.instant('common.copy'),
          copiedLabel: this.translate.instant('common.copied'),
          closeLabel: this.translate.instant('common.close'),
          note: `${this.translate.instant('userManagement.credentialsMessage')} ${this.translate.instant(
            'userManagement.credentialsWarning'
          )}`
        });
      },
      error: (error) => {
        console.error('Error resetting password:', error);
        const failed = this.translate.instant('userManagement.resetPasswordFailed');
        this.notification.error(error?.message ? `${failed}: ${error.message}` : failed);
      }
    });
  }

  viewActivity() {
    this.showActivityLog = !this.showActivityLog;
    this.showClaims = false;

    if (this.showActivityLog) {
      this.loadUserActivities();
    }
  }

  manageClaims() {
    this.showClaims = !this.showClaims;
    this.showActivityLog = false;

    if (this.showClaims) {
      this.loadUserClaims();
    }
  }

  loadUserActivities() {
    this.loadingActivities = true;
    this.userManagementService.getUserActivity(this.userId!).subscribe({
      next: (response: any) => {
        this.userActivities = response.activities || [];
        this.loadingActivities = false;
      },
      error: () => {
        this.notification.error('Failed to load user activity');
        this.loadingActivities = false;
      }
    });
  }

  loadUserClaims() {
    this.loadingClaims = true;
    this.userManagementService.getUserClaims(this.userId!).subscribe({
      next: (response: any) => {
        this.userClaims = response.claims || [];
        this.loadingClaims = false;
      },
      error: () => {
        this.notification.error('Failed to load user claims');
        this.loadingClaims = false;
      }
    });
  }

  addClaim(claimType: string, claimValue: string) {
    this.userManagementService.addUserClaim(this.userId!, claimType, claimValue).subscribe({
      next: () => {
        this.notification.success('Claim added successfully');
        this.loadUserClaims();
      },
      error: () => {
        this.notification.error('Failed to add claim');
      }
    });
  }

  removeClaim(claimType: string) {
    if (confirm(`Are you sure you want to remove claim "${claimType}"?`)) {
      this.userManagementService.removeUserClaim(this.userId!, claimType).subscribe({
        next: () => {
          this.notification.success('Claim removed successfully');
          this.loadUserClaims();
        },
        error: () => {
          this.notification.error('Failed to remove claim');
        }
      });
    }
  }

  getStatusClass(): string {
    return this.user?.isActive ? 'bg-success' : 'bg-danger';
  }

  getStatusText(): string {
    return this.user?.isActive ? 'Active' : 'Inactive';
  }
}
