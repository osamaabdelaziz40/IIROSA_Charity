import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { User } from '../../../../core/models/user.model';
import { UserManagementService } from '../../services/user-management.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header.component';
import { TranslateModule } from '@ngx-translate/core';
import { AppDatePipe } from '../../../../shared/pipes/date.pipe';
import { RouterModule } from '@angular/router';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../../shared/components';
import { PaginationComponent } from '../../../../shared/components';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [CommonModule, FormsModule, PageHeaderComponent, TranslateModule, AppDatePipe, RouterModule, BreadcrumbComponent, PaginationComponent],
  templateUrl: './user-list.component.html',
  styleUrls: ['./user-list.component.scss']
})
export class UserListComponent implements OnInit {
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

  // Filter properties
  searchValue = '';
  selectedRole: string | null = null;
  isActiveFilter: boolean | null = null;

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
    private userManagementService: UserManagementService,
    private notification: NotificationService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Page actions are now directly bound
    this.loadRoles();
    this.loadUsers();
  }

  loadRoles(): void {
    this.loadingRoles = true;
    this.userManagementService.getAllRoles().subscribe({
      next: (roles: any[]) => {
        this.allRoles = roles.map(role => role.name);
        this.loadingRoles = false;
      },
      error: () => {
        // Fallback to static roles if backend fails
        this.allRoles = ['SuperAdmin', 'Admin', 'Charity', 'Accountant', 'FinancialOfficer'];
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
    const params = {
      search: this.searchValue || undefined,
      role: this.selectedRole || undefined,
      isActive: this.isActiveFilter !== null ? this.isActiveFilter : undefined,
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
    this.searchValue = '';
    this.selectedRole = null;
    this.isActiveFilter = null;
    this.currentPage = 1;
    this.loadUsers();
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

    const exportRequest = {
      search: this.searchValue || undefined,
      role: this.selectedRole || undefined,
      isActive: this.isActiveFilter !== null ? this.isActiveFilter : undefined,
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
