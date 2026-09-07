import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterTestingModule } from '@angular/router/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';

import { ChequeStatementComponent } from './cheque-statement.component';

describe('ChequeStatementComponent', () => {
  let component: ChequeStatementComponent;
  let fixture: ComponentFixture<ChequeStatementComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        ChequeStatementComponent,
        ReactiveFormsModule,
        RouterTestingModule,
        HttpClientTestingModule,
        TranslateModule.forRoot()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ChequeStatementComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
