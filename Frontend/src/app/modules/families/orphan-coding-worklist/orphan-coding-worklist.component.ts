import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged, takeUntil } from 'rxjs/operators';
import { TranslateModule, TranslateService, LangChangeEvent } from '@ngx-translate/core';
import {
  OrphanLookupDto,
  OrphanCodingSearchRequest,
  OrphanCodeCheckDto
} from '../models/family.model';
import { FamilyService } from '../services/family.service';
import { CharityService } from '../../charities/services/charity.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { BreadcrumbComponent, BreadcrumbItem, PaginationComponent } from '../../../shared/components';
import { SharedModule } from '../../../shared/shared.module';
import { DropDownComponent } from '../../../shared/components/drop-down/drop-down.component';
import { OrphanPaymentHistoryComponent } from '../orphan-payment-history/orphan-payment-history.component';

/**
 * UC-ORP-03 — تكويد الأيتام (worklist, §13.S.2).
 * The HQ coding queue: every uncoded orphan (empty code), filterable by الجمعية,
 * sortable on the name columns, with the inline تعديل الكود edit — check (UC-ORP-05)
 * then SaveCode (UC-ORP-06). The codingStatus role gate is server-side; the route's
 * PermissionGuard hides the screen from non-HQ roles.
 */
@Component({
  selector: 'app-orphan-coding-worklist',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    PageHeaderComponent,
    BreadcrumbComponent,
    TranslateModule,
    PaginationComponent,
    SharedModule,
    DropDownComponent,
    OrphanPaymentHistoryComponent
  ],
  templateUrl: './orphan-coding-worklist.component.html',
  styleUrls: ['./orphan-coding-worklist.component.scss']
})
export class OrphanCodingWorklistComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private langChangeSubscription?: Subscription;
  private codeCheckSubscription?: Subscription;
  private codeCheckInput$ = new Subject<string>();

  filterForm: FormGroup;

  charityOptions: Array<{ id: string; name: string }> = [];

  /** The worklist — uncoded orphans (codingStatus=Pending) */
  orphans: OrphanLookupDto[] = [];
  totalCount = 0;
  totalPages = 0;
  currentPage = 1;
  pageSize = 20;
  loading = false;

  /** Server-driven sorting on the §13.S.2 ^ columns */
  sortBy = 'FullName';
  sortDescending = false;

  /** Inline تعديل الكود edit state (UC-ORP-05 check → UC-ORP-06 save) */
  editingOrphanId: string | null = null;
  editCode = '';
  codeCheckStatus: 'idle' | 'checking' | 'available' | 'taken' = 'idle';
  codeCheckResult: OrphanCodeCheckDto | null = null;
  saving = false;

  /** Payment history opened from a row (UC-ORP-08/09) */
  selectedOrphan: OrphanLookupDto | null = null;

  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'families.title', url: '/families' },
    { label: 'orphanCoding.worklistTitle' }
  ];

  constructor(
    private fb: FormBuilder,
    private familyService: FamilyService,
    private charityService: CharityService,
    private notification: NotificationService,
    private translate: TranslateService,
    private cdr: ChangeDetectorRef
  ) {
    this.filterForm = this.fb.group({
      charityId: ['all']
    });
  }

  ngOnInit(): void {
    this.langChangeSubscription = this.translate.onLangChange.subscribe((event: LangChangeEvent) => {
      this.cdr.markForCheck();
    });

    // P18 — the inline availability check debounces: one request after the coder stops typing.
    this.codeCheckInput$
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntil(this.destroy$))
      .subscribe(code => this.performCodeCheck(code));

    this.loadCharityOptions();
    this.loadWorklist();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.codeCheckInput$.complete();
    if (this.langChangeSubscription) {
      this.langChangeSubscription.unsubscribe();
    }
    if (this.codeCheckSubscription) {
      this.codeCheckSubscription.unsubscribe();
    }
  }

  private loadCharityOptions(): void {
    this.charityService.getCharities({ pageNumber: 1, pageSize: 500 }).subscribe({
      next: (response) => {
        this.charityOptions = [
          { id: 'all', name: this.translate.instant('orphanCoding.allCharities') },
          ...(response.items || []).map(c => ({ id: c.id, name: c.name }))
        ];
        this.cdr.markForCheck();
      },
      error: (error: any) => {
        console.error('Error loading charities:', error);
      }
    });
  }

  /** The coding worklist: codingStatus=Pending = uncoded (empty code) — HQ only on the server */
  loadWorklist(): void {
    this.loading = true;
    this.selectedOrphan = null;
    this.cancelEdit();
    this.cdr.markForCheck(); // P17 — OnPush: the loading state must render before the HTTP wait

    const charityId = this.filterForm.get('charityId')?.value;
    const request: OrphanCodingSearchRequest = {
      codingStatus: 'Pending',
      charityId: charityId && charityId !== 'all' ? charityId : undefined,
      sortBy: this.sortBy,
      sortDescending: this.sortDescending,
      pageNumber: this.currentPage,
      pageSize: this.pageSize
    };

    this.familyService.searchOrphansCoding(request).subscribe({
      next: (response) => {
        this.orphans = response.items || [];
        this.totalCount = response.totalCount || 0;
        this.totalPages = Math.ceil(this.totalCount / this.pageSize);

        // P9 — coding the last orphan of the last page shrinks the queue: clamp the page or
        // the grid shows an empty page while orphans remain on earlier pages.
        if (this.totalPages > 0 && this.currentPage > this.totalPages) {
          this.currentPage = this.totalPages;
          this.loadWorklist();
          return;
        }

        this.loading = false;
        this.cdr.markForCheck();
      },
      error: (error: any) => {
        console.error('Error loading coding worklist:', error);
        this.notification.error(error.message || this.translate.instant('orphanCoding.worklistLoadFailed'));
        this.loading = false;
        this.cdr.markForCheck();
      }
    });
  }

  onCharityChange(): void {
    this.currentPage = 1;
    this.loadWorklist();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadWorklist();
  }

  /** Server-driven sort on the ^ columns of §13.S.2 */
  sortByColumn(column: string): void {
    if (this.sortBy === column) {
      this.sortDescending = !this.sortDescending;
    } else {
      this.sortBy = column;
      this.sortDescending = false;
    }
    this.currentPage = 1;
    this.loadWorklist();
  }

  sortIcon(column: string): string {
    if (this.sortBy !== column) {
      return 'fe fe-chevrons-up-down text-muted';
    }
    return this.sortDescending ? 'fe fe-chevron-down' : 'fe fe-chevron-up';
  }

  getSerial(index: number): number {
    return (this.currentPage - 1) * this.pageSize + index + 1;
  }

  selectOrphan(orphan: OrphanLookupDto): void {
    this.selectedOrphan = orphan;
    this.cdr.markForCheck();
  }

  onHistoryClosed(): void {
    this.selectedOrphan = null;
    this.cdr.markForCheck();
  }

  // ==================== تعديل الكود (UC-ORP-05 → UC-ORP-06) ====================

  startEdit(orphan: OrphanLookupDto): void {
    this.editingOrphanId = orphan.orphanId;
    this.editCode = orphan.code || '';
    this.codeCheckStatus = 'idle';
    this.codeCheckResult = null;
    this.cdr.markForCheck();
  }

  cancelEdit(): void {
    this.editingOrphanId = null;
    this.editCode = '';
    this.codeCheckStatus = 'idle';
    this.codeCheckResult = null;
  }

  /** Inline availability check (BR-07) — debounced via codeCheckInput$ (P18) */
  onEditCodeChange(): void {
    const code = this.editCode.trim();
    if (!code) {
      if (this.codeCheckSubscription) {
        this.codeCheckSubscription.unsubscribe();
      }
      this.codeCheckStatus = 'idle';
      this.codeCheckResult = null;
      this.cdr.markForCheck();
      return;
    }

    this.codeCheckStatus = 'checking';
    this.cdr.markForCheck();
    this.codeCheckInput$.next(code);
  }

  private performCodeCheck(code: string): void {
    if (this.codeCheckSubscription) {
      this.codeCheckSubscription.unsubscribe();
    }

    const charityId = this.filterForm.get('charityId')?.value;
    this.codeCheckSubscription = this.familyService.checkOrphanCode({
      code,
      charityId: charityId && charityId !== 'all' ? charityId : undefined,
      excludeOrphanId: this.editingOrphanId || undefined
    }).subscribe({
      next: (result) => {
        this.codeCheckResult = result;
        this.codeCheckStatus = result.isAvailable ? 'available' : 'taken';
        this.cdr.markForCheck();
      },
      error: (error: any) => {
        console.error('Error checking code:', error);
        this.codeCheckStatus = 'idle';
        this.cdr.markForCheck();
      }
    });
  }

  /** SaveCode (UC-ORP-06) — the server re-checks uniqueness inside the save */
  saveCode(orphan: OrphanLookupDto): void {
    const code = this.editCode.trim();
    if (!code) {
      this.notification.warning(this.translate.instant('orphanCoding.codeRequired'));
      return;
    }
    if (this.codeCheckStatus === 'taken') {
      this.notification.error(this.translate.instant('orphanCoding.codeTaken'));
      return;
    }

    this.saving = true;
    this.familyService.assignOrphanCode({ orphanId: orphan.orphanId, code }).subscribe({
      next: () => {
        this.notification.success(this.translate.instant('orphanCoding.codeSaved'));
        this.saving = false;
        this.cdr.markForCheck(); // P17 — saving flips before the reload marks
        this.loadWorklist(); // the coded orphan leaves the uncoded queue
      },
      error: (error: any) => {
        console.error('Error assigning code:', error);
        this.notification.error(error.message || this.translate.instant('orphanCoding.codeSaveFailed'));
        this.saving = false;
        this.cdr.markForCheck();
      }
    });
  }

  trackByOrphanId = (_: number, orphan: OrphanLookupDto): string => orphan.orphanId;
}
