# IIROSA Frontend

Angular 18+ frontend for IIROSA Orphan Management System with TinyDash Dark RTL template.

## Structure

```
src/app/
├── core/                  # Core functionality
│   ├── services/         # Auth, HTTP, SignalR
│   ├── guards/           # Route guards
│   ├── interceptors/     # HTTP interceptors
│   └── models/           # Data models
│
├── shared/               # Shared components
│   ├── components/       # Reusable components
│   ├── pipes/            # Custom pipes
│   └── directives/       # Custom directives
│
├── layouts/              # Layout components
│   ├── main-layout/      # Main app layout
│   └── auth-layout/      # Auth layout
│
└── modules/              # Feature modules
    ├── auth/             # Authentication
    ├── dashboard/        # Dashboard
    ├── employees/        # Employee management
    └── charities/        # Charity management
```

## Key Features

- **TinyDash Dark RTL** - Professional admin template
- **Localization** - English & Arabic with RTL support
- **SignalR Integration** - Real-time notifications
- **Shared Components** - Reusable UI components
- **JWT Authentication** - Token-based auth
- **Permission Guards** - Role-based access control
- **HTTP Interceptors** - Auto token refresh, error handling

## Shared Components

### Attachment Component
```html
<app-attachment
  [maxFiles]="5"
  [maxFileSize]="10485760"
  [multiple]="true"
  (onFileSelect)="handleFiles($event)">
</app-attachment>
```

### Data List Component
```html
<app-data-list
  [data]="employees"
  [columns]="columns"
  [rowActions]="actions"
  (onAction)="handleAction($event)">
</app-data-list>
```

## Localization

### Adding new translations
Edit `src/assets/i18n/en.json` or `src/assets/i18n/ar.json`:

```json
{
  "myModule": {
    "myKey": "Translation text"
  }
}
```

Usage in templates:
```html
{{ 'myModule.myKey' | translate }}
```

## Running the Application

1. **Install dependencies:**
   ```bash
   npm install
   ```

2. **Start dev server:**
   ```bash
   ng serve
   ```

   App: `http://localhost:4200`

3. **Build for production:**
   ```bash
   ng build --configuration production
   ```

## Adding New Modules

1. Generate module:
   ```bash
   ng generate module modules/my-module
   ```

2. Generate components:
   ```bash
   ng generate component modules/my-feature/my-component
   ```

3. Add routing in `app-routing.module.ts`

4. Add menu item in `main-layout.component.html`

## Services

### AuthService
```typescript
constructor(private authService: AuthService) {}

login() {
  this.authService.login({ username, password }).subscribe();
}

logout() {
  this.authService.logout();
}
```

### HttpService
```typescript
constructor(private httpService: HttpService) {}

getData() {
  return this.httpService.get<MyData>('/endpoint');
}
```

### SignalRService
```typescript
constructor(private signalRService: SignalRService) {}

ngOnInit() {
  this.signalRService.startConnection();
  this.signalRService.notifications$.subscribe(notif => {
    // Handle notification
  });
}
```

## Styling

### Using TinyDash styles
```scss
.card {
  background-color: var(--card-bg);
  border: 1px solid var(--border-color);
}

.btn-primary {
  background-color: var(--primary);
}
```

### RTL Support
The app automatically switches to RTL when Arabic language is selected.

## Build Configuration

### Development
- Source maps enabled
- Optimizations disabled
- AOT disabled

### Production
- Source maps disabled
- Optimizations enabled
- AOT enabled
- Bundle size budgets enforced
