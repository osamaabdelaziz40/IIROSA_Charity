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
  @Input() educationLevelOptions: Array<{ id: string; name: string }> = [];
  @Input() healthStatusOptions: Array<{ id: string; name: string }> = [];
  @Input() familyId: string | null = null;
  /** UC-ORP-10 — duplicate-phone flag state, owned by the parent family form */
  @Input() phoneCheck?: PhoneCheckState;
  @Output() save = new EventEmitter<void>();

  constructor() {}

  ngOnInit(): void {}
}
