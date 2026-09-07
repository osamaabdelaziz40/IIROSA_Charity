import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpErrorResponse
} from '@angular/common/http';
import { Observable, catchError, throwError } from 'rxjs';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(private authService: AuthService, private router: Router) {}

  intercept(
    request: HttpRequest<unknown>,
    next: HttpHandler
  ): Observable<HttpEvent<unknown>> {
    // Add auth token to request
    const token = this.authService.getAccessToken();
    // Debug logging
    console.log('=== AUTH INTERCEPTOR DEBUG ===');
    console.log('Request URL:', request.url);
    console.log('Request Method:', request.method);
    console.log('Token exists:', !!token);
    console.log('Token length:', token?.length || 0);
    console.log('Token preview:', token ? `${token.substring(0, 20)}...` : 'NO TOKEN');

    if (token) {
      request = request.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      });
      console.log('✅ Authorization header added');
    } else {
      console.log('❌ NO TOKEN - Request will be sent without authorization');
    }

    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        console.error('=== AUTH INTERCEPTOR ERROR ===');
        console.error('Error Status:', error.status);
        console.error('Error URL:', error.url);
        console.error('Error Message:', error.message);

        if (error.status === 401) {
          // Token expired or invalid - redirect to login
          this.handle401Error(request);
        }

        // UC-SYS-13 — route to /error only what no screen owns. The middleware-signed 500s
        // (body carries a traceId) are by definition unhandled: no controller catch block
        // produced them and no screen-level handler knew the request would fail. Faults the
        // controllers caught themselves answer with their own { message } bodies WITHOUT a
        // traceId — their screens already surface those, so navigating would double-handle.
        // A network-level failure (status 0 — API unreachable) has no body to inspect at all.
        const body = error.error as { traceId?: string } | null;
        if (error.status === 0 || (error.status >= 500 && !!body?.traceId)) {
          this.router.navigate(['/error'], {
            state: { traceId: body?.traceId ?? null, status: error.status }
          });
        }

        // A 403 is deliberately NOT handled here. An earlier version navigated to the login
        // screen on every 403, which was wrong: the session is valid, so signing the user out
        // discards a working session and any half-completed form. Worse, it fires for background
        // and validation requests too — the charity form's name-availability check returns 403
        // for non-admin roles, which would have ejected the user mid-typing.
        //
        // "Role is not permitted" belongs at the route level (PermissionGuard), where it can be
        // decided before the screen opens. Individual callers surface their own 403s.
        return throwError(() => error);
      })
    );
  }

  private handle401Error(request: HttpRequest<unknown>) {
    // Any auth endpoint's own 401 must not re-enter this path: with an expired
    // access token POST /auth/logout itself 401s, which recursed into another
    // logout() (duplicate POSTs) and stacked nested returnUrl chains.
    if (request.url.includes('/auth/')) {
      return;
    }

    // Clear stored tokens and redirect to login
    this.authService.logout().subscribe({
      next: () => this.navigateToLogin(),
      error: () => this.navigateToLogin() // Navigate anyway even if logout fails
    });
  }

  private navigateToLogin(): void {
    // The login page must never become its own returnUrl — mid-redirect the
    // router already reports /auth/login?..., which is how
    // login?returnUrl=login?returnUrl=... chains formed.
    const current = this.router.url;
    const queryParams = current.startsWith('/auth/login')
      ? {}
      : { returnUrl: current };
    this.router.navigate(['/auth/login'], { queryParams });
  }
}
