import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClientTestingModule } from '@angular/common/http/testing';

import { OrphanDataReportComponent } from './orphan-data-report.component';

describe('OrphanDataReportComponent', () => {
  let component: OrphanDataReportComponent;
  let fixture: ComponentFixture<OrphanDataReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        OrphanDataReportComponent,
        ReactiveFormsModule,
        HttpClientTestingModule,
        TranslateModule.forRoot()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(OrphanDataReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
