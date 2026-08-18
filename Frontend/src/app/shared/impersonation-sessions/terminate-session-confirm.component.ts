import { Component, Input } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { ActiveImpersonationSession } from '../../core/models/impersonation.model';

/**
 * Terminate Session Confirmation Dialog
 * Shows before forcefully terminating another admin's impersonation session
 * Implements UC-19.4 (Terminate Impersonation Session - Admin Override)
 */
@Component({
  selector: 'app-terminate-session-confirm',
  template: `
    <div class="modal-header bg-danger text-white">
      <h4 class="modal-title">
        <i class="fas fa-exclamation-triangle"></i>
        Terminate Impersonation Session
      </h4>
      <button type="button" class="close text-white" (click)="activeModal.dismiss()" aria-label="Close">
        <span aria-hidden="true">&times;</span>
      </button>
    </div>
    <div class="modal-body">
      <div class="alert alert-danger d-flex align-items-center">
        <i class="fas fa-exclamation-circle fa-2x mr-3"></i>
        <div>
          <strong>Danger:</strong> This action cannot be undone.
        </div>
      </div>

      <p class="mt-3">
        You are about to forcefully terminate the following impersonation session:
      </p>

      <div class="card bg-light">
        <div class="card-body">
          <table class="table table-borderless table-sm mb-0">
            <tbody>
              <tr>
                <td><strong>Impersonator:</strong></td>
                <td>
                  {{ session?.impersonatorUserName }}
                  <span class="badge badge-secondary ml-2">{{ session?.impersonatorRole }}</span>
                </td>
              </tr>
              <tr>
                <td><strong>Impersonated User:</strong></td>
                <td>
                  {{ session?.impersonatedUserName }}
                  <span class="badge badge-info ml-2">{{ session?.impersonatedRole }}</span>
                </td>
              </tr>
              <tr>
                <td><strong>Started At:</strong></td>
                <td>{{ session?.startTime | date:'medium' }}</td>
              </tr>
              <tr>
                <td><strong>Duration:</strong></td>
                <td>
                  <i class="far fa-clock"></i>
                  {{ session?.durationMinutes }} minute(s)
                </td>
              </tr>
              <tr>
                <td><strong>IP Address:</strong></td>
                <td><code>{{ session?.originalUserIpAddress || 'Unknown' }}</code></td>
              </tr>
              <tr>
                <td><strong>Actions Performed:</strong></td>
                <td>{{ session?.actionsPerformedCount }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <div class="form-group mt-4">
        <label for="terminationReason">Reason for termination (required):</label>
        <textarea
          class="form-control"
          id="terminationReason"
          rows="3"
          [(ngModel)]="terminationReason"
          placeholder="Please provide a reason for this action..."
        ></textarea>
      </div>
    </div>
    <div class="modal-footer">
      <button type="button" class="btn btn-secondary" (click)="activeModal.dismiss()">
        <i class="fas fa-times"></i>
        Cancel
      </button>
      <button
        type="button"
        class="btn btn-danger"
        [disabled]="!terminationReason || terminationReason.trim().length === 0"
        (click)="confirmTermination()"
      >
        <i class="fas fa-stop-circle"></i>
        Terminate Session
      </button>
    </div>
  `
})
export class TerminateSessionConfirmComponent {
  @Input() session?: ActiveImpersonationSession;
  terminationReason = '';

  constructor(public activeModal: NgbActiveModal) {}

  confirmTermination(): void {
    if (this.terminationReason && this.terminationReason.trim().length > 0) {
      this.activeModal.close(this.terminationReason);
    }
  }
}
