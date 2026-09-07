import { Injectable, OnDestroy } from '@angular/core';
import { BehaviorSubject, Observable, Subject, timer, Subscription } from 'rxjs';
import { filter, switchMap } from 'rxjs/operators';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class SessionTimeoutService implements OnDestroy {
  private checkInterval: number = 30000; // Check every 30 seconds
  private warningThreshold: number = 5; // Show warning 5 minutes before expiration
  private countdownDuration: number = 60; // 60 seconds countdown in dialog

  private _showWarningDialog$ = new BehaviorSubject<number>(0);
  // Subject, not BehaviorSubject: an expired event must fire once to whoever is
  // mounted (the handler lives in the authenticated layout), never replay to the
  // layout that mounts after a re-login — that replay bounced freshly logged-in
  // users straight back to /auth/login.
  private _sessionExpired$ = new Subject<boolean>();

  public showWarningDialog$ = this._showWarningDialog$.asObservable();
  public sessionExpired$ = this._sessionExpired$.asObservable();

  private timerSubscription: Subscription | null = null;
  private isDialogOpen: boolean = false;

  constructor(private authService: AuthService) {
    this.startMonitoring();
  }

  /**
   * Start monitoring session expiration
   */
  private startMonitoring(): void {
    // Use timer to check periodically
    this.timerSubscription = timer(0, this.checkInterval).subscribe(() => {
      this.checkSessionExpiration();
    });
  }

  /**
   * Check if session is about to expire
   */
  private checkSessionExpiration(): void {
    if (!this.authService.isAuthenticated()) {
      return;
    }

    const tokenInfo = this.authService.getTokenExpiration();
    if (!tokenInfo) {
      return;
    }

    const { minutesRemaining, expiresAt } = tokenInfo;

    // Session expired
    if (minutesRemaining <= 0) {
      this._sessionExpired$.next(true);
      return;
    }

    // Show warning dialog if within threshold and dialog not already open
    if (minutesRemaining <= this.warningThreshold && !this.isDialogOpen) {
      this._showWarningDialog$.next(minutesRemaining);
      this.isDialogOpen = true;
    }
  }

  /**
   * Called when user extends session via dialog
   */
  extendSession(): void {
    this.isDialogOpen = false;
    this._showWarningDialog$.next(0);

    // Refresh the token
    this.authService.refreshToken().subscribe({
      next: () => {
        console.log('Session extended successfully');
      },
      error: (error) => {
        console.error('Failed to extend session:', error);
        this._sessionExpired$.next(true);
      }
    });
  }

  /**
   * Called when user clicks logout from dialog
   */
  logoutFromDialog(): void {
    this.isDialogOpen = false;
    this._showWarningDialog$.next(0);
    this.authService.logout().subscribe({
      error: () => {
        // Logout will handle redirect even on error
      }
    });
  }

  /**
   * Called when dialog is closed without action
   */
  dialogClosed(): void {
    this.isDialogOpen = false;
    this._showWarningDialog$.next(0);
  }

  /**
   * Get remaining time in formatted string
   */
  getRemainingTimeFormatted(): string {
    const tokenInfo = this.authService.getTokenExpiration();
    if (!tokenInfo) {
      return '0:00';
    }

    const { minutesRemaining, expiresAt } = tokenInfo;
    const now = new Date();
    const diffMs = expiresAt.getTime() - now.getTime();
    const diffSecs = Math.floor(diffMs / 1000);

    const mins = Math.floor(diffSecs / 60);
    const secs = diffSecs % 60;

    return `${mins}:${secs.toString().padStart(2, '0')}`;
  }

  /**
   * Check if session is valid
   */
  isSessionValid(): boolean {
    const tokenInfo = this.authService.getTokenExpiration();
    return tokenInfo !== null && tokenInfo.minutesRemaining > 0;
  }

  /**
   * Stop monitoring (called on logout)
   */
  stopMonitoring(): void {
    if (this.timerSubscription) {
      this.timerSubscription.unsubscribe();
      this.timerSubscription = null;
    }
  }

  ngOnDestroy(): void {
    this.stopMonitoring();
  }
}
