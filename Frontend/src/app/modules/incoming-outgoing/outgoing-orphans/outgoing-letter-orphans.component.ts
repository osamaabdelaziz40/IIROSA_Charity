import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { forkJoin } from 'rxjs';
import { OutgoingService } from '../services/outgoing.service';
import { CharityService } from '../../charities/services/charity.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../shared/components';
import { DropDownComponent } from '../../../shared/components/drop-down/drop-down.component';
import { SharedModule } from '../../../shared/shared.module';
import { OutgoingListDto, OrphanOptionDto } from '../models/outgoing.model';

/**
 * UC-COR-18 / §21.S.6 — ربط تقارير الأيتام بخطاب صادر.
 *
 * Read-then-save flow (mirror of 16-9): row commands stage the change locally and
 * the grids update immediately; حفظ flushes the staged attach/detach set through
 * the per-link endpoints, then reloads both grids from the server. BR-26 (an
 * orphan report attaches to at most one letter) is enforced server-side with a
 * «Operation Faild» refusal; BR-27 (charity-scoped selection) shapes the
 * unattached list the server returns.
 */
@Component({
  selector: 'app-outgoing-letter-orphans',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    PageHeaderComponent,
    BreadcrumbComponent,
    TranslateModule,
    SharedModule,
    DropDownComponent
  ],
  templateUrl: './outgoing-letter-orphans.component.html',
  styleUrls: ['./outgoing-letter-orphans.component.scss']
})
export class OutgoingLetterOrphansComponent implements OnInit {
  loading = false;
  saving = false;
  loadingOrphans = false;

  isSuperAdmin = false;

  selectedLetterId: string | null = null;

  // Dropdown sources
  charityOptions: Array<{ id: string | null; name: string }> = [{ id: null, name: 'incomingOutgoing.allCharities' }];
  letterOptions: Array<{ id: string; name: string }> = [];

  // Server state for the chosen letter
  private serverAttached: OrphanOptionDto[] = [];
  private serverUnattached: OrphanOptionDto[] = [];

  // Staged changes flushed on حفظ
  private stagedAttach = new Set<string>();
  private stagedDetach = new Set<string>();

  filterForm: FormGroup;

  pageActions = [
    {
      label: 'common.save',
      type: 'primary',
      icon: 'fe-save',
      click: () => this.save()
    }
  ];

  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'incomingOutgoing.outgoingLetters', url: '/incoming-outgoing/outgoing' },
    { label: 'incomingOutgoing.attachOrphansTitle' }
  ];

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private route: ActivatedRoute,
    private outgoingService: OutgoingService,
    private charityService: CharityService,
    private authService: AuthService,
    private notification: NotificationService,
    private translate: TranslateService
  ) {
    this.filterForm = this.fb.group({
      selectedCharity: [null],
      selectedLetter: [null]
    });
  }

  ngOnInit(): void {
    this.isSuperAdmin = this.authService.hasRole('SuperAdmin');
    if (this.isSuperAdmin) {
      this.loadCharities();
    }
    this.loadLetters();
  }

  private loadCharities(): void {
    this.charityService.getCharities({ pageNumber: 1, pageSize: 1000, isActive: true }).subscribe({
      next: result => {
        this.charityOptions = [
          { id: null, name: 'incomingOutgoing.allCharities' },
          ...(result.items || []).map(c => ({ id: c.id, name: c.name }))
        ];
      },
      error: () => console.error('Error loading charities')
    });
  }

  private loadLetters(): void {
    const charityId = this.filterForm.value.selectedCharity || undefined;

    this.outgoingService.getOutgoingLetters({ pageNumber: 1, pageSize: 1000, charityId }).subscribe({
      next: result => {
        this.letterOptions = (result.items || []).map((l: OutgoingListDto) => ({
          id: l.id,
          name: `${l.serial != null ? String(l.serial).padStart(4, '0') : ''} - ${l.subject}`
        }));
        if (this.route.snapshot.queryParamMap.get('outgoingId')) {
          this.preselectFromQuery();
        }
      },
      error: () => console.error('Error loading outgoing letters')
    });
  }

  /** Entry point from the letter's detail screen — preselect the letter in question. */
  private preselectFromQuery(): void {
    const outgoingId = this.route.snapshot.queryParamMap.get('outgoingId');
    if (!outgoingId) {
      return;
    }

    this.filterForm.patchValue({ selectedLetter: outgoingId });
    this.onLetterChange();
  }

  onCharityChange(): void {
    this.filterForm.patchValue({ selectedLetter: null });
    this.resetLetterState();
    this.loadLetters();
  }

  onLetterChange(): void {
    const letterId = this.filterForm.value.selectedLetter;
    if (!letterId) {
      this.resetLetterState();
      return;
    }
    this.loadOrphans(letterId);
  }

  private resetLetterState(): void {
    this.selectedLetterId = null;
    this.serverAttached = [];
    this.serverUnattached = [];
    this.stagedAttach.clear();
    this.stagedDetach.clear();
  }

  private loadOrphans(letterId: string): void {
    this.selectedLetterId = letterId;
    this.loadingOrphans = true;

    this.outgoingService.getOrphans(letterId).subscribe({
      next: result => {
        this.serverAttached = result.attached || [];
        this.serverUnattached = result.unattached || [];
        this.stagedAttach.clear();
        this.stagedDetach.clear();
        this.loadingOrphans = false;
      },
      error: (error: any) => {
        console.error('Error loading orphans:', error);
        this.notification.error(error.message || this.translate.instant('incomingOutgoing.loadLetterFailed'));
        this.loadingOrphans = false;
      }
    });
  }

  // ===== Grid views — server state with the staged set applied =====

  get attachedOrphans(): Array<OrphanOptionDto & { staged: 'attach' | 'none' }> {
    return [
      ...this.serverUnattached.filter(o => this.stagedAttach.has(o.orphanId)).map(o => ({ ...o, staged: 'attach' as const })),
      ...this.serverAttached.filter(o => !this.stagedDetach.has(o.orphanId)).map(o => ({ ...o, staged: 'none' as const }))
    ];
  }

  get unattachedOrphans(): Array<OrphanOptionDto & { staged: 'detach' | 'none' }> {
    return [
      ...this.serverAttached.filter(o => this.stagedDetach.has(o.orphanId)).map(o => ({ ...o, staged: 'detach' as const })),
      ...this.serverUnattached.filter(o => !this.stagedAttach.has(o.orphanId)).map(o => ({ ...o, staged: 'none' as const }))
    ];
  }

  get hasPendingChanges(): boolean {
    return this.stagedAttach.size > 0 || this.stagedDetach.size > 0;
  }

  // اضافة الى الخطاب
  addOrphan(orphan: OrphanOptionDto): void {
    if (this.stagedDetach.has(orphan.orphanId)) {
      this.stagedDetach.delete(orphan.orphanId);
    } else {
      this.stagedAttach.add(orphan.orphanId);
    }
  }

  // حذف
  removeOrphan(orphan: OrphanOptionDto): void {
    if (this.stagedAttach.has(orphan.orphanId)) {
      this.stagedAttach.delete(orphan.orphanId);
    } else {
      this.stagedDetach.add(orphan.orphanId);
    }
  }

  save(): void {
    if (!this.selectedLetterId || !this.hasPendingChanges || this.saving) return;

    this.saving = true;
    const letterId = this.selectedLetterId;

    const operations = [
      ...Array.from(this.stagedAttach).map(orphanId => this.outgoingService.attachOrphan(letterId, orphanId)),
      ...Array.from(this.stagedDetach).map(orphanId => this.outgoingService.detachOrphan(letterId, orphanId))
    ];

    forkJoin(operations).subscribe({
      next: () => {
        this.notification.success(this.translate.instant('incomingOutgoing.changesSaved'));
        this.saving = false;
        this.loadOrphans(letterId);
      },
      error: (error: any) => {
        console.error('Error saving orphan attachments:', error);
        this.notification.error(error.message || this.translate.instant('incomingOutgoing.saveFailed'));
        this.saving = false;
        this.loadOrphans(letterId);
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/incoming-outgoing/outgoing']);
  }

  trackByOrphanId(_index: number, orphan: OrphanOptionDto): string {
    return orphan.orphanId;
  }
}
