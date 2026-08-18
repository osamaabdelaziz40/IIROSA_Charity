import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

export interface RelativeDisplay {
  fullName: string;
  relationshipType: string;
  gender: string;
  dateOfBirth: string;
  phone?: string;
  isLivingWithFamily: boolean;
}

@Component({
  selector: 'app-relatives-list',
  standalone: true,
  imports: [
    CommonModule,
    TranslateModule
  ],
  templateUrl: './relatives-list.component.html',
  styleUrls: ['./relatives-list.component.scss']
})
export class RelativesListComponent implements OnInit {
  @Input() relatives: RelativeDisplay[] = [];
  @Output() removeRelative = new EventEmitter<number>();
  @Output() calculateAgeRequest = new EventEmitter<string>();

  constructor() {}

  ngOnInit(): void {}

  calculateAge(dateOfBirth: string): number {
    const today = new Date();
    const birthDate = new Date(dateOfBirth);
    let age = today.getFullYear() - birthDate.getFullYear();
    const monthDiff = today.getMonth() - birthDate.getMonth();
    if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birthDate.getDate())) {
      age--;
    }
    return age;
  }

  onRemoveRelative(index: number): void {
    this.removeRelative.emit(index);
  }
}
