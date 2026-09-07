import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClientTestingModule } from '@angular/common/http/testing';

import { BeneficiaryFamilyDetailsComponent } from './beneficiary-family-details.component';

describe('BeneficiaryFamilyDetailsComponent', () => {
  let component: BeneficiaryFamilyDetailsComponent;
  let fixture: ComponentFixture<BeneficiaryFamilyDetailsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        BeneficiaryFamilyDetailsComponent,
        ReactiveFormsModule,
        HttpClientTestingModule,
        TranslateModule.forRoot()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(BeneficiaryFamilyDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
