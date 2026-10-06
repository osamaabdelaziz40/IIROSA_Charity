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

  /**
   * Modal that shows a one-time generated password (and optionally the login it belongs to) with a
   * copy action.
   *
   * Used where the value is not retrievable afterwards — a charity password reset (UC-3.5) is the
   * current caller — so the operator has to read it off the screen and hand it over now. A toast
   * would be gone before they could write it down, which is why this is a modal with an explicit
   * close button.
   *
   * Built on SweetAlert2 like every other dialog in the application so RTL, focus handling and
   * styling stay consistent. The layout classes live in `styles.scss` because Swal renders outside
   * the component tree and scoped styles would not reach it.
   */
  showGeneratedPassword(params: {
    title: string;
    loginLabel: string;
    login?: string;
    passwordLabel: string;
    password: string;
    copyLabel: string;
    copiedLabel: string;
    closeLabel: string;
    note?: string;
  }): Promise<void> {
    // The values are server data; escape them rather than trusting them as markup.
    const escapeHtml = (value: string) =>
      value.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');

    const loginRow = params.login
      ? `<div class="swal-password-row">
           <span class="swal-password-label">${escapeHtml(params.loginLabel)}</span>
           <span class="swal-password-value" dir="ltr">${escapeHtml(params.login)}</span>
         </div>`
      : '';

    return Swal.fire({
      icon: 'success',
      title: params.title,
      html: `
        <div class="swal-password" dir="${this.languageService.isRTL() ? 'rtl' : 'ltr'}">
          ${loginRow}
          <div class="swal-password-row">
            <span class="swal-password-label">${escapeHtml(params.passwordLabel)}</span>
            <code class="swal-password-value swal-password-code" dir="ltr">${escapeHtml(params.password)}</code>
          </div>
          <button type="button" class="swal-password-copy" id="swal-password-copy">
            <i class="fe fe-copy" aria-hidden="true"></i>
            <span>${escapeHtml(params.copyLabel)}</span>
          </button>
          ${params.note ? `<p class="swal-password-note">${escapeHtml(params.note)}</p>` : ''}
        </div>`,
      confirmButtonText: params.closeLabel,
      didOpen: () => {
        const copyButton = document.getElementById('swal-password-copy');
        copyButton?.addEventListener('click', () => {
          // The value stays on screen whether or not the clipboard API is available, so a failure is
          // silent rather than an error the operator can do nothing about.
          navigator.clipboard?.writeText(params.password).then(
            () => {
              copyButton.classList.add('is-copied');
              copyButton.innerHTML =
                `<i class="fe fe-check" aria-hidden="true"></i><span>${escapeHtml(params.copiedLabel)}</span>`;
            },
            () => undefined
          );
        });
      }
    }).then(() => undefined);
  }
}
