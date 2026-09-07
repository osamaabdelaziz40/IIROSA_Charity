/**
 * Housing Family Detail (UC-HOU-04 · §11.U.4)
 * Read-only aggregate view of a registered housing family: بيانات الأسرة (+ building/flat
 * allocation), the guardian block and the full child set — GET /api/HousingProjects/projects/{id}.
 * The تعديل action routes to :id/edit (the §11.S.2 form in load mode, PUT on save); التقارير
 * الدورية routes to :id/reports (§11.S.3, UC-HOU-06).
 *
 * Closed-set values (النوع، العلاقة) are stored as Arabic literals; labels resolve through
 * the module constants + i18n. Roles: Charity + HQ (Admin/SuperAdmin) — the API itself
 * answers 404 for a foreign family (pin-never-widen), so the view only ever holds an
 * in-scope aggregate.
 */

import { Component, OnInit, OnDestroy, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { HousingProjectService } from '../services/housing-project.service';
import {
  HousingFamilyDetail,
  HousingChildDetail,
  HOUSING_CHILD_GENDERS,
  HOUSING_MAIN_RELATIONS
} from '../models/housing-project.model';
import { NotificationService } from '../../../core/services/notification.service';
import {
  BreadcrumbComponent,
  BreadcrumbItem,
  PageHeaderComponent
} from '../../../shared/components';

@Component({
  selector: 'app-housing-project-detail',
  standalone: true,
  imports: [
    CommonModule,
    TranslateModule,
    BreadcrumbComponent,
    PageHeaderComponent
  ],
  templateUrl: './housing-project-detail.component.html',
  styleUrls: ['./housing-project-detail.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HousingProjectDetailComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'housingProjects.title', url: '/housing-projects' },
    { label: 'housingProjects.familyDetails' }
  ];

  loading = true;
  family: HousingFamilyDetail | null = null;

  pageActions = [
    {
      label: 'common.edit',
      type: 'primary',
      icon: 'fe-edit-2',
      click: () => this.edit()
    },
    {
      label: 'housingProjects.reports.title',
      type: 'secondary',
      icon: 'fe-file-text',
      click: () => this.viewReports()
    },
    {
      label: 'common.back',
      type: 'secondary',
      icon: 'fe-arrow-left',
      click: () => this.router.navigate(['/housing-projects'])
    }
  ];

  constructor(
    private housingService: HousingProjectService,
    private notification: NotificationService,
    private translate: TranslateService,
    private router: Router,
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.router.navigate(['/housing-projects']);
      return;
    }
    this.loadFamily(id);
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadFamily(id: string): void {
    this.loading = true;
    this.housingService.getHousingFamily(id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: detail => {
          this.loading = false;
          this.family = detail;
          // The raw register code reads fine through the translate pipe (not a key)
          this.breadcrumbs[2].label = detail.code || 'housingProjects.familyDetails';
          // OnPush: without the dirty mark the spinner outlives the data that replaced it.
          this.cdr.markForCheck();
        },
        error: err => {
          this.loading = false;
          this.cdr.markForCheck();
          this.notification.error(
            err?.error?.message ||
            this.translate.instant('housingProjects.detail.messages.loadFailed'));
          this.router.navigate(['/housing-projects']);
        }
      });
  }

  /** تعديل — the §11.S.2 form in UC-HOU-04 edit mode (loads the aggregate, saves via PUT). */
  edit(): void {
    if (this.family) {
      this.router.navigate(['/housing-projects', this.family.id, 'edit']);
    }
  }

  /** التقارير الدورية — the §11.S.3 beneficiary reports screen (UC-HOU-06). */
  viewReports(): void {
    if (this.family) {
      this.router.navigate(['/housing-projects', this.family.id, 'reports']);
    }
  }

  // ==================== CLOSED-SET LABEL RESOLUTION ====================

  /** النوع — ذكر / انثى literal → i18n label. */
  childGenderLabel(gender?: string): string {
    if (!gender) {
      return '—';
    }
    const option = HOUSING_CHILD_GENDERS.find(item => item.value === gender);
    return option ? this.translate.instant(option.labelKey) : gender;
  }

  /** العلاقة — الاب / الام literal → i18n label (guardian block). */
  mainRelationLabel(mainRelation?: string): string {
    if (!mainRelation) {
      return '—';
    }
    const option = HOUSING_MAIN_RELATIONS.find(item => item.value === mainRelation);
    return option ? this.translate.instant(option.labelKey) : mainRelation;
  }

  /** YYYY-MM-DD slice of a server date (display-only — no timezone shift). */
  datePart(date?: string): string {
    return date ? date.slice(0, 10) : '—';
  }

  // ==================== TRACK BY ====================

  trackByChildId(index: number, child: HousingChildDetail): string {
    return child.id;
  }
}
