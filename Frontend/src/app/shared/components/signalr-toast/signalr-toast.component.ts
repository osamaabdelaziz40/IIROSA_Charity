import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { SignalRService, NotificationMessage } from '../../../core/services/signalr.service';

@Component({
  selector: 'app-signalr-toast',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="signalr-toast-container position-fixed" [class.rtl]="isRTL" style="z-index: 9999;">
      <div
        *ngFor="let toast of toasts"
        class="signalr-toast show"
        [class]=" 'toast-' + toast.type "
        role="alert"
        aria-live="assertive"
        aria-atomic="true">
        <div class="toast-content">
          <div class="toast-icon">
            <i [class]="getIcon(toast.type)"></i>
          </div>
          <div class="toast-message">
            <strong class="toast-title">{{ toast.title }}</strong>
            <p class="toast-text">{{ toast.message }}</p>
          </div>
          <button type="button" class="toast-close" (click)="removeToast(toast)">
            <i class="fe fe-x"></i>
          </button>
        </div>
        <div class="toast-progress" [style.animationDuration]="(toast.duration || 5000) + 'ms'"></div>
      </div>
    </div>
  `,
  styles: [`
    :host {
      display: block;
    }

    .signalr-toast-container {
      top: 20px;
      right: 20px;
      max-width: 400px;
      width: 100%;
    }

    .signalr-toast-container.rtl {
      right: auto;
      left: 20px;
    }

    .signalr-toast {
      position: relative;
      background: #fff;
      border-radius: 8px;
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
      margin-bottom: 12px;
      overflow: hidden;
      transform: translateX(100%);
      animation: slideIn 0.3s ease forwards;
    }

    .rtl .signalr-toast {
      transform: translateX(-100%);
      animation: slideInRTL 0.3s ease forwards;
    }

    @keyframes slideIn {
      to { transform: translateX(0); }
    }

    @keyframes slideInRTL {
      to { transform: translateX(0); }
    }

    @keyframes slideOut {
      to { transform: translateX(120%); opacity: 0; }
    }

    @keyframes slideOutRTL {
      to { transform: translateX(-120%); opacity: 0; }
    }

    .signalr-toast.toast-removing {
      animation: slideOut 0.3s ease forwards;
    }

    .rtl .signalr-toast.toast-removing {
      animation: slideOutRTL 0.3s ease forwards;
    }

    .toast-content {
      display: flex;
      align-items: flex-start;
      padding: 16px;
      gap: 12px;
    }

    .toast-icon {
      flex-shrink: 0;
      width: 24px;
      height: 24px;
      display: flex;
      align-items: center;
      justify-content: center;
      border-radius: 50%;
      font-size: 14px;
    }

    .toast-success .toast-icon {
      background-color: #d4edda;
      color: #155724;
    }

    .toast-error .toast-icon {
      background-color: #f8d7da;
      color: #721c24;
    }

    .toast-warning .toast-icon {
      background-color: #fff3cd;
      color: #856404;
    }

    .toast-info .toast-icon {
      background-color: #d1ecf1;
      color: #0c5460;
    }

    .toast-message {
      flex: 1;
      min-width: 0;
    }

    .toast-title {
      display: block;
      font-size: 14px;
      font-weight: 600;
      margin-bottom: 4px;
      color: #212529;
    }

    .toast-text {
      display: block;
      font-size: 13px;
      margin: 0;
      color: #6c757d;
      line-height: 1.4;
      word-wrap: break-word;
    }

    .toast-close {
      flex-shrink: 0;
      width: 24px;
      height: 24px;
      padding: 0;
      border: none;
      background: none;
      cursor: pointer;
      color: #6c757d;
      transition: color 0.2s;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .toast-close:hover {
      color: #212529;
    }

    .toast-progress {
      position: absolute;
      bottom: 0;
      left: 0;
      height: 3px;
      background: linear-gradient(90deg, rgba(0,0,0,0.1), rgba(0,0,0,0.2));
      animation: progress linear forwards;
    }

    @keyframes progress {
      from { width: 100%; }
      to { width: 0%; }
    }

    /* Type-specific border accents */
    .toast-success {
      border-left: 4px solid #28a745;
    }

    .toast-error {
      border-left: 4px solid #dc3545;
    }

    .toast-warning {
      border-left: 4px solid #ffc107;
    }

    .toast-info {
      border-left: 4px solid #17a2b8;
    }

    .rtl .toast-success,
    .rtl .toast-error,
    .rtl .toast-warning,
    .rtl .toast-info {
      border-left: none;
      border-right: 4px solid;
    }
  `]
})
export class SignalrToastComponent implements OnInit, OnDestroy {
  toasts: (NotificationMessage & { duration?: number })[] = [];
  private subscription?: Subscription;
  isRTL = false;

  constructor(private signalRService: SignalRService) {
    this.checkRTL();
  }

  ngOnInit() {
    this.subscription = this.signalRService.notifications$.subscribe((message: NotificationMessage) => {
      if (message) {
        this.showToast(message);
      }
    });
  }

  ngOnDestroy() {
    this.subscription?.unsubscribe();
  }

  private checkRTL() {
    if (typeof document !== 'undefined') {
      this.isRTL = document.documentElement.getAttribute('dir') === 'rtl';
    }
  }

  private showToast(message: NotificationMessage) {
    const toast = { ...message, duration: 5000 };
    this.toasts.push(toast);

    setTimeout(() => this.removeToast(toast), toast.duration);
  }

  removeToast(toast: NotificationMessage & { duration?: number }) {
    const index = this.toasts.indexOf(toast);
    if (index > -1) {
      this.toasts.splice(index, 1);
    }
  }

  getIcon(type: string): string {
    const icons = {
      success: 'fe fe-check-circle',
      error: 'fe fe-x-circle',
      warning: 'fe fe-alert-triangle',
      info: 'fe fe-info'
    };
    return icons[type as keyof typeof icons] || icons.info;
  }
}
