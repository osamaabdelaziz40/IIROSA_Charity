import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { NotificationService, NotificationMessage } from '../../../core/services/notification.service';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="toast-container position-fixed top-0 end-0 p-3" style="z-index: 1100">
      <div
        *ngFor="let toast of toasts"
        class="toast show"
        [class]="'bg-' + toast.type"
        role="alert"
        aria-live="assertive"
        aria-atomic="true">
        <div class="toast-header">
          <i [class]="getIcon(toast.type)" class="me-2"></i>
          <strong class="me-auto">{{ toast.title || toast.type | titlecase }}</strong>
          <button type="button" class="btn-close" (click)="removeToast(toast)"></button>
        </div>
        <div class="toast-body">
          {{ toast.message }}
        </div>
      </div>
    </div>
  `,
  styles: [`
    .toast-container {
      top: 1rem !important;
      right: 1rem !important;
    }
    .toast {
      min-width: 300px;
      margin-bottom: 0.5rem;
    }
    .toast-success { background-color: #d4edda; color: #155724; }
    .toast-error { background-color: #f8d7da; color: #721c24; }
    .toast-warning { background-color: #fff3cd; color: #856404; }
    .toast-info { background-color: #d1ecf1; color: #0c5460; }
  `]
})
export class ToastComponent implements OnInit, OnDestroy {
  toasts: NotificationMessage[] = [];
  private subscription?: Subscription;

  constructor(private notificationService: NotificationService) {}

  ngOnInit() {
    this.subscription = this.notificationService.notification$.subscribe((message: NotificationMessage | null) => {
      if (message) {
        this.toasts.push(message);
        setTimeout(() => this.removeToast(message), message.duration || 3000);
      }
    });
  }

  ngOnDestroy() {
    this.subscription?.unsubscribe();
  }

  removeToast(toast: NotificationMessage) {
    const index = this.toasts.indexOf(toast);
    if (index > -1) {
      this.toasts.splice(index, 1);
    }
  }

  getIcon(type: string): string {
    const icons = {
      success: 'fe-check-circle',
      error: 'fe-x-circle',
      warning: 'fe-alert-triangle',
      info: 'fe-info'
    };
    return icons[type as keyof typeof icons] || icons.info;
  }
}
