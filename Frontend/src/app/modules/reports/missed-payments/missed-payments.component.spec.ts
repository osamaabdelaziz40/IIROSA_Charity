import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterTestingModule } from '@angular/router/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';

import { MissedPaymentsComponent } from './missed-payments.component';

describe('MissedPaymentsComponent', () => {
  let component: MissedPaymentsComponent;
  let fixture: ComponentFixture<MissedPaymentsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        MissedPaymentsComponent,
        ReactiveFormsModule,
        RouterTestingModule,
        HttpClientTestingModule,
        TranslateModule.forRoot()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(MissedPaymentsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
