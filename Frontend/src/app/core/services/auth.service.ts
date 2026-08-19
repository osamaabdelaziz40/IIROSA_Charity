import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, BehaviorSubject, tap, of, throwError, timer } from 'rxjs';
import { catchError, map, switchMap, first } from 'rxjs/operators';

export interface LoginRequest {
  email: string;
  password: string;
  rememberMe?: boolean;
}

export interface LoginResponse {
  token: string;
  refreshToken: string;
  expiration: string;
  user: {
    id: string;
    username: string;
    email: string;
    fullName: string;
    roles: string[];
  };
}

export interface RefreshTokenRequest {
  token: string;
  refreshToken: string;
}

export interface RefreshTokenResponse {
  token: string;
  refreshToken: string;
  expiration: string;
  expiresInMinutes: number;
}

export interface User {
  id: string;
  username: string;
  email: string;
  fullName: string;
  roles: string[];
  profileImage?: string;
}

export interface TokenInfo {
  isValid: boolean;
  expiresAt: Date;
  minutesRemaining: number;
  shouldShowWarning: boolean;
}

/**
 * Route permission → roles that satisfy it.
 *
 * These mirror the `[Authorize(Roles = ...)]` sets on `CharitiesController` exactly. If the two
 * drift, the client hides a screen the server would have allowed, or opens one it then refuses —
 * both look like bugs to the user, so they are kept in step deliberately.
 */
const PERMISSION_ROLES: Record<string, string[]> = {
  // GET /api/Charities and GET /api/Charities/{id} admit Charity too; the service scopes the
  // result to the caller's own record.
  'Charities.View': ['SuperAdmin', 'Admin', 'Charity'],
  'Charities.Create': ['SuperAdmin', 'Admin'],
  'Charities.Edit': ['SuperAdmin', 'Admin'],
  'Charities.Delete': ['SuperAdmin']
};

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'https://localhost:60960/api/auth';
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  // Session management
  private sessionTimeoutWarningSeconds: number = 300; // 5 minutes before expiration
  private sessionCheckInterval: any;
  private isRefreshing: boolean = false;
  private refreshPromise: any = null;

  constructor(private http: HttpClient) {
    // Load user from localStorage on init
    const storedUser = localStorage.getItem('currentUser');
    if (storedUser) {
      this.currentUserSubject.next(JSON.parse(storedUser));
    }

    // Start session monitoring
    this.startSessionMonitoring();
  }

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, request).pipe(
      tap(response => {
        console.log('=== LOGIN SUCCESS ===');
        console.log('Response:', response);
        console.log('Token received:', !!response.token);
        console.log('Token length:', response.token?.length || 0);
        console.log('User:', response.user);

        const user = response.user;
        this.setAuthData(response.token, response.refreshToken, response.expiration, user);

        // Verify token was stored
        const storedToken = localStorage.getItem('accessToken');
        console.log('Token stored in localStorage:', !!storedToken);
        console.log('Stored token matches:', storedToken === response.token);
      })
    );
  }

  logout(): Observable<void> {
    const refreshToken = this.getRefreshToken();

    // Call logout endpoint to revoke refresh tokens
    if (refreshToken) {
      return this.http.post<void>(`${this.apiUrl}/logout`, { refreshToken }).pipe(
        tap(() => console.log('Logout successful')),
        catchError((err) => {
          console.error('Logout error:', err);
          // Continue with logout even if API call fails
          return of<void>(undefined);
        }),
        tap(() => {
          this.clearAuthData();
          this.stopSessionMonitoring();
        })
      );
    }

    // No refresh token, just clear local data
    this.clearAuthData();
    this.stopSessionMonitoring();
    return of<void>(undefined);
  }

  /**
   * Refresh the access token using refresh token
   */
  refreshToken(): Observable<RefreshTokenResponse> {
    // Prevent multiple simultaneous refresh attempts
    if (this.isRefreshing) {
      return this.refreshPromise;
    }

    const currentToken = this.getAccessToken();
    const currentRefreshToken = this.getRefreshToken();

    if (!currentRefreshToken) {
      return throwError(() => new Error('No refresh token available'));
    }

    this.isRefreshing = true;

    const refreshObservable = this.http.post<RefreshTokenResponse>(`${this.apiUrl}/refresh-token`, {
      token: currentToken,
      refreshToken: currentRefreshToken
    }).pipe(
      tap(response => {
        // Update stored tokens
        const user = this.getCurrentUser();
        if (user && response.token) {
          this.setAuthData(response.token, response.refreshToken, response.expiration.toString(), user);
          console.log('Token refreshed successfully. Expires in:', response.expiresInMinutes, 'minutes');
        }
      }),
      catchError(error => {
        console.error('Failed to refresh token:', error);
        this.clearAuthData();
        return throwError(() => error);
      })
    );

    this.refreshPromise = refreshObservable;

    return refreshObservable;
  }

  /**
   * Validate current token and get expiration info
   */
  validateToken(): Observable<TokenInfo> {
    return this.http.post<any>(`${this.apiUrl}/validate-token`, {}).pipe(
      map(response => ({
        isValid: response.isValid || true,
        expiresAt: new Date(response.expiresAt),
        minutesRemaining: response.minutesRemaining || 0,
        shouldShowWarning: response.shouldShowWarning || false
      })),
      catchError(error => {
        console.error('Token validation failed:', error);
        return of({
          isValid: false,
          expiresAt: new Date(),
          minutesRemaining: 0,
          shouldShowWarning: true
        });
      })
    );
  }

  /**
   * Get token expiration info from JWT (without server call)
   */
  getTokenExpiration(): { expiresAt: Date; minutesRemaining: number } | null {
    const token = this.getAccessToken();
    if (!token) return null;

    try {
      const payload = this.parseJwt(token);
      const exp = payload.exp;
      if (!exp) return null;

      const expiresAt = new Date(exp * 1000);
      const minutesRemaining = Math.floor((expiresAt.getTime() - Date.now()) / 60000);

      return { expiresAt, minutesRemaining };
    } catch (error) {
      console.error('Failed to parse token:', error);
      return null;
    }
  }

  private parseJwt(token: string): any {
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split('')
        .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
    );
    return JSON.parse(jsonPayload);
  }

  /**
   * Start monitoring session for expiration warnings
   */
  private startSessionMonitoring(): void {
    // Check every minute
    this.sessionCheckInterval = setInterval(() => {
      const tokenInfo = this.getTokenExpiration();
      if (!tokenInfo) {
        this.stopSessionMonitoring();
        return;
      }

      const { minutesRemaining } = tokenInfo;

      // Emit warning event if session is about to expire
      if (minutesRemaining <= 5 && minutesRemaining > 0) {
        this.sessionTimeoutWarning.next(minutesRemaining);
      } else if (minutesRemaining <= 0) {
        // Session expired - clear auth data directly (no need to call API)
        this.clearAuthData();
        this.stopSessionMonitoring();
      }
    }, 60000); // Check every minute
  }

  private stopSessionMonitoring(): void {
    if (this.sessionCheckInterval) {
      clearInterval(this.sessionCheckInterval);
      this.sessionCheckInterval = null;
    }
  }

  // Session timeout warning subject
  private sessionTimeoutWarning = new BehaviorSubject<number>(0);
  public sessionTimeoutWarning$ = this.sessionTimeoutWarning.asObservable();

  private setAuthData(accessToken: string, refreshToken: string, expiration: string, user: any): void {
    localStorage.setItem('currentUser', JSON.stringify(user));
    localStorage.setItem('accessToken', accessToken);
    localStorage.setItem('refreshToken', refreshToken);
    localStorage.setItem('tokenExpiration', expiration);
    this.currentUserSubject.next(user);
  }

  private clearAuthData(): void {
    localStorage.removeItem('currentUser');
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('tokenExpiration');
    this.currentUserSubject.next(null);
  }

  getAccessToken(): string | null {
    return localStorage.getItem('accessToken');
  }

  getRefreshToken(): string | null {
    return localStorage.getItem('refreshToken');
  }

  isAuthenticated(): boolean {
    return !!this.getAccessToken() && !!this.currentUserSubject.value;
  }

  hasRole(role: string): boolean {
    const user = this.currentUserSubject.value;
    return user ? user.roles.includes(role) : false;
  }

  hasAnyRole(roles: string[]): boolean {
    const user = this.currentUserSubject.value;
    return user ? user.roles.some(r => roles.includes(r)) : false;
  }

  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  /**
   * Whether the signed-in user holds the named route permission (UC-CHR-04).
   *
   * Route-level authorisation only. It decides whether a screen may open; it is never the control
   * itself. Every endpoint authorises independently server-side, because anything decided in the
   * browser can be bypassed.
   *
   * Permissions not present in {@link PERMISSION_ROLES} keep the previous behaviour — allowed for
   * any authenticated user — and warn. Denying them instead would lock every other feature module
   * out of routes that already declare permissions no one has mapped yet, which is far beyond the
   * scope of the charity stories. Each module should add its own entry.
   */
  hasPermission(permission: string): boolean {
    if (!this.isAuthenticated()) {
      return false;
    }

    const allowedRoles = PERMISSION_ROLES[permission];

    if (!allowedRoles) {
      console.warn(
        `[AuthService] No role mapping for permission "${permission}"; allowing by default. ` +
        `Add it to PERMISSION_ROLES to enforce it at the route.`
      );
      return true;
    }

    return this.hasAnyRole(allowedRoles);
  }

  getAuthorizationHeader(): HttpHeaders {
    const token = this.getAccessToken();
    return new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    });
  }

  // Debug method to check authentication status
  debugAuthStatus(): void {
    console.log('=== AUTHENTICATION STATUS DEBUG ===');
    console.log('Is Authenticated:', this.isAuthenticated());
    console.log('Current User:', this.currentUserSubject.value);
    console.log('Has Access Token:', !!this.getAccessToken());
    console.log('Access Token Length:', this.getAccessToken()?.length || 0);
    console.log('Access Token Preview:', this.getAccessToken() ? `${this.getAccessToken()?.substring(0, 20)}...` : 'NO TOKEN');
    console.log('Has Refresh Token:', !!this.getRefreshToken());
    console.log('Token Expiration:', this.getTokenExpiration());
    console.log('LocalStorage accessToken:', !!localStorage.getItem('accessToken'));
    console.log('LocalStorage currentUser:', !!localStorage.getItem('currentUser'));
  }

  ngOnDestroy(): void {
    this.stopSessionMonitoring();
  }
}
