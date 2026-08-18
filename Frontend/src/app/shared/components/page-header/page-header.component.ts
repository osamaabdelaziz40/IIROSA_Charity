import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-page-header',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule],
  template: `
    <div class="page-header">
      <div class="page-breadcrumb">
        <nav aria-label="breadcrumb">
          <ol class="breadcrumb">
            <li class="breadcrumb-item" *ngFor="let item of breadcrumbs; let last = last">
              <a *ngIf="!last; else link" [routerLink]="item.url">{{ item.label | translate }}</a>
              <ng-template #link><span>{{ item.label | translate }}</span></ng-template>
            </li>
          </ol>
        </nav>
      </div>
      <div class="page-title">
        <div class="title-section">
          <i *ngIf="icon" [class]="icon" class="title-icon"></i>
          <div>
            <h3>{{ title | translate }}</h3>
            <p *ngIf="subtitle" class="subtitle">{{ subtitle }}</p>
          </div>
        </div>
        <div class="page-actions" *ngIf="actions.length > 0">
          <ng-container *ngFor="let action of actions">
            <button
              class="btn"
              [class]="'btn-' + action.type"
              (click)="action.click()">
              <i *ngIf="action.icon" [class]="'fe ' + action.icon"></i>
              <span>{{ action.label | translate }}</span>
            </button>
          </ng-container>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .page-header {
      margin-bottom: 1.5rem;
    }
    .page-title {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-top: 0.5rem;
    }
    .title-section {
      display: flex;
      align-items: center;
      gap: 1rem;
    }
    .title-icon {
      font-size: 1.5rem;
      color: var(--primary, #4d7cfe);
    }
    .title-section h3 {
      margin: 0;
      font-size: 1.5rem;
      font-weight: 600;
    }
    .subtitle {
      margin: 0.25rem 0 0 0;
      font-size: 0.875rem;
      color: var(--text-muted, #6c7293);
    }
    .page-actions {
      display: flex;
      gap: 0.5rem;
    }
    .breadcrumb {
      margin-bottom: 0;
      background: transparent;
      padding: 0;
    }
  `]
})
export class PageHeaderComponent {
  @Input() title: string = '';
  @Input() subtitle: string = '';
  @Input() icon: string = '';
  @Input() breadcrumbs: BreadcrumbItem[] = [];
  @Input() actions: PageAction[] = [];
}

export interface BreadcrumbItem {
  label: string;
  url?: string;
}

export interface PageAction {
  label: string;
  type?: string;
  icon?: string;
  click: () => void;
}
