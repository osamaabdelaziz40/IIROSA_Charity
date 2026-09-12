import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subscription } from 'rxjs';

import { GeneralChecksService } from '../services/general-checks.service';
import { CheckListItem, CheckStatistics } from '../models/check.model';
import { CharityService } from '../../charities/services/charity.service';
import { CharityDto } from '../../charities/models/charity.model';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import {
  PaginationComponent,
  BreadcrumbComponent,
  BreadcrumbItem,
  PageHeaderComponent
} from '../../../shared/components';
import { SharedModule } from '../../../shared/shared.module';
import { DropDownComponent } from '../../../shared/components/drop-down/drop-down.component';

/**
 * §16.S.1 — the cheque register: charity dropdown (head office only), the grid
 * (الرقم، رقم الشيك، تاريخ الشيك، اسم المستفيد، المبلغ، البنك، تعديل) and paging.
 * Tenancy itself is server-side — the charity dropdown is a filter, not a control.
 */
@Component({
  selector: 'app-check-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    TranslateModule,
    PaginationComponent,
    BreadcrumbComponent,
    PageHeaderComponent,
    SharedModule,
    DropDownComponent
  ],
  templateUrl: './check-list.component.html',
  styleUrls: ['./check-list.component.scss']
})
export class CheckListComponent implements OnInit, OnDestroy {
  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'generalChecks.title' }
  ];

  pageActions = [
    {
      label: 'generalChecks.addCheck',
      type: 'primary',
      icon: 'fe-plus',
      click: () => this.addCheck()
    },
    {
      label: 'common.exportToExcel',
      type: 'success',
      icon: 'fe-file-plus',
      click: () => this.exportToExcel()
    }
  ];

  checks: CheckListItem[] = [];
  loading = false;
  totalCount = 0;
  currentPage = 1;
  pageSize = 20;

  // Register statistics band (§16.S.1) — caller-scoped server-side like the register
  // itself; describes the caller's whole register, not the active filters.
  statistics: CheckStatistics | null = null;

  /** The charity filter is a head-office concern — everyone else is pinned server-side. */
  isHeadOffice = false;
  canEdit = false;
  charities: CharityDto[] = [];
  charityOptions: Array<{ id: string; name: string }> = [];

  filterForm: FormGroup;

  /** Sentinel id of the charity filter's "كافة الجهات" option — maps to no filter. */
  private static readonly ALL_CHARITIES = 'all';

  /** Rebuilds the pre-translated option labels on language switch; torn down in ngOnDestroy. */
  private langChangeSubscription?: Subscription;

  constructor(
    private fb: FormBuilder,
    private generalChecksService: GeneralChecksService,
    private charityService: CharityService,
    private authService: AuthService,
    private notification: NotificationService,
    private router: Router,
    private translate: TranslateService
  ) {
    this.filterForm = this.fb.group({
      charityId: [CheckListComponent.ALL_CHARITIES]
    });
  }

  ngOnInit(): void {
    this.isHeadOffice = this.authService.hasAnyRole(['SuperAdmin', 'Admin']);
    this.canEdit = this.authService.hasPermission('GeneralChecks.Edit');

    if (this.isHeadOffice) {
      this.loadCharities();
    }

    // The option labels are pre-translated (app-drop-down renders raw text), so a
    // language switch needs a rebuild — the translate pipe can't refresh them.
    this.langChangeSubscription = this.translate.onLangChange.subscribe(() => {
      this.buildCharityOptions();
    });

    this.loadChecks();
    this.loadStatistics();
  }

  ngOnDestroy(): void {
    this.langChangeSubscription?.unsubscribe();
  }

  private loadCharities(): void {
    this.charityService.getCharities({ pageNumber: 1, pageSize: 1000, isActive: true }).subscribe({
      next: response => {
        this.charities = response.items || [];
        this.buildCharityOptions();
      },
      error: () => {
        this.charities = [];
        this.buildCharityOptions();
      }
    });
  }

  /** Filter options — an explicit "كافة الجهات" sentinel first, then one entry per charity. */
  private buildCharityOptions(): void {
    this.charityOptions = [
      { id: CheckListComponent.ALL_CHARITIES, name: this.translate.instant('generalChecks.allCharities') },
      ...this.charities.map(c => ({ id: c.id, name: c.name }))
    ];
  }

  loadChecks(): void {
    this.loading = true;

    const charityId = this.filterForm.value.charityId;
    this.generalChecksService.getChecks({
      charityId: charityId && charityId !== CheckListComponent.ALL_CHARITIES ? charityId : undefined,
      page: this.currentPage,
      pageSize: this.pageSize
    }).subscribe({
      next: result => {
        this.checks = result.items || [];
        this.totalCount = result.totalCount || 0;
        this.loading = false;
      },
      error: () => {
        this.checks = [];
        this.totalCount = 0;
        this.loading = false;
      }
    });
  }

  /**
   * Register statistics band — describes the caller's whole register (not the active
   * filters). Silent-fail: the band is decorative context and must not surface toasts.
   */
  loadStatistics(): void {
    this.generalChecksService.getStatistics().subscribe({
      next: statistics => this.statistics = statistics,
      error: () => console.error('Error loading cheque statistics')
    });
  }

  /** Dropdown change auto-applies — same as pressing Search. */
  onFilterChange(): void {
    this.onSearch();
  }

  onSearch(): void {
    this.currentPage = 1;
    this.loadChecks();
  }

  /** Reset to the unfiltered register ("كافة الجهات" sentinel) and reload. */
  clearFilters(): void {
    this.filterForm.reset({ charityId: CheckListComponent.ALL_CHARITIES });
    this.currentPage = 1;
    this.loadChecks();
  }

  /** Any filter off its default? Drives the conditional Clear Filters button. */
  hasActiveFilters(): boolean {
    const charityId = this.filterForm.value.charityId;
    return !!(charityId && charityId !== CheckListComponent.ALL_CHARITIES);
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadChecks();
  }

  addCheck(): void {
    this.router.navigate(['/general-checks', 'create']);
  }

  /**
   * Export the register — the charity filter as currently applied; the server
   * ignores paging and writes every matching row.
   */
  exportToExcel(): void {
    this.loading = true;

    const charityId = this.filterForm.value.charityId;
    this.generalChecksService.exportToExcel({
      charityId: charityId && charityId !== CheckListComponent.ALL_CHARITIES ? charityId : undefined
    }).subscribe({
      next: (blob: Blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `general-checks_${new Date().toISOString().split('T')[0]}.xlsx`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);

        this.notification.success(this.translate.instant('common.operationSuccess'));
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.notification.error(this.translate.instant('common.operationFailed'));
      }
    });
  }

  trackByCheck(index: number, check: CheckListItem): string {
    return check.id;
  }

  /** Serial (الرقم) — continuous across pages, like the legacy register. */
  rowNumber(index: number): number {
    return (this.currentPage - 1) * this.pageSize + index + 1;
  }
}
