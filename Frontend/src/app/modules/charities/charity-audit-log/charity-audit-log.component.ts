import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Input, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { SharedModule } from '../../../shared/shared.module';
import { CharityAuditLog, CharityAuditLogChange } from '../models/charity.model';
import { CharityService } from '../services/charity.service';
import { NotificationService } from '../../../core/services/notification.service';

/**
 * UC-17 (charity view) — سجل التدقيق.
 * Every logged Create/Update attributed to this charity, newest first (page 1 = the
 * card's first load). A row expands to the action's field changes. Rendered inside
 * the charity profile; access to the endpoint is enforced server-side by the
 * tenancy claim, so whoever can open the profile may read its log.
 */
@Component({
  selector: 'app-charity-audit-log',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    TranslateModule,
    SharedModule
  ],
  templateUrl: './charity-audit-log.component.html',
  styleUrls: ['./charity-audit-log.component.scss']
})
export class CharityAuditLogComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  @Input({ required: true }) charityId!: string;

  logs: CharityAuditLog[] = [];
  loading = false;
  currentPage = 1;
  readonly pageSize = 20;
  totalCount = 0;
  expandedId: string | null = null;

  constructor(
    private charityService: CharityService,
    private notification: NotificationService,
    private translate: TranslateService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadPage(1);
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadPage(page: number): void {
    this.loading = true;
    this.currentPage = page;
    this.expandedId = null;

    this.charityService.getAuditLogs(this.charityId, page, this.pageSize)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          this.logs = result?.items || [];
          this.totalCount = result?.totalCount || 0;
          this.loading = false;
          this.cdr.markForCheck(); // OnPush: HTTP callbacks must mark
        },
        error: (error: any) => {
          console.error('Error loading charity audit log:', error);
          this.notification.error(error.message || this.translate.instant('charities.auditLog.loadFailed'));
          this.loading = false;
          this.cdr.markForCheck();
        }
      });
  }

  toggleExpanded(id: string): void {
    this.expandedId = this.expandedId === id ? null : id;
  }

  trackByLogId = (_: number, log: CharityAuditLog): string => log.id;
  trackByChangeField = (_: number, change: CharityAuditLogChange): string => change.field;
}
