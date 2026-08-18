import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-modal',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  template: `
    <div class="modal fade show d-block" [class.show]="visible" [style.display]="visible ? 'block' : 'none'" tabindex="-1">
      <div class="modal-dialog modal-dialog-centered" [class]="'modal-' + size">
        <div class="modal-content">
          <div class="modal-header" *ngIf="title">
            <h5 class="modal-title">{{ title | translate }}</h5>
            <button type="button" class="btn-close" (click)="close.emit()"></button>
          </div>
          <div class="modal-body">
            <ng-content></ng-content>
          </div>
          <div class="modal-footer" *ngIf="showFooter">
            <button type="button" class="btn btn-secondary" (click)="cancel.emit()">
              {{ cancelText | translate }}
            </button>
            <button type="button" class="btn btn-primary" (click)="confirm.emit()">
              {{ confirmText | translate }}
            </button>
          </div>
        </div>
      </div>
    </div>
    <div class="modal-backdrop fade show" *ngIf="visible"></div>
  `,
  styles: [`
    .modal {
      z-index: 1050;
    }
    .modal-backdrop {
      z-index: 1040;
    }
  `]
})
export class ModalComponent {
  @Input() visible: boolean = false;
  @Input() title: string = '';
  @Input() size: 'sm' | 'lg' | 'xl' = 'lg';
  @Input() showFooter: boolean = true;
  @Input() cancelText: string = 'common.cancel';
  @Input() confirmText: string = 'common.confirm';

  @Output() close = new EventEmitter<void>();
  @Output() confirm = new EventEmitter<void>();
  @Output() cancel = new EventEmitter<void>();
}
