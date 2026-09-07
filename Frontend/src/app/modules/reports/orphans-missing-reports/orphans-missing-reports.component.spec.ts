import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClientTestingModule } from '@angular/common/http/testing';

import { OrphansMissingReportsComponent } from './orphans-missing-reports.component';

describe('OrphansMissingReportsComponent', () => {
  let component: OrphansMissingReportsComponent;
  let fixture: ComponentFixture<OrphansMissingReportsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        OrphansMissingReportsComponent,
        ReactiveFormsModule,
        HttpClientTestingModule,
        TranslateModule.forRoot()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(OrphansMissingReportsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
