import { Component, EventEmitter, Input, Output, booleanAttribute } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

let nextUid = 0;

/**
 * Collapsible section card — the create/edit form "accordion" idiom. Renders a
 * card whose header toggles the body; collapsed by default. The collapse is
 * driven by Angular class binding on `.card-body.collapse` — NOT data-bs-toggle:
 * bootstrap.js is double-loaded in this app and data-api clicks double-toggle.
 *
 * Usage:
 *   <app-collapsible-card titleKey="missions.basicInformation" icon="fe-info">
 *     ...section fields...
 *   </app-collapsible-card>
 *
 * Bind [(expanded)] to observe/control the state from the parent, or call
 * open() via ViewChildren to reveal hidden invalid fields after a failed submit.
 */
@Component({
  selector: 'app-collapsible-card',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  templateUrl: './collapsible-card.component.html',
  styleUrls: ['./collapsible-card.component.scss']
})
export class CollapsibleCardComponent {
  /** i18n key of the header title (no hard-coded UI strings) */
  @Input() titleKey = '';
  /** feather icon classes without the base (e.g. 'fe-info' → 'fe fe-info') */
  @Input() icon = '';
  /** collapsed by default */
  @Input({ transform: booleanAttribute }) expanded = false;
  @Output() expandedChange = new EventEmitter<boolean>();
  /** unique id linking the header (aria-controls) to the collapsible body */
  readonly bodyId = `collapsible-card-body-${nextUid++}`;

  toggle(): void {
    this.expanded = !this.expanded;
    this.expandedChange.emit(this.expanded);
  }

  /** Reveal the card (idempotent) — used after a failed submit to surface errors */
  open(): void {
    if (!this.expanded) {
      this.expanded = true;
      this.expandedChange.emit(true);
    }
  }
}
