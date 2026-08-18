import { Component, OnInit } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { ImpersonationService } from '../../core/services/impersonation.service';
import { UserSearchResult } from '../../core/models/impersonation.model';
import { Observable, Subject, of } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap, catchError, startWith } from 'rxjs/operators';
import { FormControl } from '@angular/forms';
import { NotificationService, NotificationMessage } from '../../core/services/notification.service';

/**
 * Quick Impersonation Dialog
 * Allows admin to quickly search and impersonate a user by username/email
 * Implements UC-19.5 (Impersonate by Username - Quick Access)
 */
@Component({
  selector: 'app-quick-impersonation-dialog',
  template: `
    <div class="modal-header">
      <h4 class="modal-title">
        <i class="fas fa-user-secret"></i>
        Quick Impersonation
      </h4>
      <button type="button" class="close" (click)="activeModal.dismiss()" aria-label="Close">
        <span aria-hidden="true">&times;</span>
      </button>
    </div>
    <div class="modal-body">
      <!-- Search Input -->
      <div class="search-section mb-4">
        <label for="userSearch" class="form-label font-weight-bold">
          Search User to Impersonate
        </label>
        <div class="input-group">
          <div class="input-group-prepend">
            <span class="input-group-text">
              <i class="fas fa-search"></i>
            </span>
          </div>
          <input
            type="text"
            class="form-control"
            id="userSearch"
            placeholder="Enter username or email (min. 3 characters)..."
            [formControl]="searchControl"
            autocomplete="off"
          />
          <div class="input-group-append" *ngIf="searchControl.value">
            <button class="btn btn-outline-secondary" type="button" (click)="clearSearch()">
              <i class="fas fa-times"></i>
            </button>
          </div>
        </div>
        <small class="form-text text-muted">
          Search by username or email. Results are limited to 10 users.
        </small>
      </div>

      <!-- Loading Indicator -->
      <div *ngIf="isSearching" class="text-center py-4">
        <div class="spinner-border text-primary" role="status">
          <span class="sr-only">Searching...</span>
        </div>
        <p class="text-muted mt-2">Searching users...</p>
      </div>

      <!-- Search Results -->
      <div *ngIf="!isSearching && searchResults.length > 0" class="search-results">
        <h6 class="text-muted mb-3">
          <i class="fas fa-users"></i>
          Found {{ searchResults.length }} user(s)
        </h6>
        <div class="list-group">
          <a
            *ngFor="let user of searchResults"
            class="list-group-item list-group-item-action d-flex justify-content-between align-items-center"
            [class.disabled]="!user.canImpersonate"
            (click)="selectUser(user)"
          >
            <div>
              <div class="user-name">
                <strong>{{ user.fullName || user.username }}</strong>
                <span class="badge badge-secondary ml-2">{{ user.role || 'User' }}</span>
                <span *ngIf="!user.isActive" class="badge badge-danger ml-1">Inactive</span>
              </div>
              <div class="user-email text-muted small">
                <i class="fas fa-envelope"></i> {{ user.email }}
              </div>
            </div>
            <div *ngIf="user.canImpersonate">
              <button class="btn btn-sm btn-outline-primary">
                <i class="fas fa-user-secret"></i>
                Impersonate
              </button>
            </div>
            <div *ngIf="!user.canImpersonate" class="text-muted small">
              <i class="fas fa-ban text-danger"></i>
              Cannot impersonate
            </div>
          </a>
        </div>
      </div>

      <!-- No Results -->
      <div *ngIf="!isSearching && hasSearched && searchResults.length === 0" class="alert alert-info">
        <i class="fas fa-info-circle"></i>
        No users found matching "{{ searchControl.value }}"
      </div>

      <!-- Validation Message -->
      <div *ngIf="searchControl.value && searchControl.value.length < 3" class="alert alert-warning mt-3">
        <i class="fas fa-exclamation-triangle"></i>
        Please enter at least 3 characters to search
      </div>
    </div>
    <div class="modal-footer">
      <button type="button" class="btn btn-secondary" (click)="activeModal.dismiss()">
        <i class="fas fa-times"></i>
        Cancel
      </button>
    </div>
  `
})
export class QuickImpersonationDialogComponent implements OnInit {
  searchControl = new FormControl<string>('');
  searchResults: UserSearchResult[] = [];
  isSearching = false;
  hasSearched = false;

  constructor(
    public activeModal: NgbActiveModal,
    private impersonationService: ImpersonationService,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    // Setup search with debounce
    this.searchControl.valueChanges.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      switchMap(term => {
        if (!term || term.length < 3) {
          this.searchResults = [];
          return of([]);
        }
        this.isSearching = true;
        this.hasSearched = true;
        return this.impersonationService.searchUsers(term).pipe(
          catchError((error) => {
            this.isSearching = false;
            // Show error notification using NotificationMessage
            const errorMessage: NotificationMessage = {
              type: 'error',
              title: 'Search Failed',
              message: error.error?.message || error.message || 'Failed to search users. Please try again.'
            };
            this.notificationService.show(errorMessage.message, errorMessage.type, errorMessage.title);
            return of([]);
          })
        );
      })
    ).subscribe(results => {
      this.searchResults = results;
      this.isSearching = false;
    });
  }

  clearSearch(): void {
    this.searchControl.setValue('');
    this.searchResults = [];
  }

  selectUser(user: UserSearchResult): void {
    if (!user.canImpersonate) {
      // Show warning notification using NotificationMessage
      const warningMessage: NotificationMessage = {
        type: 'warning',
        title: 'Cannot Impersonate',
        message: `You cannot impersonate ${user.fullName || user.username}. The user may be inactive or have restricted access.`
      };
      this.notificationService.show(warningMessage.message, warningMessage.type, warningMessage.title);
      return;
    }

    this.activeModal.close({
      username: user.username,
      user: user
    });
  }
}
