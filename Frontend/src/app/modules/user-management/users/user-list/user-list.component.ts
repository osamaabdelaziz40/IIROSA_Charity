import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { User } from '../../../../core/models/user.model';
import { UserManagementService } from '../../services/user-management.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header.component';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { AppDatePipe } from '../../../../shared/pipes/date.pipe';
import { RouterModule } from '@angular/router';
import { BreadcrumbComponent, BreadcrumbItem, DropDownComponent } from '../../../../shared/components';
import { PaginationComponent } from '../../../../shared/components';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, PageHeaderComponent, TranslateModule, AppDatePipe, RouterModule, BreadcrumbComponent, PaginationComponent, DropDownComponent],
  templateUrl: './user-list.component.html',
  styleUrls: ['./user-list.component.scss']
})
export class UserListComponent implements OnInit, OnDestroy {
  // Breadcrumb items
  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'userManagement.title' }
  ];
  users: User[] = [];
  allRoles: string[] = [];
  loading = false;
  loadingRoles = false;
  totalCount = 0;
  currentPage = 1;
  pageSize = 20;

  // Filter form — the shared select2 drop-downs bind to this (employee-list pattern).
  filterForm: FormGroup;

  // Select2 option arrays ({id, name}) fed to app-drop-down.
  roleOptions: Array<{ id: string; name: string }> = [];
  statusOptions: Array<{ id: string; name: string }> = [];

  /** Rebuilds translated option labels on language switch; torn down in ngOnDestroy. */
  private langChangeSubscription?: Subscription;

  /** Sentinel id meaning "no filter" — role/status use 'all'. */
  private static readonly ALL = 'all';

  // Table columns
  displayedColumns: string[] = [
    'email',
    'fullName',
    'roles',
    'isActive',
    'createdOn',
    'actions'
  ];

  pageActions = [
    {
      label: 'userManagement.addUser',
      type: 'primary',
      icon: 'fe-plus',
      click: () => this.createUser()
    }
  ];

  constructor(
    private fb: FormBuilder,
    private userManagementService: UserManagementService,
    private notification: NotificationService,
    private translate: TranslateService,
    private router: Router
  ) {
    // Initialize filter form
    this.filterForm = this.fb.group({
      searchValue: [''],
      role: [UserListComponent.ALL],
      status: [UserListComponent.ALL]
    });
  }

  ngOnInit(): void {
    // Page actions are now directly bound
    this.initializeStatusOptions();
    this.loadRoles();
    this.loadUsers();

    // The option labels are pre-translated (app-drop-down renders raw text), so a
    // language switch needs a rebuild — the translate pipe can't refresh them.
    this.langChangeSubscription = this.translate.onLangChange.subscribe(() => {
      this.initializeStatusOptions();
      this.buildRoleOptions();
    });
  }

  ngOnDestroy(): void {
    this.langChangeSubscription?.unsubscribe();
  }

  /** Status options carry string ids — toOptionId in app-drop-down keeps them verbatim. */
  initializeStatusOptions(): void {
    this.statusOptions = [
      { id: UserListComponent.ALL, name: this.translate.instant('userManagement.allStatus') },
      { id: 'active', name: this.translate.instant('common.active') },
      { id: 'inactive', name: this.translate.instant('common.inactive') }
    ];
  }

  /** Role options are the role-name strings; the "All Roles" label is translated. */
  private buildRoleOptions(): void {
    this.roleOptions = [
      { id: UserListComponent.ALL, name: this.translate.instant('userManagement.allRoles') },
      ...this.allRoles.map(role => ({ id: role, name: role }))
    ];
  }

  loadRoles(): void {
    this.loadingRoles = true;
    this.userManagementService.getAllRoles().subscribe({
      next: (roles: any[]) => {
        this.allRoles = roles.map(role => role.name);
        this.buildRoleOptions();
        this.loadingRoles = false;
      },
      error: () => {
        // Fallback to static roles if backend fails
        this.allRoles = ['SuperAdmin', 'Admin', 'Charity', 'Accountant', 'FinancialOfficer'];
        this.buildRoleOptions();
        this.loadingRoles = false;
      }
    });
  }

  /**
   * Load users with current filters and pagination
   * UC-1.9: View All Users
   */
  loadUsers(): void {
    this.loading = true;
    const filters = this.filterForm.value;
    const params = {
      search: filters.searchValue || undefined,
      role: filters.role && filters.role !== UserListComponent.ALL ? filters.role : undefined,
      isActive: filters.status === 'active' ? true : filters.status === 'inactive' ? false : undefined,
      page: this.currentPage,
      pageSize: this.pageSize
    };

    this.userManagementService.getUsers(params).subscribe({
      next: (response) => {
        this.users = response.items;
        this.totalCount = response.totalCount;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading users:', error);
        this.notification.error('Failed to load users');
        this.loading = false;
      }
    });
  }

  /**
   * Handle search input
   */
  onSearch(): void {
    this.currentPage = 1;
    this.loadUsers();
  }

  /**
   * Clear search filters
   */
  clearFilters(): void {
    this.filterForm.reset({
      searchValue: '',
      role: UserListComponent.ALL,
      status: UserListComponent.ALL
    });
    this.currentPage = 1;
    this.loadUsers();
  }

  /**
   * Check if any filters are active
   */
  hasActiveFilters(): boolean {
    const filters = this.filterForm.value;
    return !!(
      filters.searchValue ||
      (filters.role && filters.role !== UserListComponent.ALL) ||
      filters.status !== UserListComponent.ALL
    );
  }

  /**
   * Handle page change
   */
  onPageChange(event: any): void {
    this.currentPage = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadUsers();
  }

  /**
   * Change page
   */
  changePage(page: number): void {
    if (page < 1 || page > this.getTotalPages()) {
      return;
    }
    this.currentPage = page;
    this.loadUsers();
  }

  /**
   * Get total pages
   */
  getTotalPages(): number {
    return Math.ceil(this.totalCount / this.pageSize);
  }

  /**
   * Get page numbers for pagination
   */
  getPages(): number[] {
    const totalPages = this.getTotalPages();
    const pages: number[] = [];
    const maxPagesToShow = 5;

    if (totalPages <= maxPagesToShow) {
      for (let i = 1; i <= totalPages; i++) {
        pages.push(i);
      }
    } else {
      const startPage = Math.max(1, this.currentPage - 2);
      const endPage = Math.min(totalPages, this.currentPage + 2);

      if (startPage > 1) {
        pages.push(1);
        if (startPage > 2) {
          pages.push(-1); // Ellipsis
        }
      }

      for (let i = startPage; i <= endPage; i++) {
        pages.push(i);
      }

      if (endPage < totalPages) {
        if (endPage < totalPages - 1) {
          pages.push(-1); // Ellipsis
        }
        pages.push(totalPages);
      }
    }

    return pages;
  }

  /**
   * Open create user dialog
   * UC-1.2: Create User
   */
  createUser(): void {
    this.router.navigate(['/user-management/users/create']);
  }

  /**
   * Open edit user dialog
   * UC-1.3: Update User
   */
  editUser(user: User): void {
    this.router.navigate(['/user-management/users', user.id, 'edit']);
  }

  /**
   * Toggle user active status
   * UC-1.4: Deactivate User
   */
  toggleUserStatus(user: User): void {
    const action = user.isActive ? 'deactivate' : 'activate';
    // TODO: Implement confirmation dialog
    const confirmed = true; // Placeholder

    if (confirmed) {
      const action$ = user.isActive
        ? this.userManagementService.deactivateUser(user.id)
        : this.userManagementService.activateUser(user.id);

      action$.subscribe({
        next: () => {
          this.notification.success(`User ${action}d successfully`);
          this.loadUsers();
        },
        error: (error) => {
          console.error(`Error ${action}ing user:`, error);
          this.notification.error(`Failed to ${action} user`);
        }
      });
    }
  }

  /**
   * Reset user password
   * UC-1.5: Reset User Password
   */
  resetPassword(user: User): void {
    // TODO: Implement confirmation dialog
    const confirmed = true; // Placeholder

    if (confirmed) {
      this.userManagementService.resetPassword(user.id).subscribe({
        next: (response: any) => {
          this.notification.success('Password reset successfully');
          if (response.newPassword) {
            console.log('New password:', response.newPassword);
          }
        },
        error: (error) => {
          console.error('Error resetting password:', error);
          this.notification.error('Failed to reset password');
        }
      });
    }
  }

  /**
   * Assign role to user
   * UC-1.6: Assign User to Role
   */
  assignRole(user: User): void {
    this.router.navigate(['/user-management/users', user.id, 'roles']);
  }

  /**
   * Delete user
   */
  deleteUser(user: User): void {
    // TODO: Implement confirmation dialog
    const confirmed = true; // Placeholder

    if (confirmed) {
      this.userManagementService.deleteUser(user.id).subscribe({
        next: () => {
          this.notification.success('User deleted successfully');
          this.loadUsers();
        },
        error: (error) => {
          console.error('Error deleting user:', error);
          this.notification.error('Failed to delete user');
        }
      });
    }
  }

  /**
   * Export users to CSV
   */
  exportUsers(): void {
    this.loading = true;

    const filters = this.filterForm.value;
    const exportRequest = {
      search: filters.searchValue || undefined,
      role: filters.role && filters.role !== UserListComponent.ALL ? filters.role : undefined,
      isActive: filters.status === 'active' ? true : filters.status === 'inactive' ? false : undefined,
      page: 1,
      pageSize: 10000 // Export all matching records
    };

    this.userManagementService.exportUsers(exportRequest).subscribe({
      next: (blob: Blob) => {
        // Create download link
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `users_${new Date().toISOString().split('T')[0]}.csv`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);

        this.notification.success('Users exported successfully');
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Export failed:', error);
        this.notification.error('Failed to export users');
        this.loading = false;
      }
    });
  }

  /**
   * Format roles for display
   */
  getRolesDisplay(roles: string[]): string {
    if (!roles || roles.length === 0) {
      return 'No roles';
    }
    return roles.join(', ');
  }

  /**
   * Get status badge class
   */
  getStatusClass(isActive: boolean): string {
    return isActive ? 'status-active' : 'status-inactive';
  }
}
