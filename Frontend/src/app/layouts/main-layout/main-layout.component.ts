import { Component, OnInit, Renderer2, AfterViewInit } from '@angular/core';
import { Router } from '@angular/router';
import { first } from 'rxjs/operators';
import { AuthService } from '../../core/services/auth.service';
import { SignalRService } from '../../core/services/signalr.service';
import { LocalStorageService } from '../../core/services/localstorage.service';
import { LanguageService } from '../../core/services/language.service';
import { DiagnosticService } from '../../core/services/diagnostic.service';
import { EndpointDiscoveryService } from '../../core/services/endpoint-discovery.service';
import { TitleService } from '../../core/services/title.service';
import { ImpersonationService } from '../../core/services/impersonation.service';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { QuickImpersonationDialogComponent } from '../../shared/impersonation-dialogs/quick-impersonation-dialog.component';
import { ImpersonationConfirmDialogComponent } from '../../shared/impersonation-dialogs/impersonation-confirm-dialog.component';
import { NotificationService, NotificationMessage } from '../../core/services/notification.service';

@Component({
  selector: 'app-main-layout',
  templateUrl: './main-layout.component.html',
  styleUrls: ['./main-layout.component.scss']
})
export class MainLayoutComponent implements OnInit, AfterViewInit {
  currentUser$ = this.authService.currentUser$;
  isSidebarCollapsed = false;
  currentLang = 'en';
  unreadCount = 0;
  currentYear = new Date().getFullYear();
  appVersion = '1.0.0';
  startHeader: boolean = false;
  fullName: string = '';
  employeeImg: string = '';
  currentCulture: string = 'en-US';

  // Impersonation
  canImpersonate = false;
  isSuperAdmin = false;

  // jquery, popper, moment, bootstrap, select2, daterangepicker and
  // jquery.timepicker are deliberately NOT in this list: they are already
  // loaded at boot via the "scripts" array in angular.json (bundled into
  // scripts.js). Re-executing them here replaced window.jQuery and bound a
  // SECOND set of Bootstrap data-api handlers, so every dropdown click fired
  // toggle twice — open then instantly closed — and row-action menus never
  // appeared. Each library must execute exactly once.
  scripts = [
    'assets/tinydash/js/simplebar.min.js',
    'assets/tinydash/js/jquery.stickOnScroll.js',
    'assets/tinydash/js/tinycolor-min.js',
    'assets/tinydash/js/config.js',
    'assets/tinydash/js/d3.min.js',
    'assets/tinydash/js/topojson.min.js',
    'assets/tinydash/js/datamaps.all.min.js',
    'assets/tinydash/js/datamaps-zoomto.js',
    'assets/tinydash/js/datamaps.custom.js',
    'assets/tinydash/js/Chart.min.js',
    'assets/tinydash/js/CustomJsCodes/CustomJsCodes1.js',
    'assets/tinydash/js/gauge.min.js',
    'assets/tinydash/js/jquery.sparkline.min.js',
    // ApexCharts loads as the npm package via the shared apx-chart wrapper.
    // This old v3 global copy collided with it (window.ApexCharts got
    // overwritten mid-session, breaking resize with "t.put is not a function").
    'assets/tinydash/js/jquery.mask.min.js',
    'assets/tinydash/js/jquery.steps.min.js',
    'assets/tinydash/js/jquery.validate.min.js',
    'assets/tinydash/js/dropzone.min.js',
    'assets/tinydash/js/uppy.min.js',
    'assets/tinydash/js/quill.min.js',
    'assets/tinydash/js/CustomJsCodes/CustomJsCodes2.js',
    'assets/tinydash/js/CustomJsCodes/CustomJsCodes3.js',
    'assets/tinydash/js/apps.js',
  ];

  constructor(
    private authService: AuthService,
    private signalRService: SignalRService,
    private router: Router,
    private localStorage: LocalStorageService,
    private languageService: LanguageService,
    private diagnosticService: DiagnosticService,
    private endpointDiscoveryService: EndpointDiscoveryService,
    private renderer: Renderer2,
    private titleService: TitleService,
    private impersonationService: ImpersonationService,
    private modalService: NgbModal,
    private notificationService: NotificationService
  ) {
    // Load theme and language preferences from localStorage
    this.loadPreferences();
    // Initialize title service for dynamic page titles
    this.titleService.initialize();
  }

  ngOnInit(): void {
    // Add keyboard shortcuts for diagnostics and endpoint discovery
    document.addEventListener('keydown', (event) => {
      if (event.ctrlKey && event.shiftKey && event.key === 'D') {
        event.preventDefault();
        this.runDiagnostics();
      }
      if (event.ctrlKey && event.shiftKey && event.key === 'A') {
        event.preventDefault();
        this.checkAuthentication();
      }
      if (event.ctrlKey && event.shiftKey && event.key === 'E') {
        event.preventDefault();
        this.discoverEndpoints();
      }
    });

    // Initialize current language from language service
    this.currentLang = this.languageService.getCurrentLanguage();
    this.currentCulture = this.currentLang === 'ar' ? 'ar-SA' : 'en-US';

    // Subscribe to language changes
    this.languageService.currentLang$.subscribe(lang => {
      this.currentLang = lang;
      this.currentCulture = lang === 'ar' ? 'ar-SA' : 'en-US';
    });

    // Get current user info
    const currentUser = this.authService.getCurrentUser();
    if (currentUser) {
      this.fullName = currentUser.fullName || '';
      this.employeeImg = currentUser.profileImage || '';

      // Set impersonation flags
      this.canImpersonate = this.authService.hasAnyRole(['SuperAdmin', 'Admin']);
      this.isSuperAdmin = this.authService.hasRole('SuperAdmin');
    }

    // Start SignalR connection (wrapped to handle gracefully if not available)
    try {
      this.signalRService.startConnection();
    } catch (error) {
      console.warn('SignalR not available (this is expected):', error);
    }

    // Listen for notifications
    this.signalRService.notifications$.subscribe(notification => {
      this.unreadCount++;
      // Show toast notification
      this.showNotification(notification);
    });
  }

  ngAfterViewInit(): void {
    this.startHeader = true;

    // Load TinyDash assets after DOM is ready (like reference project)
    setTimeout(() => {
      this.loadTinyDashAssets();
    }, 500);
  }

  /**
   * Load TinyDash CSS and JS files dynamically (like reference)
   */
  private loadTinyDashAssets(): void {
    // Apply RTL/LTR class to body
    if (this.currentCulture === 'ar-SA' || this.currentCulture === 'ar') {
      document.body.classList.add('rtl');
      this.loadCSS('assets/tinydash/css/simplebar.css');
      this.loadCSS('assets/tinydash/css/feather.css');
      this.loadCSS('assets/tinydash/css/select2.css');
      this.loadCSS('assets/tinydash/css/dropzone.css');
      this.loadCSS('assets/tinydash/css/uppy.min.css');
      this.loadCSS('assets/tinydash/css/jquery.steps.css');
      this.loadCSS('assets/tinydash/css/jquery.timepicker.css');
      this.loadCSS('assets/tinydash/css/quill.snow.css');
      this.loadCSS('assets/tinydash/css/daterangepicker.css');
    } else {
      document.body.classList.remove('rtl');
      this.loadCSS('assets/tinydash/css/simplebar.css');
      this.loadCSS('assets/tinydash/css/feather.css');
      this.loadCSS('assets/tinydash/css/select2.css');
      this.loadCSS('assets/tinydash/css/dropzone.css');
      this.loadCSS('assets/tinydash/css/uppy.min.css');
      this.loadCSS('assets/tinydash/css/jquery.steps.css');
      this.loadCSS('assets/tinydash/css/jquery.timepicker.css');
      this.loadCSS('assets/tinydash/css/quill.snow.css');
      this.loadCSS('assets/tinydash/css/daterangepicker.css');
    }

    // Load all scripts in sequence
    this.loadAllScripts();
  }

  /**
   * Load all scripts in sequence
   */
  private loadAllScripts(): void {
    let promiseChain: Promise<void> = Promise.resolve();

    this.scripts.forEach(script => {
      promiseChain = promiseChain.then(() => this.loadScript(script));
    });

    promiseChain
      .then(() => {
        console.log('All TinyDash scripts loaded successfully');
        // Initialize Bootstrap dropdowns after all scripts load
        this.initializeBootstrapDropdowns();
      })
      .catch(error => console.error('Script loading error:', error));
  }

  /**
   * Initialize Bootstrap dropdowns after TinyDash scripts load
   */
  private initializeBootstrapDropdowns(): void {
    setTimeout(() => {
      // Check if jQuery and Bootstrap are loaded
      const win = window as any;
      const jquery = win['$'];
      if (jquery && jquery.fn.dropdown) {
        console.log('Initializing Bootstrap dropdowns');
        // Initialize all dropdowns
        jquery('[data-toggle="dropdown"]').dropdown();
        console.log('Bootstrap dropdowns initialized');
      } else {
        console.warn('jQuery or Bootstrap not loaded, dropdowns may not work');
      }
    }, 100);
  }

  /**
   * Change language (exact reference implementation)
   */
  changeLanguage(lang: string): void {
    // Set language in localStorage (reference project approach)
    localStorage.setItem('lang', lang);

    // Update language service
    this.languageService.setLanguage(lang);

    // Reload page to apply changes (reference project approach)
    setTimeout(() => {
      location.href = location.href;
    }, 100);
  }

  /**
   * Toggle between Arabic and English
   */
  toggleLanguage(): void {
    this.languageService.toggleLanguage();
  }

  /**
   * Get current language display name
   */
  getLanguageDisplay(): string {
    return this.currentLang === 'ar' ? 'العربية' : 'English';
  }

  /**
   * Get theme icon
   */
  getThemeIcon(): string {
    return 'fe-sun'; // Reference project uses static icon
  }

  /**
   * Check if current language is RTL
   */
  isCurrentLangRTL(): boolean {
    return this.languageService.isRTL();
  }

  toggleSidebar(): void {
    this.isSidebarCollapsed = !this.isSidebarCollapsed;

    // Manipulate DOM for sidebar collapse/expand
    const wrapper = document.querySelector('.wrapper');
    const sidebar = document.getElementById('leftSidebar');

    if (wrapper && sidebar) {
      if (this.isSidebarCollapsed) {
        wrapper.classList.add('sidebar-collapsed');
        sidebar.classList.add('collapsed');
      } else {
        wrapper.classList.remove('sidebar-collapsed');
        sidebar.classList.remove('collapsed');
      }
    }

    // Try to use TinyDash sidebar toggle if available
    if ((window as any).collapseSidebar && typeof (window as any).collapseSidebar === 'function') {
      (window as any).collapseSidebar();
    }
  }

  openShortcutsModal(): void {
    // TODO: Implement shortcuts modal
  }

  openNotificationsModal(): void {
    // TODO: Implement notifications modal
  }

  showNotifications(): void {
    // Placeholder for notifications functionality
    console.log('Show notifications');
    this.openNotificationsModal();
  }

  logout(): void {
    this.signalRService.stopConnection();
    this.authService.logout().pipe(first()).subscribe({
      next: () => {
        this.router.navigate(['/auth/login']);
      },
      error: () => {
        // Navigate anyway even if logout fails
        this.router.navigate(['/auth/login']);
      }
    });
  }

  hasPermission(permission: string): boolean {
    // TODO: Implement permission check
    return true;
  }

  /**
   * Check if current user has any of the specified roles
   */
  hasAnyRole(roles: string[]): boolean {
    const currentUser = this.authService.getCurrentUser();
    if (!currentUser || !currentUser.roles) {
      return false;
    }

    return roles.some(role => currentUser.roles.includes(role));
  }

  isActive(route: string): boolean {
    return this.router.url === route || this.router.url.startsWith(route + '/');
  }

  isMenuExpanded(menuKey: string): boolean {
    // TODO: Implement menu state
    return false;
  }

  getColSize(field: any): number {
    // TODO: Implement based on field
    return 6;
  }

  showNotification(notification: any): void {
    // TODO: Show toast notification
    console.log('New notification:', notification);
  }

  runDiagnostics(): void {
    this.diagnosticService.runDiagnostics();
  }

  checkAuthentication(): void {
    this.diagnosticService.checkAuthentication();
  }

  discoverEndpoints(): void {
    this.endpointDiscoveryService.discoverEndpoints();
  }

  /**
   * Load user preferences from localStorage
   */
  private loadPreferences(): void {
    const savedTheme = localStorage.getItem('theme');
    // Language is now handled by LanguageService
  }

  /**
   * Toggle between dark and light theme
   * TinyDash handles this automatically via modeSwitcher click
   */
  toggleTheme(): void {
    // TinyDash config.js handles theme switching via modeSwitcher element
    console.log('Theme toggle clicked - TinyDash modeSwitch should handle this');
  }

  /**
   * Load a single script
   */
  private loadScript(src: string): Promise<void> {
    return new Promise((resolve, reject) => {
      const script = this.renderer.createElement('script');
      script.src = src;
      script.type = 'text/javascript';
      script.defer = true;

      script.onload = () => {
        console.log(`✅ Loaded: ${src}`);
        resolve();
      };

      script.onerror = () => {
        console.error(`❌ Failed to load: ${src}`);
        reject();
      };

      this.renderer.appendChild(document.body, script);
    });
  }

  /**
   * Load a CSS file
   */
  private loadCSS(href: string): void {
    const link = this.renderer.createElement('link');
    link.rel = 'stylesheet';
    link.href = href;
    this.renderer.appendChild(document.head, link);
  }

  /**
   * Open Quick Impersonation Dialog
   */
  openQuickImpersonation(): void {
    const modalRef = this.modalService.open(QuickImpersonationDialogComponent, {
      centered: true,
      backdrop: 'static',
      size: 'lg'
    });

    modalRef.result.then(
      (result: any) => {
        if (result && result.username) {
          this.confirmImpersonation(result.username, result.user);
        }
      },
      () => {
        // Modal dismissed
      }
    );
  }

  /**
   * Confirm impersonation with warning dialog
   */
  private confirmImpersonation(username: string, user: any): void {
    const confirmModalRef = this.modalService.open(ImpersonationConfirmDialogComponent, {
      centered: true,
      backdrop: 'static'
    });

    confirmModalRef.componentInstance.user = user;

    confirmModalRef.result.then(
      (confirmedUser: any) => {
        if (confirmedUser) {
          this.performImpersonation(username);
        }
      },
      () => {
        // Modal dismissed
      }
    );
  }

  /**
   * Perform the actual impersonation
   */
  private performImpersonation(username: string): void {
    this.impersonationService.startImpersonation({ targetUsername: username }).subscribe({
      next: (response) => {
        console.log('Impersonation started successfully');

        // Show success notification using NotificationMessage
        const successMessage: NotificationMessage = {
          type: 'success',
          title: 'Impersonation Started',
          message: 'You are now impersonating the user. Page will reload...',
          duration: 3000
        };
        this.notificationService.show(successMessage.message, successMessage.type, successMessage.title, successMessage.duration);

        // Update auth with new token
        if (response.impersonationToken) {
          localStorage.setItem('accessToken', response.impersonationToken);

          // Update current user data
          localStorage.setItem('currentUser', JSON.stringify(response.impersonatedUser));
          const currentUserSubject = (this.authService as any).currentUserSubject;
          if (currentUserSubject) {
            currentUserSubject.next(response.impersonatedUser);
          }
        }

        // Reload page to apply new user context
        setTimeout(() => {
          window.location.reload();
        }, 500);
      },
      error: (error) => {
        console.error('Failed to start impersonation:', error);
        // Show error message using NotificationService with NotificationMessage
        const errorMessage: NotificationMessage = {
          type: 'error',
          title: 'Impersonation Failed',
          message: error.error?.message || error.message || 'Failed to start impersonation. Please try again.'
        };
        this.notificationService.show(errorMessage.message, errorMessage.type, errorMessage.title);
      }
    });
  }
}
