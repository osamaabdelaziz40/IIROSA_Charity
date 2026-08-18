import { Component, OnInit, OnDestroy, Input, Output, EventEmitter } from '@angular/core';
import { AuthService } from '../../../core/services/auth.service';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { Observable, throwError, of } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';

export interface SessionTimeoutData {
  minutesRemaining: number;
  sessionTimeoutMinutes: number;
}

@Component({
  selector: 'app-session-extension-dialog',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  templateUrl: './session-extension-dialog.component.html',
  styleUrls: ['./session-extension-dialog.component.css']
})
export class SessionExtensionDialogComponent implements OnInit, OnDestroy {
  @Input() visible: boolean = true;
  @Input() data: SessionTimeoutData = { minutesRemaining: 5, sessionTimeoutMinutes: 30 };
  @Output() close = new EventEmitter<string>();
  @Output() sessionExtended = new EventEmitter<void>();

  countdown: number = 60; // 60 seconds countdown
  countdownTimer: any;
  isRefreshing: boolean = false;
  errorMessage: string = '';
  refreshFailed: boolean = false;

  constructor(
    private authService: AuthService
  ) { }

  ngOnInit(): void {
    // Set initial countdown based on minutes remaining
    this.countdown = Math.min(this.data.minutesRemaining * 60, 60);
    this.startCountdown();
  }

  ngOnDestroy(): void {
    this.stopCountdown();
  }

  /**
   * Start countdown timer
   */
  private startCountdown(): void {
    this.countdownTimer = setInterval(() => {
      this.countdown--;
      if (this.countdown <= 0) {
        this.stopCountdown();
        this.onLogout();
      }
    }, 1000);
  }

  /**
   * Stop countdown timer
   */
  private stopCountdown(): void {
    if (this.countdownTimer) {
      clearInterval(this.countdownTimer);
      this.countdownTimer = null;
    }
  }

  /**
   * Extend session - refresh token
   */
  onExtendSession(): void {
    this.stopCountdown();
    this.isRefreshing = true;
    this.errorMessage = '';
    this.refreshFailed = false;

    // Call refresh token endpoint
    this.authService.refreshToken().pipe(
      tap(() => {
        this.isRefreshing = false;
        this.close.emit('extend');
        this.sessionExtended.emit();
      }),
      catchError((error) => {
        console.error('Failed to refresh token:', error);
        this.isRefreshing = false;
        this.refreshFailed = true;
        this.errorMessage = 'sessionExtension.REFRESH_FAILED';
        // Restart countdown on failure
        this.startCountdown();
        return of(null);
      })
    ).subscribe();
  }

  /**
   * Logout and close dialog
   */
  onLogout(): void {
    this.stopCountdown();
    this.authService.logout().subscribe({
      error: () => {
        // Logout will handle redirect even on error
      }
    });
    this.close.emit('logout');
  }

  /**
   * Get countdown class based on remaining seconds
   */
  getCountdownClass(): string {
    if (this.countdown > 30) return 'bg-success';
    if (this.countdown > 10) return 'bg-warning';
    return 'bg-danger';
  }
}
