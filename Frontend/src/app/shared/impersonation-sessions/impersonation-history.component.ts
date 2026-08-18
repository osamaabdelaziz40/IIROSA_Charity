import { Component, OnInit } from '@angular/core';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { ImpersonationService } from '../../core/services/impersonation.service';
import { ImpersonationSessionHistory, ImpersonationHistoryFilter } from '../../core/models/impersonation.model';
import { NotificationService, NotificationMessage } from '../../core/services/notification.service';

/**
 * Impersonation History Component
 * Displays historical impersonation sessions with filters
 * Super Admin only (UC-19.6)
 */
@Component({
  selector: 'app-impersonation-history',
  template: `
    <div class="card">
      <div class="card-header">
        <h5 class="mb-0">
          <i class="fas fa-history text-primary"></i>
          Impersonation History
        </h5>
      </div>
      <div class="card-body">
        <!-- Filters -->
        <div class="filter-section mb-4">
          <div class="row">
            <div class="col-md-3">
              <label class="form-label">Start Date</label>
              <input
                type="date"
                class="form-control"
                [(ngModel)]="filter.startDate"
                (change)="applyFilters()"
              />
            </div>
            <div class="col-md-3">
              <label class="form-label">End Date</label>
              <input
                type="date"
                class="form-control"
                [(ngModel)]="filter.endDate"
                (change)="applyFilters()"
              />
            </div>
            <div class="col-md-2">
              <label class="form-label">Status</label>
              <select class="form-control" [(ngModel)]="filter.isActive" (change)="applyFilters()">
                <option [ngValue]="null">All</option>
                <option [ngValue]="true">Active</option>
                <option [ngValue]="false">Ended</option>
              </select>
            </div>
            <div class="col-md-4 d-flex align-items-end">
              <button class="btn btn-outline-secondary" (click)="clearFilters()">
                <i class="fas fa-times"></i>
                Clear Filters
              </button>
              <button class="btn btn-primary ml-2" (click)="exportToExcel()">
                <i class="fas fa-file-excel"></i>
                Export
              </button>
            </div>
          </div>
        </div>

        <!-- Loading State -->
        <div *ngIf="isLoading" class="text-center py-5">
          <div class="spinner-border text-primary" role="status">
            <span class="sr-only">Loading...</span>
          </div>
          <p class="text-muted mt-2">Loading history...</p>
        </div>

        <!-- Empty State -->
        <div *ngIf="!isLoading && (!history || history.length === 0)" class="alert alert-info">
          <i class="fas fa-info-circle"></i>
          No impersonation history found for the selected filters.
        </div>

        <!-- History Table -->
        <div *ngIf="!isLoading && history && history.length > 0" class="table-responsive">
          <table class="table table-hover">
            <thead class="thead-light">
              <tr>
                <th>Impersonator</th>
                <th>Impersonated User</th>
                <th>Start Time</th>
                <th>End Time</th>
                <th>Duration</th>
                <th>Actions Count</th>
                <th>Status</th>
                <th>Termination Details</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let session of history">
                <td>
                  <div>
                    <strong>{{ session.impersonatorUserName }}</strong>
                  </div>
                  <small class="text-muted">{{ session.impersonatorUserId }}</small>
                </td>
                <td>
                  <div>
                    <strong>{{ session.impersonatedUserName }}</strong>
                  </div>
                  <small class="text-muted">{{ session.impersonatedUserId }}</small>
                </td>
                <td>
                  <div>{{ session.startTime | date:'medium' }}</div>
                  <small class="text-muted">{{ session.startTime | date:'short' }}</small>
                </td>
                <td>
                  <div *ngIf="session.endTime">{{ session.endTime | date:'medium' }}</div>
                  <div *ngIf="!session.endTime" class="text-muted">
                    <em>Still active</em>
                  </div>
                </td>
                <td>
                  <div *ngIf="session.durationSeconds">
                    {{ formatDuration(session.durationSeconds) }}
                  </div>
                  <div *ngIf="!session.durationSeconds" class="text-muted">
                    <em>N/A</em>
                  </div>
                </td>
                <td>
                  <span class="badge badge-info">{{ session.actionsPerformedCount }}</span>
                </td>
                <td>
                  <span *ngIf="session.isActive" class="badge badge-success">Active</span>
                  <span *ngIf="!session.isActive" class="badge badge-secondary">Ended</span>
                </td>
                <td>
                  <div *ngIf="session.terminatedByName">
                    <small class="text-muted">By: {{ session.terminatedByName }}</small>
                    <br />
                    <small class="text-muted text-truncate d-inline-block" style="max-width: 150px;">
                      {{ session.terminationReason }}
                    </small>
                  </div>
                  <div *ngIf="!session.terminatedByName && !session.isActive">
                    <em class="text-muted">User ended</em>
                  </div>
                  <div *ngIf="session.isActive">
                    <em class="text-muted">N/A</em>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- Summary -->
        <div *ngIf="!isLoading && history && history.length > 0" class="alert alert-secondary mt-3">
          <strong>Summary:</strong>
          <span class="ml-2">{{ history.length }} session(s) found</span>
          <span class="ml-3" *ngIf="activeCount > 0">
            <span class="badge badge-success">{{ activeCount }}</span> active
          </span>
          <span class="ml-3" *ngIf="endedCount > 0">
            <span class="badge badge-secondary">{{ endedCount }}</span> ended
          </span>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .filter-section {
      background-color: #f8f9fa;
      padding: 1rem;
      border-radius: 0.25rem;
    }
    .table th {
      font-weight: 600;
      font-size: 0.875rem;
      text-transform: uppercase;
      color: #495057;
    }
  `]
})
export class ImpersonationHistoryComponent implements OnInit {
  history$: Observable<ImpersonationSessionHistory[]>;
  history: ImpersonationSessionHistory[] = [];
  isLoading = true;

  filter: ImpersonationHistoryFilter = {};
  activeCount = 0;
  endedCount = 0;

  constructor(
    private impersonationService: ImpersonationService,
    private notificationService: NotificationService
  ) {
    this.history$ = this.impersonationService.getHistory(this.filter);
  }

  ngOnInit(): void {
    this.loadHistory();
  }

  loadHistory(): void {
    this.isLoading = true;
    this.history$ = this.impersonationService.getHistory(this.filter);

    this.history$.pipe(
      catchError(error => {
        console.error('Failed to load history:', error);
        return of([]);
      }),
      map(history => {
        this.isLoading = false;
        this.history = history;
        this.calculateCounts();
        return history;
      })
    ).subscribe();
  }

  applyFilters(): void {
    this.loadHistory();
  }

  clearFilters(): void {
    this.filter = {};
    this.loadHistory();
  }

  calculateCounts(): void {
    this.activeCount = this.history.filter(h => h.isActive).length;
    this.endedCount = this.history.filter(h => !h.isActive).length;
  }

  formatDuration(seconds: number): string {
    return this.impersonationService.formatDuration(seconds);
  }

  exportToExcel(): void {
    // TODO: Implement Excel export
    console.log('Export to Excel - filters:', this.filter);

    // Show info notification using NotificationMessage
    const infoMessage: NotificationMessage = {
      type: 'info',
      title: 'Export Feature',
      message: 'Excel export functionality will be implemented soon.'
    };
    this.notificationService.show(infoMessage.message, infoMessage.type, infoMessage.title);
  }
}
