/**
 * Housing Report List (UC-HOU-06 · §11.S.3) + beneficiary resolve (UC-HOU-07 · §11.U.7)
 * The periodic reports of ONE housing family's beneficiaries — GET
 * /api/PeriodicOrphanReports/by-orphan/{beneficiaryId}?childOrParent=. The §11.U.6
 * discriminator picks the branch: Child ⇒ a family child (the البحث بالكود resolve picks
 * exactly which — GET /api/HousingProjects/projects/{id}/beneficiaries?code= — falling back
 * to the family's first child / the name-search match), Parent ⇒ the guardian row of the
 * same beneficiaries listing. Scoping to the caller's charity is server-side; the HQ
 * الجمعية drop-down narrows the view to the family's own charity.
 *
 * Commands per the §11.S.3 contract: add / edit are live through the §11.S.4 form
 * (`:id/reports/new` with the resolved beneficiary pair · `:id/reports/{reportId}`, 6-8);
 * the print icon stays DISABLED until epic 18; delete rides the epic-9 UC-ORR-06 endpoint
 * (server roles SuperAdmin/Admin; accepted reports are refused).
 * Roles: Charity + HQ (Admin/SuperAdmin).
 */

import { Component, OnInit, OnDestroy, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subject, Subscription } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { HousingProjectService } from '../services/housing-project.service';
import {
  HousingFamilyDetail,
  HousingBeneficiaryRow,
  HousingReportListItem,
  HousingBeneficiaryType
} from '../models/housing-project.model';
import { CharityService } from '../../charities/services/charity.service';
import { CharityDto } from '../../charities/models/charity.model';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import {
  PaginationComponent,
  BreadcrumbComponent,
  BreadcrumbItem,
  PageHeaderComponent,
  DropDownComponent
} from '../../../shared/components';
import { SharedModule } from '../../../shared/shared.module';

@Component({
  selector: 'app-housing-report-list',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    PaginationComponent,
    BreadcrumbComponent,
    PageHeaderComponent,
    DropDownComponent,
    SharedModule
  ],
  templateUrl: './housing-report-list.component.html',
  styleUrls: ['./housing-report-list.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HousingReportListComponent implements OnInit, OnDestroy {
  /** كافة الجهات sentinel — maps to no charity narrowing. */
  private static readonly ALL_CHARITIES = 'all';
  /** §11.S.3 default يخص branch (children — codes resolve children). */
  private static readonly DEFAULT_BENEFICIARY_TYPE: HousingBeneficiaryType = 'Child';

  private destroy$ = new Subject<void>();
  private familyId = '';
  private langChangeSubscription?: Subscription;

  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'housingProjects.title', url: '/housing-projects' },
    { label: 'housingProjects.familyDetails', url: '/housing-projects' },
    { label: 'housingProjects.reports.title' }
  ];

  pageActions = [
    {
      label: 'common.back',
      type: 'secondary',
      icon: 'fe-arrow-left',
      click: () => this.router.navigate(['/housing-projects', this.familyId])
    }
  ];

  // Family context (the 6-4 aggregate) — header + HQ charity filter
  familyLoading = true;
  family: HousingFamilyDetail | null = null;

  // UC-HOU-07 beneficiary picker state
  beneficiaries: HousingBeneficiaryRow[] = [];
  codeNotFound = false;
  /** The child the البحث بالكود resolve pinned — survives until type/search changes. */
  resolvedBeneficiaryId: string | null = null;

  // §11.S.3 filter form — the shared select2 drop-downs bind to this (employee-list pattern)
  filterForm: FormGroup;

  // Select2 option arrays ({id, name}) fed to app-drop-down
  charityOptions: Array<{ id: string; name: string }> = [];
  beneficiaryTypeOptions: Array<{ id: string; name: string }> = [];

  isHQ = false;
  charities: CharityDto[] = [];

  // Delete is server-gated to SuperAdmin/Admin (UC-ORR-06)
  canDelete = false;

  // Grid
  reports: HousingReportListItem[] = [];
  loading = false;
  currentPage = 1;
  pageSize = 10;
  totalCount = 0;
  totalPages = 0;

  constructor(
    private fb: FormBuilder,
    private housingService: HousingProjectService,
    private charityService: CharityService,
    private auth: AuthService,
    private notification: NotificationService,
    private translate: TranslateService,
    private router: Router,
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef
  ) {
    this.filterForm = this.fb.group({
      charityId: [HousingReportListComponent.ALL_CHARITIES],
      beneficiaryType: [HousingReportListComponent.DEFAULT_BENEFICIARY_TYPE],
      childSearch: [''],
      codeSearch: ['']
    });
  }

  ngOnInit(): void {
    this.initializeBeneficiaryTypeOptions();

    // The option labels are pre-translated (app-drop-down renders raw text), so a
    // language switch needs a rebuild — the translate pipe can't refresh them.
    this.langChangeSubscription = this.translate.onLangChange
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.initializeBeneficiaryTypeOptions();
        this.buildCharityOptions();
        this.cdr.markForCheck();
      });

    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.router.navigate(['/housing-projects']);
      return;
    }
    this.familyId = id;
    this.isHQ = this.auth.hasRole('SuperAdmin') || this.auth.hasRole('Admin');
    this.canDelete = this.isHQ;
    this.loadFamily(id);
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    if (this.langChangeSubscription) {
      this.langChangeSubscription.unsubscribe();
    }
  }

  // ==================== FILTER FORM READS ====================

  /** يخص — the §11.U.6 discriminator (template + beneficiary resolution read the form). */
  get beneficiaryType(): HousingBeneficiaryType {
    return this.filterForm.get('beneficiaryType')?.value || HousingReportListComponent.DEFAULT_BENEFICIARY_TYPE;
  }

  /** اسم الابن narrowing term. */
  get childSearch(): string {
    return this.filterForm.get('childSearch')?.value || '';
  }

  /** البحث بالكود term. */
  get codeSearch(): string {
    return this.filterForm.get('codeSearch')?.value || '';
  }

  /** الجمعية filter — 'all' means no narrowing. */
  get selectedCharityId(): string {
    return this.filterForm.get('charityId')?.value || HousingReportListComponent.ALL_CHARITIES;
  }

  // ==================== CONTEXT + LOOKUPS ====================

  /** The 6-4 aggregate — family header context; the beneficiaries read follows it. */
  private loadFamily(id: string): void {
    this.familyLoading = true;
    this.housingService.getHousingFamily(id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: detail => {
          this.familyLoading = false;
          this.family = detail;
          // The raw register code reads fine through the translate pipe (not a key)
          this.breadcrumbs[2].label = detail.code || 'housingProjects.familyDetails';
          if (this.isHQ) {
            this.filterForm.patchValue({
              charityId: detail.charityId || HousingReportListComponent.ALL_CHARITIES
            });
            this.loadCharities();
          }
          this.loadBeneficiaries();
          this.cdr.markForCheck();
        },
        error: () => {
          this.familyLoading = false;
          this.cdr.markForCheck();
          this.notification.error(
            this.translate.instant('housingProjects.reports.messages.familyLoadFailed'));
          this.router.navigate(['/housing-projects']);
        }
      });
  }

  /** UC-HOU-07: the beneficiaries listing (children + guardian) that feeds the picker. */
  private loadBeneficiaries(): void {
    this.housingService.getHousingBeneficiaries(this.familyId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: rows => {
          this.beneficiaries = rows || [];
          this.loadReports();
          this.cdr.markForCheck();
        },
        error: err => {
          this.beneficiaries = [];
          this.notification.error(
            err?.error?.message ||
            this.translate.instant('housingProjects.reports.messages.loadFailed'));
          this.loadReports();
          this.cdr.markForCheck();
        }
      });
  }

  /** الجمعية options for the HQ-only filter (كافة الجهات = 'all' sentinel). */
  private loadCharities(): void {
    this.charityService.getCharities({ pageNumber: 1, pageSize: 1000, isActive: true })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => {
          this.charities = result.items || [];
          this.buildCharityOptions();
          this.cdr.markForCheck();
        },
        error: () => {
          this.charities = [];
          this.buildCharityOptions();
          this.cdr.markForCheck();
        }
      });
  }

  /** HQ charity options with the كافة الجهات sentinel first. */
  private buildCharityOptions(): void {
    this.charityOptions = [
      { id: HousingReportListComponent.ALL_CHARITIES, name: this.translate.instant('housingProjects.filters.allCharities') },
      ...this.charities.map(charity => ({ id: charity.id, name: charity.name }))
    ];
  }

  /** يخص options — pre-translated names, rebuilt on language switch. */
  initializeBeneficiaryTypeOptions(): void {
    this.beneficiaryTypeOptions = [
      { id: 'Child', name: this.translate.instant('housingProjects.reports.filters.child') },
      { id: 'Parent', name: this.translate.instant('housingProjects.reports.filters.parent') }
    ];
  }

  // ==================== BENEFICIARY RESOLUTION (§11.U.6 / §11.U.7) ====================

  get childBeneficiaries(): HousingBeneficiaryRow[] {
    return this.beneficiaries.filter(row => row.childOrParent === 'Child');
  }

  get guardianBeneficiary(): HousingBeneficiaryRow | null {
    return this.beneficiaries.find(row => row.childOrParent === 'Parent') || null;
  }

  /** Child rows matching the name search (all of them when the box is empty). */
  get matchingChildren(): HousingBeneficiaryRow[] {
    const term = this.childSearch.trim().toLowerCase();
    if (!term) {
      return this.childBeneficiaries;
    }
    return this.childBeneficiaries.filter(row =>
      (row.fullName || '').toLowerCase().includes(term) ||
      (row.code || '').toLowerCase().includes(term));
  }

  /** The §11.S.3 beneficiary: code-resolved child > name-search match > first child; guardian under Parent. */
  get selectedBeneficiary(): HousingBeneficiaryRow | null {
    if (this.beneficiaryType === 'Parent') {
      return this.guardianBeneficiary;
    }
    if (this.resolvedBeneficiaryId) {
      const resolved = this.childBeneficiaries.find(
        row => row.beneficiaryId === this.resolvedBeneficiaryId);
      if (resolved) {
        return resolved;
      }
    }
    return this.matchingChildren[0] || null;
  }

  /** HQ picked a charity the family does not belong to — the view is out of scope by choice. */
  get charityOutOfScope(): boolean {
    const charityId = this.selectedCharityId;
    return this.isHQ && !!charityId &&
      charityId !== HousingReportListComponent.ALL_CHARITIES &&
      !!this.family?.charityId && charityId !== this.family.charityId;
  }

  /** النوع label for the summary strip. */
  beneficiaryTypeLabel(row: HousingBeneficiaryRow): string {
    return this.translate.instant(row.childOrParent === 'Parent'
      ? 'housingProjects.reports.filters.parent'
      : 'housingProjects.reports.filters.child');
  }

  // ==================== GRID ====================

  loadReports(): void {
    const beneficiary = this.selectedBeneficiary;
    if (this.charityOutOfScope || !beneficiary) {
      this.reports = [];
      this.totalCount = 0;
      this.totalPages = 0;
      this.loading = false;
      // Reached from async chains (beneficiaries landing) as well as clicks —
      // the OnPush view needs the explicit dirty mark in the async case.
      this.cdr.markForCheck();
      return;
    }
    this.loading = true;
    this.housingService.getHousingBeneficiaryReports(
      beneficiary.beneficiaryId,
      beneficiary.childOrParent === 'Parent' ? 'Parent' : 'Child',
      this.currentPage, this.pageSize)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => {
          this.reports = result.items || [];
          this.totalCount = result.totalCount;
          this.totalPages = result.totalPages;
          this.loading = false;
          this.cdr.markForCheck();
        },
        error: err => {
          this.loading = false;
          this.reports = [];
          this.totalCount = 0;
          this.totalPages = 0;
          this.notification.error(
            err?.error?.message ||
            this.translate.instant('housingProjects.reports.messages.loadFailed'));
          this.cdr.markForCheck();
        }
      });
  }

  /** §11.S.3 row serial — continuous across pages (13-1 formula). */
  rowSerial(index: number): number {
    return (this.currentPage - 1) * this.pageSize + index + 1;
  }

  /** YYYY-MM-DD slice of a server date (display-only — no timezone shift). */
  datePart(date?: string): string {
    return date ? date.slice(0, 10) : '—';
  }

  // ==================== FILTER HANDLERS ====================

  /** UC-HOU-07 البحث بالكود — exact child-code resolve; 0 rows is an explicit not-found. */
  resolveByCode(): void {
    const code = this.codeSearch.trim();
    if (!code) {
      return;
    }
    this.codeNotFound = false;
    this.housingService.getHousingBeneficiaries(this.familyId, code)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: rows => {
          if (rows.length === 1) {
            // Codes resolve children — pin the child and read its reports
            this.resolvedBeneficiaryId = rows[0].beneficiaryId;
            this.filterForm.patchValue({ beneficiaryType: 'Child' });
            this.currentPage = 1;
            this.loadReports();
          } else {
            this.codeNotFound = true;
          }
          this.cdr.markForCheck();
        },
        error: err => {
          this.notification.error(
            err?.error?.message ||
            this.translate.instant('housingProjects.reports.messages.loadFailed'));
          this.cdr.markForCheck();
        }
      });
  }

  /** Action-row Search — re-resolve the beneficiary from the current filter values. */
  onSearch(): void {
    this.resolvedBeneficiaryId = null;
    this.codeNotFound = false;
    this.currentPage = 1;
    this.loadReports();
  }

  onBeneficiaryTypeChanged(): void {
    this.resolvedBeneficiaryId = null;
    this.codeNotFound = false;
    this.currentPage = 1;
    this.loadReports();
  }

  onCharityChanged(): void {
    // getCharityData() legacy semantics on a single-family dataset: choosing another
    // charity legitimately empties the view (the family is not that charity's data)
    this.loadReports();
  }

  clearFilters(): void {
    this.filterForm.reset({
      charityId: this.family?.charityId || HousingReportListComponent.ALL_CHARITIES,
      beneficiaryType: HousingReportListComponent.DEFAULT_BENEFICIARY_TYPE,
      childSearch: '',
      codeSearch: ''
    });
    this.resolvedBeneficiaryId = null;
    this.codeNotFound = false;
    this.currentPage = 1;
    this.loadReports();
  }

  /** Any filter away from its default — drives the Clear Filters button. */
  hasActiveFilters(): boolean {
    const filters = this.filterForm.value;
    const defaultCharityId = this.family?.charityId || HousingReportListComponent.ALL_CHARITIES;
    return !!(
      filters.childSearch ||
      filters.codeSearch ||
      filters.beneficiaryType !== HousingReportListComponent.DEFAULT_BENEFICIARY_TYPE ||
      (filters.charityId && filters.charityId !== defaultCharityId)
    );
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadReports();
  }

  // ==================== COMMANDS ====================

  /**
   * §11.S.4 add — open the report form for the resolved beneficiary (the 6-7 gate).
   * The form refuses the entry itself when the beneficiary pair is missing.
   */
  addReport(): void {
    const beneficiary = this.selectedBeneficiary;
    if (!beneficiary || this.charityOutOfScope) {
      this.notification.warning(
        this.translate.instant('housingProjects.reports.noBeneficiary'));
      return;
    }
    this.router.navigate(
      ['/housing-projects', this.familyId, 'reports', 'new'],
      {
        queryParams: {
          beneficiary: beneficiary.beneficiaryId,
          type: beneficiary.childOrParent === 'Parent' ? 'Parent' : 'Child'
        }
      });
  }

  /** §11.S.4 edit — the epic-9 full-replace PUT behind the form (accepted reports immutable). */
  editReport(report: HousingReportListItem): void {
    this.router.navigate(['/housing-projects', this.familyId, 'reports', report.id]);
  }

  /** نعم (DeleteOrpReport) — confirm dialog then the epic-9 soft delete. */
  async deleteReport(report: HousingReportListItem): Promise<void> {
    const confirmed = await this.notification.confirm(
      this.translate.instant('common.deleteConfirm'));
    if (!confirmed) {
      return;
    }
    this.housingService.deleteHousingReport(report.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.notification.success(
            this.translate.instant('housingProjects.reports.messages.deleted'));
          // Step back a page when the deleted row was the last one on this page
          if (this.currentPage > 1 && this.reports.length === 1) {
            this.currentPage--;
          }
          this.loadReports();
          this.cdr.markForCheck();
        },
        error: err => {
          this.notification.error(
            err?.error?.message ||
            this.translate.instant('housingProjects.reports.messages.deleteFailed'));
          this.cdr.markForCheck();
        }
      });
  }

  // ==================== TRACK BY ====================

  trackByReportId(index: number, report: HousingReportListItem): string {
    return report.id;
  }
}
