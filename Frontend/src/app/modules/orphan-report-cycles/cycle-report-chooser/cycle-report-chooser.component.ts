import { ChangeDetectionStrategy, ChangeDetectorRef, Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { OrphanReportCycleService } from '../services/orphan-report-cycle.service';
import { OrphanCycleReportStatusDto } from '../models/orphan-report-cycle.model';
import { NotificationService } from '../../../core/services/notification.service';

/** The normalized orphan handle both host screens can supply from their row models. */
export interface ChooserOrphanContext {
  orphanId: string;
  code?: string;
  fullName?: string;
}

/**
 * UC-ORC-06 cycle chooser — the «إضافة تقرير دوري» action's brain, hosted once and
 * imported by both orphan screens (search results, family members).
 *
 * openFor() fetches the orphan's live In Progress cycles with their submission state
 * (one call; the read refreshes due status transitions server-side first, so a delayed
 * background tick can never serve a stale window). Then:
 *  - no running cycle  → warning toast (the hosting button is hidden in that state, so
 *    this is only the race where the cycle ended between render and click);
 *  - one cycle         → straight through: create deep-link, or the existing report's
 *    edit route when already submitted (BR-ORC-05 — edit while the cycle runs);
 *  - several cycles    → the modal: one row per cycle, «رفع تقرير» or «تعديل التقرير»
 *    by that cycle's own submission state (the 2-open-cycles requirement).
 *
 * The button is UX; every window rule is re-derived server-side at write time (BR-ORC-11).
 */
@Component({
  selector: 'app-cycle-report-chooser',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, TranslateModule],
  templateUrl: './cycle-report-chooser.component.html',
  styleUrls: ['./cycle-report-chooser.component.scss']
})
export class CycleReportChooserComponent {
  /** Modal rows — set when more than one running cycle needs an explicit choice. */
  rows: OrphanCycleReportStatusDto[] = [];
  show = false;
  loading = false;

  /** Context of the open() call the modal is deciding for. */
  private pending?: ChooserOrphanContext;

  constructor(
    private cycleService: OrphanReportCycleService,
    private router: Router,
    private notification: NotificationService,
    private translate: TranslateService,
    private cdr: ChangeDetectorRef
  ) {}

  /**
   * Entry point from a host screen's row action. Resolves the decision above; only the
   * multi-cycle branch leaves the modal open.
   */
  openFor(orphan: ChooserOrphanContext): void {
    if (this.loading) {
      return;
    }
    this.loading = true;
    this.cdr.markForCheck();

    this.cycleService.getOrphanReportStatus(orphan.orphanId).subscribe({
      next: cycles => {
        this.loading = false;

        if (cycles.length === 0) {
          this.notification.warning(this.translate.instant('orphanReportCycles.chooser.noActiveCycles'));
          this.cdr.markForCheck();
          return;
        }

        // Single running cycle — no choice to make.
        if (cycles.length === 1) {
          this.go(orphan, cycles[0]);
          this.cdr.markForCheck();
          return;
        }

        this.pending = orphan;
        this.rows = cycles;
        this.show = true;
        this.cdr.markForCheck();
      },
      error: () => {
        this.loading = false;
        this.notification.error(this.translate.instant('orphanReportCycles.chooser.loadFailed'));
        this.cdr.markForCheck();
      }
    });
  }

  /** Modal row action — same target as the single-cycle shortcut. */
  choose(row: OrphanCycleReportStatusDto): void {
    if (this.pending) {
      this.go(this.pending, row);
    }
    this.close();
  }

  close(): void {
    this.show = false;
    this.rows = [];
    this.pending = undefined;
    this.cdr.markForCheck();
  }

  /** Create deep-link (the form pre-resolves the ?code= subject) or the report's edit route. */
  private go(orphan: ChooserOrphanContext, cycle: OrphanCycleReportStatusDto): void {
    if (cycle.submitted && cycle.reportId) {
      this.router.navigate(['/periodic-orphan-reports', cycle.reportId, 'edit']);
      return;
    }
    this.router.navigate(['/periodic-orphan-reports/create'], {
      queryParams: { code: orphan.code, cycleId: cycle.cycleId }
    });
  }

  /** Review-state chip translation ('Accepted' | 'Refused' | 'Pending' from the server). */
  reviewStateKey(state?: string): string {
    switch (state) {
      case 'Accepted': return 'orphanReportCycles.review.accepted';
      case 'Refused': return 'orphanReportCycles.review.refused';
      default: return 'orphanReportCycles.review.pending';
    }
  }

  reviewStateClass(state?: string): string {
    switch (state) {
      case 'Accepted': return 'badge-success';
      case 'Refused': return 'badge-danger';
      default: return 'badge-warning';
    }
  }

  trackByCycleId = (_: number, row: OrphanCycleReportStatusDto): string => row.cycleId;
}
