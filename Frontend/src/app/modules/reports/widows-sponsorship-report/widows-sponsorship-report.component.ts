import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { firstValueFrom } from 'rxjs';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { ReportViewerComponent } from '../report-viewer/report-viewer.component';
import { DropDownComponent } from '../../../shared/components/drop-down/drop-down.component';
import { ReportService } from '../services/report.service';
import { CharityService } from '../../charities/services/charity.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { WidowSponsorshipFilter, WidowSponsorshipRow } from '../models/report.model';
import { ReportExportRequest } from '../models/report-columns';

/**
 * UC-RPT-06 (§23.S.9 ارامل مطلوب لهم كفاله) — the 22-column widow grid.
 *
 * The widow is the family's MOTHER; residence/income columns come from the family (the
 * epic-7 refugee contract's column set). No widow-sponsorship flag exists — the predicate
 * is the data's floor (mother alive + husband death date present + orphans present),
 * resolved server-side. §23.S.9 itself lists no استخراج screen command; the export rides
 * 18-41's generic engine through the shell.
 */
@Component({
  selector: 'app-widows-sponsorship-report',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    ReportViewerComponent,
    DropDownComponent
  ],
  templateUrl: './widows-sponsorship-report.component.html',
  styleUrls: ['./widows-sponsorship-report.component.scss']
})
export class WidowsSponsorshipReportComponent implements OnInit, OnDestroy {
  rows: WidowSponsorshipRow[] = [];
  loading = false;
  hasRun = false;

  currentPage = 1;
  pageSize = 20;
  totalCount = 0;

  isHQ = false;
  charityOptions: Array<{ id: string; name: string }> = [];

  filterForm: FormGroup;

  private readonly destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private reportService: ReportService,
    private charityService: CharityService,
    private authService: AuthService,
    private notification: NotificationService,
    private translate: TranslateService
  ) {
    this.filterForm = this.fb.group({
      charityId: ['']
    });
  }

  ngOnInit(): void {
    this.isHQ = this.authService.hasAnyRole(['SuperAdmin', 'Admin']);
    if (this.isHQ) {
      this.loadCharities();
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /** الجمعية — HQ narrow; a charity caller is scoped server-side regardless. */
  private loadCharities(): void {
    this.charityService.getCharities({ pageNumber: 1, pageSize: 500 })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: response => {
          this.charityOptions = [
            { id: '', name: this.translate.instant('reports.widowsSponsorship.allCharities') },
            ...(response.items || []).map(c => ({ id: c.id, name: c.name }))
          ];
          // Review P26 2026-08-26: a capped lookup says so — a silently truncated dropdown hides choices.
          if ((response.totalCount || 0) > (response.items || []).length) {
            this.notification.warning(this.translate.instant('reports.lookup.truncated'));
          }
        },
        error: () => console.error('Error loading charities')
      });
  }

  // ==================== search ====================

  /** بحث — a read; nothing stored changes. Runs from page 1 with the current filter. */
  search(): void {
    if (this.loading) {
      return;
    }
    this.currentPage = 1;
    this.runSearch();
  }

  onPageChange(page: number): void {
    if (page < 1) {
      return;
    }
    this.currentPage = page;
    this.runSearch();
  }

  private runSearch(): void {
    this.loading = true;
    this.reportService.getWidowsAllowingSponsorship(this.buildFilter())
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => {
          this.rows = result.items || [];
          this.totalCount = result.totalCount || 0;
          this.loading = false;
          this.hasRun = true;
        },
        error: (httpError: any) => {
          this.loading = false;
          this.notification.error(
            httpError?.message || this.translate.instant('reports.widowsSponsorship.searchFailed')
          );
        }
      });
  }

  // ==================== helpers ====================

  private buildFilter(): WidowSponsorshipFilter {
    const v = this.filterForm.getRawValue();
    const filter: WidowSponsorshipFilter = {
      page: this.currentPage,
      pageSize: this.pageSize
    };
    if (v.charityId) {
      filter.charityId = String(v.charityId);
    }
    return filter;
  }

  /**
   * §23.U.41 استخراج البيانات — the 21-column widow workbook through the engine (18-41),
   * every page walked under the endpoint's 200 cap.
   */
  buildExportRequest = (): ReportExportRequest<WidowSponsorshipRow> | null  => {
    if (!this.hasRun || !this.totalCount) {
      return null;
    }
    const filter = this.buildFilter();
    return {
      reportKey: 'widows-sponsorship',
      pageSize: 200,
      fetchPage: (page, pageSize) => firstValueFrom(
        this.reportService.getWidowsAllowingSponsorship({ ...filter, page, pageSize })
      )
    };
  }

  /** 13-1 serial formula — continuous across pages. */
  serial(index: number): number {
    return (this.currentPage - 1) * this.pageSize + index + 1;
  }

  trackByRow(_index: number, row: WidowSponsorshipRow): string {
    return `${row.charityId ?? ''}-${row.nationalId ?? ''}-${row.widowName}`;
  }
}
