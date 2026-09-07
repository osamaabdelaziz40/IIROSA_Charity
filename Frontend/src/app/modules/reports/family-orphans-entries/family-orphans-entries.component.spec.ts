import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClientTestingModule } from '@angular/common/http/testing';

import { FamilyOrphansEntriesComponent } from './family-orphans-entries.component';

describe('FamilyOrphansEntriesComponent', () => {
  let component: FamilyOrphansEntriesComponent;
  let fixture: ComponentFixture<FamilyOrphansEntriesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        FamilyOrphansEntriesComponent,
        ReactiveFormsModule,
        HttpClientTestingModule,
        TranslateModule.forRoot()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(FamilyOrphansEntriesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
