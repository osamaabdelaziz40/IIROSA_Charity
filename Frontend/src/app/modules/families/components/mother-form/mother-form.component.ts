import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { SharedModule } from '../../../../shared/shared.module';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { PhoneCheckState } from '../../models/family.model';

@Component({
  selector: 'app-mother-form',
  standalone: true,
  imports: [
    FormsModule,
    ReactiveFormsModule,
    SharedModule,
    TranslateModule
  ],
  templateUrl: './mother-form.component.html',
  styleUrls: ['./mother-form.component.scss']
})
export class MotherFormComponent implements OnInit {
  @Input() motherForm!: FormGroup;
  /** الجنسية — Country lookup options (owned by the parent family form) */
  @Input() nationalityOptions: Array<{ id: number; name: string }> = [];
  /** الحالة الصحية — HealthStatus lookup options */
  @Input() healthStatusOptions: Array<{ id: number; name: string }> = [];
  /** سبب الوفاة — DeathReason lookup options (طبيعية / مرض / حادث) */
  @Input() deathReasonOptions: Array<{ id: number; name: string }> = [];
  /** The uploaded/stored death certificate shown in edit mode (server id + display name) */
  @Input() deathCertificate: { id: string; fileName: string } | null = null;
  @Input() familyId: string | null = null;
  /** UC-ORP-10 — duplicate-phone flag state, owned by the parent family form */
  @Input() phoneCheck?: PhoneCheckState;
  @Output() save = new EventEmitter<void>();
  /** A newly picked certificate file — the parent uploads it and stores the returned id */
  @Output() deathCertificateSelected = new EventEmitter<File>();
  @Output() deathCertificateRemoved = new EventEmitter<void>();

  /** Collapsed by default — header click toggles the card (accordion idiom) */
  expanded = false;

  constructor() {}

  toggleCard(): void {
    this.expanded = !this.expanded;
  }

  ngOnInit(): void {}

  onDeathCertificateChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.deathCertificateSelected.emit(input.files[0]);
      input.value = '';
    }
  }
}
