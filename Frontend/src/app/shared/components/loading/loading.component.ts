import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-loading',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  template: `
    <div class="loading-overlay" *ngIf="loading">
      <div class="loading-spinner">
        <div class="spinner-border text-primary" role="status">
          <span class="visually-hidden">Loading...</span>
        </div>
        <p *ngIf="message">{{ message | translate }}</p>
      </div>
    </div>
  `,
  styles: [`
    .loading-overlay {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(0, 0, 0, 0.5);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 9999;
    }
    .loading-spinner {
      text-align: center;
      color: white;
    }
    .spinner-border {
      width: 3rem;
      height: 3rem;
    }
  `]
})
export class LoadingComponent {
  @Input() loading: boolean = true;
  @Input() message: string = 'common.loading';
}
