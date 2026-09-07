import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterTestingModule } from '@angular/router/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';

import { NewBeneficiariesReportComponent } from './new-beneficiaries-report.component';

describe('NewBeneficiariesReportComponent', () => {
  let component: NewBeneficiariesReportComponent;
  let fixture: ComponentFixture<NewBeneficiariesReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        NewBeneficiariesReportComponent,
        ReactiveFormsModule,
        RouterTestingModule,
        HttpClientTestingModule,
        TranslateModule.forRoot()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(NewBeneficiariesReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
