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
    // Don't redirect if already on login page
    if (!request.url.includes('/auth/login')) {
      // Clear stored tokens and redirect to login
      this.authService.logout().subscribe({
        next: () => {
          this.router.navigate(['/auth/login'], {
            queryParams: { returnUrl: this.router.url }
          });
        },
        error: () => {
          // Navigate anyway even if logout fails
          this.router.navigate(['/auth/login'], {
            queryParams: { returnUrl: this.router.url }
          });
        }
      });
    }
  }
}
