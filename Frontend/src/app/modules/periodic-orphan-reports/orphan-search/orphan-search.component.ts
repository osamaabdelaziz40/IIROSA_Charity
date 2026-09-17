import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, takeUntil } from 'rxjs/operators';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { FamilyService } from '../../families/services/family.service';
import { CharityService } from '../../charities/services/charity.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { OrphanCodingSearchRequest, OrphanLookupDto } from '../../families/models/family.model';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { BreadcrumbComponent, BreadcrumbItem, PaginationComponent } from '../../../shared/components';
import { DropDownComponent } from '../../../shared/components/drop-down/drop-down.component';
import { SharedModule } from '../../../shared/shared.module';

/**
 * Report-entry shortcut: search an orphan by name or sponsorship code (UC-ORP-02
 * shared read), then jump straight into the periodic report create form with the
 * orphan pre-resolved. The report itself is entered on the UC-6.11 form — this
 * screen only picks the subject.
 *
 * Uncoded orphans render without the action: the report form resolves its subject
 * by code (UC-ORR-02), so no code means no report yet.
 */
@Component({
  selector: 'app-orphan-search',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule,
    TranslateModule,
    PageHeaderComponent,
    BreadcrumbComponent,
    PaginationComponent,
    DropDownComponent,
    SharedModule
  ],
  templateUrl: './orphan-search.component.html',
  styleUrls: ['./orphan-search.component.scss']
})
export class OrphanSearchComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  filterForm: FormGroup;

  orphans: OrphanLookupDto[] = [];
  totalCount = 0;
  currentPage = 1;
  pageSize = 20;
  loading = false;

  /** HQ narrowing (كافة الجهات head option included); hidden from charity-scoped callers. */
  isSuperAdmin = false;
  charityOptions: Array<{ id: string | null; name: string }> = [];

  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'periodicReports.title', url: '/periodic-orphan-reports' },
    { label: 'periodicReports.orphanSearch.title' }
  ];

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private familyService: FamilyService,
    private charityService: CharityService,
    private authService: AuthService,
    private notification: NotificationService,
    private translate: TranslateService,
    private cdr: ChangeDetectorRef
  ) {
    this.filterForm = this.fb.group({
      searchValue: [''],
      selectedCharity: [null]
    });
  }

  ngOnInit(): void {
    this.isSuperAdmin = this.authService.hasRole('SuperAdmin');
    if (this.isSuperAdmin) {
      this.loadCharityOptions();
    }

    // Type-ahead: one request after the caller stops typing, not one per keystroke.
    this.filterForm.get('searchValue')!.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntil(this.destroy$))
      .subscribe(() => {
        this.currentPage = 1;
        this.loadOrphans();
      });

    this.loadOrphans();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadCharityOptions(): void {
    this.charityService.getCharities({ pageNumber: 1, pageSize: 1000, isActive: true }).subscribe({
      next: result => {
        this.charityOptions = [
          { id: null, name: this.translate.instant('orphanCoding.allCharities') },
          ...(result.items || []).map(c => ({ id: c.id, name: c.name }))
        ];
        this.cdr.markForCheck();
      },
      error: (error: any) => console.error('Error loading charities:', error)
    });
  }

  /** The plain UC-ORP-02 search — every role, charity scope enforced server-side. */
  loadOrphans(): void {
    this.loading = true;
    this.cdr.markForCheck();

    const charityId = this.filterForm.get('selectedCharity')?.value;
    const search = (this.filterForm.get('searchValue')?.value || '').trim();
    const request: OrphanCodingSearchRequest = {
      search: search || undefined,
      charityId: charityId || undefined,
      pageNumber: this.currentPage,
      pageSize: this.pageSize
    };

    this.familyService.searchOrphansCoding(request).subscribe({
      next: response => {
        this.orphans = response.items || [];
        this.totalCount = response.totalCount || 0;
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: (error: any) => {
        console.error('Error searching orphans:', error);
        this.notification.error(error?.error?.message || error?.message
          || this.translate.instant('periodicReports.orphanSearch.loadFailed'));
        this.loading = false;
        this.cdr.markForCheck();
      }
    });
  }

  onFilterChange(): void {
    this.currentPage = 1;
    this.loadOrphans();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadOrphans();
  }

  /**
   * Open the UC-6.11 create form with the orphan pre-resolved — the form reads the
   * code query param and runs its UC-ORR-02 lookup itself, so an out-of-scope or
   * since-decoded code still resolves through the one authoritative path.
   */
  addReport(orphan: OrphanLookupDto): void {
    if (!orphan.code) {
      return;
    }
    this.router.navigate(['/periodic-orphan-reports/create'], { queryParams: { code: orphan.code } });
  }

  getSerial(index: number): number {
    return (this.currentPage - 1) * this.pageSize + index + 1;
  }

  trackByOrphanId = (_: number, orphan: OrphanLookupDto): string => orphan.orphanId;
}
