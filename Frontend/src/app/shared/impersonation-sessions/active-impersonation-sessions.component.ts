import { Component, OnInit } from '@angular/core';
import { Observable, of, timer, Subscription } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { ImpersonationService } from '../../core/services/impersonation.service';
import { ActiveImpersonationSession } from '../../core/models/impersonation.model';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { TerminateSessionConfirmComponent } from './terminate-session-confirm.component';
import { NotificationService, NotificationMessage } from '../../core/services/notification.service';

/**
 * Active Impersonation Sessions Component
 * Displays all currently active impersonation sessions in the system
 * Super Admin only (UC-19.3)
 */
@Component({
  selector: 'app-active-impersonation-sessions',
  template: `
    <div class="card">
      <div class="card-header d-flex justify-content-between align-items-center">
        <h5 class="mb-0">
          <i class="fas fa-users-cog text-primary"></i>
          Active Impersonation Sessions
        </h5>
        <button class="btn btn-sm btn-outline-primary" (click)="refreshSessions()">
          <i class="fas fa-sync-alt" [class.spinning]="isRefreshing"></i>
          Refresh
        </button>
      </div>
      <div class="card-body">
        <!-- Loading State -->
        <div *ngIf="isLoading" class="text-center py-5">
          <div class="spinner-border text-primary" role="status">
            <span class="sr-only">Loading...</span>
          </div>
          <p class="text-muted mt-2">Loading active sessions...</p>
        </div>

        <!-- Empty State -->
        <div *ngIf="!isLoading && (!sessions || sessions.length === 0)" class="alert alert-info">
          <i class="fas fa-info-circle"></i>
          No active impersonation sessions at this time.
        </div>

        <!-- Sessions Table -->
        <div *ngIf="!isLoading && sessions && sessions.length > 0" class="table-responsive">
          <table class="table table-hover">
            <thead class="thead-light">
              <tr>
                <th>Impersonator</th>
                <th>Impersonated User</th>
                <th>Started At</th>
                <th>Duration</th>
                <th>IP Address</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let session of sessions" [class.bg-warning-light]="isSessionExpiringSoon(session)">
                <td>
                  <div>
                    <strong>{{ session.impersonatorUserName }}</strong>
                  </div>
                  <small class="text-muted">{{ session.impersonatorEmail }}</small>
                  <div>
                    <span class="badge badge-secondary badge-sm">{{ session.impersonatorRole || 'Admin' }}</span>
                  </div>
                </td>
                <td>
                  <div>
                    <strong>{{ session.impersonatedUserName }}</strong>
                  </div>
                  <small class="text-muted">{{ session.impersonatedEmail }}</small>
                  <div>
                    <span class="badge badge-info badge-sm">{{ session.impersonatedRole || 'User' }}</span>
                  </div>
                </td>
                <td>
                  <div>{{ session.startTime | date:'medium' }}</div>
                  <small class="text-muted">{{ session.startTime | date:'short' }}</small>
                </td>
                <td>
                  <div>
                    <i class="far fa-clock text-muted"></i>
                    {{ formatDuration(session.durationMinutes * 60) }}
                  </div>
                  <small *ngIf="isSessionExpiringSoon(session)" class="text-danger">
                    <i class="fas fa-exclamation-triangle"></i>
                    Expiring soon
                  </small>
                </td>
                <td>
                  <code class="small">{{ session.originalUserIpAddress || 'Unknown' }}</code>
                </td>
                <td>
                  <button
                    class="btn btn-sm btn-outline-danger"
                    (click)="terminateSession(session)"
                    title="Terminate Session"
                  >
                    <i class="fas fa-stop-circle"></i>
                    Terminate
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- Summary -->
        <div *ngIf="!isLoading && sessions && sessions.length > 0" class="alert alert-secondary mt-3">
          <strong>Summary:</strong>
          <span class="ml-2">{{ sessions.length }} active session(s)</span>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .badge-sm {
      font-size: 0.7rem;
      padding: 0.2rem 0.4rem;
    }
    .bg-warning-light {
      background-color: rgba(255, 193, 7, 0.1) !important;
    }
    .spinning {
      animation: spin 1s linear infinite;
    }
    @keyframes spin {
      from { transform: rotate(0deg); }
      to { transform: rotate(360deg); }
    }
  `]
})
export class ActiveImpersonationSessionsComponent implements OnInit {
  sessions$: Observable<ActiveImpersonationSession[]>;
  sessions: ActiveImpersonationSession[] = [];
  isLoading = true;
  isRefreshing = false;

  private autoRefreshSubscription: Subscription | null = null;

  constructor(
    private impersonationService: ImpersonationService,
    private modalService: NgbModal,
    private notificationService: NotificationService
  ) {
    this.sessions$ = this.impersonationService.getActiveSessions();
  }

  ngOnInit(): void {
    this.loadSessions();
    // Auto-refresh every 30 seconds
    this.startAutoRefresh();
  }

  ngOnDestroy(): void {
    this.autoRefreshSubscription?.unsubscribe();
  }

  private startAutoRefresh(): void {
    this.autoRefreshSubscription = timer(0, 30000).subscribe(() => {
      if (!this.isRefreshing) {
        this.loadSessions();
      }
    });
  }

  loadSessions(): void {
    this.sessions$.pipe(
      catchError(error => {
        console.error('Failed to load active sessions:', error);
        return of([]);
      }),
      map(sessions => {
        this.isLoading = false;
        this.isRefreshing = false;
        return sessions;
      })
    ).subscribe(sessions => {
      this.sessions = sessions;
    });
  }

  refreshSessions(): void {
    this.isRefreshing = true;
    this.loadSessions();
  }

  terminateSession(session: ActiveImpersonationSession): void {
    const modalRef = this.modalService.open(TerminateSessionConfirmComponent, {
      centered: true,
      backdrop: 'static'
    });

    modalRef.componentInstance.session = session;

    modalRef.result.then(
      (reason: any) => {
        if (reason) {
          this.impersonationService.terminateSession(session.sessionId, { reason }).subscribe({
            next: () => {
              console.log('Session terminated successfully');

              // Show success notification using NotificationMessage
              const successMessage: NotificationMessage = {
                type: 'success',
                title: 'Session Terminated',
                message: `Impersonation session for ${session.impersonatedUserName} has been terminated.`,
                duration: 3000
              };
              this.notificationService.show(successMessage.message, successMessage.type, successMessage.title, successMessage.duration);

              this.refreshSessions();
            },
            error: (error) => {
              console.error('Failed to terminate session:', error);

              // Show error notification using NotificationMessage
              const errorMessage: NotificationMessage = {
                type: 'error',
                title: 'Termination Failed',
                message: error.error?.message || error.message || 'Failed to terminate session.'
              };
              this.notificationService.show(errorMessage.message, errorMessage.type, errorMessage.title);
            }
          });
        }
      },
      () => {
        // Modal dismissed
      }
    );
  }

  formatDuration(minutes: number): string {
    return this.impersonationService.formatDuration(minutes * 60);
  }

  isSessionExpiringSoon(session: ActiveImpersonationSession): boolean {
    // Assume 8-hour session duration, warn if less than 15 minutes remaining
    const startTime = new Date(session.startTime);
    const sessionExpiry = new Date(startTime.getTime() + 8 * 60 * 60 * 1000); // 8 hours
    const now = new Date();
    const minutesRemaining = Math.floor((sessionExpiry.getTime() - now.getTime()) / 60000);
    return minutesRemaining <= 15 && minutesRemaining > 0;
  }
}
