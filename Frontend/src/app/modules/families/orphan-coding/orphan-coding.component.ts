import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { Router } from '@angular/router';
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
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../shared/components';
import { SharedModule } from '../../../shared/shared.module';
import { DropDownComponent } from '../../../shared/components/drop-down/drop-down.component';
import { OrphanPaymentHistoryComponent } from '../orphan-payment-history/orphan-payment-history.component';

/**
 * UC-ORP-02/04/05/06/07 — تكويد الأيتام (§13.S.1).
 * The legacy coding screen: find an orphan by name (type-ahead), resolve a name
 * from a code, and assign the sponsorship code inline — check first (UC-ORP-05),
 * then save (UC-ORP-06). تم opens the orphan's payment history (UC-ORP-08/09).
 */
@Component({
  selector: 'app-orphan-coding',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    PageHeaderComponent,
    BreadcrumbComponent,
    TranslateModule,
    SharedModule,
    DropDownComponent,
    OrphanPaymentHistoryComponent
  ],
  templateUrl: './orphan-coding.component.html',
  styleUrls: ['./orphan-coding.component.scss']
})
export class OrphanCodingComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private langChangeSubscription?: Subscription;
  private codeCheckSubscription?: Subscription;
  private codeCheckInput$ = new Subject<string>();

  filterForm: FormGroup;

  /** الجمعية selector is an HQ-only control; a Charity caller is pinned server-side */
  isHQ = false;
  charityOptions: Array<{ id: string; name: string }> = [];

  /** Search results — §13.S.1 grid */
  results: OrphanLookupDto[] = [];
  totalCount = 0;
  loading = false;
  searched = false;

  /** تم — the orphan whose payment history is open (UC-ORP-08/09) */
  selectedOrphan: OrphanLookupDto | null = null;

  /** تعديل الكود inline edit state (UC-ORP-05 check → UC-ORP-06 save) */
  editingOrphanId: string | null = null;
  editCode = '';
  codeCheckStatus: 'idle' | 'checking' | 'available' | 'taken' = 'idle';
  codeCheckResult: OrphanCodeCheckDto | null = null;
  saving = false;

  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'families.title', url: '/families' },
    { label: 'orphanCoding.title' }
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
      orphanName: [''],
      code: ['']
    });
  }

  ngOnInit(): void {
    this.isHQ = this.auth.hasAnyRole(['SuperAdmin', 'Admin']);

    this.langChangeSubscription = this.translate.onLangChange.subscribe((event: LangChangeEvent) => {
      this.cdr.markForCheck();
    });

    if (this.isHQ) {
      this.loadCharityOptions();
    }

    // اسم اليتيم type-ahead (UC-ORP-02): 3+ characters, debounced
    this.filterForm.get('orphanName')!.valueChanges
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        takeUntil(this.destroy$)
      )
      .subscribe(term => {
        if (term && term.trim().length >= 3) {
          this.search(term.trim());
        } else if (this.searched && (!term || term.trim().length < 3)) {
          // Clear the grid when the term drops below the type-ahead threshold
          this.results = [];
          this.searched = false;
          this.cdr.markForCheck();
        }
      });

    // P18 — the inline availability check debounces like the type-ahead: one request after the
    // coder stops typing, not one per keystroke.
    this.codeCheckInput$
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntil(this.destroy$))
      .subscribe(code => this.performCodeCheck(code));
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

  private resolvedCharityId(): string | undefined {
    const charityId = this.filterForm.get('charityId')?.value;
    return this.isHQ && charityId && charityId !== 'all' ? charityId : undefined;
  }

  /** Shared search (UC-ORP-02 by name / UC-ORP-07 by code) */
  search(term?: string): void {
    const name = term ?? this.filterForm.get('orphanName')?.value ?? '';
    const code = this.filterForm.get('code')?.value ?? '';
    const effectiveTerm = code.trim() || name.trim();

    if (!effectiveTerm) {
      this.notification.warning(this.translate.instant('orphanCoding.enterNameOrCode'));
      return;
    }

    this.loading = true;
    this.selectedOrphan = null;
    this.cancelEdit();
    this.cdr.markForCheck(); // P17 — OnPush: the loading state must render before the HTTP wait

    const request: OrphanCodingSearchRequest = {
      search: effectiveTerm,
      charityId: this.resolvedCharityId(),
      pageNumber: 1,
      pageSize: 20
    };

    this.familyService.searchOrphansCoding(request).subscribe({
      next: (response) => {
        this.results = response.items || [];
        this.totalCount = response.totalCount || 0;
        this.loading = false;
        this.searched = true;
        this.cdr.markForCheck();
      },
      error: (error: any) => {
        console.error('Error searching orphans:', error);
        this.notification.error(error.message || this.translate.instant('orphanCoding.searchFailed'));
        this.loading = false;
        this.cdr.markForCheck();
      }
    });
  }

  onCharityChange(): void {
    // Re-run the standing search under the new scope when a term is present
    const name = this.filterForm.get('orphanName')?.value;
    const code = this.filterForm.get('code')?.value;
    if ((name && name.trim().length >= 3) || (code && code.trim())) {
      this.search();
    }
  }

  /** تم — open the orphan's payment history (UC-ORP-08/09) */
  selectOrphan(orphan: OrphanLookupDto): void {
    this.selectedOrphan = orphan;
    this.cdr.markForCheck();
  }

  onHistoryClosed(): void {
    this.selectedOrphan = null;
    this.cdr.markForCheck();
  }

  /** غلق — back to the families register */
  close(): void {
    this.router.navigate(['/families']);
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

    this.codeCheckSubscription = this.familyService.checkOrphanCode({
      code,
      charityId: this.resolvedCharityId(),
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
        this.cdr.markForCheck(); // P17 — cancelEdit mutates without marking
        this.cancelEdit();
        this.search(); // refresh the grid under the standing term
      },
      error: (error: any) => {
        console.error('Error assigning code:', error);
        this.notification.error(error.message || this.translate.instant('orphanCoding.codeSaveFailed'));
        this.saving = false;
        this.cdr.markForCheck();
      }
    });
  }

  getSerial(index: number): number {
    return index + 1;
  }

  trackByOrphanId = (_: number, orphan: OrphanLookupDto): string => orphan.orphanId;
}
