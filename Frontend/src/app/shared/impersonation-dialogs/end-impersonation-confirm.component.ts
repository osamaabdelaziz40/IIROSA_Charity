import { Component, Input } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { ImpersonationService } from '../../core/services/impersonation.service';
import { DatePipe } from '@angular/common';

/**
 * End Impersonation Confirmation Dialog
 * Shows before ending an impersonation session to prevent accidental termination
 */
@Component({
  selector: 'app-end-impersonation-confirm',
  template: `
    <div class="modal-header">
      <h4 class="modal-title">
        <i class="fas fa-sign-out-alt text-warning"></i>
        End Impersonation Session
      </h4>
      <button type="button" class="close" (click)="activeModal.dismiss()" aria-label="Close">
        <span aria-hidden="true">&times;</span>
      </button>
    </div>
    <div class="modal-body">
      <div class="alert alert-warning d-flex align-items-center">
        <i class="fas fa-exclamation-triangle fa-2x mr-3"></i>
        <div>
          <strong>Warning:</strong> You are about to end your impersonation session.
        </div>
      </div>

      <div class="session-info mt-4">
        <h5>Session Summary</h5>
        <table class="table table-borderless">
          <tbody>
            <tr *ngIf="currentStatus">
              <td><strong>Impersonated User:</strong></td>
              <td>
                {{ currentStatus.currentUser?.fullName || currentStatus.currentUser?.username }}
                <span class="badge badge-secondary ml-2">
                  {{ getFirstRole() }}
                </span>
              </td>
            </tr>
            <tr *ngIf="currentStatus">
              <td><strong>Session Started:</strong></td>
              <td>{{ getFormattedStartTime() }}</td>
            </tr>
            <tr *ngIf="duration > 0">
              <td><strong>Session Duration:</strong></td>
              <td>{{ formatDuration() }}</td>
            </tr>
            <tr>
              <td colspan="2" class="text-muted">
                <small>
                  <i class="fas fa-info-circle"></i>
                  After ending impersonation, you will return to your original admin account.
                </small>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
    <div class="modal-footer">
      <button type="button" class="btn btn-secondary" (click)="activeModal.dismiss()">
        <i class="fas fa-times"></i>
        Cancel
      </button>
      <button type="button" class="btn btn-warning" (click)="confirmEnd()">
        <i class="fas fa-sign-out-alt"></i>
        End Impersonation
      </button>
    </div>
  `
})
export class EndImpersonationConfirmComponent {
  currentStatus = this.impersonationService.getCurrentStatus();
  duration = 0;

  constructor(
    public activeModal: NgbActiveModal,
    public impersonationService: ImpersonationService,
    private datePipe: DatePipe
  ) {
    // Calculate session duration
    if (this.currentStatus && this.currentStatus.sessionStartTime) {
      const startTime = new Date(this.currentStatus.sessionStartTime);
      const now = new Date();
      this.duration = Math.floor((now.getTime() - startTime.getTime()) / 1000);
    }
  }

  confirmEnd(): void {
    this.activeModal.close(true);
  }

  getFirstRole(): string {
    const roles = this.currentStatus?.currentUser?.roles;
    if (roles && roles.length > 0) {
      return roles[0];
    }
    return 'User';
  }

  getFormattedStartTime(): string {
    if (!this.currentStatus?.sessionStartTime) {
      return '';
    }
    return this.datePipe.transform(this.currentStatus.sessionStartTime, 'medium') || '';
  }

  formatDuration(): string {
    return this.impersonationService.formatDuration(this.duration);
  }
}
