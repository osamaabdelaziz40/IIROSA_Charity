import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import Swal from 'sweetalert2';
import { LanguageService } from './language.service';

export interface NotificationMessage {
  type: 'success' | 'error' | 'warning' | 'info';
  message: string;
  title?: string;
  duration?: number;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private notificationSubject = new BehaviorSubject<NotificationMessage | null>(null);
  public notification$: Observable<NotificationMessage | null> = this.notificationSubject.asObservable();

  constructor(private languageService: LanguageService) {}

  show(message: string, type: 'success' | 'error' | 'warning' | 'info' = 'info', title?: string, duration = 3000) {
    // Also emit to observable for toast component
    this.notificationSubject.next({ type, message, title, duration });

    const Toast = Swal.mixin({
      toast: true,
      position: this.languageService.isRTL() ? 'top-start' : 'top-end',
      showConfirmButton: false,
      timer: duration,
      timerProgressBar: true,
      didOpen: (toast) => {
        toast.addEventListener('mouseenter', Swal.stopTimer);
        toast.addEventListener('mouseleave', Swal.resumeTimer);
      },
      customClass: {
        popup: `notification-toast notification-${type}`
      }
    });

    const iconMap: Record<string, 'success' | 'error' | 'warning' | 'info'> = {
      success: 'success',
      error: 'error',
      warning: 'warning',
      info: 'info'
    };

    Toast.fire({
      icon: iconMap[type],
      title: title ? `${title}: ${message}` : message
    });
  }

  success(message: string, title?: string) {
    this.show(message, 'success', title || 'Success');
  }

  error(message: string, title?: string) {
    this.show(message, 'error', title || 'Error', 5000);
  }

  warning(message: string, title?: string) {
    this.show(message, 'warning', title || 'Warning');
  }

  info(message: string, title?: string) {
    this.show(message, 'info', title || 'Information');
  }

  // Modal methods for full-screen alerts
  showModal(message: string, type: 'success' | 'error' | 'warning' | 'info' = 'info', title?: string) {
    const iconMap: Record<string, 'success' | 'error' | 'warning' | 'info'> = {
      success: 'success',
      error: 'error',
      warning: 'warning',
      info: 'info'
    };

    return Swal.fire({
      icon: iconMap[type],
      title: title || type.charAt(0).toUpperCase() + type.slice(1),
      text: message,
      confirmButtonText: this.languageService.isRTL() ? 'حسناً' : 'OK'
    });
  }

  async confirm(message: string, title?: string): Promise<boolean> {
    const result = await Swal.fire({
      title: title || 'Confirm',
      text: message,
      icon: 'question',
      showCancelButton: true,
      confirmButtonText: this.languageService.isRTL() ? 'نعم' : 'Yes',
      cancelButtonText: this.languageService.isRTL() ? 'لا' : 'No',
      confirmButtonColor: '#3085d6',
      cancelButtonColor: '#d33',
      reverseButtons: this.languageService.isRTL()
    });
    return result.isConfirmed;
  }
}
