import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterTestingModule } from '@angular/router/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';

import { OrphanFilesComponent } from './orphan-files.component';

describe('OrphanFilesComponent', () => {
  let component: OrphanFilesComponent;
  let fixture: ComponentFixture<OrphanFilesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        OrphanFilesComponent,
        ReactiveFormsModule,
        RouterTestingModule,
        HttpClientTestingModule,
        TranslateModule.forRoot()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(OrphanFilesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
