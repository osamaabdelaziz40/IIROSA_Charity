import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, Subscription, timer } from 'rxjs';
import { ImpersonationService } from '../../core/services/impersonation.service';
import { ImpersonationStatus } from '../../core/models/impersonation.model';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { EndImpersonationConfirmComponent } from '../impersonation-dialogs/end-impersonation-confirm.component';
import { NotificationService, NotificationMessage } from '../../core/services/notification.service';

/**
 * Impersonation Banner Component
 * Displays prominent banner when user is in impersonation mode
 * Shows original user info, current impersonated user, session duration, and expiry warning
 */
@Component({
  selector: 'app-impersonation-banner',
  templateUrl: './impersonation-banner.component.html',
  styleUrls: ['./impersonation-banner.component.scss']
})
export class ImpersonationBannerComponent implements OnInit, OnDestroy {
  impersonationStatus$: Observable<ImpersonationStatus | null>;
  currentStatus: ImpersonationStatus | null = null;

  // Time tracking
  sessionDuration = 0;
  minutesRemaining = 0;
  isExpiringSoon = false;

  private statusSubscription: Subscription | null = null;
  private timerSubscription: Subscription | null = null;

  constructor(
    private impersonationService: ImpersonationService,
    private router: Router,
    private modalService: NgbModal,
    private notificationService: NotificationService
  ) {
    this.impersonationStatus$ = this.impersonationService.impersonationStatus$;
  }

  ngOnInit(): void {
    // Subscribe to impersonation status changes
    this.statusSubscription = this.impersonationStatus$.subscribe(status => {
      this.currentStatus = status;
      if (status && status.isImpersonating) {
        this.startTimer();
      } else {
        this.stopTimer();
      }
    });
  }

  ngOnDestroy(): void {
    this.statusSubscription?.unsubscribe();
    this.stopTimer();
  }

  /**
   * Start timer for updating session duration and expiry warning
   */
  private startTimer(): void {
    this.stopTimer(); // Clear any existing timer

    // Update every second
    this.timerSubscription = timer(0, 1000).subscribe(() => {
      this.updateSessionInfo();
    });
  }

  /**
   * Stop the timer
   */
  private stopTimer(): void {
    if (this.timerSubscription) {
      this.timerSubscription.unsubscribe();
      this.timerSubscription = null;
    }
  }

  /**
   * Update session duration and check expiry
   */
  private updateSessionInfo(): void {
    if (!this.currentStatus || !this.currentStatus.isImpersonating) {
      return;
    }

    const startTime = new Date(this.currentStatus.sessionStartTime);
    const now = new Date();
    this.sessionDuration = Math.floor((now.getTime() - startTime.getTime()) / 1000);

    const expiryInfo = this.impersonationService.checkSessionExpiry();
    this.minutesRemaining = expiryInfo.minutesRemaining;
    this.isExpiringSoon = expiryInfo.isExpiring;

    // Auto-end session if expired
    if (this.minutesRemaining <= 0) {
      this.handleSessionExpired();
    }
  }

  /**
   * Handle session expiration
   */
  private handleSessionExpired(): void {
    console.warn('Impersonation session expired');
    this.impersonationService.forceClearImpersonation();
    this.stopTimer();

    // Redirect to login with message
    this.router.navigate(['/auth/login'], {
      queryParams: { sessionExpired: true, message: 'impersonation_session_expired' }
    });
  }

  /**
   * Open confirmation dialog before ending impersonation
   */
  endImpersonation(): void {
    const modalRef = this.modalService.open(EndImpersonationConfirmComponent, {
      centered: true,
      backdrop: 'static'
    });

    modalRef.result.then(
      (confirmed: any) => {
        if (confirmed) {
          this.performEndImpersonation();
        }
      },
      () => {
        // Modal dismissed
      }
    );
  }

  /**
   * Perform the actual end impersonation action
   */
  private performEndImpersonation(): void {
    const sessionId = this.impersonationService.getCurrentSessionId();

    this.impersonationService.endImpersonation({ sessionId }).subscribe({
      next: (response) => {
        console.log('Impersonation ended successfully');

        // Show success notification using NotificationMessage
        const successMessage: NotificationMessage = {
          type: 'success',
          title: 'Impersonation Ended',
          message: 'You have returned to your original account.',
          duration: 3000
        };
        this.notificationService.show(successMessage.message, successMessage.type, successMessage.title, successMessage.duration);

        // Reload page to reset all state
        setTimeout(() => {
          window.location.reload();
        }, 500);
      },
      error: (error) => {
        console.error('Failed to end impersonation:', error);

        // Show error notification using NotificationMessage
        const errorMessage: NotificationMessage = {
          type: 'error',
          title: 'Error Ending Impersonation',
          message: error.error?.message || error.message || 'Failed to end impersonation session.'
        };
        this.notificationService.show(errorMessage.message, errorMessage.type, errorMessage.title);

        // Even on error, force clear state
        this.impersonationService.forceClearImpersonation();
        setTimeout(() => {
          window.location.reload();
        }, 500);
      }
    });
  }

  /**
   * Get formatted duration string
   */
  getFormattedDuration(): string {
    return this.impersonationService.formatDuration(this.sessionDuration);
  }

  /**
   * Get warning message based on time remaining
   */
  getWarningMessage(): string | null {
    if (!this.isExpiringSoon) {
      return null;
    }

    if (this.minutesRemaining <= 5) {
      return `⚠️ Session expires in ${this.minutesRemaining} minute(s)!`;
    } else if (this.minutesRemaining <= 15) {
      return `Session expires in ${this.minutesRemaining} minute(s)`;
    }

    return null;
  }

  /**
   * Get CSS class for banner based on expiry status
   */
  getBannerClass(): string {
    if (this.isExpiringSoon && this.minutesRemaining <= 5) {
      return 'impersonation-banner critical';
    } else if (this.isExpiringSoon) {
      return 'impersonation-banner warning';
    }
    return 'impersonation-banner';
  }

  /**
   * Get the first role of current user or 'User' as default
   */
  getFirstRole(): string {
    const roles = this.currentStatus?.currentUser?.roles;
    if (roles && roles.length > 0) {
      return roles[0];
    }
    return 'User';
  }
}
