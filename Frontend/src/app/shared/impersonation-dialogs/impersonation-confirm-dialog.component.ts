import { Component, Input } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { UserSearchResult } from '../../core/models/impersonation.model';
import { NotificationService, NotificationMessage } from '../../core/services/notification.service';

/**
 * Impersonation Confirmation Dialog
 * Shows details of the user to be impersonated and requires explicit confirmation
 * Implements UC-19.1 (Start User Impersonation - Confirmation Step)
 */
@Component({
  selector: 'app-impersonation-confirm-dialog',
  template: `
    <div class="modal-header">
      <h4 class="modal-title">
        <i class="fas fa-exclamation-triangle text-warning"></i>
        Confirm Impersonation
      </h4>
      <button type="button" class="close" (click)="activeModal.dismiss()" aria-label="Close">
        <span aria-hidden="true">&times;</span>
      </button>
    </div>
    <div class="modal-body">
      <div class="alert alert-warning">
        <h5 class="alert-heading">
          <i class="fas fa-shield-alt"></i>
          IMPORTANT WARNING
        </h5>
        <p class="mb-0">
          You are about to impersonate another user. Please read carefully:
        </p>
        <ul class="mb-0 mt-2">
          <li>You will be logged in as the target user</li>
          <li>All your actions will be <strong>audited</strong></li>
          <li>The impersonation indicator will be visible at all times</li>
          <li>Some operations may be restricted for security reasons</li>
        </ul>
      </div>

      <div class="card mt-4">
        <div class="card-header bg-light">
          <h6 class="mb-0">
            <i class="fas fa-user"></i>
            User to Impersonate
          </h6>
        </div>
        <div class="card-body">
          <table class="table table-borderless">
            <tbody>
              <tr>
                <td><strong>Name:</strong></td>
                <td>{{ user?.fullName || user?.username }}</td>
              </tr>
              <tr>
                <td><strong>Email:</strong></td>
                <td>{{ user?.email }}</td>
              </tr>
              <tr>
                <td><strong>Username:</strong></td>
                <td>{{ user?.username }}</td>
              </tr>
              <tr>
                <td><strong>Role:</strong></td>
                <td>
                  <span class="badge badge-primary">
                    {{ user?.role || 'User' }}
                  </span>
                </td>
              </tr>
              <tr>
                <td><strong>Status:</strong></td>
                <td>
                  <span *ngIf="user?.isActive" class="badge badge-success">Active</span>
                  <span *ngIf="!user?.isActive" class="badge badge-danger">Inactive</span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <div class="form-check mt-4">
        <input
          type="checkbox"
          class="form-check-input"
          id="confirmCheck"
          [(ngModel)]="isConfirmed"
        />
        <label class="form-check-label" for="confirmCheck">
          <strong>I understand the consequences and want to proceed with impersonation</strong>
        </label>
      </div>
    </div>
    <div class="modal-footer">
      <button type="button" class="btn btn-secondary" (click)="activeModal.dismiss()">
        <i class="fas fa-times"></i>
        Cancel
      </button>
      <button
        type="button"
        class="btn btn-warning"
        [disabled]="!isConfirmed"
        (click)="confirmImpersonation()"
      >
        <i class="fas fa-user-secret"></i>
        Start Impersonation
      </button>
    </div>
  `,
  styles: [`
    .table td {
      width: 40%;
    }
    .table td + td {
      width: 60%;
    }
  `]
})
export class ImpersonationConfirmDialogComponent {
  @Input() user?: UserSearchResult;
  isConfirmed = false;

  constructor(
    public activeModal: NgbActiveModal,
    private notificationService: NotificationService
  ) {}

  confirmImpersonation(): void {
    if (this.isConfirmed && this.user) {
      this.activeModal.close(this.user);
    }
  }
}
