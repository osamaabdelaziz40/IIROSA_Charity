import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { SharedModule } from '../../../../shared/shared.module';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-father-form',
  standalone: true,
  imports: [
    FormsModule,
    ReactiveFormsModule,
    SharedModule,
    TranslateModule
  ],
  templateUrl: './father-form.component.html',
  styleUrls: ['./father-form.component.scss']
})
export class FatherFormComponent implements OnInit {
  @Input() fatherForm!: FormGroup;
  @Input() educationLevelOptions: Array<{ id: string; name: string }> = [];
  @Input() healthStatusOptions: Array<{ id: string; name: string }> = [];
  @Input() familyId: string | null = null;
  @Output() save = new EventEmitter<void>();

  constructor() {}

  ngOnInit(): void {}
}
