/**
 * Notification List Component (UC-NTF list / my notifications)
 * One screen, two reads: Admin/SuperAdmin get the full register with the edit
 * and resend actions; everyone else gets their own notifications via /my.
 * The SignalR stream refreshes the grid live — a push arriving while the screen
 * is open appears without a manual reload.
 */

import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { NotificationLogService } from '../services/notification-log.service';
import { NotificationsLog } from '../models/notification.model';
import { AuthService } from '../../../core/services/auth.service';
import { SignalRService } from '../../../core/services/signalr.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PaginationComponent, BreadcrumbComponent, PageHeaderComponent, BreadcrumbItem } from '../../../shared/components';
import type { PageAction } from '../../../shared/components/page-header/page-header.component';

@Component({
  selector: 'app-notification-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule, PaginationComponent, BreadcrumbComponent, PageHeaderComponent],
  templateUrl: './notification-list.component.html',
  styleUrls: ['./notification-list.component.scss']
})
export class NotificationListComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  notifications: NotificationsLog[] = [];
  loading = false;
  resendingId: string | null = null;

  currentPage = 1;
  pageSize = 20;
  totalCount = 0;
  totalPages = 0;

  filterForm: FormGroup;

  /** Admin/SuperAdmin see the register and the management actions (server-authorised). */
  canManage = false;

  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'notifications.title' }
  ];

  pageActions: PageAction[] = [];

  constructor(
    private fb: FormBuilder,
    private notificationLogService: NotificationLogService,
    private auth: AuthService,
    private signalRService: SignalRService,
    private router: Router,
    private translate: TranslateService,
    private notification: NotificationService
  ) {
    this.filterForm = this.fb.group({
      search: ['']
    });
  }

  ngOnInit(): void {
    this.canManage = this.auth.hasAnyRole(['Admin', 'SuperAdmin']);

    if (this.canManage) {
      this.pageActions = [
        {
          label: 'notifications.addNotification',
          icon: 'fe-plus',
          type: 'primary',
          click: () => this.router.navigate(['/notifications/create'])
        }
      ];
    }

    this.loadNotifications();

    // Live refresh: a push landing while the screen is open belongs in the grid
    // immediately (for the admin register too — it lists every pushed row).
    this.signalRService.notifications$
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.loadNotifications(this.currentPage));
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  get pageTitleKey(): string {
    return this.canManage ? 'notifications.title' : 'notifications.myNotifications';
  }

  /**
   * Load the applicable read — the register for admins, /my for everyone else.
   */
  loadNotifications(page: number = 1): void {
    this.loading = true;
    this.currentPage = page;

    const request = {
      search: this.filterForm.value.search?.trim() || undefined,
      page: this.currentPage,
      pageSize: this.pageSize
    };

    const read$ = this.canManage
      ? this.notificationLogService.getNotifications(request)
      : this.notificationLogService.getMyNotifications(request);

    read$.pipe(takeUntil(this.destroy$)).subscribe({
      next: response => {
        this.notifications = response.items || [];
        this.totalCount = response.totalCount || 0;
        this.totalPages = Math.ceil(this.totalCount / Math.max(1, this.pageSize));
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.notification.error(this.translate.instant('notifications.loadFailed'));
      }
    });
  }

  onSearch(): void {
    this.loadNotifications(1);
  }

  clearFilters(): void {
    this.filterForm.reset({ search: '' });
    this.loadNotifications(1);
  }

  hasActiveFilters(): boolean {
    return !!this.filterForm.value.search?.trim();
  }

  serial(index: number): number {
    return (this.currentPage - 1) * this.pageSize + index + 1;
  }

  editNotification(id: string): void {
    this.router.navigate(['/notifications', id, 'edit']);
  }

  /**
   * Resend (UC-NTF resend) — confirm first; declining pushes nothing.
   */
  async resendNotification(id: string): Promise<void> {
    const confirmed = await this.notification.confirm(
      this.translate.instant('notifications.confirmResend'),
      this.translate.instant('notifications.resend')
    );
    if (!confirmed) return;

    this.resendingId = id;
    this.notificationLogService.resendNotification(id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.resendingId = null;
          this.notification.success(this.translate.instant('notifications.resent'));
          this.loadNotifications(this.currentPage);
        },
        error: () => {
          this.resendingId = null;
          this.notification.error(this.translate.instant('notifications.resendFailed'));
        }
      });
  }

  trackNotification(index: number, item: NotificationsLog): string {
    return item.id;
  }

  onPageChange(page: number): void {
    this.loadNotifications(page);
  }
}
