import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClientTestingModule } from '@angular/common/http/testing';

import { OrphansMissingFilesComponent } from './orphans-missing-files.component';

describe('OrphansMissingFilesComponent', () => {
  let component: OrphansMissingFilesComponent;
  let fixture: ComponentFixture<OrphansMissingFilesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        OrphansMissingFilesComponent,
        ReactiveFormsModule,
        HttpClientTestingModule,
        TranslateModule.forRoot()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(OrphansMissingFilesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
