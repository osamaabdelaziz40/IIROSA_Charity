import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, of, throwError } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';
import { ApiService } from './api.service';
import {
  StartImpersonationRequest,
  StartImpersonationResponse,
  EndImpersonationRequest,
  EndImpersonationResponse,
  ActiveImpersonationSession,
  ImpersonationSessionHistory,
  ImpersonationHistoryFilter,
  UserSearchResult,
  TerminateSessionRequest,
  ImpersonationSettings,
  ImpersonationStatus,
  IncrementActionsRequest
} from '../../core/models/impersonation.model';
import { AuthService } from './auth.service';

/**
 * Service for User Impersonation functionality
 * Implements use cases UC-19.1 through UC-19.8
 */
@Injectable({
  providedIn: 'root'
})
export class ImpersonationService {
  private readonly endpoint = '/api/impersonation';

  // Impersonation state
  private isImpersonatingSubject = new BehaviorSubject<boolean>(false);
  public isImpersonating$ = this.isImpersonatingSubject.asObservable();

  private impersonationStatusSubject = new BehaviorSubject<ImpersonationStatus | null>(null);
  public impersonationStatus$ = this.impersonationStatusSubject.asObservable();

  // Store original user data before impersonation
  private originalUserData: any = null;

  constructor(
    private api: ApiService,
    private http: HttpClient,
    private authService: AuthService
  ) {
    // Check if user is currently impersonating on init
    this.checkImpersonationStatus();
  }

  /**
   * Check current impersonation status on service initialization
   */
  private checkImpersonationStatus(): void {
    const storedStatus = localStorage.getItem('impersonationStatus');
    if (storedStatus) {
      try {
        const status = JSON.parse(storedStatus);
        this.impersonationStatusSubject.next(status);
        this.isImpersonatingSubject.next(status.isImpersonating);
      } catch (e) {
        console.error('Failed to parse impersonation status from localStorage:', e);
        localStorage.removeItem('impersonationStatus');
      }
    }
  }

  /**
   * Start an impersonation session (UC-19.1, UC-19.5)
   */
  startImpersonation(request: StartImpersonationRequest): Observable<StartImpersonationResponse> {
    return this.http.post<StartImpersonationResponse>(`${this.endpoint}/start`, request).pipe(
      tap(response => {
        console.log('=== IMPERSONATION STARTED ===');
        console.log('Target user:', response.impersonatedUser.username);
        console.log('Session ID:', response.sessionId);
        console.log('Expires at:', response.expiresAt);

        // Store original user data
        const currentUser = this.authService.getCurrentUser();
        if (currentUser) {
          this.originalUserData = { ...currentUser };
        }

        // Store impersonation status
        const status: ImpersonationStatus = {
          isImpersonating: true,
          sessionId: response.sessionId,
          sessionStartTime: new Date().toISOString(),
          sessionExpiresAt: response.expiresAt,
          currentUser: response.impersonatedUser
        };
        localStorage.setItem('impersonationStatus', JSON.stringify(status));
        this.impersonationStatusSubject.next(status);
        this.isImpersonatingSubject.next(true);
      }),
      catchError(error => {
        console.error('Failed to start impersonation:', error);
        return throwError(() => error);
      })
    );
  }

  /**
   * End impersonation session and return to original account (UC-19.2)
   */
  endImpersonation(request?: EndImpersonationRequest): Observable<EndImpersonationResponse> {
    return this.http.post<EndImpersonationResponse>(`${this.endpoint}/end`, request || {}).pipe(
      tap(response => {
        console.log('=== IMPERSONATION ENDED ===');
        console.log('Session duration:', response.sessionDurationSeconds, 'seconds');
        console.log('Actions performed:', response.actionsPerformed);

        // Clear impersonation status
        this.clearImpersonationState();

        // Update auth service with original token if provided
        if (response.originalToken) {
          localStorage.setItem('accessToken', response.originalToken);
          // Restore original user data
          if (this.originalUserData) {
            localStorage.setItem('currentUser', JSON.stringify(this.originalUserData));
            const currentUserSubject = (this.authService as any).currentUserSubject;
            if (currentUserSubject) {
              currentUserSubject.next(this.originalUserData);
            }
          }
        }
      }),
      catchError(error => {
        console.error('Failed to end impersonation:', error);
        // Even if the call fails, clear local state
        this.clearImpersonationState();
        return throwError(() => error);
      })
    );
  }

  /**
   * Get all active impersonation sessions (UC-19.3)
   * Super Admin only
   */
  getActiveSessions(): Observable<ActiveImpersonationSession[]> {
    return this.api.get<ActiveImpersonationSession[]>(`${this.endpoint}/active`);
  }

  /**
   * Terminate an active session (admin override) (UC-19.4)
   * Super Admin only
   */
  terminateSession(sessionId: string, request?: TerminateSessionRequest): Observable<any> {
    return this.http.post(`${this.endpoint}/terminate/${sessionId}`, request || {}).pipe(
      tap(() => {
        console.log('Session terminated:', sessionId);
      }),
      catchError(error => {
        console.error('Failed to terminate session:', error);
        return throwError(() => error);
      })
    );
  }

  /**
   * Search for users by username/email (UC-19.5)
   */
  searchUsers(term: string): Observable<UserSearchResult[]> {
    if (!term || term.length < 3) {
      return of([]);
    }
    return this.api.get<UserSearchResult[]>(`${this.endpoint}/search`, { term });
  }

  /**
   * Get impersonation history (UC-19.6)
   * Super Admin only
   */
  getHistory(filter?: ImpersonationHistoryFilter): Observable<ImpersonationSessionHistory[]> {
    return this.http.post<ImpersonationSessionHistory[]>(`${this.endpoint}/history`, filter || {});
  }

  /**
   * Get current impersonation status
   */
  getStatus(): Observable<ImpersonationStatus> {
    return this.http.get<ImpersonationStatus>(`${this.endpoint}/status`);
  }

  /**
   * Get impersonation settings (UC-19.8)
   * Super Admin only
   */
  getSettings(): Observable<ImpersonationSettings> {
    return this.api.get<ImpersonationSettings>(`${this.endpoint}/settings`);
  }

  /**
   * Update impersonation settings (UC-19.8)
   * Super Admin only
   */
  updateSettings(settings: ImpersonationSettings): Observable<any> {
    return this.http.put(`${this.endpoint}/settings`, settings);
  }

  /**
   * Increment action counter for an active session (UC-19.7)
   */
  incrementActions(request: IncrementActionsRequest): Observable<any> {
    return this.http.post(`${this.endpoint}/increment-actions`, request);
  }

  /**
   * Validate if current user can impersonate target user
   */
  validateImpersonation(targetUserId: string): Observable<{ canImpersonate: boolean; reason?: string }> {
    return this.api.get(`${this.endpoint}/validate/${targetUserId}`);
  }

  /**
   * Get current impersonation status from local state
   */
  getCurrentStatus(): ImpersonationStatus | null {
    return this.impersonationStatusSubject.value;
  }

  /**
   * Check if currently impersonating
   */
  isCurrentlyImpersonating(): boolean {
    return this.isImpersonatingSubject.value;
  }

  /**
   * Get session ID of current impersonation
   */
  getCurrentSessionId(): string | undefined {
    return this.impersonationStatusSubject.value?.sessionId;
  }

  /**
   * Get original user data before impersonation
   */
  getOriginalUserData(): any {
    return this.originalUserData;
  }

  /**
   * Clear impersonation state (local)
   */
  private clearImpersonationState(): void {
    localStorage.removeItem('impersonationStatus');
    this.impersonationStatusSubject.next(null);
    this.isImpersonatingSubject.next(false);
    this.originalUserData = null;
  }

  /**
   * Force clear impersonation (for use when sessions expire)
   */
  forceClearImpersonation(): void {
    this.clearImpersonationState();
    // Also reload auth state
    const currentUser = this.authService.getCurrentUser();
    if (currentUser) {
      console.log('Impersonation force cleared. Current user:', currentUser);
    }
  }

  /**
   * Check if impersonation session is about to expire
   */
  checkSessionExpiry(): { isExpiring: boolean; minutesRemaining: number } {
    const status = this.impersonationStatusSubject.value;
    if (!status || !status.isImpersonating) {
      return { isExpiring: false, minutesRemaining: 0 };
    }

    const expiresAt = new Date(status.sessionExpiresAt);
    const now = new Date();
    const minutesRemaining = Math.floor((expiresAt.getTime() - now.getTime()) / 60000);

    // Show warning if less than 15 minutes remaining
    const warningThreshold = 15;
    const isExpiring = minutesRemaining <= warningThreshold && minutesRemaining > 0;

    return { isExpiring, minutesRemaining };
  }

  /**
   * Format session duration for display
   */
  formatDuration(seconds: number): string {
    const hours = Math.floor(seconds / 3600);
    const minutes = Math.floor((seconds % 3600) / 60);
    const secs = seconds % 60;

    if (hours > 0) {
      return `${hours}h ${minutes}m`;
    } else if (minutes > 0) {
      return `${minutes}m ${secs}s`;
    } else {
      return `${secs}s`;
    }
  }
}
