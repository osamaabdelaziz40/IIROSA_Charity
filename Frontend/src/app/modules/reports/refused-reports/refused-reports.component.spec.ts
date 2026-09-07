import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterTestingModule } from '@angular/router/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';

import { RefusedReportsComponent } from './refused-reports.component';

describe('RefusedReportsComponent', () => {
  let component: RefusedReportsComponent;
  let fixture: ComponentFixture<RefusedReportsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        RefusedReportsComponent,
        ReactiveFormsModule,
        RouterTestingModule,
        HttpClientTestingModule,
        TranslateModule.forRoot()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(RefusedReportsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
