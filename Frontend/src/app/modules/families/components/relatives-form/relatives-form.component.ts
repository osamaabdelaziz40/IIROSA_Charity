import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { SharedModule } from '../../../../shared/shared.module';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

export interface RelativeData {
  fullName: string;
  relationshipType: string;
  gender: string;
  dateOfBirth: string;
  placeOfBirth?: string;
  nationalId?: string;
  educationLevel?: string;
  job?: string;
  monthlyIncome?: number;
  healthStatus?: string;
  phone?: string;
  address?: string;
  isAlive: boolean;
  isLivingWithFamily: boolean;
  deathDate?: string;
  notes?: string;
}

@Component({
  selector: 'app-relatives-form',
  standalone: true,
  imports: [
    FormsModule,
    ReactiveFormsModule,
    SharedModule,
    TranslateModule
  ],
  templateUrl: './relatives-form.component.html',
  styleUrls: ['./relatives-form.component.scss']
})
export class RelativesFormComponent implements OnInit {
  @Input() relativesForm!: FormGroup;
  @Input() educationLevelOptions: Array<{ id: string; name: string }> = [];
  /** الحالة الصحية — HealthStatus lookup rows (numeric ids, shared with father/mother) */
  @Input() healthStatusOptions: Array<{ id: number; name: string }> = [];
  @Input() relationshipOptions: Array<{ id: string; name: string }> = [];
  @Input() relativesCount: number = 0;
  @Output() addRelative = new EventEmitter<RelativeData>();
  @Output() calculateAge = new EventEmitter<string>();

  constructor() {}

  ngOnInit(): void {}

  onAddRelative(): void {
    if (this.relativesForm.invalid) {
      // Mark all fields as touched to show validation errors
      Object.keys(this.relativesForm.controls).forEach(key => {
        const control = this.relativesForm.get(key);
        control?.markAsTouched();
      });
      return;
    }

    const formValue = this.relativesForm.value;
    const relativeData: RelativeData = {
      fullName: formValue.fullName,
      relationshipType: formValue.relationshipType,
      gender: formValue.gender,
      dateOfBirth: formValue.dateOfBirth,
      placeOfBirth: formValue.placeOfBirth,
      nationalId: formValue.nationalId,
      educationLevel: formValue.educationLevel,
      job: formValue.job,
      monthlyIncome: formValue.monthlyIncome,
      healthStatus: formValue.healthStatus,
      phone: formValue.phone,
      address: formValue.address,
      isAlive: formValue.isAlive !== false,
      isLivingWithFamily: formValue.isLivingWithFamily === true,
      deathDate: formValue.deathDate || undefined,
      notes: formValue.notes
    };

    this.addRelative.emit(relativeData);
  }

  getRelativesCount(): number {
    return this.relativesCount;
  }
}
