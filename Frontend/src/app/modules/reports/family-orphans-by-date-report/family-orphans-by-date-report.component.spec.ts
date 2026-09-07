import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';

import { FamilyOrphansByDateReportComponent } from './family-orphans-by-date-report.component';

describe('FamilyOrphansByDateReportComponent', () => {
  let component: FamilyOrphansByDateReportComponent;
  let fixture: ComponentFixture<FamilyOrphansByDateReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        HttpClientTestingModule,
        ReactiveFormsModule,
        TranslateModule.forRoot(),
        FamilyOrphansByDateReportComponent
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(FamilyOrphansByDateReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
