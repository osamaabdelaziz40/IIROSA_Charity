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
  'Charities.Delete': ['SuperAdmin'],
  // FamiliesController authorises SuperAdmin,Admin,Charity on every action (including the
  // refugee child-routes); Create/Edit for HQ callers additionally require an explicit charity.
  'Families.View': ['SuperAdmin', 'Admin', 'Charity'],
  'Families.Create': ['SuperAdmin', 'Admin', 'Charity'],
  'Families.Edit': ['SuperAdmin', 'Admin', 'Charity'],
  // PUT /api/Families/{id}/charity is HQ-only on the server (SuperAdmin, Admin) — moving a
  // family between charities is an HQ decision, never the charity's own (UC-FAM-06).
  'Families.Transfer': ['SuperAdmin', 'Admin'],
  // POST /api/Families/{familyId}/members/{memberId}/control — the members screen and the
  // orphan/guardian move commands are HQ corrections too (UC-FAM-07/08, legacy role gate 0/3/4).
  'Families.Members': ['SuperAdmin', 'Admin'],
  // GET+POST /api/Families/provider-requests — the guardian-change queue. HQ reviews; a Charity
  // raises requests and sees its own rows (the server scopes Charity callers to their charity).
  'Families.ProviderRequests': ['SuperAdmin', 'Admin', 'Charity'],
  // GET /api/Families/follow-up — UC-FAM-11 register-activity report. HQ sees all (narrowable);
  // a Charity caller is scoped server-side to its own register's activity.
  'Families.FollowUp': ['SuperAdmin', 'Admin', 'Charity'],
  // POST /api/Reports/* — EP-18 report screens (18-1 onwards). Reports are caller-scoped
  // server-side: a Charity caller is pinned to its own register whatever the payload says.
  'Reports.View': ['SuperAdmin', 'Admin', 'Charity'],

  // Orphan coding (UC-ORP / epic 8): the coding screens belong to HQ. The shared endpoints are
  // deliberately wider server-side — GET /api/Families/orphans admits every role (a Charity
  // caller is pinned to its own register) and code assignment tenancy-checks rather than
  // role-gates — but both §13 screens are the HQ coding clerk's, so the route/menu hide them
  // from Charity users. The worklist mode (codingStatus=Pending) is additionally refused
  // server-side for Charity callers.
  'OrphanCoding.View': ['SuperAdmin', 'Admin'],
  'OrphanCoding.Edit': ['SuperAdmin', 'Admin'],

  // OrphanPaymentsController (epic 10, chapter 15): batch CRUD, enrolment and the bank-file
  // family are the HQ financial set per 00-Overview §4.3 (Accountant/FinancialOfficer = the
  // WAR Fin. Director / Financial Officer actors); bank files & CSV stay HQ-only. Charity is
  // admitted on the list/details reads (its rows are item-filtered to its own charity by the
  // service — 10-7) and on Disburse, the row-action set (stop/print/receipt/cheque —
  // UC-PAY-09..13, the charity's receipt workflow). The Charity-without-orphanId list guard
  // on GET /api/OrphanPayments stays a server-side 403 regardless of this map.
  'OrphanPayments.View': ['SuperAdmin', 'Admin', 'Accountant', 'FinancialOfficer', 'Charity'],
  'OrphanPayments.Create': ['SuperAdmin', 'Admin', 'Accountant', 'FinancialOfficer'],
  'OrphanPayments.Edit': ['SuperAdmin', 'Admin', 'Accountant', 'FinancialOfficer'],
  'OrphanPayments.Delete': ['SuperAdmin', 'Admin', 'Accountant', 'FinancialOfficer'],
  'OrphanPayments.AddOrphans': ['SuperAdmin', 'Admin', 'Accountant', 'FinancialOfficer'],
  'OrphanPayments.Disburse': ['SuperAdmin', 'Admin', 'Accountant', 'FinancialOfficer', 'Charity'],
  'OrphanPayments.BankFile': ['SuperAdmin', 'Admin', 'Accountant', 'FinancialOfficer'],
  'OrphanPayments.Import': ['SuperAdmin', 'Admin', 'Accountant', 'FinancialOfficer'],

  // Housing register (UC-HOU): the module's primary actor is the charity user; reads and
  // writes ride the families surface (GET /api/Families?familyType=Housing and the re-cut
  // HousingProjectsController), which authorise SuperAdmin,Admin,Charity server-side.
  'HousingProjects.View': ['SuperAdmin', 'Admin', 'Charity'],
  'HousingProjects.Create': ['SuperAdmin', 'Admin', 'Charity'],
  'HousingProjects.Edit': ['SuperAdmin', 'Admin', 'Charity'],
  'HousingProjects.Delete': ['SuperAdmin'],

  // PeriodicOrphanReportsController (epic 9, UC-ORR): reads are broad (reviewers included),
  // create/edit is the charity write path, delete is head-office only, review matches the
  // server-side [Authorize(Roles)] split on the review endpoint.
  'PeriodicReports.View': ['SuperAdmin', 'Admin', 'Accountant', 'Employee', 'Charity'],
  'PeriodicReports.Create': ['SuperAdmin', 'Admin', 'Charity'],
  'PeriodicReports.Edit': ['SuperAdmin', 'Admin', 'Charity'],
  'PeriodicReports.Delete': ['SuperAdmin', 'Admin'],
  'PeriodicReports.Review': ['SuperAdmin', 'Admin', 'Accountant', 'Employee'],
  // Review P24 2026-08-24: the compare route declares this permission but the map had
  // no entry, and an unmapped permission fails OPEN in hasPermission — every authenticated
  // role could route to the HQ-only compare screen. Matches OrphanReportsController's
  // [Authorize(Roles = "SuperAdmin,Admin")] on POST /compare.
  'OrphanReports.Compare': ['SuperAdmin', 'Admin'],

  // OfficeProjectManagementController: everything is Admin,SuperAdmin except delete, which is
  // SuperAdmin only (UC-OFP-05 — the General Director's alone; permission matrix F/F row).
  'OfficeDevelopmentProjects.View': ['SuperAdmin', 'Admin'],
  'OfficeDevelopmentProjects.Create': ['SuperAdmin', 'Admin'],
  'OfficeDevelopmentProjects.Edit': ['SuperAdmin', 'Admin'],
  'OfficeDevelopmentProjects.Delete': ['SuperAdmin'],

  // SeasonalAidController: campaign management, closure and reports are HQ only; the
  // family-facing surfaces (eligible families, beneficiary registration, distributions,
  // receipt confirmation) admit Charity too — the service scopes rows to the caller's charity.
  'SeasonalAid.View': ['SuperAdmin', 'Admin', 'Charity'],
  'SeasonalAid.Create': ['SuperAdmin', 'Admin'],
  'SeasonalAid.Edit': ['SuperAdmin', 'Admin'],
  'SeasonalAid.ManageBeneficiaries': ['SuperAdmin', 'Admin', 'Charity'],
  'SeasonalAid.RecordDistribution': ['SuperAdmin', 'Admin', 'Charity'],
  'SeasonalAid.Reports': ['SuperAdmin', 'Admin'],

  // CheckManagementController: the register, statement and detail reads admit the
  // financial read roles (FinancialOfficer = Financial Director); issue and edit are
  // the financial approver set. Charity holds no cheque screen, but the service still
  // scopes every row to the caller's charity token claim.
  'GeneralChecks.View': ['SuperAdmin', 'Admin', 'Accountant', 'FinancialOfficer'],
  'GeneralChecks.Create': ['SuperAdmin', 'Accountant', 'FinancialOfficer'],
  'GeneralChecks.Edit': ['SuperAdmin', 'Accountant', 'FinancialOfficer'],

  // MissionManagementController (epic 15): everything is Admin,SuperAdmin except delete,
  // which is SuperAdmin only (UC-MSN-08 — the General Director's alone).
  'Missions.View': ['SuperAdmin', 'Admin'],
  'Missions.Create': ['SuperAdmin', 'Admin'],
  'Missions.Edit': ['SuperAdmin', 'Admin'],
  'Missions.Delete': ['SuperAdmin'],

  // NotificationController (UC-NTF web notifications): the list/my read is every
  // authenticated role's; compose/push/edit/resend are the HQ push roles only,
  // mirroring the controller's [Authorize(Roles = "Admin,SuperAdmin")] set.
  'Notifications.View': ['SuperAdmin', 'Admin', 'Charity', 'Accountant', 'FinancialOfficer', 'Employee'],
  'Notifications.Create': ['SuperAdmin', 'Admin'],
  'Notifications.Edit': ['SuperAdmin', 'Admin'],

  // IncomingOutgoingController (epic 16): the whole correspondence module is HQ staff —
  // Admin,SuperAdmin on every action; delete is SuperAdmin only, mirroring the
  // controller's [Authorize] shapes (UC-COR-01…19). Reads scope to the caller's charity.
  'IncomingOutgoing.View': ['SuperAdmin', 'Admin'],
  'IncomingOutgoing.Create': ['SuperAdmin', 'Admin'],
  'IncomingOutgoing.Edit': ['SuperAdmin', 'Admin'],
  'IncomingOutgoing.Delete': ['SuperAdmin'],

  // HqTransfersController (epic 17): an HQ module — Fin. Director and Gen. Director map to
  // Admin/SuperAdmin; reads are country-claim scoped server-side. ManageLimits (UC-TRF-07) is
  // SuperAdmin-only — the consequential, few-should-do-it write (13-5 precedent).
  'HqTransfers.View': ['SuperAdmin', 'Admin'],
  'HqTransfers.Create': ['SuperAdmin', 'Admin'],
  'HqTransfers.Edit': ['SuperAdmin', 'Admin'],
  'HqTransfers.ManageLimits': ['SuperAdmin']
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

        // Only persist a real token. On a failed login the API returns an ApiResponse
        // without `token`, and localStorage.setItem(key, undefined) stores the literal
        // string "undefined" — the interceptor then attaches `Bearer undefined` to every
        // request (login included), which the backend rejects as a malformed JWT.
        if (response.token) {
          this.setAuthData(response.token, response.refreshToken, response.expiration, user);
        } else {
          this.clearAuthData();
        }

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
