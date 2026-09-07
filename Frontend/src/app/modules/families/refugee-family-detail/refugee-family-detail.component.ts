import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnDestroy, OnInit, TrackByFunction } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject, forkJoin, of } from 'rxjs';
import { catchError, takeUntil } from 'rxjs/operators';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { FamilyService } from '../services/family.service';
import { LookupManagementService } from '../../lookup-management/services/lookup-management.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../shared/components';
import { SharedModule } from '../../../shared/shared.module';
import { FamilyDto, OrphanDto, ProviderDto, RelativeListDto } from '../models/family.model';

/**
 * UC-REF-04 (7-4) — عرض أسرة لاجئة: the read-only view mode of §12.S.2. Loads the
 * household (GET /api/Families/{id}) plus the per-member reads (provider / orphans /
 * relatives) and renders the cheque-grid section wired-but-empty — no family-filtered
 * payment read exists yet (data belongs to epic 10; gap recorded in the story notes).
 */
@Component({
  selector: 'app-refugee-family-detail',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    PageHeaderComponent,
    BreadcrumbComponent,
    TranslateModule,
    SharedModule
  ],
  templateUrl: './refugee-family-detail.component.html',
  styleUrls: ['./refugee-family-detail.component.scss']
})
export class RefugeeFamilyDetailComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  family?: FamilyDto;
  provider?: ProviderDto;
  orphans: OrphanDto[] = [];
  relatives: RelativeListDto[] = [];

  loading = true;
  /** P17 — the household read itself failed; the member sections fell back to empty */
  loadFailed = false;
  familyId = '';

  /** P18 — resolves provider.nationalityCountryId to the country's name */
  countryOptions: Array<{ id: number | string; name: string }> = [];

  /** تعديل اعضاء الاسرة — shown only to callers the server would accept */
  canEdit = false;

  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'families.refugeeTitle', url: '/families/refugees' },
    { label: 'families.refugeeDetailTitle' }
  ];

  // Untyped so one helper serves both the orphan and companion rows without narrowing the
  // *ngFor item to their union (which would drop OrphanDto-only fields like socialStatusName)
  trackById: TrackByFunction<any> = (_: number, item: any) => item.id;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private familyService: FamilyService,
    private lookupService: LookupManagementService,
    private auth: AuthService,
    private notification: NotificationService,
    private translate: TranslateService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.familyId = this.route.snapshot.paramMap.get('id') || '';
    this.canEdit = this.auth.hasPermission('Families.Edit');
    if (!this.familyId) {
      this.router.navigate(['/families/refugees']);
      return;
    }

    // P18 — country catalogue for the provider's جنسية resolution
    this.lookupService.getCountries({ page: 1, pageSize: 1000, isActive: true })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => {
          this.countryOptions = (result.items || []).map(c => ({ id: c.id, name: c.nameAr || c.name || c.nameEn || String(c.id) }));
          this.cdr.markForCheck();
        },
        error: () => this.countryOptions = []
      });

    // P17 — the household read decides the screen; each member read degrades alone (a
    // missing guardian must not blank the whole file). Only the family stream's failure
    // fails the screen.
    forkJoin({
      family: this.familyService.getFamily(this.familyId),
      provider: this.familyService.getProvider(this.familyId)
        .pipe(catchError(() => of(undefined as unknown as ProviderDto))),
      orphans: this.familyService.getFamilyOrphans(this.familyId)
        .pipe(catchError(() => of([] as OrphanDto[]))),
      relatives: this.familyService.getRelatives(this.familyId)
        .pipe(catchError(() => of([] as RelativeListDto[])))
    })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => {
          // P16 — this screen renders the refugee register's file; anything else bounces
          if ((result.family.familyType || '') !== 'Refugee') {
            this.notification.warning(this.translate.instant('families.refugeeNotRefugeeFamily'));
            this.router.navigate(['/families/refugees']);
            return;
          }
          this.family = result.family;
          this.provider = result.provider;
          this.orphans = result.orphans || [];
          this.relatives = result.relatives || [];
          this.loading = false;
          this.cdr.markForCheck();
        },
        error: () => {
          this.loading = false;
          this.loadFailed = true;
          this.notification.error(this.translate.instant('families.refugeeDetailLoadFailed'));
          this.cdr.markForCheck();
        }
      });
  }

  /** P18 — the provider's جنسية by name; the raw lookup id never reaches the screen */
  countryName(id?: number | null): string {
    if (id === null || id === undefined) {
      return '—';
    }
    return this.countryOptions.find(c => c.id === id)?.name || '—';
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /** تعديل اعضاء الاسرة — the §12.S.2 edit screen carries the same :id */
  edit(): void {
    this.router.navigate(['/families/refugees', this.familyId, 'edit']);
  }

  back(): void {
    this.router.navigate(['/families/refugees']);
  }
}
