import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClientTestingModule } from '@angular/common/http/testing';

import { ExcludedOrphansReportComponent } from './excluded-orphans-report.component';

describe('ExcludedOrphansReportComponent', () => {
  let component: ExcludedOrphansReportComponent;
  let fixture: ComponentFixture<ExcludedOrphansReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        ExcludedOrphansReportComponent,
        ReactiveFormsModule,
        HttpClientTestingModule,
        TranslateModule.forRoot()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ExcludedOrphansReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
