import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { forkJoin } from 'rxjs';
import { IncomingService } from '../services/incoming.service';
import { CharityService } from '../../charities/services/charity.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../shared/components';
import { DropDownComponent } from '../../../shared/components/drop-down/drop-down.component';
import { SharedModule } from '../../../shared/shared.module';
import { IncomingListDto, EmployeeOptionDto } from '../models/incoming.model';

/**
 * UC-COR-09 / §21.S.3 — ربط الموظفين بخطاب وارد.
 *
 * Read-then-save flow: row commands stage the change locally and the grids
 * update immediately; حفظ flushes the staged attach/detach set through the
 * per-link endpoints, then reloads both grids from the server.
 */
@Component({
  selector: 'app-incoming-letter-employees',
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
  templateUrl: './incoming-letter-employees.component.html',
  styleUrls: ['./incoming-letter-employees.component.scss']
})
export class IncomingLetterEmployeesComponent implements OnInit {
  loading = false;
  saving = false;
  loadingEmployees = false;

  isSuperAdmin = false;

  // The letter chosen on the screen (Guid) — null until picked
  selectedLetterId: string | null = null;
  selectedLetter: IncomingListDto | null = null;

  // Dropdown sources
  charityOptions: Array<{ id: string | null; name: string }> = [{ id: null, name: 'incomingOutgoing.allCharities' }];
  letterOptions: Array<{ id: string; name: string }> = [];

  // Server state for the chosen letter
  private serverAttached: EmployeeOptionDto[] = [];
  private serverAvailable: EmployeeOptionDto[] = [];

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
    { label: 'incomingOutgoing.incomingLetters', url: '/incoming-outgoing/incoming' },
    { label: 'incomingOutgoing.attachEmployeesTitle' }
  ];

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private route: ActivatedRoute,
    private incomingService: IncomingService,
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

  /** Entry point from the letter's detail screen — preselect the letter in question. */
  private preselectFromQuery(): void {
    const incomingId = this.route.snapshot.queryParamMap.get('incomingId');
    if (!incomingId) {
      return;
    }

    this.filterForm.patchValue({ selectedLetter: incomingId });
    this.onLetterChange();
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

    this.incomingService.getIncomingLetters({ pageNumber: 1, pageSize: 1000, charityId }).subscribe({
      next: result => {
        this.letterOptions = (result.items || []).map(l => ({
          id: l.id,
          name: `${l.serialTxt || ''} - ${l.subject}`
        }));
        if (this.route.snapshot.queryParamMap.get('incomingId')) {
          this.preselectFromQuery();
        }
      },
      error: () => console.error('Error loading incoming letters')
    });
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
    this.loadEmployees(letterId);
  }

  private resetLetterState(): void {
    this.selectedLetterId = null;
    this.selectedLetter = null;
    this.serverAttached = [];
    this.serverAvailable = [];
    this.stagedAttach.clear();
    this.stagedDetach.clear();
  }

  private loadEmployees(letterId: string): void {
    this.selectedLetterId = letterId;
    this.loadingEmployees = true;

    this.incomingService.getEmployees(letterId).subscribe({
      next: result => {
        this.serverAttached = result.attached || [];
        this.serverAvailable = result.available || [];
        this.stagedAttach.clear();
        this.stagedDetach.clear();
        this.loadingEmployees = false;
      },
      error: (error: any) => {
        console.error('Error loading employees:', error);
        this.notification.error(error.message || this.translate.instant('incomingOutgoing.loadLetterFailed'));
        this.loadingEmployees = false;
      }
    });
  }

  // ===== Grid views — server state with the staged set applied =====

  get attachedEmployees(): Array<EmployeeOptionDto & { staged: 'attach' | 'none' }> {
    return [
      ...this.serverAvailable.filter(e => this.stagedAttach.has(e.userId)).map(e => ({ ...e, staged: 'attach' as const })),
      ...this.serverAttached.filter(e => !this.stagedDetach.has(e.userId)).map(e => ({ ...e, staged: 'none' as const }))
    ];
  }

  get availableEmployees(): Array<EmployeeOptionDto & { staged: 'detach' | 'none' }> {
    return [
      ...this.serverAttached.filter(e => this.stagedDetach.has(e.userId)).map(e => ({ ...e, staged: 'detach' as const })),
      ...this.serverAvailable.filter(e => !this.stagedAttach.has(e.userId)).map(e => ({ ...e, staged: 'none' as const }))
    ];
  }

  get hasPendingChanges(): boolean {
    return this.stagedAttach.size > 0 || this.stagedDetach.size > 0;
  }

  // اضافة الى الخطاب — stage the attach (un-detach if it was staged for removal)
  addEmployee(employee: EmployeeOptionDto): void {
    if (this.stagedDetach.has(employee.userId)) {
      this.stagedDetach.delete(employee.userId);
    } else {
      this.stagedAttach.add(employee.userId);
    }
  }

  // حذف — stage the detach (un-stage the attach if it was staged for addition)
  removeEmployee(employee: EmployeeOptionDto): void {
    if (this.stagedAttach.has(employee.userId)) {
      this.stagedAttach.delete(employee.userId);
    } else {
      this.stagedDetach.add(employee.userId);
    }
  }

  save(): void {
    if (!this.selectedLetterId || !this.hasPendingChanges || this.saving) return;

    this.saving = true;
    const letterId = this.selectedLetterId;

    const operations = [
      ...Array.from(this.stagedAttach).map(userId => this.incomingService.attachEmployee(letterId, userId)),
      ...Array.from(this.stagedDetach).map(userId => this.incomingService.detachEmployee(letterId, userId))
    ];

    forkJoin(operations).subscribe({
      next: () => {
        this.notification.success(this.translate.instant('incomingOutgoing.changesSaved'));
        this.saving = false;
        this.loadEmployees(letterId);
      },
      error: (error: any) => {
        console.error('Error saving employee attachments:', error);
        this.notification.error(error.message || this.translate.instant('incomingOutgoing.saveFailed'));
        this.saving = false;
        this.loadEmployees(letterId);
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/incoming-outgoing/incoming']);
  }

  trackByUserId(_index: number, employee: EmployeeOptionDto): string {
    return employee.userId;
  }
}
