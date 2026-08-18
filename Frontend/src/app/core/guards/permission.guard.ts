import { Injectable } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class PermissionGuard implements CanActivate {
  constructor(private authService: AuthService, private router: Router) { }

  canActivate(route: ActivatedRouteSnapshot): Observable<boolean> | Promise<boolean> | boolean {
    // Check if user is authenticated
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/auth/login']);
      return false;
    }

    // Check if user has required permission
    const requiredPermission = route.data['permission'] as string;
    if (requiredPermission) {
      if (!this.authService.hasPermission(requiredPermission)) {
        this.router.navigate(['/dashboard'], { queryParams: { error: 'insufficient_permissions' } });
        return false;
      }
    }

    return true;
  }
}
