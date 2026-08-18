import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { SessionTimeoutService } from '../../../core/services/session-timeout.service';
import { SessionExtensionDialogComponent } from '../session-extension-dialog/session-extension-dialog.component';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-session-timeout-handler',
  standalone: true,
  imports: [CommonModule, RouterModule, SessionExtensionDialogComponent],
  template: `
    <app-session-extension-dialog
      *ngIf="showDialog"
      [visible]="showDialog"
      [data]="{ minutesRemaining: minutesRemaining, sessionTimeoutMinutes: 30 }"
      (close)="onDialogClose($event)"
      (sessionExtended)="onSessionExtended()">
    </app-session-extension-dialog>
  `
})
export class SessionTimeoutHandlerComponent implements OnInit, OnDestroy {
  showDialog: boolean = false;
  minutesRemaining: number = 5;

  private warningSubscription: Subscription | null = null;
  private expiredSubscription: Subscription | null = null;

  constructor(
    private sessionTimeoutService: SessionTimeoutService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Subscribe to warning dialog events
    this.warningSubscription = this.sessionTimeoutService.showWarningDialog$.subscribe(
      (minutes) => {
        if (minutes > 0) {
          this.showDialog = true;
          this.minutesRemaining = minutes;
        }
      }
    );

    // Subscribe to session expired events
    this.expiredSubscription = this.sessionTimeoutService.sessionExpired$.subscribe(
      (expired) => {
        if (expired) {
          this.showDialog = false;
          this.handleSessionExpired();
        }
      }
    );
  }

  onDialogClose(action: string): void {
    if (action === 'logout') {
      this.sessionTimeoutService.logoutFromDialog();
    } else {
      this.sessionTimeoutService.dialogClosed();
    }
    this.showDialog = false;
  }

  onSessionExtended(): void {
    this.showDialog = false;
    console.log('Session extended successfully');
  }

  private handleSessionExpired(): void {
    // Redirect to login page
    this.router.navigate(['/auth/login'], {
      queryParams: { sessionExpired: true }
    });
  }

  ngOnDestroy(): void {
    this.warningSubscription?.unsubscribe();
    this.expiredSubscription?.unsubscribe();
  }
}
