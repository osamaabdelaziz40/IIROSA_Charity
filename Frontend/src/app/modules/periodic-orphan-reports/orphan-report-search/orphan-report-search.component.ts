/**
 * Orphan Report Search Component
 * Implements UC-ORR-09 (§14.S.4): filter reports by orphan status حالة اليتيم.
 *
 * The 10-field status search over the 9-1 filter read (GET /api/PeriodicOrphanReports):
 * - الجمعية (HQ only) · And/Or radio · نوع التعليم · الحالة الصحية · الحالة
 *   الاجتماعية · الحالة التعليمية · المرحلة الدراسية · اخر تقدير
 * - Wide 38-column orphan-centric grid (horizontal scroll), ExcelJS extract.
 *
 * Access: HQ roles + Charity, scoped server-side by the caller's claims.
 */

import { Component, OnInit, OnDestroy, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { PeriodicOrphanReportService } from '../services/periodic-orphan-report.service';
import { PeriodicOrphanReportListDto, PeriodicOrphanReportFilterDto } from '../models/periodic-orphan-report.model';
import { LoadingComponent } from '../../../shared/components/loading/loading.component';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { NotificationService } from '../../../core/services/notification.service';
import { AuthService } from '../../../core/services/auth.service';
import { CharityService } from '../../charities/services/charity.service';
import { CharityDto } from '../../charities/models/charity.model';
import { LookupManagementService } from '../../lookup-management/services/lookup-management.service';
import { LookupDto } from '../../lookup-management/models/lookup.model';

/** One §14.S.4 grid column — value accessors are shared by the grid and the ExcelJS extract. */
interface StatusGridColumn {
  key: string;
  value: (r: PeriodicOrphanReportListDto) => string;
}

@Component({
  selector: 'app-orphan-report-search',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    PageHeaderComponent,
    CommonModule,
    FormsModule,
    RouterLink,
    TranslateModule,
    LoadingComponent,
    PaginationComponent,
    EmptyStateComponent
  ],
  templateUrl: './orphan-report-search.component.html',
  styleUrls: ['./orphan-report-search.component.scss']
})
export class OrphanReportSearchComponent implements OnInit, OnDestroy {
  /** Review P34 2026-08-24: the export's recursive page fetch must stop when the
   *  view goes away — otherwise it kept reading pages and writing state into a
   *  destroyed component. */
  private destroyed = false;
  loading = false;
  exporting = false;
  reports: PeriodicOrphanReportListDto[] = [];
  totalCount = 0;
  currentPage = 1;
  pageSize = 20;

  // §14.S.4 filter controls — '' means الكل / not applied
  isHeadOffice = false;
  charities: CharityDto[] = [];
  charityId = '';
  andOr: 'and' | 'or' = 'and';
  schoolType = '';
  medicalStatus = '';
  maritalStatus = '';
  educationalStatus = '';
  educationDegree = '';
  educationalLevelId = '';
  educationLevels: LookupDto[] = [];

  // Prefilled orphan shortcut (orphan-reports/orphan/:orphanId) — rides the OrphanId filter
  private orphanId?: string;

  /** §14.S.4 grid — spec column order. Gap columns (no source field on this stack) render '—'. */
  readonly columns: StatusGridColumn[] = [
    { key: 'orphanCode', value: r => r.orphanCode ?? '' },
    { key: 'orphanName', value: r => r.orphanName ?? '' },
    { key: 'orphanAge', value: r => this.ageOf(r) },
    { key: 'guardianName', value: r => r.guardianName ?? '' },
    { key: 'guardianRelation', value: r => r.guardianRelation ?? '' },
    { key: 'region', value: r => r.regionName ?? '' },
    { key: 'center', value: r => r.centerName ?? '' },
    { key: 'cityVillage', value: r => r.cityVillage ?? '' },
    { key: 'address', value: r => r.address ?? '' },
    { key: 'homePhone', value: r => r.homePhone ?? '' },
    { key: 'mobile', value: r => r.orphanPhone ?? '' },
    { key: 'birthDate', value: r => this.fmtDate(r.orphanDateOfBirth) },
    { key: 'nationalId', value: r => r.orphanNationalId ?? '' },
    { key: 'gender', value: r => r.orphanGender ?? '' },
    { key: 'guardianEducation', value: r => r.guardianEducationLevelName ?? '' },
    { key: 'guardianJob', value: r => r.guardianJob ?? '' },
    { key: 'guardianNationalId', value: r => r.guardianNationalId ?? '' },
    // مشروع تنموى للمعيل · الاستبعاد · سبب الاستبعاد — no source field on this stack (gap recorded)
    { key: 'devProject', value: () => '' },
    { key: 'exclusion', value: () => '' },
    { key: 'exclusionReason', value: () => '' },
    { key: 'charityName', value: r => r.charityName ?? '' },
    { key: 'lastUpdate', value: r => this.fmtDate(r.updatedOn ?? r.createdOn) },
    { key: 'schoolType', value: r => r.schoolType ?? '' },
    { key: 'faculty', value: r => r.faculty ?? '' },
    { key: 'school', value: r => r.school ?? '' },
    { key: 'deathDate', value: r => this.fmtDate(r.deathDate) },
    { key: 'marriageDate', value: r => this.fmtDate(r.marriageDate) },
    { key: 'reportDate', value: r => this.fmtDate(r.reportDate) },
    { key: 'medicalStatus', value: r => r.medicalStatus ?? '' },
    { key: 'disability', value: r => r.disability ?? '' },
    { key: 'disease', value: r => r.disease ?? '' },
    { key: 'educationalLevel', value: r => r.educationalLevelName ?? '' },
    { key: 'lastGrade', value: r => r.grade ?? '' },
    { key: 'specialization', value: r => r.specialization ?? '' },
    { key: 'degree', value: r => r.educationDegree ?? '' },
    { key: 'status', value: r => this.translate.instant('periodicReports.status.' + (r.reviewStatus || 'pending').toLowerCase()) },
    { key: 'familyCode', value: r => r.familyCode ?? '' }
  ];

  constructor(
    private periodicReportService: PeriodicOrphanReportService,
    private lookupService: LookupManagementService,
    private charityService: CharityService,
    public auth: AuthService,
    private notification: NotificationService,
    private translate: TranslateService,
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.isHeadOffice = this.auth.hasRole('SuperAdmin') || this.auth.hasRole('Admin');
    if (this.isHeadOffice) {
      this.charityService.getCharities({ pageNumber: 1, pageSize: 500 }).subscribe({
        next: result => (this.charities = result.items ?? []),
        error: () => (this.charities = [])
      });
    }
    this.lookupService.getEducationLevels().subscribe({
      next: levels => (this.educationLevels = levels),
      error: () => (this.educationLevels = [])
    });

    // orphan/:orphanId shortcut — same screen, prefilled subject
    const orphanId = this.route.snapshot.params['orphanId'];
    if (orphanId) {
      this.orphanId = orphanId;
    }

    this.loadReports();
  }

  /** بحث — always reloads page 1 (§14.U.9). */
  search(): void {
    this.currentPage = 1;
    this.loadReports();
  }

  private buildFilter(): PeriodicOrphanReportFilterDto {
    return {
      pageNumber: this.currentPage,
      pageSize: this.pageSize,
      charityId: this.isHeadOffice && this.charityId ? this.charityId : undefined,
      orphanId: this.orphanId,
      andOr: this.andOr,
      schoolType: this.schoolType || undefined,
      medicalStatus: this.medicalStatus || undefined,
      maritalStatus: this.maritalStatus || undefined,
      educationalStatus: this.educationalStatus || undefined,
      educationDegree: this.educationDegree || undefined,
      educationalLevelId: this.educationalLevelId ? Number(this.educationalLevelId) : undefined
    };
  }

  private loadReports(): void {
    this.loading = true;
    this.cdr.markForCheck();

    this.periodicReportService.getReports(this.buildFilter()).subscribe({
      next: result => {
        this.reports = result.items ?? [];
        this.totalCount = result.totalCount ?? 0;
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: error => {
        console.error('Error loading orphan status search:', error);
        this.loading = false;
        this.notification.error(error?.message || this.translate.instant('periodicReports.search.loadFailed'));
        this.cdr.markForCheck();
      }
    });
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadReports();
  }

  ngOnDestroy(): void {
    this.destroyed = true;
  }

  // ========== استخراج البيانات — client-side ExcelJS (16-1 precedent) ==========

  /** Exports the current filtered result (paged reads of 100, capped at 2000 rows). */
  exportData(): void {
    if (this.exporting || this.totalCount === 0) {
      if (this.totalCount === 0) {
        this.notification.info(this.translate.instant('periodicReports.search.nothingToExport'));
      }
      return;
    }

    this.exporting = true;
    this.cdr.markForCheck();

    const collected: PeriodicOrphanReportListDto[] = [];
    const maxRows = Math.min(this.totalCount, 2000);
    const fetchPage = (page: number): void => {
      const filter = { ...this.buildFilter(), pageNumber: page, pageSize: 100 };
      this.periodicReportService.getReports(filter).subscribe({
        next: result => {
          // Review P34: stop the recursion the moment the view is gone.
          if (this.destroyed) return;
          collected.push(...(result.items ?? []));
          if (collected.length >= maxRows || (result.items ?? []).length === 0) {
            this.finishExport(collected);
          } else {
            fetchPage(page + 1);
          }
        },
        error: () => {
          if (this.destroyed) return;
          this.exporting = false;
          this.notification.error(this.translate.instant('periodicReports.search.exportFailed'));
          this.cdr.markForCheck();
        }
      });
    };
    fetchPage(1);
  }

  private finishExport(rows: PeriodicOrphanReportListDto[]): void {
    // Review P34: nothing to build once the view is gone.
    if (this.destroyed) return;
    import('exceljs').then(({ default: ExcelJS }) => {
      const workbook = new ExcelJS.Workbook();
      const sheet = workbook.addWorksheet(this.translate.instant('periodicReports.search.title'));
      sheet.addRow(this.columns.map(c => this.translate.instant(`periodicReports.search.columns.${c.key}`)));
      rows.forEach(r => sheet.addRow(this.columns.map(c => c.value(r) || '—')));

      workbook.xlsx.writeBuffer().then(buffer => {
        const blob = new Blob([buffer], {
          type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
        });
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `orphan_status_${new Date().toISOString().slice(0, 10)}.xlsx`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);

        this.exporting = false;
        this.cdr.markForCheck();
      }).catch(() => {
        this.exporting = false;
        this.notification.error(this.translate.instant('periodicReports.search.exportFailed'));
        this.cdr.markForCheck();
      });
    }).catch(() => {
      this.exporting = false;
      this.notification.error(this.translate.instant('periodicReports.search.exportFailed'));
      this.cdr.markForCheck();
    });
  }

  // ========== helpers ==========

  /** عمر اليتيم — years from the orphan's date of birth (blank when unknown). */
  private ageOf(r: PeriodicOrphanReportListDto): string {
    if (!r.orphanDateOfBirth) return '';
    const dob = new Date(r.orphanDateOfBirth);
    if (isNaN(dob.getTime())) return '';
    const now = new Date();
    let age = now.getFullYear() - dob.getFullYear();
    const beforeBirthday = now.getMonth() < dob.getMonth()
      || (now.getMonth() === dob.getMonth() && now.getDate() < dob.getDate());
    if (beforeBirthday) age--;
    return age >= 0 ? String(age) : '';
  }

  private fmtDate(value?: string | Date): string {
    if (!value) return '';
    const d = new Date(value);
    if (isNaN(d.getTime())) return '';
    // Review P47a 2026-08-24: toISOString is UTC — a Riyadh evening entry shifted
    // back a day in the grid and the Excel extract. Format local date parts.
    const pad = (n: number) => String(n).padStart(2, '0');
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`;
  }

  trackByColumnKey(_index: number, col: StatusGridColumn): string {
    return col.key;
  }

  trackByReportId(_index: number, report: PeriodicOrphanReportListDto): string {
    return report.id;
  }

  trackByCharityId(_index: number, charity: CharityDto): string {
    return charity.id;
  }

  trackByLevelId(_index: number, level: LookupDto): number {
    return level.id;
  }
}
