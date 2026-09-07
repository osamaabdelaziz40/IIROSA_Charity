/**
 * Housing Families List (UC-HOU-01 · §11.S.1)
 * The register of families housed in organisation-owned buildings. Re-cut from the
 * invented construction tracker: rows are Family records filtered to familyType=Housing,
 * served by GET /api/Families. Search is the UC-HOU-02 typed search (6 selectors).
 * Roles: Charity + HQ (Admin/SuperAdmin).
 */

import { Component, OnInit, OnDestroy, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subject, Subscription } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { FamilyService } from '../../families/services/family.service';
import { FamilyListItemDto } from '../../families/models/family.model';
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

/** §11.S.1 searchType options (البحث عن طريق) — values bind FamilyFilterDto.SearchType */
export const HOUSING_SEARCH_TYPES: ReadonlyArray<{ id: string; labelKey: string }> = [
  { id: 'father', labelKey: 'housingProjects.searchTypes.father' },
  { id: 'mother', labelKey: 'housingProjects.searchTypes.mother' },
  { id: 'student', labelKey: 'housingProjects.searchTypes.student' },
  { id: 'nationalId', labelKey: 'housingProjects.searchTypes.nationalId' },
  { id: 'code', labelKey: 'housingProjects.searchTypes.code' },
  { id: 'phone', labelKey: 'housingProjects.searchTypes.phone' }
];

@Component({
  selector: 'app-housing-project-list',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    TranslateModule,
    PaginationComponent,
    BreadcrumbComponent,
    PageHeaderComponent,
    DropDownComponent,
    SharedModule
  ],
  templateUrl: './housing-project-list.component.html',
  styleUrls: ['./housing-project-list.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HousingProjectListComponent implements OnInit, OnDestroy {
  /** كافة الجهات sentinel — maps to no charityId in the request. */
  private static readonly ALL_CHARITIES = 'all';
  /** §11.S.1 default البحث عن طريق selector (father). */
  private static readonly DEFAULT_SEARCH_TYPE = 'father';

  private destroy$ = new Subject<void>();
  private langChangeSubscription?: Subscription;

  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'housingProjects.title' }
  ];

  pageActions = [
    {
      label: 'housingProjects.addFamily',
      type: 'primary',
      icon: 'fe-plus',
      click: () => this.router.navigate(['/housing-projects', 'create'])
    },
    {
      label: 'common.exportToExcel',
      type: 'success',
      icon: 'fe-download',
      click: () => this.exportToExcel()
    }
  ];

  // §11.S.1 filter form — the shared select2 drop-downs bind to this (employee-list pattern)
  filterForm: FormGroup;

  // Select2 option arrays ({id, name}) fed to app-drop-down
  charityOptions: Array<{ id: string; name: string }> = [];
  searchTypeOptions: Array<{ id: string; name: string }> = [];

  // HQ roles see the charity drop-down; charity callers are pinned server-side
  isHQ = false;

  // Data
  families: FamilyListItemDto[] = [];
  loading = false;

  // Lookup data
  charities: CharityDto[] = [];

  // Pagination
  currentPage = 1;
  pageSize = 10;
  totalCount = 0;
  totalPages = 0;

  constructor(
    private fb: FormBuilder,
    private familyService: FamilyService,
    private charityService: CharityService,
    private auth: AuthService,
    private notification: NotificationService,
    private translate: TranslateService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {
    this.filterForm = this.fb.group({
      charityId: [HousingProjectListComponent.ALL_CHARITIES],
      searchType: [HousingProjectListComponent.DEFAULT_SEARCH_TYPE],
      searchValue: ['']
    });
  }

  ngOnInit(): void {
    this.initializeSearchTypeOptions();
    this.isHQ = this.auth.hasRole('SuperAdmin') || this.auth.hasRole('Admin');
    if (this.isHQ) {
      this.loadCharities();
    }
    this.loadHousingFamilies();

    // The option labels are pre-translated (app-drop-down renders raw text), so a
    // language switch needs a rebuild — the translate pipe can't refresh them.
    this.langChangeSubscription = this.translate.onLangChange
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.initializeSearchTypeOptions();
        this.buildCharityOptions();
        this.cdr.markForCheck();
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    if (this.langChangeSubscription) {
      this.langChangeSubscription.unsubscribe();
    }
  }

  /** Translated البحث عن طريق options (UC-HOU-02) — rebuilt on language switch. */
  initializeSearchTypeOptions(): void {
    this.searchTypeOptions = HOUSING_SEARCH_TYPES.map(option => ({
      id: option.id,
      name: this.translate.instant(option.labelKey)
    }));
  }

  /**
   * Load charities for the HQ-only الجمعية filter (كافة الجهات = 'all' sentinel)
   */
  private loadCharities(): void {
    this.charityService.getCharities({ pageNumber: 1, pageSize: 1000, isActive: true })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          this.charities = result.items || [];
          this.buildCharityOptions();
          this.cdr.markForCheck();
        },
        error: () => {
          // The filter degrades to كافة الجهات only — not fatal
          this.charities = [];
          this.buildCharityOptions();
          this.cdr.markForCheck();
        }
      });
  }

  /** HQ charity options with the كافة الجهات sentinel first. */
  private buildCharityOptions(): void {
    this.charityOptions = [
      { id: HousingProjectListComponent.ALL_CHARITIES, name: this.translate.instant('housingProjects.filters.allCharities') },
      ...this.charities.map(charity => ({ id: charity.id, name: charity.name }))
    ];
  }

  /**
   * Load the housing register: GET /api/Families?familyType=Housing&…
   */
  loadHousingFamilies(): void {
    this.loading = true;
    const filters = this.filterForm.value;
    this.familyService.getFamilies({
      familyType: 'Housing',
      charityId: filters.charityId && filters.charityId !== HousingProjectListComponent.ALL_CHARITIES
        ? filters.charityId
        : undefined,
      searchTerm: filters.searchValue || undefined,
      searchType: filters.searchValue ? filters.searchType : undefined,
      pageNumber: this.currentPage,
      pageSize: this.pageSize
    }).pipe(takeUntil(this.destroy$)).subscribe({
      next: (response) => {
        this.families = (response.items || []) as unknown as FamilyListItemDto[];
        this.totalCount = response.totalCount;
        this.totalPages = response.totalPages;
        this.loading = false;
        // OnPush: the template never re-evaluates unless the component is marked
        // dirty — without this the spinner outlives the data that replaced it.
        this.cdr.markForCheck();
      },
      error: () => {
        this.notification.error(this.translate.instant('housingProjects.loadFailed'));
        this.loading = false;
        this.cdr.markForCheck();
      }
    });
  }

  /**
   * Export the §11.S.1 register to Excel — client-side ExcelJS (§21.S.7 extract
   * precedent): /api/Families has no server-side export endpoint, so re-run the
   * current filter with one page holding every row, then build the workbook here.
   */
  exportToExcel(): void {
    if (this.families.length === 0) {
      this.notification.info(this.translate.instant('housingProjects.nothingToExport'));
      return;
    }

    this.loading = true;
    this.cdr.markForCheck();

    const filters = this.filterForm.value;
    this.familyService.getFamilies({
      familyType: 'Housing',
      charityId: filters.charityId && filters.charityId !== HousingProjectListComponent.ALL_CHARITIES
        ? filters.charityId
        : undefined,
      searchTerm: filters.searchValue || undefined,
      searchType: filters.searchValue ? filters.searchType : undefined,
      pageNumber: 1,
      pageSize: Math.max(this.totalCount, this.pageSize)
    }).pipe(takeUntil(this.destroy$)).subscribe({
      next: response => {
        const rows = (response.items || []) as unknown as FamilyListItemDto[];
        this.loading = false;
        this.cdr.markForCheck();

        if (rows.length === 0) {
          this.notification.info(this.translate.instant('housingProjects.nothingToExport'));
          return;
        }

        import('exceljs').then(({ default: ExcelJS }) => {
          const workbook = new ExcelJS.Workbook();
          const sheet = workbook.addWorksheet(this.translate.instant('housingProjects.title'));
          sheet.addRow([
            this.translate.instant('housingProjects.columns.serial'),
            this.translate.instant('housingProjects.columns.fatherName'),
            this.translate.instant('housingProjects.columns.motherName'),
            this.translate.instant('housingProjects.columns.children'),
            this.translate.instant('housingProjects.columns.phones'),
            this.translate.instant('housingProjects.columns.familyCode'),
            this.translate.instant('housingProjects.columns.charity')
          ]);
          rows.forEach((family, index) => sheet.addRow([
            index + 1,
            family.fatherName || '',
            family.motherName || '',
            family.orphansCount || 0,
            family.phoneNumber || '',
            family.code || '',
            family.charityName || ''
          ]));

          workbook.xlsx.writeBuffer().then(buffer => {
            const blob = new Blob([buffer], {
              type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
            });
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = `housing_families_${new Date().toISOString().slice(0, 10)}.xlsx`;
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            window.URL.revokeObjectURL(url);
          }).catch(() => {
            // Review P11: a failed write is a user-facing failure, not a silent non-event
            this.notification.error(this.translate.instant('housingProjects.exportFailed'));
          });
        }).catch(() => {
          // Review P11: the ExcelJS chunk itself can fail to load (offline first hit)
          this.notification.error(this.translate.instant('housingProjects.exportFailed'));
        });
      },
      error: () => {
        this.notification.error(this.translate.instant('housingProjects.loadFailed'));
        this.loading = false;
        this.cdr.markForCheck();
      }
    });
  }

  /**
   * §11.S.1 row serial — continuous across pages (13-1 formula)
   */
  rowSerial(index: number): number {
    return (this.currentPage - 1) * this.pageSize + index + 1;
  }

  onSearch(): void {
    this.currentPage = 1;
    this.loadHousingFamilies();
  }

  /** Auto-apply: the الجمعية filter reloads immediately (legacy select behaviour). */
  onCharityChanged(): void {
    this.onSearch();
  }

  clearFilters(): void {
    this.filterForm.reset({
      charityId: HousingProjectListComponent.ALL_CHARITIES,
      searchType: HousingProjectListComponent.DEFAULT_SEARCH_TYPE,
      searchValue: ''
    });
    this.currentPage = 1;
    this.loadHousingFamilies();
  }

  /** Any filter away from its no-filter default — drives the Clear Filters button. */
  hasActiveFilters(): boolean {
    const filters = this.filterForm.value;
    return !!(
      filters.searchValue ||
      (filters.charityId && filters.charityId !== HousingProjectListComponent.ALL_CHARITIES)
    );
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadHousingFamilies();
  }

  viewFamily(id: string): void {
    // UC-HOU-04 (6-4): the housing detail screen — the §11.U.4 aggregate view of the
    // Family record (FamilyType=Housing) with its guardian + full child set.
    this.router.navigate(['/housing-projects', id]);
  }

  trackByFamilyId(index: number, family: FamilyListItemDto): string {
    return family.id;
  }
}
