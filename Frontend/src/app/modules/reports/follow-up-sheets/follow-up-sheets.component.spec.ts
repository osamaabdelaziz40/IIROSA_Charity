import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterTestingModule } from '@angular/router/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';

import { FollowUpSheetsComponent } from './follow-up-sheets.component';

describe('FollowUpSheetsComponent', () => {
  let component: FollowUpSheetsComponent;
  let fixture: ComponentFixture<FollowUpSheetsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        FollowUpSheetsComponent,
        ReactiveFormsModule,
        RouterTestingModule,
        HttpClientTestingModule,
        TranslateModule.forRoot()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(FollowUpSheetsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
