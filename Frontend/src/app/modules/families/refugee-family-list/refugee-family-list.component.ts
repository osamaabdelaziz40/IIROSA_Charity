import { ChangeDetectorRef, ChangeDetectionStrategy, Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged, takeUntil } from 'rxjs/operators';
import { TranslateModule, TranslateService, LangChangeEvent } from '@ngx-translate/core';
import { FamilyListItemDto, FamilySearchRequest, FamilyStatistics } from '../models/family.model';
import { FamilyService } from '../services/family.service';
import { CharityService } from '../../charities/services/charity.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../shared/components';
import { PaginationComponent } from '../../../shared/components';
import { SharedModule } from '../../../shared/shared.module';
import { DropDownComponent } from '../../../shared/components/drop-down/drop-down.component';

@Component({
  selector: 'app-refugee-family-list',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    PageHeaderComponent,
    BreadcrumbComponent,
    TranslateModule,
    RouterModule,
    PaginationComponent,
    SharedModule,
    DropDownComponent
  ],
  templateUrl: './refugee-family-list.component.html',
  styleUrls: ['./refugee-family-list.component.scss']
})
export class RefugeeFamilyListComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private langChangeSubscription?: Subscription;

  /** P19 — debounced typing + selector changes can overlap; only the newest read may render */
  private requestSeq = 0;

  Math = Math;
  families: FamilyListItemDto[] = [];
  loading = false;
  totalCount = 0;
  currentPage = 1;
  pageSize = 20;
  totalPages = 0;

  /** Statistics band — describes the caller's whole refugee register, not the current filter. */
  statistics: FamilyStatistics | null = null;

  /** الجمعية selector is an HQ-only control; a Charity caller is pinned server-side */
  isHQ = false;

  /** Server also authorises POST /api/Families — this only hides the toolbar button (7-3) */
  canCreate = false;

  filterForm: FormGroup;

  charityOptions: Array<{ id: string; name: string }> = [];
  searchTypeOptions: Array<{ id: string; name: string }> = [];

  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'families.refugeeTitle' }
  ];

  constructor(
    private fb: FormBuilder,
    private familyService: FamilyService,
    private charityService: CharityService,
    private auth: AuthService,
    private notification: NotificationService,
    private translate: TranslateService,
    private cdr: ChangeDetectorRef,
    private router: Router
  ) {
    this.filterForm = this.fb.group({
      charityId: ['all'],
      searchValue: [''],
      searchType: ['all']
    });
  }

  ngOnInit(): void {
    this.isHQ = this.auth.hasAnyRole(['SuperAdmin', 'Admin']);
    this.canCreate = this.auth.hasPermission('Families.Create');

    this.initializeSearchTypeOptions();

    this.langChangeSubscription = this.translate.onLangChange.subscribe((event: LangChangeEvent) => {
      this.initializeSearchTypeOptions();
      this.cdr.markForCheck();
    });

    // Debounced free-text search (seasonal-aid pattern): typing fires after 400ms of quiet,
    // clearing the box reloads the unfiltered page.
    this.filterForm.get('searchValue')!.valueChanges.pipe(
      debounceTime(400),
      distinctUntilChanged(),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.currentPage = 1;
      this.loadRefugeeFamilies();
    });

    if (this.isHQ) {
      this.loadCharityOptions();
    }

    this.loadRefugeeFamilies();
    this.loadStatistics();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    if (this.langChangeSubscription) {
      this.langChangeSubscription.unsubscribe();
    }
  }

  /**
   * البحث عن طريق selector options — rebuilt only on language change, never per CD cycle.
   * Values are the SHARED FamilyFilterDto.SearchType vocabulary (6-2 + 7-2, one switch
   * server-side); labels are §12.S.1's (إسم اليتيم for `student`, كود اليتيم for `code`).
   */
  private initializeSearchTypeOptions(): void {
    this.searchTypeOptions = [
      { id: 'all', name: this.translate.instant('common.all') },
      { id: 'father', name: this.translate.instant('families.refugeeSearchTypeFather') },
      { id: 'mother', name: this.translate.instant('families.refugeeSearchTypeMother') },
      { id: 'student', name: this.translate.instant('families.refugeeSearchTypeOrphan') },
      { id: 'provider', name: this.translate.instant('families.refugeeSearchTypeProvider') },
      { id: 'nationalId', name: this.translate.instant('families.refugeeSearchTypeNationalId') },
      { id: 'code', name: this.translate.instant('families.refugeeSearchTypeOrphanCode') },
      { id: 'phone', name: this.translate.instant('families.refugeeSearchTypePhone') }
    ];
  }

  private loadCharityOptions(): void {
    this.charityService.getCharities({ pageNumber: 1, pageSize: 500 })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.charityOptions = [
            { id: 'all', name: this.translate.instant('families.refugeeAllCharities') },
            ...(response.items || []).map(c => ({ id: c.id, name: c.name }))
          ];
          this.cdr.markForCheck();
        },
        error: (error: any) => {
          console.error('Error loading charities:', error);
        }
      });
  }

  loadRefugeeFamilies(): void {
    this.loading = true;
    const formValues = this.filterForm.value;
    const seq = ++this.requestSeq;

    const searchRequest: FamilySearchRequest = {
      familyType: 'Refugee',
      pageNumber: this.currentPage,
      pageSize: this.pageSize
    };

    if (this.isHQ && formValues.charityId && formValues.charityId !== 'all') {
      searchRequest.charityId = formValues.charityId;
    }

    if (formValues.searchValue && formValues.searchValue.trim()) {
      searchRequest.searchTerm = formValues.searchValue.trim();
    }

    if (formValues.searchType && formValues.searchType !== 'all') {
      searchRequest.searchType = formValues.searchType;
    }

    this.familyService.getFamilies(searchRequest)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          if (seq !== this.requestSeq) {
            return; // a newer request already superseded this response
          }
          this.families = (response.items as unknown as FamilyListItemDto[]) || [];
          this.totalCount = response.totalCount || 0;
          this.totalPages = Math.ceil(this.totalCount / this.pageSize);
          this.loading = false;
          this.cdr.markForCheck();
        },
        error: (error: any) => {
          if (seq !== this.requestSeq) {
            return;
          }
          console.error('Error loading refugee families:', error);
          this.notification.error(error?.message || this.translate.instant('families.refugeeListLoadFailed'));
          // P19 — never leave the previous page's rows standing under an error toast
          this.families = [];
          this.totalCount = 0;
          this.totalPages = 0;
          this.loading = false;
          this.cdr.markForCheck();
        }
      });
  }

  /** Statistics band — silent fail: the list stays fully usable without it. */
  loadStatistics(): void {
    this.familyService.getStatistics('Refugee')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (statistics) => {
          this.statistics = statistics;
          // OnPush: the band's *ngIf never re-evaluates unless the component is marked dirty
          this.cdr.markForCheck();
        },
        error: (error: any) => {
          console.error('Error loading refugee family statistics:', error);
        }
      });
  }

  onCharityChange(): void {
    this.currentPage = 1;
    this.loadRefugeeFamilies();
  }

  /** بحث command — reload page 1 with whatever the controls hold */
  onSearch(): void {
    this.currentPage = 1;
    this.loadRefugeeFamilies();
  }

  /** البحث عن طريق selector change — a different branch means a different query, page 1 */
  onSearchTypeChange(): void {
    this.currentPage = 1;
    this.loadRefugeeFamilies();
  }

  /** مسح التصفية — back to the initial sentinels, then page 1 */
  clearFilters(): void {
    this.filterForm.reset({
      charityId: 'all',
      searchValue: '',
      searchType: 'all'
    });
    this.currentPage = 1;
    this.loadRefugeeFamilies();
  }

  /** Any control off its 'all' sentinel or with search text typed */
  hasActiveFilters(): boolean {
    const formValues = this.filterForm.value;
    return !!(
      (this.isHQ && formValues.charityId && formValues.charityId !== 'all') ||
      (formValues.searchValue && formValues.searchValue.trim()) ||
      (formValues.searchType && formValues.searchType !== 'all')
    );
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadRefugeeFamilies();
  }

  getSerial(index: number): number {
    return (this.currentPage - 1) * this.pageSize + index + 1;
  }

  trackByFamilyId = (_: number, family: FamilyListItemDto): string => family.id;

  /**
   * تصدير (UC-4.12 export sheet) — same filters/scoping as the on-screen list, pinned to
   * the Refugee register. The endpoint re-authorises and widens the page server-side.
   */
  exportToExcel(): void {
    this.loading = true;
    const formValues = this.filterForm.value;

    const searchRequest: FamilySearchRequest = {
      familyType: 'Refugee',
      pageNumber: 1,
      pageSize: 10000
    };

    if (this.isHQ && formValues.charityId && formValues.charityId !== 'all') {
      searchRequest.charityId = formValues.charityId;
    }

    if (formValues.searchValue && formValues.searchValue.trim()) {
      searchRequest.searchTerm = formValues.searchValue.trim();
    }

    if (formValues.searchType && formValues.searchType !== 'all') {
      searchRequest.searchType = formValues.searchType;
    }

    this.familyService.exportFamiliesToExcel(searchRequest)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (blob: Blob) => {
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = `refugee_families_${new Date().toISOString().split('T')[0]}.xlsx`;
          document.body.appendChild(a);
          a.click();
          document.body.removeChild(a);
          window.URL.revokeObjectURL(url);

          this.notification.success(this.translate.instant('families.exportSuccess'));
          this.loading = false;
          this.cdr.markForCheck();
        },
        error: (error: any) => {
          console.error('Export failed:', error);
          this.notification.error(this.translate.instant('families.exportFailed'));
          this.loading = false;
          this.cdr.markForCheck();
        }
      });
  }

  viewRefugeeFamily(id: string): void {
    this.router.navigate(['/families/refugees', id]);
  }
}
