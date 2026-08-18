import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  template: `
    <div class="empty-state text-center py-5">
      <div class="empty-state-icon">
        <i [class]="'fe ' + icon"></i>
      </div>
      <h4 class="empty-state-title">{{ title | translate }}</h4>
      <p class="empty-state-description">{{ description | translate }}</p>
      <button *ngIf="action" class="btn btn-primary" (click)="action.click.emit($event)">
        <i *ngIf="action.icon" [class]="'fe ' + action.icon"></i>
        {{ action.label | translate }}
      </button>
    </div>
  `,
  styles: [`
    .empty-state {
      padding: 3rem 1rem;
    }
    .empty-state-icon {
      font-size: 4rem;
      color: #6c757d;
      margin-bottom: 1rem;
      opacity: 0.5;
    }
    .empty-state-title {
      font-size: 1.25rem;
      font-weight: 500;
      margin-bottom: 0.5rem;
      color: #495057;
    }
    .empty-state-description {
      color: #6c757d;
      margin-bottom: 1.5rem;
    }
  `]
})
export class EmptyStateComponent {
  @Input() icon: string = 'fe-info';
  @Input() title: string = 'common.noData';
  @Input() description: string = '';
  @Input() action?: { label: string; icon?: string; click: any };
}
