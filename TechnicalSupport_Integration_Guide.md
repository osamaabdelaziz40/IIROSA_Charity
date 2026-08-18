# Technical Support Module - Integration Guide

## 1. Add Module to App Routing

Update your main app routing module to include the technical support routes:

```typescript
// app-routing.module.ts
const routes: Routes = [
  // ... existing routes
  {
    path: 'technical-support',
    loadChildren: () => import('./modules/technical-support/technical-support.module')
      .then(m => m.TechnicalSupportModule)
  }
];
```

## 2. Add Sidebar Navigation Item

Add the technical support menu item to your sidebar component:

```html
<!-- sidebar.component.html -->
<li class="nav-item">
  <a class="nav-link" routerLink="/technical-support" routerLinkActive="active">
    <i class="fe fe-life-buoy fe-16"></i>
    <span class="ml-3 item-text">{{ 'technicalSupport.title' | translate }}</span>
  </a>
</li>
```

## 3. Update Environment Configuration

Ensure your `environment.ts` has the API URL configured:

```typescript
// environment.ts
export const environment = {
  production: false,
  apiUrl: 'https://your-api-url.com'
};
```

## 4. Import HttpClient and Translate Module

Ensure your `app.config.ts` or `app.module.ts` includes:

```typescript
import { provideHttpClient } from '@angular/common/http';
import { TranslateModule } from '@ngx-translate/core';

export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient(),
    // ... other providers
  ]
};
```

## 5. Setup Translation Loader

If not already configured, add the translation loader:

```typescript
// app.config.ts
import { TranslateLoader, TranslateModule } from '@ngx-translate/core';
import { TranslateHttpLoader } from '@ngx-translate/http-loader';
import { HttpClient } from '@angular/common/http';

export function HttpLoaderFactory(http: HttpClient): TranslateHttpLoader {
  return new TranslateHttpLoader(http, './assets/i18n/', '.json');
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient(),
    TranslateModule.forRoot({
      defaultLanguage: 'ar',
      loader: {
        provide: TranslateLoader,
        useFactory: HttpLoaderFactory,
        deps: [HttpClient]
      }
    }).providers!
  ]
};
```

## 6. Add API Interceptor (Optional)

If you need JWT authentication for API calls:

```typescript
// core/interceptors/auth.interceptor.ts
@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = localStorage.getItem('authToken');
    if (token) {
      req = req.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      });
    }
    return next.handle(req);
  }
}
```

## 7. Test the Integration

1. Start your Angular application: `ng serve`
2. Navigate to `/technical-support`
3. Test creating a new ticket
4. Test viewing ticket list (as user and admin)
5. Test ticket details and responses
6. Test admin actions (status update, mark as solved)

## 8. Backend API Requirements

Ensure your .NET backend exposes the following API endpoints:

### Tickets
- `GET /api/technicalsupport/mytickets` - Get current user's tickets
- `GET /api/technicalsupport/alltickets` - Get all tickets (admin only)
- `GET /api/technicalsupport/{id}` - Get ticket by ID
- `POST /api/technicalsupport` - Create new ticket
- `PUT /api/technicalsupport/{id}/status` - Update ticket status
- `PUT /api/technicalsupport/{id}/solve` - Mark ticket as solved
- `PUT /api/technicalsupport/{id}/close` - Close ticket
- `DELETE /api/technicalsupport/{id}` - Delete ticket

### Responses
- `GET /api/technicalsupport/{id}/responses` - Get ticket responses
- `POST /api/technicalsupport/{id}/responses` - Add response
- `DELETE /api/technicalsupport/{id}/responses/{responseId}` - Delete response

### Attachments
- `POST /api/technicalsupport/{id}/attachments` - Upload attachment
- `GET /api/technicalsupport/{id}/attachments/{fileName}` - Download attachment
- `DELETE /api/technicalsupport/{id}/attachments/{attachmentId}` - Delete attachment

### Search & Statistics
- `GET /api/technicalsupport/search` - Search tickets
- `GET /api/technicalsupport/statistics` - Get ticket statistics

### Reports
- `POST /api/technicalsupport/reports` - Generate report
- `POST /api/technicalsupport/reports/excel` - Export to Excel
- `POST /api/technicalsupport/reports/pdf` - Export to PDF

## 9. Permission System Integration

Update your permission guard to check for technical support permissions:

```typescript
// core/guards/permission.guard.ts
@Injectable()
export class PermissionGuard implements CanActivate {
  constructor(private store: Store) {}

  canActivate(route: ActivatedRouteSnapshot): boolean {
    const requiredPermission = route.data['permission'];

    if (!requiredPermission) return true;

    // Check user permissions from store or local storage
    const userRole = localStorage.getItem('userRole');

    switch (requiredPermission) {
      case 'technicalsupport.view':
        return true; // All logged-in users can view
      case 'technicalsupport.manage':
        return userRole === 'Admin' || userRole === 'SuperAdmin';
      default:
        return false;
    }
  }
}
```

## 10. Styling Integration

The components use CSS variables that should match your TinyDash theme:

```scss
// styles.scss
:root {
  --primary: #4d7cfe;
  --primary-rgb: 77, 124, 254;
  --success: #3cc09a;
  --info: #38adf9;
  --warning: #f5b921;
  --danger: #e55b6b;

  --card-bg: #1c1e21;
  --body-bg: #121417;
  --text-color: #e1e1e1;
  --text-muted: #6c7293;
  --border-color: #2c2f36;
}
```

## 11. SignalR Integration (Optional)

For real-time notifications when new tickets or responses are added:

```typescript
// core/services/signalr.service.ts
@Injectable()
export class SignalRService {
  private hubConnection: signalR.HubConnection;

  startConnection() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/technicalsupport')
      .build();

    this.hubConnection.start().catch(err => console.error(err));
  }

  addTicketListener(callback: (ticket: SupportTicket) => void) {
    this.hubConnection.on('NewTicket', callback);
  }

  addResponseListener(callback: (response: TicketResponse) => void) {
    this.hubConnection.on('NewResponse', callback);
  }
}
```

## 12. Testing Checklist

- [ ] Users can create support tickets
- [ ] Users can view their own tickets
- [ ] Admins can view all tickets
- [ ] Admins can update ticket status
- [ ] Admins can mark tickets as solved
- [ ] Admins can close tickets
- [ ] Users can add responses to their tickets
- [ ] Admins can add responses (internal and public)
- [ ] File attachments work correctly
- [ ] Search and filter functionality works
- [ ] Pagination works correctly
- [ ] RTL layout displays correctly in Arabic
- [ ] All translations are displayed
- [ ] Permission-based access control works

## 13. Performance Optimization

For better performance with large ticket lists:

```typescript
// Use OnPush change detection strategy
@Component({
  // ...
  changeDetection: ChangeDetectionStrategy.OnPush
})
```

## 14. Error Handling

Consider adding global error handling:

```typescript
// core/interceptors/error.interceptor.ts
@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(req).pipe(
      catchError(error => {
        if (error.status === 401) {
          // Handle unauthorized
        } else if (error.status === 403) {
          // Handle forbidden
        } else if (error.status === 500) {
          // Handle server error
        }
        return throwError(error);
      })
    );
  }
}
```

---

## Notes

1. All components are standalone and use modern Angular 18+ syntax
2. The module follows the approved Angular Frontend Architecture
3. Responsive design is implemented for mobile, tablet, and desktop
4. RTL (Right-to-Left) support is included for Arabic language
5. The module is fully integrated with TinyDash Dark RTL template styling
6. All components use Reactive Forms for better validation and control
7. The implementation follows Angular best practices and coding standards

## Next Steps

1. Review the component files and adjust styling to match your specific TinyDash theme
2. Test the module with your actual backend API
3. Add any additional features specific to your requirements
4. Implement unit tests for components and services
5. Add end-to-end tests for critical user flows

For questions or issues, refer to the Angular Frontend Architecture document or contact the development team.
